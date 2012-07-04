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
// License along with NinjaTurtles.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Mono.Cecil;
using Mono.Cecil.Cil;

using NUnit.Framework;

using NinjaTurtles.Turtles;

using MethodAttributes = System.Reflection.MethodAttributes;
using ParameterAttributes = System.Reflection.ParameterAttributes;
using TypeAttributes = System.Reflection.TypeAttributes;

namespace NinjaTurtles.Tests.Turtles
{
    [TestFixture]
    public class VariableWriteTurtleTests
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
                               Mono.Cecil.TypeAttributes.Class | Mono.Cecil.TypeAttributes.Public);
            var intType = assembly.MainModule.Import(typeof(int));
            var boolType = assembly.MainModule.Import(typeof(bool));
            var method = new MethodDefinition("TestMethod", Mono.Cecil.MethodAttributes.Public, intType);
            var dispose = new MethodDefinition("Dispose", Mono.Cecil.MethodAttributes.Public, boolType);
            var variable1 = new VariableDefinition(intType);
            var variable2 = new VariableDefinition(intType);
            var resultVariable = new VariableDefinition(intType);
            method.Body.Variables.Add(variable1);
            method.Body.Variables.Add(variable2);
            method.Body.Variables.Add(resultVariable);

            var processor = method.Body.GetILProcessor();
            var finalNop = processor.Create(OpCodes.Nop);
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldc_I4, 0));
            method.Body.Instructions.Add(processor.Create(OpCodes.Stloc, variable2));
            method.Body.Instructions.Add(processor.Create(OpCodes.Nop));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldc_I4, 7));
            method.Body.Instructions.Add(processor.Create(OpCodes.Stloc, variable1));
            method.Body.Instructions.Add(processor.Create(OpCodes.Nop));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldc_I4, 8));
            method.Body.Instructions.Add(processor.Create(OpCodes.Stloc, variable2));
            method.Body.Instructions.Add(processor.Create(OpCodes.Leave, finalNop));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldarg_0));
            method.Body.Instructions.Add(processor.Create(OpCodes.Call, dispose));
            method.Body.Instructions.Add(processor.Create(OpCodes.Nop));
            method.Body.Instructions.Add(processor.Create(OpCodes.Endfinally));
            method.Body.Instructions.Add(finalNop);
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldc_I4, -1));
            method.Body.Instructions.Add(processor.Create(OpCodes.Stloc, resultVariable));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldloc, resultVariable));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ret));


            type.Methods.Add(method);
            type.Methods.Add(dispose);
            assembly.MainModule.Types.Add(type);
            return assembly;
        }

        private string GetTempAssemblyFileName()
        {
            return Path.Combine(_testFolder, "Test.dll");
        }


        [Test]
        public void DoMutate_Returns_Correct_Sequences()
        {
            var module = new Module(Assembly.GetExecutingAssembly().Location);
            module.LoadDebugInformation();
            var method = module.Definition
                .Types.Single(t => t.Name == "VariableWriteClassUnderTest")
                .Methods.Single(t => t.Name == "AddWithPointlessNonsense");

            var mutator = new VariableWriteTurtle();
            IList<MutantMetaData> mutations = mutator
                .Mutate(method, module, method.Body.Instructions.Select(i => i.Offset).ToArray()).ToList();

            Assert.AreEqual(2, mutations.Count);
            StringAssert.EndsWith("write substitution Int32.pointlessA => Int32.CS$1$0000", mutations[0].Description);
            Assert.AreEqual(2, mutations[0].ILIndex);
            StringAssert.EndsWith("write substitution Int32.pointlessB => Int32.CS$1$0000", mutations[1].Description);
            Assert.AreEqual(4, mutations[1].ILIndex);
        }

        [Test]
        public void DoMutate_Skips_Writes_In_Dispose()
        {
            var assembly = CreateTestAssembly();

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var module = new Module(tempAssemblyFileName);
            module.LoadDebugInformation();

            var method = module.Definition
                .Types.Single(t => t.Name == "TestClass")
                .Methods.Single(t => t.Name == "TestMethod");

            var mutator = new VariableWriteTurtle();
            IList<MutantMetaData> mutations = mutator
                .Mutate(method, module, method.Body.Instructions.Select(i => i.Offset).ToArray()).ToList();

            Assert.AreEqual(1, mutations.Count);
            StringAssert.EndsWith("write substitution Int32. => Int32.", mutations[0].Description);
        }

        [Test]
        public void DoMutate_Returns_Correct_Sequences_With_Fields()
        {
            var module = new Module(Assembly.GetExecutingAssembly().Location);
            module.LoadDebugInformation();
            var method = module.Definition
                .Types.Single(t => t.Name == "VariableWriteClassUnderTest")
                .Methods.Single(t => t.Name == "AddWithPointlessNonsenseViaFields");

            var mutator = new VariableWriteTurtle();
            IList<MutantMetaData> mutations = mutator
                .Mutate(method, module, method.Body.Instructions.Select(i => i.Offset).ToArray()).ToList();

            Assert.AreEqual(2, mutations.Count);
            StringAssert.EndsWith("write substitution Int32._pointlessA => Int32.CS$1$0000", mutations[0].Description);
            StringAssert.EndsWith("write substitution Int32._pointlessB => Int32.CS$1$0000", mutations[1].Description);
        }

        [Test]
        public void DoMutate_Returns_No_Results_As_Appropriate()
        {
            var module = new Module(Assembly.GetExecutingAssembly().Location);
            module.LoadDebugInformation();
            var method = module.Definition
                .Types.Single(t => t.Name == "VariableWriteClassUnderTest")
                .Methods.Single(t => t.Name == "AddWithoutPointlessNonsense");

            var mutator = new VariableWriteTurtle();
            IList<MutantMetaData> mutations = mutator
                .Mutate(method, module, method.Body.Instructions.Select(i => i.Offset).ToArray()).ToList();

            Assert.AreEqual(0, mutations.Count);
        }

        [Test, Category("Mutation"), MutationTest]
        public void DoMutate_Mutation_Tests()
        {
            MutationTestBuilder<VariableWriteTurtle>.For("DoMutate")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }
    }
}
