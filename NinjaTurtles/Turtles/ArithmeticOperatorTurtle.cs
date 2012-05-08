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
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NinjaTurtles.Turtles
{
	public class ArithmeticOperatorTurtle : MethodTurtleBase
	{
		private static readonly List<OpCode> _opCodes = new List<OpCode> {
			OpCodes.Add, OpCodes.Sub, OpCodes.Mul, OpCodes.Div, OpCodes.Rem };
		
		public override IEnumerable<MutationTestMetaData> DoMutate(MethodDefinition method, AssemblyDefinition assembly, string testAssemblyLocation)
		{
			foreach (var instruction in method.Body.Instructions)
			{
				if (_opCodes.Contains(instruction.OpCode))
				{
					var originalOpCode = instruction.OpCode;
					
					foreach (var opCode in _opCodes)
					{
						if (opCode == originalOpCode) continue;
						instruction.OpCode = opCode;
					    var description = string.Format("{0} => {1}", originalOpCode.Code, opCode.Code);
					    yield return DoYield(method, assembly, testAssemblyLocation, description);
					}
					
					instruction.OpCode = originalOpCode;
				}
			}
		}
	}
}
