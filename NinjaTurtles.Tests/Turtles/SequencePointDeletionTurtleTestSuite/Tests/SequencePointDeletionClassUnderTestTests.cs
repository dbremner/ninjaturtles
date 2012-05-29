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

using NinjaTurtles.Turtles;

namespace NinjaTurtles.Tests.Turtles.SequencePointDeletionTurtleTestSuite.Tests
{
    [TestFixture]
    public class SequencePointDeletionClassUnderTestTests
    {
        [Test]
        [MethodTested(typeof(SequencePointDeletionClassUnderTest), "SimpleMethod")]
        public void StupidParse_Works()
        {
            Assert.AreEqual(7, new SequencePointDeletionClassUnderTest().SimpleMethod(1, 0, 3, 2));
        }

        [Test]
        [MethodTested(typeof(SequencePointDeletionClassUnderTest), "WorkingSimpleMethod")]
        public void WorkingStupidParse_Works()
        {
            Assert.AreEqual(7, new SequencePointDeletionClassUnderTest().WorkingSimpleMethod(1, 0, 3, 2));
            Assert.AreEqual(11, new SequencePointDeletionClassUnderTest().WorkingSimpleMethod(1, 2, 3, 2));
            Assert.AreEqual(24, new SequencePointDeletionClassUnderTest().WorkingSimpleMethod(2, -1, 3, -3));
        }

        [Test]
        [MethodTested("NinjaTurtles.MutationTest", "Run")]
        [MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
        [MethodTested(typeof(MethodTurtleBase), "Mutate")]
        [MethodTested(typeof(MethodTurtleBase), "DoYield")]
        [MethodTested(typeof(SequencePointDeletionTurtle), "DoMutate")]
        [MethodTested(typeof(SequencePointDeletionTurtle), "ShouldDeleteSequence")]
        public void StupidParse_Mutation_Tests_Fail()
        {
            try
            {
                MutationTestBuilder<SequencePointDeletionClassUnderTest>
                    .For("SimpleMethod")
                    .With<SequencePointDeletionTurtle>()
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
        [MethodTested(typeof(SequencePointDeletionTurtle), "DoMutate")]
        [MethodTested(typeof(SequencePointDeletionTurtle), "ShouldDeleteSequence")]
        public void WorkingStupidParse_Mutation_Tests_Pass()
        {
            MutationTestBuilder<SequencePointDeletionClassUnderTest>
                .For("WorkingSimpleMethod")
                .With<SequencePointDeletionTurtle>()
                .Run();
        }
    }
}
