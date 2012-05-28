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
// License along with Refix.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System;

using NUnit.Framework;

namespace NinjaTurtles.Tests
{
    [TestFixture]
    public class MethodTestedAttributeTests
    {
        [Test]
        [MethodTested(typeof(MethodTestedAttribute), Methods.CONSTRUCTOR, ParameterTypes = new[] { typeof(Type), typeof(string) })]
        public void Constructor_Sets_Properties_From_Type()
        {
            var type = typeof(Module);
            var method = "MyMethod";
            var attribute = new MethodTestedAttribute(type, method);

            Assert.AreEqual(type, attribute.TargetType);
            Assert.AreEqual(method, attribute.TargetMethod);
        }

        [Test]
        [MethodTested(typeof(MethodTestedAttribute), Methods.CONSTRUCTOR, ParameterTypes = new[] { typeof(string), typeof(string) })]
        public void Constructor_Sets_Properties_From_String()
        {
            var type = typeof(Module);
            var method = "MyMethod";
            var attribute = new MethodTestedAttribute("NinjaTurtles.Module", method);

            Assert.AreEqual(type, attribute.TargetType);
            Assert.AreEqual(method, attribute.TargetMethod);
        }

        [Test, Category("Mutation")]
        public void Constructor_With_Type_Mutation_Tests()
        {
            MutationTestBuilder<MethodTestedAttribute>.For(Methods.CONSTRUCTOR, new[] { typeof(Type), typeof(string) })
                .MergeReportTo("SampleReport.xml")
                .Run();
        }

        [Test, Category("Mutation")]
        public void Constructor_With_String_Mutation_Tests()
        {
            MutationTestBuilder<MethodTestedAttribute>.For(Methods.CONSTRUCTOR, new[] { typeof(string), typeof(string) })
                .MergeReportTo("SampleReport.xml")
                .Run();
        }
    }
}
