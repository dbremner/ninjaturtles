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

using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NinjaTurtles.Turtles.Method
{
    public class OpCodeDeletionTurtle : MethodTurtle
    {
        public override string Description
        {
            get { return "Deleting OpCodes in turn"; }
        }

        protected override IEnumerable<string> DoMutate(MethodDefinition method, AssemblyDefinition assembly, string fileName)
        {
            foreach (var instruction in method.Body.Instructions)
            {
                if (!ShouldDeleteOpCode(instruction)) continue;

                var originalCode = instruction.OpCode;
                var originalOperand = instruction.Operand;

                instruction.OpCode = OpCodes.Nop;
                instruction.Operand = null;
                var output = string.Format("OpCode deletion {0} at {1:x4} in {2}.{3}", originalCode.Name,
                                           instruction.Offset, method.DeclaringType.Name, method.Name);

                foreach (var p in PlaceFileAndYield(assembly, fileName, output))
                {
                    yield return p;
                }

                instruction.OpCode = originalCode;
                instruction.Operand = originalOperand;
            }
        }


        private bool ShouldDeleteOpCode(Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Nop) return false;
            if (instruction.OpCode == OpCodes.Ret) return false;
            if (instruction.OpCode == OpCodes.Br
                && instruction.Next.Offset == instruction.Offset + instruction.GetSize())
            {
                return false;
            }
            return true;
        }
    }
}
      
