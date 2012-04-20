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
// License along with Refix.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using NinjaTurtles.Utilities;

namespace NinjaTurtles.Turtles.Method
{
    public class ParameterAndVariablePermutationTurtle : MethodTurtle
    {
        private ILProcessor _ilProcessor;

        public override string Description
        {
            get { return "Permuting method parameters and variables"; }
        }

        protected override IEnumerable<string> DoMutate(MethodDefinition method, AssemblyDefinition assembly, string fileName)
        {
            var parametersAndVariablesByType = GroupMethodParametersAndVariablesByType(method);
            _ilProcessor = method.Body.GetILProcessor();

            if (!parametersAndVariablesByType.Any(kv => parametersAndVariablesByType[kv.Key].Count > 1))
            {
                yield break;
            }

            foreach (var keyValuePair in parametersAndVariablesByType.Where(kv => kv.Value.Count > 1))
            {
                var indices = keyValuePair.Value.ToArray();
                foreach (var order in new AllPermutationsEnumerable<int>(indices).ToArray())
                {
                    int[] orderArray = order.ToArray();
                    int j = 0;
                    bool isOriginalOrder = orderArray.All(index => index == indices[j++]);
                    if (isOriginalOrder) continue;

                    PermuteIndices(method, indices, orderArray);
                    var output = string.Format("Parameter/variable permutation change for type {0} ({1}) => ({2}) in {3}.{4}",
                                               keyValuePair.Key.Name,
                                               GetIndicesAsString(indices),
                                               GetIndicesAsString(orderArray),
                                               method.DeclaringType.Name,
                                               method.Name);

                    foreach (var p in PlaceFileAndYield(assembly, fileName, output))
                    {
                        yield return p;
                    }

                    PermuteIndices(method, orderArray, indices);
                }
            }
        }

        private string GetIndicesAsString(IEnumerable<int> indices)
        {
            return string.Join(",", indices.Select(i => i < 0 ? "P" + (-1 - i) : "V" + i));
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

        private static void PermuteIndices(MethodDefinition method, int[] fromIndices, int[] toIndices)
        {
            var ldargOperands = GetOperandsForHigherIndexParametersAndVariables(method);
            foreach (var instruction in method.Body.Instructions)
            {
                int? oldIndex = null;

                if (instruction.OpCode == OpCodes.Ldarg)
                {
                    int ldargIndex = ((ParameterDefinition)instruction.Operand).Sequence;
                    if (method.IsStatic || ldargIndex > 0)
                    {
                        oldIndex = -1 - ldargIndex;
                    }
                }
                if (instruction.OpCode == OpCodes.Ldloc)
                {
                    int ldlocIndex = ((VariableDefinition)instruction.Operand).Index;
                    oldIndex = ldlocIndex;
                }
                if (oldIndex.HasValue)
                {
                    int parameterPosition = Array.IndexOf(fromIndices, oldIndex);
                    if (parameterPosition == -1) continue;
                    int newIndex = toIndices[parameterPosition];

                    instruction.OpCode = newIndex >= 0 ? OpCodes.Ldloc : OpCodes.Ldarg;
                    instruction.Operand = ldargOperands[newIndex];
                }
            }
        }

        private static IDictionary<int, object> GetOperandsForHigherIndexParametersAndVariables(MethodDefinition method)
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
