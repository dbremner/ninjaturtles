#region Copyright & licence

// This file is part of NinjaTurtles.
// 
// NinjaTurtles is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// NinjaTurtles is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with NinjaTurtles.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NinjaTurtles.Turtles
{
    /// <summary>
    /// An implementation of <see cref="IMethodTurtle" /> that identifies local
    /// variables and method parameters of the same type, and permutes any
    /// reads from them. For example, if two <see cref="Int32" /> parameters
    /// <c>a</c> and <c>b</c> exist, along with a local variable <c>c</c> of
    /// the same type, then a read from <c>a</c> will be replaced by one from
    /// <c>b</c> and <c>c</c> in turn, and so on.
    /// </summary>
    public class VariableAndParameterReadTurtle : MethodTurtleBase
    {
        /// <summary>
        /// Performs the actual code mutations, returning each with
        /// <c>yield</c> for the calling code to use.
        /// </summary>
        /// <remarks>
        /// Implementing classes should yield the result obtained by calling
        /// the <see mref="DoYield" /> method.
        /// </remarks>
        /// <param name="method">
        /// A <see cref="MethodDefinition" /> for the method on which mutation
        /// testing is to be carried out.
        /// </param>
        /// <param name="module">
        /// A <see cref="Module" /> representing the main module of the
        /// containing assembly.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}" /> of
        /// <see cref="MutantMetaData" /> structures.
        /// </returns>
        protected override IEnumerable<MutantMetaData> DoMutate(MethodDefinition method, Module module)
        {
            var parametersAndVariablesByType = GroupMethodParametersAndVariablesByType(method);

            if (!parametersAndVariablesByType.Any(kv => parametersAndVariablesByType[kv.Key].Count > 1))
            {
                yield break;
            }

            foreach (var keyValuePair in parametersAndVariablesByType.Where(kv => kv.Value.Count > 1))
            {
                var indices = keyValuePair.Value.ToArray();
                var ldargOperands = GetOperandsForParametersAndVariables(method);
                for (int index = 0; index < method.Body.Instructions.Count; index++)
                {
                    var instruction = method.Body.Instructions[index];
                    int? oldIndex = null;
                    if (instruction.OpCode == OpCodes.Ldarg)
                    {
                        int ldargIndex = ((ParameterDefinition)instruction.Operand).Sequence;
                        if (method.IsStatic || ldargIndex > 0)
                        {
                            oldIndex = -1 - ldargIndex;
                        }
                    }
                    if (instruction.OpCode == OpCodes.Ldloc && instruction.Next.OpCode != OpCodes.Ret)
                    {
                        int ldlocIndex = ((VariableDefinition)instruction.Operand).Index;
                        oldIndex = ldlocIndex;
                    }

                    if (!oldIndex.HasValue) continue;

                    int parameterPosition = Array.IndexOf(indices, oldIndex.Value);
                    if (parameterPosition == -1) continue;

                    OpCode originalOpCode = instruction.OpCode;
                    object originalOperand = instruction.Operand;
                    foreach (var sequence in indices)
                    {
                        if (sequence == oldIndex.Value) continue;

                        if (instruction.OpCode == OpCodes.Ldloc
                            && instruction.Previous.OpCode == OpCodes.Stloc
                            &&
                            ((VariableDefinition)instruction.Operand).Index ==
                            ((VariableDefinition)instruction.Previous.Operand).Index
                            && instruction.Previous.Previous.OpCode == OpCodes.Ldarg
                            && ((ParameterDefinition)instruction.Previous.Previous.Operand).Index == -1 - sequence)
                        {
                            // The .NET compiler sometimes adds a pointless
                            // cache of a parameter into a local variable
                            // (oddly, Mono doesn't seem to). We need to not
                            // mutate in this scenario.
                            continue;
                        }

                        if (instruction.IsPartOfCompilerGeneratedDispose())
                        {
                            continue;
                        }

                        instruction.OpCode = sequence >= 0 ? OpCodes.Ldloc : OpCodes.Ldarg;
                        instruction.Operand = ldargOperands[sequence];

                        var description =
                            string.Format(
                                "{0:x4}: read substitution {1}.{2} => {1}.{3}",
                                GetOriginalOffset(index),
                                keyValuePair.Key.Name,
                                GetIndexAsString(oldIndex.Value),
                                GetIndexAsString(sequence));

                        var mutantMetaData = DoYield(method, module, description, index);
                        yield return mutantMetaData;
                    }
                    instruction.OpCode = originalOpCode;
                    instruction.Operand = originalOperand;
                }
            }
        }

        private string GetIndexAsString(int index)
        {
            return index < 0 ? "P" + (-1 - index) : "V" + index;
        }

        private static IDictionary<TypeReference, IList<int>> GroupMethodParametersAndVariablesByType(MethodDefinition method)
        {
            IDictionary<TypeReference, IList<int>> parametersAndVariables = new Dictionary<TypeReference, IList<int>>();
            int offset = method.IsStatic ? 0 : 1;
            foreach (var parameter in method.Parameters)
            {
                var type = parameter.ParameterType;
                if (!parametersAndVariables.ContainsKey(type))
                {
                    parametersAndVariables.Add(type, new List<int>());
                }
                parametersAndVariables[type].Add(-1 - parameter.Index - offset);
            }
            foreach (var variable in method.Body.Variables)
            {
                var type = variable.VariableType;
                if (!parametersAndVariables.ContainsKey(type))
                {
                    parametersAndVariables.Add(type, new List<int>());
                }
                parametersAndVariables[type].Add(variable.Index);
            }
            return parametersAndVariables;
        }

        private static IDictionary<int, object> GetOperandsForParametersAndVariables(MethodDefinition method)
        {
            IDictionary<int, object> operands = new Dictionary<int, object>();
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Ldarg)
                {
                    var parameterDefinition = (ParameterDefinition)instruction.Operand;
                    int sequence = parameterDefinition.Sequence;
                    if (!operands.ContainsKey(-1 - sequence))
                    {
                        operands.Add(-1 - sequence, parameterDefinition);
                    }
                }
            }
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Ldloc)
                {
                    var variableDefinition = (VariableDefinition)instruction.Operand;
                    int index = variableDefinition.Index;
                    if (!operands.ContainsKey(index))
                    {
                        operands.Add(index, variableDefinition);
                    }
                }
            }
            return operands;
        }
    }
}
