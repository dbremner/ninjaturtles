using System;
using System.IO;
using System.Text;

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

