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

using Mono.Cecil.Cil;

using NUnit.Framework;

namespace NinjaTurtles.Tests
{
    [TestFixture]
    public class InstructionExtensionsTests
    {
        [Test]
        [MethodTested("NinjaTurtles.InstructionExtensions", "IsMeaninglessUnconditionalBranch")]
        public void IsMeaninglessUnconditionalBranch_Works()
        {
            var i1 = Instruction.Create(OpCodes.Nop);
            var i5 = Instruction.Create(OpCodes.Nop);
            var i3 = Instruction.Create(OpCodes.Br, i5);
            var i2 = Instruction.Create(OpCodes.Br, i3);
            var i4 = Instruction.Create(OpCodes.Nop);
            i1.Offset = 1;
            i2.Offset = 2;
            i3.Offset = 3;
            i4.Offset = 4;
            i5.Offset = 5;
            i1.Next = i2;
            i2.Next = i3;
            i3.Next = i4;
            i4.Next = i5;
            Assert.IsFalse(i1.IsMeaninglessUnconditionalBranch());
            Assert.IsTrue(i2.IsMeaninglessUnconditionalBranch());
            Assert.IsFalse(i3.IsMeaninglessUnconditionalBranch());
            Assert.IsFalse(i4.IsMeaninglessUnconditionalBranch());
            Assert.IsFalse(i5.IsMeaninglessUnconditionalBranch());
        }

        [Test, Category("Mutation")]
        public void IsMeaninglessUnconditionalBranch_Mutation_Tests()
        {
            MutationTestBuilder.For("NinjaTurtles.InstructionExtensions", "IsMeaninglessUnconditionalBranch")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }
    }
}
