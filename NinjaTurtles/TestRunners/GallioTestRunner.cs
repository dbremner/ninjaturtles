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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NinjaTurtles.TestRunners
{
    /// <summary>
    /// An implementation of <see cref="ITestRunner" /> to run a unit test
    /// suite using the Gallio console runner.
    /// </summary>
    public class GallioTestRunner : ITestRunner
    {
        /// <summary>
        /// Runs the tests specified from the test assembly, found within the
        /// test directory identified in the provided
        /// <see cref="MutantMetaData" /> instance.
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
        public Process GetRunnerProcess(MutantMetaData mutation, string testAssemblyLocation, IEnumerable<string> testsToRun)
        {
            IDictionary<string, IList<string>> testsByFixture = new Dictionary<string, IList<string>>();
            foreach (var test in testsToRun)
            {
                int position = test.LastIndexOf(".");
                string fixture = test.Substring(0, position);
                string member = test.Substring(position + 1);
                if (!testsByFixture.ContainsKey(fixture))
                {
                    testsByFixture.Add(fixture, new List<string>());
                }
                testsByFixture[fixture].Add(member);
            }
            string filter = string.Join(" or ", testsByFixture.Select(
                kv => string.Format("(Type: {0} and Member: {1})",
                                    kv.Key,
                                    string.Join(", ", kv.Value))));

            testAssemblyLocation = Path.Combine(mutation.TestDirectoryName, Path.GetFileName(testAssemblyLocation));
            string arguments = string.Format("\"{0}\" {{0}}f:\"{1}\" {{0}}r:IsolatedProcess",
                                 testAssemblyLocation, filter);

            var searchPath = new List<string>();
            string programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            searchPath.AddRange(new[]
                                    {
                                        Path.Combine(programFilesFolder, "Gallio\\bin")
                                    });
            string programFilesX86Folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (!string.IsNullOrEmpty(programFilesX86Folder))
            {
                searchPath.AddRange(new[]
                        {
                                        Path.Combine(programFilesFolder, "Gallio\\bin")
                        });
            }
            return ConsoleProcessFactory.CreateProcess("Gallio.Echo.exe", arguments, searchPath.ToArray());
        }
    }
}
