using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace NinjaTurtles.Tests
{
    [TestFixture]
    public class ModuleTests
    {
        [Test]
        public void Module_Loads_Definition()
        {
            var module = new Module(typeof(MutationTest).Assembly.Location);
            Assert.AreEqual("NinjaTurtles.dll", module.Definition.Name);
        }

        [Test]
        public void Module_Loads_Source_File_List()
        {
            var module = new Module(typeof(MutationTest).Assembly.Location);
            module.LoadDebugInformation();
            Assert.NotNull(module.SourceFiles.SingleOrDefault(s => s.Key.Contains("MutationTest.cs")));
        }

        [Test]
        public void Module_Loads_Debug_Information()
        {
            var module = new Module(typeof(MutationTest).Assembly.Location);
            Assert.IsTrue(module.Definition.Types
                .Single(t => t.Name == "MutationTest")
                .Methods.Single(m => m.Name == "Run")
                .Body.Instructions.All(i => i.SequencePoint == null));
            module.LoadDebugInformation();
            Assert.IsTrue(module.Definition.Types
                .Single(t => t.Name == "MutationTest")
                .Methods.Single(m => m.Name == "Run")
                .Body.Instructions.Any(i => i.SequencePoint != null));
        }
    }
}
