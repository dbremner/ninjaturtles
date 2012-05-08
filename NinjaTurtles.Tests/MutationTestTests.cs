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
        [MethodTested("NinjaTurtles.MutationTest", "CheckTestProcessFails")]
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
        [MethodTested("NinjaTurtles.MutationTest", "CheckTestProcessFails")]
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
		
		[Test]
		[MethodTested("NinjaTurtles.MutationTest", "Run")]
		[MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
        [MethodTested("NinjaTurtles.MutationTest", "CheckTestProcessFails")]
        public void Mutation_Tests_Produce_Correct_Output()
		{
			using (var capturer = new ConsoleCapturer())
			{
				MutationTestBuilder<AdditionClassUnderTest>
					.For("WorkingAdd")
					.With<ArithmeticOperatorTurtle>()
					.Run();
				string output = capturer.Output;
				StringAssert.Contains("Mutant: ", output);
				StringAssert.Contains("Killed.", output);
				StringAssert.Contains("Add => Sub", output);
				StringAssert.Contains("Add => Mul", output);
				StringAssert.Contains("Add => Div", output);
				StringAssert.Contains("Add => Rem", output);
			}
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

        [Test, Category("Mutation")]
        public void CheckTestProcessFails_Mutation_Tests()
        {
            MutationTestBuilder.For("NinjaTurtles.MutationTest", "CheckTestProcessFails")
                .Run();
        }
    }
}

