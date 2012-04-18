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
using System.IO;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using NinjaTurtles.Utilities;

namespace NinjaTurtles.Turtles.Method
{
    public class ParameterPermutationTurtle : MethodTurtle
    {
        public override string Description
        {
            get { return "Permuting method parameters"; }
        }

        public override IEnumerable<string> Mutate(MethodDefinition method, AssemblyDefinition assembly, string fileName)
        {
            if (method.HasBody)
            {
                string originalFileName = fileName.Replace(".dll", ".ninjaoriginal.dll");
                if (File.Exists(originalFileName)) File.Delete(originalFileName);

                int offset = method.IsStatic ? 0 : 1;
                IDictionary<TypeReference, IList<int>> parameters = new Dictionary<TypeReference, IList<int>>();
                foreach (var parameter in method.Parameters)
                {
                    var type = parameter.ParameterType;
                    if (!parameters.ContainsKey(type))
                    {
                        parameters.Add(type, new List<int>());
                    }
                    parameters[type].Add(parameter.Index);
                }
                if (!parameters.Any(kv => parameters[kv.Key].Count > 1))
                {
                    yield break;
                }
                foreach (var type in parameters.Keys)
                {
                    if (parameters[type].Count < 2) continue;
                    var indices = parameters[type].ToArray();
                    foreach (var order in new AllPermutationsEnumerable<int>(indices).ToArray())
                    {
                        int[] orderArray = order.ToArray();
                        int j = 0;
                        bool isOriginalOrder = orderArray.All(index => index == indices[j++]);
                        if (isOriginalOrder) continue;

                        PermuteIndices(method, indices, orderArray, offset);
                        var output = string.Format("Parameter permutation change for type {0} ({1}) => ({2}) in {3}.{4}",
                            type.Name,
                            string.Join(",", indices),
                            string.Join(",", orderArray),
                            method.DeclaringType.Name,
                            method.Name);

                        if (orderArray[0] == 3) assembly.Write(fileName + ".kept");
                        foreach (var p in PlaceFileAndYield(assembly, fileName, output, originalFileName))
                        {
                            yield return p;
                        }

                        PermuteIndices(method, orderArray, indices, offset);
                    }
                }
            }
        }

        private static void PermuteIndices(MethodDefinition method, int[] fromIndices, int[] toIndices, int offset)
        {
            IDictionary<int, ParameterDefinition> ldargOperands = new Dictionary<int, ParameterDefinition>();
            foreach (var instruction in method.Body.Instructions)
            {

                if (instruction.OpCode == OpCodes.Ldarg_S)
                {
                    var parameterDefinition = (ParameterDefinition)instruction.Operand;
                    int sequence = parameterDefinition.Sequence;
                    if (!ldargOperands.ContainsKey(sequence))
                    {
                        ldargOperands.Add(sequence, parameterDefinition);
                    }
                }
            }
            foreach (var instruction in method.Body.Instructions)
            {
                int parameterIndex = -1;
                if (instruction.OpCode == OpCodes.Ldarg_0 && offset == 0)
                {
                    parameterIndex = 0;
                }
                else if (instruction.OpCode == OpCodes.Ldarg_1)
                {
                    parameterIndex = 1 - offset;
                }
                else if (instruction.OpCode == OpCodes.Ldarg_2)
                {
                    parameterIndex = 2 - offset;
                }
                else if (instruction.OpCode == OpCodes.Ldarg_3)
                {
                    parameterIndex = 3 - offset;
                }
                else if (instruction.OpCode == OpCodes.Ldarg_S)
                {
                    int ldargIndex = ((ParameterDefinition)instruction.Operand).Sequence;
                    parameterIndex = ldargIndex - offset;
                }
                if (parameterIndex >= 0)
                {
                    int parameterOrder = Array.IndexOf(fromIndices, parameterIndex);
                    if (parameterOrder == -1) continue;
                    int newIndex = toIndices[parameterOrder];
                    switch (newIndex)
                    {
                        case 0:
                            instruction.OpCode = offset == 0 ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1;
                            instruction.Operand = null;
                            break;
                        case 1:
                            instruction.OpCode = offset == 0 ? OpCodes.Ldarg_1 : OpCodes.Ldarg_2;
                            instruction.Operand = null;
                            break;
                        case 2:
                            instruction.OpCode = offset == 0 ? OpCodes.Ldarg_2 : OpCodes.Ldarg_3;
                            instruction.Operand = null;
                            break;
                        case 3:
                            if (offset == 0)
                            {
                                instruction.OpCode = OpCodes.Ldarg_3;
                                instruction.Operand = null;
                            }
                            else
                            {
                                instruction.OpCode = OpCodes.Ldarg_S;
                                instruction.Operand = ldargOperands[newIndex + offset];
                            }
                            break;
                        default:
                            instruction.OpCode = OpCodes.Ldarg_S;
                            instruction.Operand = ldargOperands[newIndex + offset];
                            break;
                    }
                }
            }
        }
    }
}
