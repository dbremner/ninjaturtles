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
        private static readonly IDictionary<OpCode, IEnumerable<OpCode>> _opcodeMap 
            = new Dictionary<OpCode, IEnumerable<OpCode>>
                {
                    {OpCodes.Brfalse, new[] {OpCodes.Brtrue}},
                    {OpCodes.Brtrue, new[] {OpCodes.Brfalse}},
                    {OpCodes.Beq, new[] {OpCodes.Bne_Un}},
                    {OpCodes.Bne_Un, new[] {OpCodes.Beq}},
                    {OpCodes.Bge, new[] {OpCodes.Blt}},
                    {OpCodes.Blt, new[] {OpCodes.Bge}},
                    {OpCodes.Bge_Un, new[] {OpCodes.Blt_Un}},
                    {OpCodes.Blt_Un, new[] {OpCodes.Bge_Un}},
                    {OpCodes.Ble, new[] {OpCodes.Bgt}},
                    {OpCodes.Bgt, new[] {OpCodes.Ble}},
                    {OpCodes.Ble_Un, new[] {OpCodes.Bgt_Un}},
                    {OpCodes.Bgt_Un, new[] {OpCodes.Ble_Un}}
                
                };

        /// <summary>
        /// Defines a mapping from input opcodes to a set of replacement output
        /// opcodes for mutation purposes.
        /// </summary>
        public override IDictionary<OpCode, IEnumerable<OpCode>> OpCodeMap
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
