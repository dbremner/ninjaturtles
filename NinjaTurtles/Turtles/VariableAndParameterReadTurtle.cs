﻿#region Copyright & licence

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
            var variablesByType = GroupVariablesByType(method);
            PopulateOperandsInVariables(method, variablesByType);

            if (!variablesByType.Any(kv => variablesByType[kv.Key].Count > 1))
            {
                yield break;
            }

            foreach (var keyValuePair in variablesByType.Where(kv => kv.Value.Count > 1))
            {
                var variables = keyValuePair.Value.ToList();
                for (int index = 0; index < method.Body.Instructions.Count; index++)
                {
                    var instruction = method.Body.Instructions[index];
                    if (instruction.OpCode == OpCodes.Ldloc && instruction.Next.OpCode == OpCodes.Ret) continue;

                    int oldIndex = -1;
                    if (instruction.OpCode == OpCodes.Ldarg)
                    {
                        int parameterIndex = ((ParameterDefinition)instruction.Operand).Sequence;
                        oldIndex = variables.FindIndex(v => v.Type == VariableType.Parameter && v.Index == parameterIndex);
                    }
                    if (instruction.OpCode == OpCodes.Ldloc)
                    {
                        int variableIndex = ((VariableDefinition)instruction.Operand).Index;
                        oldIndex = variables.FindIndex(v => v.Type == VariableType.Local && v.Index == variableIndex);
                    }
                    if (instruction.OpCode == OpCodes.Ldfld)
                    {
                        string fieldName = ((FieldDefinition)instruction.Operand).Name;
                        oldIndex = variables.FindIndex(v => v.Type == VariableType.Field && v.Name == fieldName);
                    }

                    if (oldIndex < 0) continue;

                    OpCode originalOpCode = instruction.OpCode;
                    object originalOperand = instruction.Operand;
                    var originalVariable = variables[oldIndex];

                    for (int newIndex = 0; newIndex < variables.Count; newIndex++)
                    {
                        if (newIndex == oldIndex) continue;
                        var variable = variables[newIndex];
                        if (variable.Operand == null) continue;

                        if (variable.Type == VariableType.Parameter
                            && instruction.OpCode == OpCodes.Ldloc
                            && instruction.Previous.OpCode == OpCodes.Stloc
                            && ((VariableDefinition)instruction.Operand).Index == ((VariableDefinition)instruction.Previous.Operand).Index
                            && instruction.Previous.Previous.OpCode == OpCodes.Ldarg
                            && ((ParameterDefinition)instruction.Previous.Previous.Operand).Index == variable.Index)
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

                        instruction.OpCode = variable.GetOpCode();
                        instruction.Operand = variable.Operand;

                        var description =
                            string.Format(
                                "{0:x4}: read substitution {1}.{2} => {1}.{3}",
                                GetOriginalOffset(index),
                                keyValuePair.Key.Name,
                                originalVariable.Name,
                                variable.Name);

                        var mutantMetaData = DoYield(method, module, description, index);
                        yield return mutantMetaData;
                    }
                    instruction.OpCode = originalOpCode;
                    instruction.Operand = originalOperand;
                }
            }
        }

        private enum VariableType
        {
            Local,
            Parameter,
            Field
        }

        private class Variable
        {
            public Variable(VariableType type, int index, string name)
            {
                Type = type;
                Index = index;
                Name = name;
            }

            public VariableType Type { get; set; }
            public int Index { get; set; }
            public string Name { get; set; }
            public object Operand { get; set; }

            public OpCode GetOpCode()
            {
                switch (Type)
                {
                    case VariableType.Local:
                        return OpCodes.Ldloc;
                    case VariableType.Parameter:
                        return OpCodes.Ldarg;
                    default:
                        return OpCodes.Ldfld;
                }
            }
        }

        private static IDictionary<TypeReference, IList<Variable>> GroupVariablesByType(MethodDefinition method)
        {
            IDictionary<TypeReference, IList<Variable>> variables = new Dictionary<TypeReference, IList<Variable>>();
            int offset = method.IsStatic ? 0 : 1;
            foreach (var parameter in method.Parameters)
            {
                var type = parameter.ParameterType;
                if (!variables.ContainsKey(type))
                {
                    variables.Add(type, new List<Variable>());
                }
                variables[type].Add(new Variable(VariableType.Parameter, parameter.Index + offset, parameter.Name));
            }
            foreach (var variable in method.Body.Variables)
            {
                var type = variable.VariableType;
                if (!variables.ContainsKey(type))
                {
                    variables.Add(type, new List<Variable>());
                }
                variables[type].Add(new Variable(VariableType.Local, variable.Index, variable.Name));
            }
            foreach (var field in method.DeclaringType.Fields)
            {
                var type = field.FieldType;
                if (!variables.ContainsKey(type))
                {
                    variables.Add(type, new List<Variable>());
                }
                variables[type].Add(new Variable(VariableType.Field, -1, field.Name));
            }
            return variables;
        }

        private static void PopulateOperandsInVariables(MethodDefinition method, IDictionary<TypeReference, IList<Variable>> variables)
        {
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Ldarg)
                {
                    var parameterDefinition = (ParameterDefinition)instruction.Operand;
                    int sequence = parameterDefinition.Sequence;
                    if (!variables.ContainsKey(parameterDefinition.ParameterType)) continue;
                    var variable =
                        variables[parameterDefinition.ParameterType]
                            .SingleOrDefault(v => v.Type == VariableType.Parameter && v.Index == sequence);
                    if (variable != null)
                    {
                        variable.Operand = instruction.Operand;
                    }
                }
            }
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Ldloc)
                {
                    var variableDefinition = (VariableDefinition)instruction.Operand;
                    int index = variableDefinition.Index;
                    if (!variables.ContainsKey(variableDefinition.VariableType)) continue;
                    var variable =
                        variables[variableDefinition.VariableType]
                            .SingleOrDefault(v => v.Type == VariableType.Local && v.Index == index);
                    if (variable != null)
                    {
                        variable.Operand = instruction.Operand;
                    }
                }
            }
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Ldfld)
                {
                    var fieldDefinition = (FieldDefinition)instruction.Operand;
                    string name = fieldDefinition.Name;
                    if (!variables.ContainsKey(fieldDefinition.FieldType)) continue;
                    var variable =
                        variables[fieldDefinition.FieldType]
                            .SingleOrDefault(v => v.Type == VariableType.Field && v.Name == name);
                    if (variable != null)
                    {
                        variable.Operand = instruction.Operand;
                    }
                }
            }
        }
    }
}
