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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Mono.Cecil;

namespace NinjaTurtles.TestRunner
{
    public abstract class ConsoleTestRunner : ITestRunner
    {
        public int RunTestsWithMutations(MethodDefinition method, string library, string testLibrary)
        {
            var testAssembly = AssemblyDefinition.ReadAssembly(testLibrary);
            var testsToRun = new List<string>();
            foreach (var typeDefinition in testAssembly.MainModule.Types)
            {
                if (!typeDefinition.CustomAttributes
                         .Where(a => a.AttributeType.Name == "ClassTestedAttribute")
                         .Any(a => ((TypeReference)a.ConstructorArguments[0].Value).Name == method.DeclaringType.Name))
                {
                    continue;
                }
                testsToRun.AddRange(typeDefinition.Methods
                                        .Where(m => m.CustomAttributes
                                                        .Where(a => a.AttributeType.Name == "MethodTestedAttribute")
                                                        .Any(a => (string)a.ConstructorArguments[0].Value == method.Name))
                                        .Select(m => typeDefinition.FullName + "." + m.Name));
            }
            if (!testsToRun.Any())
            {
                return -1;
            }

            var startInfo = new ProcessStartInfo(GetCommandLine(testLibrary, testsToRun))
                                {
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                };
            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit(30000);
                return process.ExitCode;
            }
        }

        protected abstract string GetCommandLine(string testLibrary, IEnumerable<string> tests);
    }
}
