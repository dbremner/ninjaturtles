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

using NUnit.Framework;

using NinjaTurtles.Turtles;

namespace NinjaTurtles.Tests.Turtles.BranchConditionTurtleTestSuite.Tests
{
    [TestFixture]
    public class BranchConditionClassUnderTestTests
    {
        [Test]
        public void StupidParse_Works()
        {
            Assert.AreEqual(7, new BranchConditionClassUnderTest().StupidParse("Seven"));
        }

        [Test]
        public void WorkingStupidParse_Works()
        {
            Assert.AreEqual(7, new BranchConditionClassUnderTest().WorkingStupidParse("Seven"));
            Assert.AreEqual(-1, new BranchConditionClassUnderTest().WorkingStupidParse("Not Seven"));
        }

        [Test]
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
        public void WorkingStupidParse_Mutation_Tests_Pass()
        {
            MutationTestBuilder<BranchConditionClassUnderTest>
                .For("WorkingStupidParse")
                .With<BranchConditionTurtle>()
                .Run();
        }
    }
}
