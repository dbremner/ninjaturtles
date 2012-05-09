using System;

using NUnit.Framework;

namespace NinjaTurtles.Tests
{
	[TestFixture]
	public class MutationTestBuilderTests
	{
		[Test]
		[MethodTested(typeof(MutationTestBuilder), "For")]
		public void For_Returns_Correct_Type()
		{
			IMutationTest result = MutationTestBuilder.For("System.DateTime", "TryParse");
			Assert.IsNotNull(result);
			Assert.AreEqual("MutationTest", result.GetType().Name, "For should return an instance of MutationTest.");
		}
		
		[Test]
		[MethodTested(typeof(MutationTestBuilder), "For")]
		public void For_Resolves_Type_And_Stores_Values_Passed()
		{
			const string METHOD_NAME = "methodName";

			IMutationTest result = MutationTestBuilder.For("System.DateTime", METHOD_NAME);
			Assert.AreEqual(typeof(DateTime), result.TargetType, "For should instantiate MutationTest with TargetClass property set.");
			Assert.AreEqual(METHOD_NAME, result.TargetMethod, "For should instantiate MutationTest with TargetMethod property set.");
			
			result = MutationTestBuilder.For("NinjaTurtles.MutationTest", METHOD_NAME);
			Assert.AreEqual("MutationTest", result.TargetType.Name, "For should instantiate MutationTest with TargetClass property set.");
			Assert.AreEqual(METHOD_NAME, result.TargetMethod, "For should instantiate MutationTest with TargetMethod property set.");
		}
		
		[Test]
		[MethodTested(typeof(MutationTestBuilder), "For")]
		public void Generic_For_Stores_Type()
		{
			IMutationTest result = MutationTestBuilder<DateTime>.For("Xxx");
			Assert.AreEqual(typeof(DateTime), result.TargetType);
		}
		
		[Test, Category("Mutation")]
		public void For_Mutation_Tests()
		{
			MutationTestBuilder<MutationTestBuilder>.For("For", new[] { typeof(string), typeof(string), typeof(Type[]) })
                .MergeReportTo("C:\\Working\\hg\\ninjaturtles\\SampleReport.xml")
                .Run();
		}
	}
}

