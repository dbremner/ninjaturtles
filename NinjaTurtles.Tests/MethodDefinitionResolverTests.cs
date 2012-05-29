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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Mono.Cecil;

using NUnit.Framework;

namespace NinjaTurtles.Tests
{
    [TestFixture]
    public class MethodDefinitionResolverTests
    {
        [Test]
        [MethodTested(typeof(MethodDefinitionResolver), "ResolveMethod", ParameterTypes = new[] { typeof(Type), typeof(string) })]
        public void ResolveMethod_Returns_Null_If_Ambiguous()
        {
            var assembly = AssemblyDefinition.ReadAssembly(typeof(TypeResolver).Assembly.Location);
            var type = assembly.MainModule.Types.Single(t => t.Name == "TypeResolver");
            var method = MethodDefinitionResolver.ResolveMethod(type, "ResolveTypeFromReferences");
            Assert.IsNull(method);
        }

        [Test]
        [MethodTested(typeof(MethodDefinitionResolver), "ResolveMethod", ParameterTypes = new[] { typeof(Type), typeof(string) })]
        public void ResolveMethod_Returns_Instance_If_Unambiguous()
        {
            var assembly = AssemblyDefinition.ReadAssembly(typeof(MutationTest).Assembly.Location);
            var type = assembly.MainModule.Types.Single(t => t.Name == "MutationTest");
            var method = MethodDefinitionResolver.ResolveMethod(type, "Run");
            Assert.IsNotNull(method);
        }

        [Test]
        [MethodTested(typeof(MethodDefinitionResolver), "ResolveMethod", ParameterTypes = new[] { typeof(Type), typeof(string) })]
        [MethodTested(typeof(MethodDefinitionResolver), "ResolveMethod", ParameterTypes = new[] { typeof(Type), typeof(string), typeof(Type[]) })]
        public void ResolveMethod_Returns_Instance_If_Overload_Called_With_Null_Parameter_Types()
        {
            var assembly = AssemblyDefinition.ReadAssembly(typeof(MutationTest).Assembly.Location);
            var type = assembly.MainModule.Types.Single(t => t.Name == "MutationTest");
            var method = MethodDefinitionResolver.ResolveMethod(type, "Run", null);
            Assert.IsNotNull(method);
        }

        [Test]
        [MethodTested(typeof(MethodDefinitionResolver), "ResolveMethod", ParameterTypes = new[] { typeof(Type), typeof(string), typeof(Type[]) })]
        public void ResolveMethod_Returns_Instance_If_Disambiguated_With_Parameter_Types()
        {
            var assembly = AssemblyDefinition.ReadAssembly(typeof(TypeResolver).Assembly.Location);
            var type = assembly.MainModule.Types.Single(t => t.Name == "TypeResolver");
            var parameterTypes = new[] { typeof(Assembly), typeof(string), typeof(IList<string>) };
            var method = MethodDefinitionResolver.ResolveMethod(type, "ResolveTypeFromReferences", parameterTypes);
            Assert.IsNotNull(method);
        }

        [Test]
        [MethodTested(typeof(MethodDefinitionResolver), "ResolveMethod", ParameterTypes = new[] { typeof(Type), typeof(string), typeof(Type[]) })]
        public void ResolveMethod_Returns_Null_If_No_Matching_Parameters()
        {
            var assembly = AssemblyDefinition.ReadAssembly(typeof(TypeResolver).Assembly.Location);
            var type = assembly.MainModule.Types.Single(t => t.Name == "TypeResolver");
            var parameterTypes = new[] { typeof(Assembly), typeof(string), typeof(ICollection<int>) };
            var method = MethodDefinitionResolver.ResolveMethod(type, "ResolveTypeFromReferences", parameterTypes);
            Assert.IsNull(method);
        }

        [Test, Category("Mutation")]
        public void ResolveMethod_2_Parameters_Mutation_Tests()
        {
            MutationTestBuilder<MethodDefinitionResolver>.For("ResolveMethod", new[] { typeof(Type), typeof(string) })
                .MergeReportTo("SampleReport.xml")
                .Run();
        }

        [Test, Category("Mutation")]
        public void ResolveMethod_3_Parameters_Mutation_Tests()
        {
            MutationTestBuilder<MethodDefinitionResolver>.For("ResolveMethod", new[] { typeof(Type), typeof(string), typeof(Type[]) })
                .MergeReportTo("SampleReport.xml")
                .Run();
        }
    }
}
