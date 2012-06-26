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

namespace NinjaTurtles
{
    internal static class InstructionExtensions
    {
        internal static  bool IsMeaninglessUnconditionalBranch(this Instruction instruction)
        {
            return instruction.OpCode == OpCodes.Br
                && ((Instruction)instruction.Operand).Offset == instruction.Next.Offset;
        }

        internal static bool FollowsSequence(this Instruction instruction, params OpCode[] sequence)
        {
            if (instruction.OpCode != sequence[0]) return false;
            if (sequence.Length == 1) return true;
            var newSequence = new OpCode[sequence.Length - 1];
            Array.Copy(sequence, 1, newSequence, 0, newSequence.Length);
            return instruction.Next.FollowsSequence(newSequence);
        }

        internal static bool IsPartOfSequence(this Instruction instruction, params OpCode[] sequence)
        {
            if (!sequence.Distinct().Contains(instruction.OpCode)) return false;
            var startInstruction = instruction;
            for (int i = 0; i < sequence.Length; i++)
            {
                if (startInstruction == null) break;
                if (startInstruction.FollowsSequence(sequence)) return true;
                startInstruction = startInstruction.Previous;
            }
            return false;
        }

        internal static bool IsPartOfCompilerGeneratedDispose(this Instruction instruction)
        {
            if (instruction.IsPartOfSequence(OpCodes.Leave,
                OpCodes.Ldloc, OpCodes.Ldnull, OpCodes.Ceq,
                OpCodes.Stloc, OpCodes.Ldloc, OpCodes.Brtrue,
                OpCodes.Ldloc, OpCodes.Callvirt))
            {
                while (instruction.OpCode != OpCodes.Callvirt)
                {
                    instruction = instruction.Next;
                }
                var method = ((MethodReference)instruction.Operand);
                return method.Name == "Dispose";
            }
            if (instruction.IsPartOfSequence(OpCodes.Leave,
                OpCodes.Ldloc, OpCodes.Ldnull, OpCodes.Ceq,
                OpCodes.Brtrue,
                OpCodes.Ldloc, OpCodes.Callvirt))
            {
                while (instruction.OpCode != OpCodes.Callvirt)
                {
                    instruction = instruction.Next;
                }
                var method = ((MethodReference)instruction.Operand);
                return method.Name == "Dispose";
            }
            return false;
        }

        internal static bool ShouldReportSequencePoint(this Instruction instruction)
        {
            var instructions = new List<Instruction>();
            do
            {
                if (instruction.OpCode != OpCodes.Nop)
                {
                    instructions.Add(instruction);
                }
                instruction = instruction.Next;
            } while (instruction != null && instruction.SequencePoint == null);
            if (instructions.All(i => i.OpCode == OpCodes.Ret))
            {
                return false;
            }
            if (instructions.Count == 2)
            {
                Instruction first = instructions[0];
                Instruction second = instructions[1];
                if (((first.OpCode == OpCodes.Ldarg
                    && ((ParameterDefinition)first.Operand).Sequence == 0 )
                    || first.OpCode == OpCodes.Ldarg_0)
                    && second.OpCode == OpCodes.Call
                    && ((MethodReference)second.Operand).Name == Methods.CONSTRUCTOR)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
