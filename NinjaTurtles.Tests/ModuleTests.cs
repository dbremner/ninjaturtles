using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using TestLibraryMono;

using TestLibraryNoPdb;

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
        [MethodTested(typeof(Module), "LoadDebugInformation")]
        public void Module_Loads_Source_File_List()
        {
            var module = new Module(typeof(MutationTest).Assembly.Location);
            module.LoadDebugInformation();
            Assert.NotNull(module.SourceFiles.SingleOrDefault(s => s.Key.Contains("MutationTest.cs")));
        }

        [Test]
        [MethodTested(typeof(Module), "LoadDebugInformation")]
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

        [Test]
        [MethodTested(typeof(Module), "LoadDebugInformation")]
        public void Module_Loads_Debug_Information_For_Mono()
        {
            var module = new Module(typeof(TestClassMono).Assembly.Location);
            Assert.IsTrue(module.Definition.Types
                .Single(t => t.Name == "TestClassMono")
                .Methods.Single(m => m.Name == "Run")
                .Body.Instructions.All(i => i.SequencePoint == null));
            module.LoadDebugInformation();
//            Assert.IsTrue(module.Definition.Types
//                .Single(t => t.Name == "TestClassMono")
//                .Methods.Single(m => m.Name == "Run")
//                .Body.Instructions.Any(i => i.SequencePoint != null));
        }

        [Test]
        [MethodTested(typeof(Module), "LoadDebugInformation")]
        public void Module_Does_Not_Error_With_No_Debug_Information()
        {
            var module = new Module(typeof(TestClassNoPdb).Assembly.Location);
            Assert.IsTrue(module.Definition.Types
                .Single(t => t.Name == "TestClassNoPdb")
                .Methods.Single(m => m.Name == "Run")
                .Body.Instructions.All(i => i.SequencePoint == null));
            module.LoadDebugInformation();
            Assert.IsTrue(module.Definition.Types
                .Single(t => t.Name == "TestClassNoPdb")
                .Methods.Single(m => m.Name == "Run")
                .Body.Instructions.All(i => i.SequencePoint == null));
        }

        [Test, Category("Mutation")]
        public void LoadDebugInformation_Mutation_Tests()
        {
            MutationTestBuilder<Module>.For("LoadDebugInformation")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }
    }
}
