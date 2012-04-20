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
// License along with Refix.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System.Reflection;

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
    /// This code creates and runs the default set of mutation tests for the
    /// <b>ClassUnderTest</b> class's <b>MethodUnderTest</b> method:
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
    /// </example>
    public static class MutationTestBuilder<T> where T : class
    {
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
            return new MutationTest(typeof(T), methodName, testAssembly);
        }
    }
}