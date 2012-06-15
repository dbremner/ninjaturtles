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

namespace NinjaTurtles.Tests.Turtles.ArithmeticOperatorTurtleTestSuite.Tests
{
	[TestFixture]
	public class AdditionClassUnderTestTests
	{
		[Test]
		[MethodTested(typeof(AdditionClassUnderTest), "Dummy")]
		public void Dummy_Dummies()
		{
			Assert.AreEqual(0, new AdditionClassUnderTest().Dummy());
		}
		
		[Test]
		[MethodTested(typeof(AdditionClassUnderTest), "Add")]
		public void Add_Works()
		{
			Assert.AreEqual(3, new AdditionClassUnderTest().Add(3, 0));
		}
		
		[Test]
		[MethodTested(typeof(AdditionClassUnderTest), "WorkingAdd")]
		public void WorkingAdd_Works()
		{
			Assert.AreEqual(3, new AdditionClassUnderTest().WorkingAdd(3, 0));
			Assert.AreEqual(7, new AdditionClassUnderTest().WorkingAdd(3, 4));
		}
		
		[Test]
		[MethodTested("NinjaTurtles.MutationTest", "Run")]
		[MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
		public void Dummy_Mutation_Tests_Pass()
		{
			using (var console = new ConsoleCapturer())
			{
				MutationTestBuilder<AdditionClassUnderTest>
					.For("Dummy")
					.With<ArithmeticOperatorTurtle>()
					.Run();
				StringAssert.Contains("No valid mutations found (this is fine).", console.Output);
			}
		}
		
		[Test]
		[MethodTested("NinjaTurtles.MutationTest", "Run")]
        [MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
        [MethodTested(typeof(MethodTurtleBase), "Mutate")]
        [MethodTested(typeof(MethodTurtleBase), "DoYield")]
        [MethodTested(typeof(ArithmeticOperatorTurtle), "DoMutate")]
        public void Add_Mutation_Tests_Fail()
		{
			try
			{
				MutationTestBuilder<AdditionClassUnderTest>
					.For("Add")
					.With<ArithmeticOperatorTurtle>()
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
        [MethodTested(typeof(ArithmeticOperatorTurtle), "DoMutate")]
        public void WorkingAdd_Mutation_Tests_Pass()
		{
			MutationTestBuilder<AdditionClassUnderTest>
				.For("WorkingAdd")
				.With<ArithmeticOperatorTurtle>()
				.Run();
		}
	}
}

