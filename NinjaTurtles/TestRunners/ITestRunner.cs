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
using System.Diagnostics;

namespace NinjaTurtles.TestRunners
{
    /// <summary>
    /// Interface defining core behavior of a unit test runner used by
    /// NinjaTurtles.
    /// </summary>
    public interface ITestRunner
    {
        /// <summary>
        /// Runs the tests specified from the test assembly, found within the
        /// test directory identified in the provided
        /// <see cref="MutantMetaData" /> instance.
        /// <remarks>
        /// This method won't be called
        /// from a user's testing code, it is called internally by
        /// NinjaTurtles, and is only exposed publicly to allow for a new
        /// implementation to be provided as an extension to NinjaTurtles.
        /// </remarks>
        /// </summary>
        /// <param name="mutation">
        /// An instance of <see cref="MutantMetaData" /> describing the
        /// mutant under test.
        /// </param>
        /// <param name="testAssemblyLocation">
        /// The file name (with or without path) of the unit test assembly.
        /// </param>
        /// <param name="testsToRun">
        /// A list of qualified unit test names.
        /// </param>
        /// <returns>
        /// A <see cref="Process" /> instance to run the unit test runner.
        /// </returns>
        Process GetRunnerProcess(MutantMetaData mutation, string testAssemblyLocation, IEnumerable<string> testsToRun);
    }
}
