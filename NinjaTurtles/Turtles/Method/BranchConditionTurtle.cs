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

using Mono.Cecil.Cil;

namespace NinjaTurtles.Turtles.Method
{
    /// <summary>
    /// A concrete implementation of <see cref="IMethodTurtle" /> that replaces
    /// conditional branches in IL with their converses, as well as with
    /// hard-wired <b>true</b> and <b>false</b> values (i.e. always branch, and
    /// never branch).
    /// </summary>
    public class BranchConditionTurtle : OpCodeRotationTurtle
    {
        private static readonly IList<KeyValuePair<OpCode, OpCode>> _opcodeMap = new List<KeyValuePair<OpCode, OpCode>>
                                                                             {
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Brfalse, OpCodes.Brtrue),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Brtrue, OpCodes.Brfalse),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Beq, OpCodes.Bne_Un),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Bne_Un, OpCodes.Beq),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Bge, OpCodes.Blt),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Blt, OpCodes.Bge),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Bge_Un, OpCodes.Blt_Un),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Blt_Un, OpCodes.Bge_Un),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Ble, OpCodes.Bgt),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Bgt, OpCodes.Ble),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Ble_Un, OpCodes.Bgt_Un),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Bgt_Un, OpCodes.Ble_Un)
                                                                             };

        /// <summary>
        /// Defines a mapping from input opcodes to a set of replacement output
        /// opcodes for mutation purposes.
        /// </summary>
        public override IList<KeyValuePair<OpCode, OpCode>> OpCodeMap
        {
            get { return _opcodeMap; }
        }

        /// <summary>
        /// A description for the particular implementation class.
        /// </summary>
        public override string Description
        {
            get { return "Rotating branching conditions"; }
        }
    }
}
