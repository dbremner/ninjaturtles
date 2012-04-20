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
    /// any arithmetic operator in the method body with an alternative.
    /// </summary>
    public class ArithmeticOperatorTurtle : OpCodeRotationTurtle
    {
        private static readonly IDictionary<OpCode, IEnumerable<OpCode>> _opcodeMap 
            = new Dictionary<OpCode, IEnumerable<OpCode>>
                {
                    {OpCodes.Add, new[] {OpCodes.Rem, OpCodes.Sub, OpCodes.Mul, OpCodes.Div}},
                    {OpCodes.Sub, new[] {OpCodes.Rem, OpCodes.Add, OpCodes.Mul, OpCodes.Div}},
                    {OpCodes.Mul, new[] {OpCodes.Rem, OpCodes.Sub, OpCodes.Add, OpCodes.Div}},
                    {OpCodes.Div, new[] {OpCodes.Rem, OpCodes.Sub, OpCodes.Mul, OpCodes.Add}},
                    {OpCodes.Rem, new[] {OpCodes.Add, OpCodes.Sub, OpCodes.Mul, OpCodes.Div}}
                };

        public override IDictionary<OpCode, IEnumerable<OpCode>> OpCodeMap
        {
            get { return _opcodeMap; }
        }

        /// <summary>
        /// A description for the particular implementation class.
        /// </summary>
        public override string Description
        {
            get { return "Rotating arithmetic operators"; }
        }
    }
}
