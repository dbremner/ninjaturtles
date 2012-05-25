using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

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

        [Test]
        [MethodTested("NinjaTurtles.MutationTest", "Run")]
        [MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
        public void Mutate_Failed_Tests_Report_Source_Code()
        {
            using (var capturer = new ConsoleCapturer())
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
                    StringAssert.Contains("return left + right;", capturer.Output);
                    return;
                }
            }
            Assert.Fail("MutationTestFailureException was not thrown.");
        }

        [Test]
        public void Mutate_Merges_Results_Into_Single_File()
        {
            string file = Path.GetTempFileName();
            if (File.Exists(file)) File.Delete(file);
            try
            {
                MutationTestBuilder<AdditionClassUnderTest>
                    .For("Add")
                    .With<ArithmeticOperatorTurtle>()
                    .WriteReportTo(file)
                    .Run();
            }
            catch (MutationTestFailureException)
            {
            }
            MutationTestBuilder<AdditionClassUnderTest>
                .For("WorkingAdd")
                .With<ArithmeticOperatorTurtle>()
                .MergeReportTo(file)
                .Run();
            Assert.IsTrue(File.Exists(file));
            var xDocument = XDocument.Load(file);
            Assert.AreEqual(1, xDocument.Root.Descendants().Where(e => e.Name == "SourceFile").Count());
            Assert.GreaterOrEqual(xDocument.Root.Descendants().Where(e => e.Name == "SequencePoint").Count(), 2);
            Assert.AreEqual(8, xDocument.Root.Descendants().Where(e => e.Name == "AppliedMutant").Count());
            Assert.AreEqual(1, xDocument.Root.Descendants().Where(e => e.Name == "AppliedMutant" && e.Attributes().Any(a => a.Name == "Killed" && a.Value == "false")).Count());
            Assert.AreEqual(7, xDocument.Root.Descendants().Where(e => e.Name == "AppliedMutant" && e.Attributes().Any(a => a.Name == "Killed" && a.Value == "true")).Count());
            File.Delete(file);
        }

		[Test, Category("Mutation")]
		public void Run_Mutation_Tests()
		{
			MutationTestBuilder.For("NinjaTurtles.MutationTest", "Run")
                .MergeReportTo("SampleReport.xml")
                .Run();
		}

        [Test, Category("Mutation")]
        public void RunMutation_Mutation_Tests()
        {
            MutationTestBuilder.For("NinjaTurtles.MutationTest", "RunMutation")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }

        [Test, Category("Mutation")]
        public void CheckTestProcessFails_Mutation_Tests()
        {
            MutationTestBuilder.For("NinjaTurtles.MutationTest", "CheckTestProcessFails")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }
    }
}

