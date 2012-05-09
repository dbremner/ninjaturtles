using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil.Cil;

using NUnit.Framework;

namespace NinjaTurtles.Tests
{
    [TestFixture]
    public class InstructionExtensionsTests
    {
        [Test]
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
    }
}
