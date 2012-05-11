using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using NinjaTurtles.Turtles;

namespace NinjaTurtles.Tests.Turtles.BranchConditionTurtleTestSuite.Tests
{
    [TestFixture]
    public class BranchConditionClassUnderTestTests
    {
        [Test]
        [MethodTested(typeof(BranchConditionClassUnderTest), "StupidParse")]
        public void StupidParse_Works()
        {
            Assert.AreEqual(7, new BranchConditionClassUnderTest().StupidParse("Seven"));
        }

        [Test]
        [MethodTested(typeof(BranchConditionClassUnderTest), "WorkingStupidParse")]
        public void WorkingStupidParse_Works()
        {
            Assert.AreEqual(7, new BranchConditionClassUnderTest().WorkingStupidParse("Seven"));
            Assert.AreEqual(-1, new BranchConditionClassUnderTest().WorkingStupidParse("Not Seven"));
        }

        [Test]
        [MethodTested("NinjaTurtles.MutationTest", "Run")]
        [MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
        [MethodTested(typeof(MethodTurtleBase), "Mutate")]
        [MethodTested(typeof(MethodTurtleBase), "DoYield")]
        [MethodTested(typeof(BranchConditionTurtle), "DoMutate")]
        public void StupidParse_Mutation_Tests_Fail()
        {
            try
            {
                MutationTestBuilder<BranchConditionClassUnderTest>
                    .For("StupidParse")
                    .With<BranchConditionTurtle>()
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
        [MethodTested(typeof(BranchConditionTurtle), "DoMutate")]
        public void WorkingStupidParse_Mutation_Tests_Pass()
        {
            MutationTestBuilder<BranchConditionClassUnderTest>
                .For("WorkingStupidParse")
                .With<BranchConditionTurtle>()
                .Run();
        }
    }
}
