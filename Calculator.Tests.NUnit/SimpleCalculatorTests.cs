﻿using System;

using NUnit.Framework;

using NinjaTurtles.Attributes;
using NinjaTurtles.Fluent;
using NinjaTurtles.Mutators.Method;

namespace Calculator.Tests.NUnit
{
    [TestFixture]
    [ClassTested(typeof(SimpleCalculator))]
    public class SimpleCalculatorTests
    {
        [TestCase(3, 4, Result = 7)]
        [TestCase(3, 0, Result = 3)]
        [MethodTested("Add")]
        public int Add_SimpleTests(int left, int right)
        {
            return new SimpleCalculator().Add(left, right);
        }

        [TestCase(1, 2, 3, 4, Result = 10)]
        [MethodTested("MultiAdd")]
        public int MultiAdd_SimpleTests(int i1, int i2, int i3, int i4)
        {
            return new SimpleCalculator().MultiAdd(i1, i2, i3, i4);
        }

        [TestCase(1, 2, 3, 4, 5, 6, Result = 21)]
        [MethodTested("MixedAdd")]
        public int MixedAdd_SimpleTests(short i1, short i2, short i3, int i4, int i5, int i6)
        {
            return new SimpleCalculator().MixedAdd(i1, i2, i3, i4, i5, i6);
        }

        [TestCase(4, 2, Result = 2)]
        [TestCase(3, 2, Result = 1)]
        [TestCase(-8, 2, Result = -4)]
        [MethodTested("Divide")]
        public int Divide_SimpleTests(int left, int right)
        {
            return new SimpleCalculator().Divide(left, right);
        }

        [Test]
        [MethodTested("Divide")]
        public void Divide_DivideByZero()
        {
            Assert.Throws<ArgumentException>(() => new SimpleCalculator().Divide(1, 0));
        }

        [Test, Category("Mutation")]
        public void Add_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Add")
                .ExpectedInvariantFor<ParameterPermutationMutator>()
                .Run();
        }

        [Test, Category("Mutation")]
        public void MultiAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("MultiAdd")
                .ExpectedInvariantFor<ParameterPermutationMutator>()
                .Run();
        }

        [Test, Category("Mutation")]
        public void MixedAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("MixedAdd")
                .ExpectedInvariantFor<ParameterPermutationMutator>()
                .Run();
        }

        [Test, Category("Mutation")]
        public void Divide_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Divide")
                .Run();
        }
    }
}
