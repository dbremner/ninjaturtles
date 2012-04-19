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
        public override string Description
        {
            get { return "Permuting method parameters and variables"; }
        }

        protected override IEnumerable<string> DoMutate(MethodDefinition method, AssemblyDefinition assembly, string fileName)
        {
            int offset = method.IsStatic ? 0 : 1;
            var parametersAndVariablesByType = GroupMethodParametersAndVariablesByType(method);

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

                    PermuteIndices(method, indices, orderArray, offset);
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

                    PermuteIndices(method, orderArray, indices, offset);
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
            foreach (var parameter in method.Parameters)
            {
                var type = parameter.ParameterType;
                if (!parametersAndVariables.ContainsKey(type))
                {
                    parametersAndVariables.Add(type, new List<int>());
                }
                parametersAndVariables[type].Add(-1 - parameter.Index);
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

        private static void PermuteIndices(MethodDefinition method, int[] fromIndices, int[] toIndices, int offset)
        {
            var ldargOperands = GetOperandsForHigherIndexParametersAndVariables(method);
            foreach (var instruction in method.Body.Instructions)
            {
                int? parameterIndex = null;
                if (instruction.OpCode == OpCodes.Ldarg_0 && offset == 0)
                {
                    parameterIndex = -1;
                }
                else if (instruction.OpCode == OpCodes.Ldarg_1)
                {
                    parameterIndex = -2 + offset;
                }
                else if (instruction.OpCode == OpCodes.Ldarg_2)
                {
                    parameterIndex = -3 + offset;
                }
                else if (instruction.OpCode == OpCodes.Ldarg_3)
                {
                    parameterIndex = -4 + offset;
                }
                else if (instruction.OpCode == OpCodes.Ldarg_S)
                {
                    int ldargIndex = ((ParameterDefinition)instruction.Operand).Sequence;
                    parameterIndex = -1 - ldargIndex + offset;
                }
                else if (instruction.OpCode == OpCodes.Ldloc_0)
                {
                    parameterIndex = 0;
                }
                else if (instruction.OpCode == OpCodes.Ldloc_1)
                {
                    parameterIndex = 1;
                }
                else if (instruction.OpCode == OpCodes.Ldloc_2)
                {
                    parameterIndex = 2;
                }
                else if (instruction.OpCode == OpCodes.Ldloc_3)
                {
                    parameterIndex = 3;
                }
                else if (instruction.OpCode == OpCodes.Ldloc_S)
                {
                    int ldlocIndex = ((ParameterDefinition)instruction.Operand).Sequence;
                    parameterIndex = ldlocIndex;
                }
                if (parameterIndex.HasValue)
                {
                    int parameterOrder = Array.IndexOf(fromIndices, parameterIndex);
                    if (parameterOrder == -1) continue;
                    int newIndex = toIndices[parameterOrder];
                    switch (newIndex)
                    {
                        case 0:
                            instruction.OpCode = OpCodes.Ldloc_0;
                            instruction.Operand = null;
                            break;
                        case 1:
                            instruction.OpCode = OpCodes.Ldloc_1;
                            instruction.Operand = null;
                            break;
                        case 2:
                            instruction.OpCode = OpCodes.Ldloc_2;
                            instruction.Operand = null;
                            break;
                        case 3:
                            instruction.OpCode = OpCodes.Ldloc_3;
                            instruction.Operand = null;
                            break;
                        case -1:
                            instruction.OpCode = offset == 0 ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1;
                            instruction.Operand = null;
                            break;
                        case -2:
                            instruction.OpCode = offset == 0 ? OpCodes.Ldarg_1 : OpCodes.Ldarg_2;
                            instruction.Operand = null;
                            break;
                        case -3:
                            instruction.OpCode = offset == 0 ? OpCodes.Ldarg_2 : OpCodes.Ldarg_3;
                            instruction.Operand = null;
                            break;
                        case -4:
                            if (offset == 0)
                            {
                                instruction.OpCode = OpCodes.Ldarg_3;
                                instruction.Operand = null;
                            }
                            else
                            {
                                instruction.OpCode = OpCodes.Ldarg_S;
                                instruction.Operand = ldargOperands[newIndex - 1];
                            }
                            break;
                        default:
                            instruction.OpCode = newIndex >= 0 ? OpCodes.Ldloc_S : OpCodes.Ldarg_S;
                            instruction.Operand = ldargOperands[newIndex >= 0 ? newIndex : newIndex - offset];
                            break;
                    }
                }
            }
        }

        private static IDictionary<int, ParameterDefinition> GetOperandsForHigherIndexParametersAndVariables(MethodDefinition method)
        {
            IDictionary<int, ParameterDefinition> operands = new Dictionary<int, ParameterDefinition>();
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Ldarg_S)
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
                if (instruction.OpCode == OpCodes.Ldloc_S)
                {
                    var parameterDefinition = (ParameterDefinition)instruction.Operand;
                    int sequence = parameterDefinition.Sequence;
                    if (!operands.ContainsKey(sequence))
                    {
                        operands.Add(sequence, parameterDefinition);
                    }
                }
            }
            return operands;
        }
    }
}
