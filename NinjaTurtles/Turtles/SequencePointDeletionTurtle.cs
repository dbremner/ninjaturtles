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

using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NinjaTurtles.Turtles
{
    public class SequencePointDeletionTurtle : MethodTurtleBase
    {
        protected override IEnumerable<MutationTestMetaData> DoMutate(MethodDefinition method, Module module)
        {
            var sequence = new Dictionary<int, OpCode>();
            int startIndex = -1;
            for (int index = 0; index < method.Body.Instructions.Count; index++)
            {
                var instruction = method.Body.Instructions[index];
                if (instruction.SequencePoint != null)
                {
                    startIndex = index;
                    sequence.Clear();
                }
                if (startIndex >= 0)
                {
                    sequence.Add(index, instruction.OpCode);
                }
                if (index == method.Body.Instructions.Count - 1 || instruction.Next.SequencePoint != null)
                {
                    if (!ShouldDeleteSequence(method.Body, sequence)) continue;

                    OpCode originalOpCode = method.Body.Instructions[startIndex].OpCode;
                    object originalOperand = method.Body.Instructions[startIndex].Operand;
                    method.Body.Instructions[startIndex].OpCode = OpCodes.Br;
                    method.Body.Instructions[startIndex].Operand = instruction.Next;

                    var codes = string.Join(", ", sequence.Values.Select(o => o.Code));
                    var description = string.Format("{0:x4}: deleting {1}", GetOriginalOffset(startIndex), codes);
                    MutationTestMetaData mutation = DoYield(method, module, description, startIndex);
                    yield return mutation;

                    method.Body.Instructions[startIndex].OpCode = originalOpCode;
                    method.Body.Instructions[startIndex].Operand = originalOperand;
                }
            }
        }

        private bool ShouldDeleteSequence(MethodBody method, IDictionary<int, OpCode> opCodes)
        {
            if (opCodes.Values.All(o => o == OpCodes.Nop)) return false;
            if (opCodes.Values.All(o => o == OpCodes.Pop)) return false;
            if (opCodes.Values.All(o => o == OpCodes.Leave)) return false;
            if (opCodes.Values.Any(o => o == OpCodes.Ret)) return false;

            // If just setting compiler-generated return variable in Debug mode, don't delete.
            if (opCodes.Values.Last().Code == Code.Br)
            {
                if (((Instruction)method.Instructions[opCodes.Keys.Last()].Operand).Offset
                    == method.Instructions[opCodes.Keys.Last() + 1].Offset)
                {
                    if (method.Instructions[opCodes.Keys.Last() + 2].OpCode == OpCodes.Ret)
                    {
                        return false;
                    }
                }
            }

            // If calling base constructor, don't delete.
            if (opCodes.Any(kv => kv.Value == OpCodes.Call))
            {
                if (((MethodReference)method.Instructions[opCodes.First(kv => kv.Value == OpCodes.Call).Key].Operand).Name == Methods.CONSTRUCTOR)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
