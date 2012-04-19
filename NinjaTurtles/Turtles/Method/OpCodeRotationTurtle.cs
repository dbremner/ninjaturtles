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
using System.IO;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NinjaTurtles.Turtles.Method
{
    public abstract class OpCodeRotationTurtle : MethodTurtle
    {
        public abstract IEnumerable<OpCode> FromOpCodes { get; }
        public abstract IEnumerable<OpCode> ToOpCodes { get; }

        protected override IEnumerable<string> DoMutate(MethodDefinition method, AssemblyDefinition assembly, string fileName)
        {
            foreach (var instruction in method.Body.Instructions)
            {
                if (FromOpCodes.Any(o => o.Equals(instruction.OpCode)))
                {
                    var originalCode = instruction.OpCode;
                    foreach (var opCode in ToOpCodes)
                    {
                        if (originalCode != opCode)
                        {
                            instruction.OpCode = opCode;
                            var output = string.Format("OpCode change {0} => {1} at {2:x4} in {3}.{4}",
                                                       originalCode.Name, opCode.Name, instruction.Offset,
                                                       method.DeclaringType.Name, method.Name);

                            foreach (var p in PlaceFileAndYield(assembly, fileName, output))
                            {
                                yield return p;
                            }

                            instruction.OpCode = originalCode;
                        }
                    }
                }
            }
        }
    }
}