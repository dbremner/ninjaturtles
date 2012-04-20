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

namespace NinjaTurtles.Turtles.Method
{
    public class ArithmeticOperatorTurtle : OpCodeRotationTurtle
    {
        private static readonly IList<KeyValuePair<OpCode, OpCode>> _opcodeMap = new List<KeyValuePair<OpCode, OpCode>>
                                                                             {
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Add, OpCodes.Rem),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Add, OpCodes.Sub),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Add, OpCodes.Mul),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Add, OpCodes.Div),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Sub, OpCodes.Add),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Sub, OpCodes.Rem),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Sub, OpCodes.Mul),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Sub, OpCodes.Div),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Mul, OpCodes.Add),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Mul, OpCodes.Sub),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Mul, OpCodes.Rem),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Mul, OpCodes.Div),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Div, OpCodes.Add),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Div, OpCodes.Sub),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Div, OpCodes.Mul),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Div, OpCodes.Rem),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Rem, OpCodes.Add),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Rem, OpCodes.Sub),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Rem, OpCodes.Mul),
                                                                                 new KeyValuePair<OpCode, OpCode>(OpCodes.Rem, OpCodes.Div)
                                                                             };

        public override IList<KeyValuePair<OpCode, OpCode>> OpCodeMap
        {
            get { return _opcodeMap; }
        }

        public override string Description
        {
            get { return "Rotating arithmetic operators"; }
        }
    }
}
