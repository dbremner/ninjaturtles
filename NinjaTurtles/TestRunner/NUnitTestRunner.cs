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
// Copyright (C) 2012 David Musgrove.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NinjaTurtles.TestRunner
{
    public class NUnitTestRunner : ConsoleTestRunner
    {
        private const string EXECUTABLE_NAME = "nunit-console.exe";

        private string _runnerPath;

        public string RunnerPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_runnerPath))
                {
                    _runnerPath = FindConsoleRunner();
                }
                return _runnerPath;
            }
            set { _runnerPath = value; }
        }

        private string FindConsoleRunner()
        {
            var searchPath = new List<string>();
            try
            {
                var assemblyOriginalLocation =
                    new DirectoryInfo(Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).LocalPath));
                DirectoryInfo guessedSolutionRoot = assemblyOriginalLocation.Parent.Parent.Parent;
                if (guessedSolutionRoot.Exists && guessedSolutionRoot.EnumerateDirectories("packages").Any())
                {
                    var guessedPackagesFolder = new DirectoryInfo(Path.Combine(guessedSolutionRoot.FullName, "packages"));
                    foreach (DirectoryInfo directory in guessedPackagesFolder.GetDirectories("NUnit.Runners.2.6.*"))
                    {
                        ((IList<string>)searchPath).Add(Path.Combine(directory.FullName, "tools"));
                    }
                    foreach (DirectoryInfo directory in guessedPackagesFolder.GetDirectories("NUnit.Runners.2.5.*"))
                    {
                        ((IList<string>)searchPath).Add(Path.Combine(directory.FullName, "tools"));
                    }
                }
            }
            catch (NullReferenceException)
            {
                // If we can't find a packages directory for any reason, just swallow the exception and continue...
            }
            string programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86Folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            searchPath.AddRange(new[]
                                    {
                                        Path.Combine(programFilesFolder, "NUnit 2.6\\bin"),
                                        Path.Combine(programFilesX86Folder, "NUnit 2.6\\bin"),
                                        Path.Combine(programFilesFolder, "NUnit 2.5\\bin"),
                                        Path.Combine(programFilesX86Folder, "NUnit 2.5\\bin"),
                                    });
            string environmentSearchPath = Environment.GetEnvironmentVariable("PATH") ?? "";
            searchPath.AddRange(environmentSearchPath.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries));
            foreach (string folder in searchPath)
            {
                if (File.Exists(Path.Combine(folder, EXECUTABLE_NAME)))
                {
                    return RunnerPath = folder;
                }
            }
            return string.Empty;
        }

        protected override string GetCommandLine(string testLibrary, IEnumerable<string> tests)
        {
            string path = Path.GetTempFileName();
            File.WriteAllLines(path, tests);
            return string.Format("\"{0}\" \"{1}\" /runlist=\"{2}\"",
                                 Path.Combine(RunnerPath, EXECUTABLE_NAME), testLibrary, path);
        }
    }
}