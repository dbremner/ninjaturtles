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

using NinjaTurtles.TestRunners;
using NinjaTurtles.Tests.TestUtilities;
using NinjaTurtles.Turtles;

using N = NUnit.Framework;
using Ms = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NinjaTurtles.Tests.MSTest
{
    [N.TestFixture]
    [Ms.TestClass]
    public class ClassUnderTestTests
    {
        [Ms.TestMethod]
        [MethodTested(typeof(ClassUnderTest), "Dummy")]
        public void Dummy_Dummies()
        {
            Ms.Assert.AreEqual(0, new ClassUnderTest().Dummy());
        }

        [Ms.TestMethod]
        [MethodTested(typeof(ClassUnderTest), "Add")]
        public void Add_Works()
        {
            Ms.Assert.AreEqual(3, new ClassUnderTest().Add(3, 0));
        }

        [Ms.TestMethod]
        [MethodTested(typeof(ClassUnderTest), "WorkingAdd")]
        public void WorkingAdd_Works()
        {
            Ms.Assert.AreEqual(3, new ClassUnderTest().WorkingAdd(3, 0));
            Ms.Assert.AreEqual(7, new ClassUnderTest().WorkingAdd(3, 4));
        }

        [N.Test]
        [MethodTested("NinjaTurtles.MutationTest", "Run")]
        [MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
        public void Dummy_Mutation_Tests_Pass()
        {
            using (var console = new ConsoleCapturer())
            {
                MutationTestBuilder<ClassUnderTest>
                    .For("Dummy")
                    .With<ArithmeticOperatorTurtle>()
                    .UsingRunner<MSTestTestRunner>()
                    .Run();
                N.StringAssert.Contains("No valid mutations found (this is fine).", console.Output);
            }
        }

        [N.Test]
        [MethodTested("NinjaTurtles.MutationTest", "Run")]
        [MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
        [MethodTested(typeof(MethodTurtleBase), "Mutate")]
        [MethodTested(typeof(MethodTurtleBase), "DoYield")]
        [MethodTested(typeof(ArithmeticOperatorTurtle), "DoMutate")]
        public void Add_Mutation_Tests_Fail()
        {
            try
            {
                MutationTestBuilder<ClassUnderTest>
                    .For("Add")
                    .With<ArithmeticOperatorTurtle>()
                    .UsingRunner<MSTestTestRunner>()
                    .Run();
            }
            catch (MutationTestFailureException)
            {
                return;
            }
            N.Assert.Fail("MutationTestFailureException was not thrown.");
        }

        [N.Test]
        [MethodTested("NinjaTurtles.MutationTest", "Run")]
        [MethodTested("NinjaTurtles.MutationTest", "RunMutation")]
        [MethodTested(typeof(MethodTurtleBase), "Mutate")]
        [MethodTested(typeof(MethodTurtleBase), "DoYield")]
        [MethodTested(typeof(ArithmeticOperatorTurtle), "DoMutate")]
        public void WorkingAdd_Mutation_Tests_Pass()
        {
            MutationTestBuilder<ClassUnderTest>
                .For("WorkingAdd")
                .With<ArithmeticOperatorTurtle>()
                .UsingRunner<MSTestTestRunner>()
                .Run();
        }
    }
}
