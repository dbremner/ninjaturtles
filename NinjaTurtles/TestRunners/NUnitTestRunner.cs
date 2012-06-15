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
// License along with NinjaTurtles.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NinjaTurtles.TestRunners
{
    /// <summary>
    /// An implementation of <see cref="ITestRunner" /> to run a unit test
    /// suite using the NUnit console runner.
    /// </summary>
    public class NUnitTestRunner : ITestRunner
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
            string testListFile = Path.GetTempFileName();
            File.WriteAllLines(testListFile, testsToRun);

            testAssemblyLocation = Path.Combine(mutation.TestDirectoryName, Path.GetFileName(testAssemblyLocation));
            string arguments = string.Format("\"{0}\" {{0}}runlist=\"{1}\" {{0}}nologo {{0}}nodots {{0}}stoponerror", testAssemblyLocation, testListFile);

            var searchPath = new List<string>();
            string programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            searchPath.AddRange(new[]
                                    {
                                        Path.Combine(programFilesFolder, "NUnit 2.6\\bin"),
                                        Path.Combine(programFilesFolder, "NUnit 2.5\\bin")
                                    });
            string programFilesX86Folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (!string.IsNullOrEmpty(programFilesX86Folder))
            {
                searchPath.AddRange(new[]
                        {
                            Path.Combine(programFilesX86Folder, "NUnit 2.6\\bin"),
                            Path.Combine(programFilesX86Folder, "NUnit 2.5\\bin")
                        });
            }

            return ConsoleProcessFactory.CreateProcess("nunit-console.exe", arguments, searchPath.ToArray());
        }
    }
}