using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NinjaTurtles;
using NinjaTurtles.Attributes;
using NinjaTurtles.TestRunner;
using NinjaTurtles.Turtles.Method;

namespace Calculator.Tests.MSTest
{
    [TestClass]
    [ClassTested(typeof(SimpleCalculator))]
    public class SimpleCalculatorTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            MutationTestBuilder<SimpleCalculator>.Use<MSTestTestRunner>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MutationTestBuilder<SimpleCalculator>.Clear();
        }

        [TestMethod]
        [MethodTested("Add")]
        public void Add_SimpleTests()
        {
            Assert.AreEqual(7, new SimpleCalculator().Add(3, 4));
            Assert.AreEqual(3, new SimpleCalculator().Add(3, 0));
        }

        [TestMethod]
        [MethodTested("StaticAdd")]
        public void StaticAdd_SimpleTests()
        {
            Assert.AreEqual(7, SimpleCalculator.StaticAdd(3, 4));
            Assert.AreEqual(7, SimpleCalculator.StaticAdd(3, 4));
        }

        [TestMethod]
        [MethodTested("MultiAdd")]
        public void MultiAdd_SimpleTests()
        {
            Assert.AreEqual(10, new SimpleCalculator().MultiAdd(1, 2, 3, 4));
        }

        [TestMethod]
        [MethodTested("MixedAdd")]
        public void MixedAdd_SimpleTests()
        {
            Assert.AreEqual(22, new SimpleCalculator().MixedAdd(1, 2, 3, 4, 5, 7));
        }

        [TestMethod]
        [MethodTested("Divide")]
        public void Divide_SimpleTests()
        {
            Assert.AreEqual(2, new SimpleCalculator().Divide(4, 2));
            Assert.AreEqual(1, new SimpleCalculator().Divide(3, 2));
            Assert.AreEqual(-4, new SimpleCalculator().Divide(-8, 2));
        }

        [TestMethod]
        [MethodTested("Divide")]
        [ExpectedException(typeof(ArgumentException))]
        public void Divide_DivideByZero()
        {
            new SimpleCalculator().Divide(1, 0);
        }

        [TestMethod, TestCategory("Mutation")]
        public void Add_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Add")
                .Run();
        }

        [TestMethod, TestCategory("Mutation")]
        public void StaticAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("StaticAdd")
                .Run();
        }

        [TestMethod, TestCategory("Mutation")]
        public void MultiAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("MultiAdd")
                .Run();
        }

        [TestMethod, TestCategory("Mutation")]
        public void MixedAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("MixedAdd")
                .With<ParameterAndVariableReadSubstitutionTurtle>()
                .With<VariableWriteSubstitutionTurtle>()
                .Run();
        }

        [TestMethod, TestCategory("Mutation")]
        public void Divide_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Divide")
                .Run();
        }

    }
}
