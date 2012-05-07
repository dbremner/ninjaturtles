using System;

using NUnit.Framework;

using NinjaTurtles.Tests.Turtles.ArithmeticOperatorTurtleTestSuite;
using NinjaTurtles.Turtles;

namespace NinjaTurtles.Tests
{
	[TestFixture]
	public class MutationTestTests
	{
		[Test]
		[MethodTested("NinjaTurtles.MutationTest", "Run")]
		[MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
		public void UncoveredAdd_Mutation_Tests_Fail()
		{
			try
			{
				MutationTestBuilder<AdditionClassUnderTest>
					.For("UncoveredAdd")
					.With<ArithmeticOperatorTurtle>()
					.Run();
			}
			catch (MutationTestFailureException ex)
			{
				Assert.AreEqual("No matching tests were found to run.", ex.Message);
				return;
			}
			Assert.Fail("MutationTestFailureException was not thrown.");
		}
		
		[Test]
		[MethodTested("NinjaTurtles.MutationTest", "Run")]
		[MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
		public void Unknown_Method_Mutation_Tests_Fail()
		{
			try
			{
				MutationTestBuilder<AdditionClassUnderTest>
					.For("UnknownMethod")
					.With<ArithmeticOperatorTurtle>()
					.Run();
			}
			catch (MutationTestFailureException ex)
			{
				Assert.AreEqual("Method 'UnknownMethod' was not recognised.", ex.Message);
				return;
			}
			Assert.Fail("MutationTestFailureException was not thrown.");
		}
		
		[Test, Category("Mutation")]
		public void Run_Mutation_Tests()
		{
			MutationTestBuilder.For("NinjaTurtles.MutationTest", "Run")
				.Run();
		}

		[Test, Category("Mutation")]
		public void RunMutation_Mutation_Tests()
		{
			MutationTestBuilder.For("NinjaTurtles.MutationTest", "RunMutation")
				.Run();
		}
}
}

