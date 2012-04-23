using System;

using NinjaTurtles;
using NinjaTurtles.Attributes;
using NinjaTurtles.TestRunner;
using NinjaTurtles.Turtles.Method;

using Xunit;
using Xunit.Extensions;

namespace Calculator.Tests.NUnit
{
    [ClassTested(typeof(SimpleCalculator))]
    public class SimpleCalculatorTests : IDisposable
    {
        public SimpleCalculatorTests()
        {
            MutationTestBuilder<SimpleCalculator>.Use<xUnitTestRunner>();
        }

        public void Dispose()
        {
            MutationTestBuilder<SimpleCalculator>.Clear();
        }

        [Theory]
        [InlineData(3, 4, 7)]
        [InlineData(3, 0, 3)]
        [MethodTested("Add")]
        public void Add_SimpleTests(int left, int right, int result)
        {
            Assert.Equal(result, new SimpleCalculator().Add(left, right));
        }

        [Theory]
        [InlineData(3, 4, 7)]
        [InlineData(3, 0, 3)]
        [MethodTested("StaticAdd")]
        public void StaticAdd_SimpleTests(int left, int right, int result)
        {
            Assert.Equal(result, SimpleCalculator.StaticAdd(left, right));
        }

        [Theory]
        [InlineData(1, 2, 3, 4, 10)]
        [MethodTested("MultiAdd")]
        public void MultiAdd_SimpleTests(int i1, int i2, int i3, int i4, int result)
        {
            Assert.Equal(result, new SimpleCalculator().MultiAdd(i1, i2, i3, i4));
        }

        [Theory]
        [InlineData((short)1, (short)2, (short)3, 4, 5, 7, 22)]
        [MethodTested("MixedAdd")]
        public void MixedAdd_SimpleTests(short i1, short i2, short i3, int i4, int i5, int i6, int result)
        {
            Assert.Equal(result, new SimpleCalculator().MixedAdd(i1, i2, i3, i4, i5, i6));
        }

        [Theory]
        [InlineData(4, 2, 2)]
        [InlineData(3, 2, 1)]
        [InlineData(-8, 2, -4)]
        [MethodTested("Divide")]
        public void Divide_SimpleTests(int left, int right, int result)
        {
            Assert.Equal(result, new SimpleCalculator().Divide(left, right));
        }

        [Fact]
        [MethodTested("Divide")]
        public void Divide_DivideByZero()
        {
            Assert.Throws<ArgumentException>(() => new SimpleCalculator().Divide(1, 0));
        }

        [Fact, Trait("Category", "Mutation")]
        public void Add_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Add")
                .Run();
        }

        [Fact, Trait("Category", "Mutation")]
        public void StaticAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("StaticAdd")
                .Run();
        }

        [Fact, Trait("Category", "Mutation")]
        public void MultiAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("MultiAdd")
                .Run();
        }

        [Fact, Trait("Category", "Mutation")]
        public void MixedAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("MixedAdd")
                .With<ParameterAndVariableReadSubstitutionTurtle>()
                .With<VariableWriteSubstitutionTurtle>()
                .Run();
        }

        [Fact, Trait("Category", "Mutation")]
        public void Divide_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Divide")
                .Run();
        }
    }
}
