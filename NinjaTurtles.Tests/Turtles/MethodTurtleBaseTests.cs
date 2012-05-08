using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

using NUnit.Framework;

using NinjaTurtles.Turtles;

namespace NinjaTurtles.Tests.Turtles
{
    [TestFixture]
    public class MethodTurtleBaseTests
    {
        private string _testFolder;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _testFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testFolder);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Directory.Delete(_testFolder);
        }

        private string GetTempAssemblyFileName()
        {
            return Path.Combine(_testFolder, "Test.dll");
        }

        private static AssemblyDefinition CreateTestAssembly()
        {
            var name = new AssemblyNameDefinition("TestArithmeticOperatorTurtleAdd", new Version(1, 0));
            var assembly = AssemblyDefinition.CreateAssembly(name, "TestClass", ModuleKind.Dll);
            var type = new TypeDefinition("TestArithmeticOperatorTurtleAdd", "TestClass",
                               TypeAttributes.Class | TypeAttributes.Public);
            var intType = assembly.MainModule.Import(typeof(int));
            var method = new MethodDefinition("TestMethod", MethodAttributes.Public, intType);
            var leftParam = new ParameterDefinition("left", ParameterAttributes.In, intType);
            var rightParam = new ParameterDefinition("right", ParameterAttributes.In, intType);
            method.Parameters.Add(leftParam);
            method.Parameters.Add(rightParam);
            var resultVariable = new VariableDefinition(intType);
            method.Body.Variables.Add(resultVariable);

            var processor = method.Body.GetILProcessor();
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldarg_1));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldarg_2));
            method.Body.Instructions.Add(processor.Create(OpCodes.Add));
            method.Body.Instructions.Add(processor.Create(OpCodes.Stloc, resultVariable));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldloc, resultVariable));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ret));

            type.Methods.Add(method);
            assembly.MainModule.Types.Add(type);
            return assembly;
        }

        [Test]
        [MethodTested(typeof(MethodTurtleBase), "Mutate")]
        public void Mutate_Simplifies_Macros_In_IL()
        {
            var assembly = CreateTestAssembly();
            var turtle = new DummyTurtle();
            var method = assembly.MainModule.Types
                .Single(t => t.Name == "TestClass")
                .Methods.Single(m => m.Name == "TestMethod");
            var mutation = turtle.Mutate(method, assembly, GetTempAssemblyFileName()).First();
            Assert.AreEqual(OpCodes.Ldarg, mutation.MethodDefinition.Body.Instructions[0].OpCode);
        }

        [Test]
        [MethodTested(typeof(MethodTurtleBase), "Mutate")]
        [MethodTested(typeof(MethodTurtleBase), "DoYield")]
        [MethodTested(typeof(MethodTurtleBase), "MutantComplete")]
        public void Mutate_Creates_And_Destroys_Directories()
        {
            var assembly = CreateTestAssembly();

            var addMethod = assembly.MainModule
                .Types.Single(t => t.Name == "TestClass")
                .Methods.Single(t => t.Name == "TestMethod");

            var mutator = new DummyTurtle();
            IEnumerable<MutationTestMetaData> mutations = mutator
                .Mutate(addMethod, assembly, GetTempAssemblyFileName());

            var directories = new List<string>();

            foreach (var mutation in mutations)
            {
                string directoryName = mutation.TestDirectoryName;
                directories.Add(directoryName);
                Assert.IsTrue(Directory.Exists(directoryName));
                mutator.MutantComplete(mutation);
            }

            foreach (var directory in directories)
            {
                Assert.IsFalse(Directory.Exists(directory));
            }
        }

        [Test, Category("Mutation")]
        public void Mutate_Mutation_Tests()
        {
            MutationTestBuilder<MethodTurtleBase>.For("Mutate")
                .Run();
        }

        [Test, Category("Mutation")]
        public void MutantComplete_Mutation_Tests()
        {
            MutationTestBuilder<MethodTurtleBase>.For("MutantComplete")
                .Run();
        }

        [Test, Category("Mutation")]
        public void DoYield_Mutation_Tests()
        {
            MutationTestBuilder<MethodTurtleBase>.For("DoYield")
                .Run();
        }

        private class DummyTurtle : MethodTurtleBase
        {
            protected override IEnumerable<MutationTestMetaData> DoMutate(MethodDefinition method, AssemblyDefinition assembly, string testAssemblyLocation)
            {
                yield return DoYield(method, assembly, testAssemblyLocation, "Dummy");
            }
        }
    }
}
