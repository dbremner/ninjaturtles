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
// License along with Refix.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System;
using System.Reflection;

using NinjaTurtles.TestRunner;

namespace NinjaTurtles
{
    /// <summary>
    /// A static class used as the starting point for a fluent definition of
    /// a set of mutation tests.
    /// </summary>
    /// <typeparam name="T">
    /// The type to be tested.
    /// </typeparam>
    /// <example>
    /// <para>
    /// This code creates and runs the default set of mutation tests for the
    /// <b>ClassUnderTest</b> class's <b>MethodUnderTest</b> method:
    /// </para>
    /// <code lang="cs">
    /// MutationTestBuilder&lt;ClassUnderTest&gt;
    ///     .For("MethodUnderTest")
    ///     .Run();
    /// </code>
    /// <code lang="vbnet">
    /// Call MutationTestBuilder(Of ClassUnderTest) _
    ///     .For("MethodUnderTest") _
    ///     .Run
    /// </code>
    /// <para>
    /// When this code is included in a test, it causes the matching tests to
    /// be run for each mutation that is found of the code under test. By
    /// default, NinjaTurtles assumes it is running under NUnit, and thus uses
    /// an NUnit runner to run the suite against the mutated code. This can be
    /// changed using the fluent interface:
    /// </para>
    /// <code lang="cs">
    /// MutationTestBuilder&lt;ClassUnderTest&gt;
    ///     .For("MethodUnderTest")
    ///     .Using&lt;GallioTestRunner&gt;()
    ///     .Run();
    /// </code>
    /// <code lang="vbnet">
    /// Call MutationTestBuilder(Of ClassUnderTest) _
    ///     .For("MethodUnderTest") _
    ///     .Using(Of GallioTestRunner)() _
    ///     .Run
    /// </code>
    /// <para>
    /// Alternatively, this option can be set across all tests in a fixture by
    /// including this line in the test fixture's setup method:
    /// </para>
    /// <code lang="cs">
    /// MutationTestBuilder&lt;ClassUnderTest&gt;
    ///     .Use&lt;GallioTestRunner&gt;();
    /// </code>
    /// <code lang="vbnet">
    /// Call MutationTestBuilder(Of ClassUnderTest) _
    ///     .Use(Of GallioTestRunner)
    /// </code>
    /// </example>
    public static class MutationTestBuilder<T> where T : class
    {
        private static Type _testRunner;

        /// <summary>
        /// Returns an <see cref="IMutationTest" /> instance allowing a fluent
        /// definition of a set of mutation tests for a particular method.
        /// </summary>
        /// <param name="methodName">
        /// The name of the method to mutate.
        /// </param>
        /// <returns>
        /// An <see cref="IMutationTest" /> instance to allow fluent
        /// method chaining.
        /// </returns>
        public static IMutationTest For(string methodName)
        {
            var testAssembly = Assembly.GetCallingAssembly().Location;
            var mutationTest = new MutationTest(typeof(T), methodName, testAssembly);
            if (_testRunner != null)
            {
                mutationTest.TestRunner = _testRunner;
            }
            return mutationTest;
        }

        /// <summary>
        /// Sets a default test runner type for mutation testing.
        /// </summary>
        /// <typeparam name="TRunner">
        /// The type of test runner to store as default.
        /// </typeparam>
        public static void Use<TRunner>() where TRunner : ITestRunner
        {
            _testRunner = typeof(TRunner);
        }

        /// <summary>
        /// Clears any defaults that have been set.
        /// </summary>
        public static void Clear()
        {
            _testRunner = null;
        }
    }
}
