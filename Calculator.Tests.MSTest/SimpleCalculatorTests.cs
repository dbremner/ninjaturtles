using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NinjaTurtles.Attributes;
using NinjaTurtles.Fluent;
using NinjaTurtles.TestRunner;
using NinjaTurtles.Turtles.Method;

namespace Calculator.Tests.MSTest
{
    [TestClass]
    [ClassTested(typeof(SimpleCalculator))]
    public class SimpleCalculatorTests
    {
        [TestMethod]
        [MethodTested("Add")]
        public void Add_SimpleTests()
        {
            Assert.AreEqual(7, new SimpleCalculator().Add(3, 4));
            Assert.AreEqual(3, new SimpleCalculator().Add(3, 0));
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
        public void Divide_DivideByZero()
        {
            try
            {
                new SimpleCalculator().Divide(1, 0);
            }
            catch (ArgumentException)
            {
                return;
            }
            Assert.Fail("ArgumentException not thrown.");
        }

        [TestMethod]
        public void Add_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Add")
                .UsingRunner<MSTestTestRunner>()
                .Run();
        }

        [TestMethod]
        public void Divide_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Divide")
                .UsingRunner<MSTestTestRunner>()
                .Run();
        }
    }
}
