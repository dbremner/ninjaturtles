using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
