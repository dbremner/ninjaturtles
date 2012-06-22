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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using NUnit.Framework;

using NinjaTurtles.Turtles;

namespace NinjaTurtles.Tests.Turtles
{
    [TestFixture]
    public class VariableWriteTurtleTests
    {
        [Test]
        [MethodTested(typeof(MethodTurtleBase), "Mutate")]
        [MethodTested(typeof(MethodTurtleBase), "DoYield")]
        [MethodTested(typeof(VariableWriteTurtle), "DoMutate")]
        public void DoMutate_Returns_Correct_Seqeuences()
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
            StringAssert.EndsWith("write substitution Int32.pointlessB => Int32.CS$1$0000", mutations[1].Description);
        }

        [Test]
        [MethodTested(typeof(MethodTurtleBase), "Mutate")]
        [MethodTested(typeof(MethodTurtleBase), "DoYield")]
        [MethodTested(typeof(VariableWriteTurtle), "DoMutate")]
        public void DoMutate_Returns_Correct_Seqeuences_With_Fields()
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
        
        [Test, Category("Mutation")]
        public void DoMutate_Mutation_Tests()
        {
            MutationTestBuilder<VariableWriteTurtle>.For("DoMutate")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }
    }
}
