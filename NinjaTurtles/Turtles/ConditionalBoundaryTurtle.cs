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
    /// <summary>
    /// An implementation of <see cref="IMethodTurtle"/> that changes
    /// whether or not equality is included in comparison operators, so
    /// for example <see cref="OpCodes.Clt" /> is replaced by a combination
    /// of <see cref="OpCodes.Cgt" /> and a comparison with zero to give the
    /// effect of a &lt;= operator.
    /// </summary>
    public class ConditionalBoundaryTurtle : MethodTurtleBase
    {
        /// <summary>
        /// Performs the actual code mutations, returning each with
        /// <c>yield</c> for the calling code to use.
        /// </summary>
        /// <remarks>
        /// Implementing classes should yield the result obtained by calling
        /// the <see mref="DoYield" /> method.
        /// </remarks>
        /// <param name="method">
        /// A <see cref="MethodDefinition" /> for the method on which mutation
        /// testing is to be carried out.
        /// </param>
        /// <param name="module">
        /// A <see cref="Module" /> representing the main module of the
        /// containing assembly.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}" /> of
        /// <see cref="MutantMetaData" /> structures.
        /// </returns>
        protected override IEnumerable<MutantMetaData> DoMutate(MethodDefinition method, Module module)
        {
            for (int index = 0; index < method.Body.Instructions.Count; index++)
            {
                var instruction = method.Body.Instructions[index];
                if (instruction.OpCode == OpCodes.Clt
                    || instruction.OpCode == OpCodes.Cgt)
                {
                    var originalOpCode = instruction.OpCode;

                    var loadZero = method.Body.GetILProcessor().Create(OpCodes.Ldc_I4_0);
                    var compareEqual = method.Body.GetILProcessor().Create(OpCodes.Ceq);
                    loadZero.Next = compareEqual;
                    loadZero.Offset = instruction.Offset + 1;
                    compareEqual.Next = instruction.Next;
                    compareEqual.Offset = instruction.Offset + 2;

                    method.Body.Instructions.Insert(index + 1, compareEqual);
                    method.Body.Instructions.Insert(index + 1, loadZero);
                    for (int remaining = index + 1; remaining < method.Body.Instructions.Count; remaining++)
                    {
                        method.Body.Instructions[remaining].Offset += 2;
                    }

                    instruction.OpCode = originalOpCode == OpCodes.Clt ? OpCodes.Cgt : OpCodes.Clt;
                    instruction.Next = loadZero;

                    var description = string.Format("{0:x4}: {1} => not {2}", GetOriginalOffset(index), originalOpCode.Code, instruction.OpCode.Code);
                    MutantMetaData mutation = DoYield(method, module, description, index);
                    yield return mutation;

                    instruction.OpCode = originalOpCode;
                    instruction.Next = compareEqual.Next;

                    method.Body.Instructions.RemoveAt(index + 1);
                    method.Body.Instructions.RemoveAt(index + 1);
                }
            }
        }
    }
}