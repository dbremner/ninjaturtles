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

using Mono.Cecil.Cil;

namespace NinjaTurtles.Mutators.Method
{
    public class BranchConditionMutator : OpCodeRotationMutator
    {
        private static readonly IEnumerable<OpCode> _fromOpCodes = new[]
                                                                        {
                                                                            OpCodes.Brfalse, OpCodes.Brtrue
                                                                        };

        private static readonly IEnumerable<OpCode> _toOpCodes = new[]
                                                                        {
                                                                            OpCodes.Brfalse, OpCodes.Brtrue,
                                                                            OpCodes.Br, OpCodes.Nop
                                                                        };

        public override IEnumerable<OpCode> FromOpCodes
        {
            get { return _fromOpCodes; }
        }

        public override IEnumerable<OpCode> ToOpCodes
        {
            get { return _toOpCodes; }
        }

        public override string Description
        {
            get { return "Rotating branching conditions"; }
        }
    }
}
