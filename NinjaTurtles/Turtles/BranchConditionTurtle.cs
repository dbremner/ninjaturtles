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

using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NinjaTurtles.Turtles
{
    public class BranchConditionTurtle : MethodTurtleBase
    {
        private static readonly IDictionary<OpCode, IEnumerable<OpCode>> _opCodes = new Dictionary<OpCode, IEnumerable<OpCode>> {
            { OpCodes.Br, new[] { OpCodes.Nop } },
            { OpCodes.Brtrue, new[] { OpCodes.Nop, OpCodes.Brfalse, OpCodes.Br } },
            { OpCodes.Brfalse, new[] { OpCodes.Nop, OpCodes.Brtrue, OpCodes.Br } }
        };
        
        protected override IEnumerable<MutationTestMetaData> DoMutate(MethodDefinition method, Module module)
        {
            for (int index = 0; index < method.Body.Instructions.Count; index++)
            {
                var instruction = method.Body.Instructions[index];
                if (_opCodes.ContainsKey(instruction.OpCode))
                {
                    if (instruction.IsMeaninglessUnconditionalBranch()) continue;

                    var originalOpCode = instruction.OpCode;

                    foreach (var opCode in _opCodes[originalOpCode])
                    {
                        instruction.OpCode = opCode;
                        var description = string.Format("{0:x4}: {1} => {2}", GetOriginalOffset(index), originalOpCode.Code, opCode.Code);
                        MutationTestMetaData mutation = DoYield(method, module, description, index);
                        yield return mutation;
                    }

                    instruction.OpCode = originalOpCode;
                }
            }
        }
    }
}
