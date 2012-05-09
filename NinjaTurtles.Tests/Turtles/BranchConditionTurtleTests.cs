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
    public class BranchConditionTurtleTests
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
            Directory.Delete(_testFolder, true);
        }

        private static AssemblyDefinition CreateTestAssembly()
        {
            var name = new AssemblyNameDefinition("TestBranchConditionTurtle", new Version(1, 0));
            var assembly = AssemblyDefinition.CreateAssembly(name, "TestClass", ModuleKind.Dll);
            var type = new TypeDefinition("TestBranchConditionTurtle", "TestClass",
                               TypeAttributes.Class | TypeAttributes.Public);
            var intType = assembly.MainModule.Import(typeof(int));
            var boolType = assembly.MainModule.Import(typeof(bool));
            var method = new MethodDefinition("TestMethod", MethodAttributes.Public, intType);
            var leftParam = new ParameterDefinition("left", ParameterAttributes.In, intType);
            var rightParam = new ParameterDefinition("right", ParameterAttributes.In, intType);
            method.Parameters.Add(leftParam);
            method.Parameters.Add(rightParam);
            var resultVariable = new VariableDefinition(boolType);
            method.Body.Variables.Add(resultVariable);

            var processor = method.Body.GetILProcessor();
            Instruction loadReturnValueInstruction = processor.Create(OpCodes.Ldloc, resultVariable);
            Instruction storeTrueInReturnValueInstruction = processor.Create(OpCodes.Ldc_I4, -1);
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldarg, leftParam));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldarg, rightParam));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ceq));
            method.Body.Instructions.Add(processor.Create(OpCodes.Brtrue_S, storeTrueInReturnValueInstruction));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldc_I4, 0));
            method.Body.Instructions.Add(processor.Create(OpCodes.Stloc, resultVariable));
            method.Body.Instructions.Add(processor.Create(OpCodes.Br_S, loadReturnValueInstruction));
            method.Body.Instructions.Add(storeTrueInReturnValueInstruction);
            method.Body.Instructions.Add(processor.Create(OpCodes.Stloc, resultVariable));
            method.Body.Instructions.Add(loadReturnValueInstruction);
            method.Body.Instructions.Add(processor.Create(OpCodes.Ret));

            type.Methods.Add(method);
            assembly.MainModule.Types.Add(type);
            return assembly;
        }

        private string GetTempAssemblyFileName()
        {
            return Path.Combine(_testFolder, "Test.dll");
        }

        [Test]
        [MethodTested(typeof(BranchConditionTurtle), "DoMutate")]
        public void DoMutate_Returns_Correct_Replacements_For_Addition()
        {
            var assembly = CreateTestAssembly();

            var addMethod = assembly.MainModule
                .Types.Single(t => t.Name == "TestClass")
                .Methods.Single(t => t.Name == "TestMethod");

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var module = new Module(tempAssemblyFileName);

            var mutator = new BranchConditionTurtle();
            IEnumerable<MutationTestMetaData> mutations = mutator
                .Mutate(addMethod, module);

            int brTrue = 0;
            int brFalse = 0;
            int br = 0;
            int total = 0;
            foreach (var metaData in mutations)
            {
                total++;
                if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Brtrue)) brTrue++;
                if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Brfalse)) brFalse++;
                if (metaData.MethodDefinition.Body.Instructions.Any(i => i.OpCode == OpCodes.Br)) br++;
            }

            Assert.AreEqual(4, total);
            Assert.AreEqual(1, brTrue);
            Assert.AreEqual(1, brFalse);
            Assert.AreEqual(3, br);
        }
    }
}
