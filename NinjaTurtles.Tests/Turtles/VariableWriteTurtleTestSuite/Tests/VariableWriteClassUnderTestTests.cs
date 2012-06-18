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

namespace NinjaTurtles.Tests.Turtles.VariableWriteTurtleTestSuite.Tests
{
    [TestFixture]
    public class VariableWriteClassUnderTestTests
    {
        [Test]
        [MethodTested(typeof(VariableWriteClassUnderTest), "AddWithPointlessNonsense")]
        public void AddWithPointlessNonsense_Works()
        {
            Assert.AreEqual(4, new VariableWriteClassUnderTest().AddWithPointlessNonsense(1, 3));
            Assert.AreEqual(85, new VariableWriteClassUnderTest().AddWithPointlessNonsense(-7, 92));
        }

        [Test]
        [MethodTested(typeof(VariableWriteClassUnderTest), "AddWithoutPointlessNonsense")]
        public void AddWithoutPointlessNonsense_Works()
        {
            Assert.AreEqual(4, new VariableWriteClassUnderTest().AddWithoutPointlessNonsense(1, 3));
            Assert.AreEqual(85, new VariableWriteClassUnderTest().AddWithoutPointlessNonsense(-7, 92));
        }

		[Test]
		[MethodTested("NinjaTurtles.MutationTest", "Run")]
		[MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
        [MethodTested(typeof(MethodTurtleBase), "Mutate")]
        [MethodTested(typeof(MethodTurtleBase), "DoYield")]
        [MethodTested(typeof(VariableWriteTurtle), "DoMutate")]
        public void AddWithPointlessNonsense_Fails_Mutation_Testing()
		{
            try
            {
                MutationTestBuilder<VariableWriteClassUnderTest>
                    .For("AddWithPointlessNonsense")
                    .With<VariableWriteTurtle>()
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
        [MethodTested(typeof(VariableWriteTurtle), "DoMutate")]
        public void AddWithoutPointlessNonsense_Passes_Mutation_Testing()
        {
            MutationTestBuilder<VariableWriteClassUnderTest>
                .For("AddWithoutPointlessNonsense")
                .With<VariableWriteTurtle>()
                .Run();
        }
    }
}
