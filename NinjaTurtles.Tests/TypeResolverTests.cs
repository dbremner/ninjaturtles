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
// License along with Refix.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using NUnit.Framework;

using NinjaTurtles.Tests.TestUtilities;

namespace NinjaTurtles.Tests
{
    [TestFixture]
    public class TypeResolverTests
    {
        [Test]
        [MethodTested(typeof(TypeResolver), "ResolveTypeFromReferences")]
        public void ResolveTypeFromReferences_Resolves_Within_Same_Assembly()
        {
            var type = TypeResolver.ResolveTypeFromReferences(GetType().Assembly, "NinjaTurtles.Tests.TestUtilities.ConsoleCapturer");
            Assert.IsNotNull(type);
            Assert.AreSame(typeof(ConsoleCapturer), type);
        }

        [Test]
        [MethodTested(typeof(TypeResolver), "ResolveTypeFromReferences")]
        public void ResolveTypeFromReferences_Resolves_Within_Referenced_Assembly()
        {
            var type = TypeResolver.ResolveTypeFromReferences(GetType().Assembly, "System.Linq.ParallelEnumerable");
            Assert.IsNotNull(type);
            Assert.AreSame(typeof(ParallelEnumerable), type);
        }

        [Test]
        [MethodTested(typeof(TypeResolver), "ResolveTypeFromReferences")]
        public void ResolveTypeFromReferences_Resolves_Non_Public_Type()
        {
            var type = TypeResolver.ResolveTypeFromReferences(GetType().Assembly, "NinjaTurtles.TypeResolver");
            Assert.IsNotNull(type);
        }

        [Test]
        [MethodTested(typeof(TypeResolver), "ResolveTypeFromReferences")]
        public void ResolveTypeFromReferences_Returns_Null_If_Unrecognised()
        {
            var type = TypeResolver.ResolveTypeFromReferences(GetType().Assembly, "System.NonexistentWidget");
            Assert.IsNull(type);
        }

        [Test, Category("Mutation")]
        public void ResolveTypeFromReferences_Internal_Mutation_Tests()
        {
            MutationTestBuilder<TypeResolver>.For("ResolveTypeFromReferences",
                                                  new[] {typeof(Assembly), typeof(string)})
                .MergeReportTo("SampleReport.xml")
                .Run();
        }

        [Test, Category("Mutation")]
        public void ResolveTypeFromReferences_Private_Mutation_Tests()
        {
            MutationTestBuilder<TypeResolver>.For("ResolveTypeFromReferences",
                                                  new[] {typeof(Assembly), typeof(string), typeof(IList<string>)})
                .MergeReportTo("SampleReport.xml")
                .Run();
        }
    }
}
