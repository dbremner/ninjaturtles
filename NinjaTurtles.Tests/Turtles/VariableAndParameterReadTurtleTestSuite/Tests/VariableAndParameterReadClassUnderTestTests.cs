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

namespace NinjaTurtles.Tests.Turtles.VariableAndParameterReadTurtleTestSuite.Tests
{
    [TestFixture]
    public class VariableAndParameterReadClassUnderTestTests
    {
        [Test]
        [MethodTested(typeof(VariableAndParameterReadClassUnderTest), "AddAndDouble")]
        public void AddAndDouble_Works()
        {
            Assert.AreEqual(0, new VariableAndParameterReadClassUnderTest().AddAndDouble(0, 0));
            Assert.AreEqual(0, new VariableAndParameterReadClassUnderTest().AddAndDouble(1, -1));
        }

        [Test]
        [MethodTested(typeof(VariableAndParameterReadClassUnderTest), "AddAndDoubleViaField")]
        public void AddAndDoubleViaField_Works()
        {
            Assert.AreEqual(0, new VariableAndParameterReadClassUnderTest().AddAndDoubleViaField(0, 0));
            Assert.AreEqual(0, new VariableAndParameterReadClassUnderTest().AddAndDoubleViaField(1, -1));
        }

        [Test]
        [MethodTested(typeof(VariableAndParameterReadClassUnderTest), "WorkingAddAndDouble")]
        public void WorkingAddAndDouble_Works()
        {
            Assert.AreEqual(0, new VariableAndParameterReadClassUnderTest().WorkingAddAndDouble(0, 0));
            Assert.AreEqual(2, new VariableAndParameterReadClassUnderTest().WorkingAddAndDouble(0, 1));
            Assert.AreEqual(4, new VariableAndParameterReadClassUnderTest().WorkingAddAndDouble(1, 1));
            Assert.AreEqual(6, new VariableAndParameterReadClassUnderTest().WorkingAddAndDouble(2, 1));
        }

        [Test]
        [MethodTested(typeof(VariableAndParameterReadClassUnderTest), "WorkingAddAndDoubleViaField")]
        public void WorkingAddAndDoubleViaField_Works()
        {
            Assert.AreEqual(0, new VariableAndParameterReadClassUnderTest().WorkingAddAndDoubleViaField(0, 0));
            Assert.AreEqual(2, new VariableAndParameterReadClassUnderTest().WorkingAddAndDoubleViaField(0, 1));
            Assert.AreEqual(4, new VariableAndParameterReadClassUnderTest().WorkingAddAndDoubleViaField(1, 1));
            Assert.AreEqual(6, new VariableAndParameterReadClassUnderTest().WorkingAddAndDoubleViaField(2, 1));
        }

        [Test]
        [MethodTested("NinjaTurtles.MutationTest", "Run")]
        [MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
        [MethodTested(typeof(MethodTurtleBase), "Mutate")]
        [MethodTested(typeof(MethodTurtleBase), "DoYield")]
        [MethodTested(typeof(VariableWriteTurtle), "DoMutate")]
        public void AddAndDouble_Fails_Mutation_Testing()
        {
            try
            {
                MutationTestBuilder<VariableAndParameterReadClassUnderTest>
                    .For("AddAndDouble")
                    .With<VariableAndParameterReadTurtle>()
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
        public void AddAndDoubleViaField_Fails_Mutation_Testing()
        {
            try
            {
                MutationTestBuilder<VariableAndParameterReadClassUnderTest>
                    .For("AddAndDoubleViaField")
                    .With<VariableAndParameterReadTurtle>()
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
        public void WorkingAddAndDouble_Passes_Mutation_Testing()
        {
            MutationTestBuilder<VariableAndParameterReadClassUnderTest>
                .For("WorkingAddAndDouble")
                .With<VariableAndParameterReadTurtle>()
                .Run();
        }

        [Test]
        [MethodTested("NinjaTurtles.MutationTest", "Run")]
        [MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
        [MethodTested(typeof(MethodTurtleBase), "Mutate")]
        [MethodTested(typeof(MethodTurtleBase), "DoYield")]
        [MethodTested(typeof(VariableWriteTurtle), "DoMutate")]
        public void WorkingAddAndDoubleViaField_Passes_Mutation_Testing()
        {
            MutationTestBuilder<VariableAndParameterReadClassUnderTest>
                .For("WorkingAddAndDoubleViaField")
                .With<VariableAndParameterReadTurtle>()
                .Run();
        }
    }
}
