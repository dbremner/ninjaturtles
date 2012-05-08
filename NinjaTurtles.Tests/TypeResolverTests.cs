using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace NinjaTurtles.Tests
{
    [TestFixture]
    public class TypeResolverTests
    {
        [Test]
        public void ResolveTypeFromReferences_Resolves_Within_Same_Assembly()
        {
            var type = TypeResolver.ResolveTypeFromReferences(GetType().Assembly, "NinjaTurtles.Tests.ConsoleCapturer");
            Assert.AreSame(typeof(ConsoleCapturer), type);
        }

        [Test]
        public void ResolveTypeFromReferences_Resolves_Within_Referenced_Assembly()
        {
            var type = TypeResolver.ResolveTypeFromReferences(GetType().Assembly, "NinjaTurtles.IMutationTest");
            Assert.AreSame(typeof(IMutationTest), type);
        }

        [Test]
        public void ResolveTypeFromReferences_Resolves_Non_Public_Type()
        {
            var type = TypeResolver.ResolveTypeFromReferences(GetType().Assembly, "System.Configuration.ConfigXmlAttribute");
            Assert.IsNotNull(type);
        }

        [Test]
        public void ResolveTypeFromReferences_Returns_Null_If_Unrecognised()
        {
            var type = TypeResolver.ResolveTypeFromReferences(GetType().Assembly, "System.NonexistentWidger");
            Assert.IsNull(type);
        }
    }
}
