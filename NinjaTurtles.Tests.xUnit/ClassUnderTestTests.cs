﻿#region Copyright & licence

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
using x = Xunit;

namespace NinjaTurtles.Tests.xUnit
{
    [N.TestFixture]
    public class ClassUnderTestTests
    {
        [x.Fact]
        public void Dummy_Dummies()
        {
            x.Assert.Equal(0, new ClassUnderTest().Dummy());
        }

        [x.Fact]
        public void Add_Works()
        {
            x.Assert.Equal(3, new ClassUnderTest().Add(3, 0));
        }

        [x.Fact]
        public void WorkingAdd_Works()
        {
            x.Assert.Equal(3, new ClassUnderTest().WorkingAdd(3, 0));
            x.Assert.Equal(7, new ClassUnderTest().WorkingAdd(3, 4));
        }

        [N.Test]
        public void Dummy_Mutation_Tests_Pass()
        {
            using (var console = new ConsoleCapturer())
            {
                MutationTestBuilder<ClassUnderTest>
                    .For("Dummy")
                    .With<ArithmeticOperatorTurtle>()
                    .UsingRunner<xUnitTestRunner>()
                    .Run();
                N.StringAssert.Contains("No valid mutations found (this is fine).", console.Output);
            }
        }

        [N.Test]
        public void Add_Mutation_Tests_Fail()
        {
            try
            {
                MutationTestBuilder<ClassUnderTest>
                    .For("Add")
                    .With<ArithmeticOperatorTurtle>()
                    .UsingRunner<xUnitTestRunner>()
                    .Run();
            }
            catch (MutationTestFailureException)
            {
                return;
            }
            N.Assert.Fail("MutationTestFailureException was not thrown.");
        }

        [N.Test]
        public void WorkingAdd_Mutation_Tests_Pass()
        {
            MutationTestBuilder<ClassUnderTest>
                .For("WorkingAdd")
                .With<ArithmeticOperatorTurtle>()
                .UsingRunner<xUnitTestRunner>()
                .Run();
        }
    }
}
