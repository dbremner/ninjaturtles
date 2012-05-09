using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using NUnit.Framework;

namespace NinjaTurtles.Tests
{
    [TestFixture]
    public class TypeResolverTests
    {
        [Test]
        [MethodTested(typeof(TypeResolver), "ResolveTypeFromReferences")]
        public void ResolveTypeFromReferences_Resolves_Within_Same_Assembly()
        {
            var type = TypeResolver.ResolveTypeFromReferences(GetType().Assembly, "NinjaTurtles.Tests.ConsoleCapturer");
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
                .MergeReportTo("C:\\Working\\hg\\ninjaturtles\\SampleReport.xml")
                .Run();
        }

        [Test, Category("Mutation")]
        public void ResolveTypeFromReferences_Private_Mutation_Tests()
        {
            MutationTestBuilder<TypeResolver>.For("ResolveTypeFromReferences",
                                                  new[] {typeof(Assembly), typeof(string), typeof(IList<string>)})
                .MergeReportTo("C:\\Working\\hg\\ninjaturtles\\SampleReport.xml")
                .Run();
        }
    }
}
