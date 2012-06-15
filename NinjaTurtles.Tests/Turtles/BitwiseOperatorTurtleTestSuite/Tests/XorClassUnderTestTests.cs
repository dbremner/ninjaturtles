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

using NUnit.Framework;

using NinjaTurtles.Tests.TestUtilities;
using NinjaTurtles.Turtles;

namespace NinjaTurtles.Tests.Turtles.BitwiseOperatorTurtleTestSuite.Tests
{
	[TestFixture]
	public class XorClassUnderTestTests
	{
		[Test]
		[MethodTested(typeof(XorClassUnderTest), "Dummy")]
		public void Dummy_Dummies()
		{
			Assert.AreEqual(0, new XorClassUnderTest().Dummy());
		}
		
		[Test]
        [MethodTested(typeof(XorClassUnderTest), "Xor")]
        public void Xor_Works()
		{
			Assert.AreEqual(11, new XorClassUnderTest().Xor(3, 8));
		}
		
		[Test]
        [MethodTested(typeof(XorClassUnderTest), "WorkingXor")]
		public void WorkingXor_Works()
		{
			Assert.AreEqual(11, new XorClassUnderTest().WorkingXor(3, 8));
            Assert.AreEqual(4, new XorClassUnderTest().WorkingXor(3, 7));
		}
		
		[Test]
		[MethodTested("NinjaTurtles.MutationTest", "Run")]
		[MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
		public void Dummy_Mutation_Tests_Pass()
		{
			using (var console = new ConsoleCapturer())
			{
				MutationTestBuilder<XorClassUnderTest>
					.For("Dummy")
					.With<BitwiseOperatorTurtle>()
					.Run();
				StringAssert.Contains("No valid mutations found (this is fine).", console.Output);
			}
		}
		
		[Test]
		[MethodTested("NinjaTurtles.MutationTest", "Run")]
        [MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
        [MethodTested(typeof(MethodTurtleBase), "Mutate")]
        [MethodTested(typeof(MethodTurtleBase), "DoYield")]
        [MethodTested(typeof(OpCodeRotationTurtle), "DoMutate")]
        public void Xor_Mutation_Tests_Fail()
		{
			try
			{
				MutationTestBuilder<XorClassUnderTest>
                    .For("Xor")
                    .With<BitwiseOperatorTurtle>()
					.Run();
			}
			catch (MutationTestFailureException)
			{
				return;
			}
			Assert.Fail("MutationTestFailureException was not thrown.");
		}

		[Test]
		[MethodTested("NinjaTurtles.MutationTest", "Run")]
		[MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
        [MethodTested(typeof(MethodTurtleBase), "Mutate")]
        [MethodTested(typeof(MethodTurtleBase), "DoYield")]
        [MethodTested(typeof(OpCodeRotationTurtle), "DoMutate")]
        public void WorkingXor_Mutation_Tests_Pass()
		{
			MutationTestBuilder<XorClassUnderTest>
                .For("WorkingXor")
                .With<BitwiseOperatorTurtle>()
				.Run();
		}
	}
}

