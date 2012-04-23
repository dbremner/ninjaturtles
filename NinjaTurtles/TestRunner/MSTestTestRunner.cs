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
using System.Collections.Generic;
using System.Linq;

namespace NinjaTurtles.TestRunner
{
    /// <summary>
    /// A concrete implementation of <see cref="ConsoleTestRunner" /> that
    /// attempts to locate and run the MSTest console runner.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public class MSTestTestRunner : ConsoleTestRunner
// ReSharper restore InconsistentNaming
    {
        /// <summary>
        /// Gets the command line executable file name used to run the unit
        /// tests.
        /// </summary>
        /// <returns></returns>
        protected override string GetCommandLineFileName()
        {
            string path = "C:\\Program Files (x86)\\Microsoft Visual Studio 10.0\\Common7\\IDE\\";
            return string.Format("\"{0}MSTest.exe\"", path);
        }

        /// <summary>
        /// Gets the arguments used to run the unit tests specified in the
        /// <paramref name="tests" /> parameter from the library found at path
        /// <paramref name="testLibraryPath" />.
        /// </summary>
        /// <param name="testLibraryPath">
        /// The path to the test assembly.
        /// </param>
        /// <param name="tests">
        /// A list of the fully qualified names of the test methods to be run.
        /// </param>
        /// <returns></returns>
        protected override string GetCommandLineArguments(string testLibraryPath, IEnumerable<string> tests)
        {
            string testArguments = string.Join(" ", tests.Select(t => string.Format("/test:\"{0}\"", t)));
            return string.Format("/testcontainer:\"{0}\" {1}",
                testLibraryPath, testArguments);
        }

        /// <summary>
        /// Maps a process exit code to the success status of the test suite.
        /// </summary>
        /// <param name="exitCode">
        /// The exit code of the console test runner process.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test suite passed, otherwise <b>false</b>.
        /// </returns>
        protected override bool InterpretExitCode(int exitCode)
        {
            return exitCode == 0;
        }
    }
}
