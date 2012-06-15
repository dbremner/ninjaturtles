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
using System.IO;
using System.Linq;

using Mono.Cecil;

using Xunit;

namespace NinjaTurtles.TestRunners
{
    /// <summary>
    /// An implementation of <see cref="ITestRunner" /> to run a unit test
    /// suite using the xUnit console runner.
    /// </summary>
// ReSharper disable InconsistentNaming
    public class xUnitTestRunner : ITestRunner
// ReSharper restore InconsistentNaming
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
            testAssemblyLocation = Path.Combine(mutation.TestDirectoryName, Path.GetFileName(testAssemblyLocation));

            // HACKTAGE: In the absence of a simple way to limit the tests
            // xUnit runs, we inject TraitAttributes to specify this.
            var testModule = new Module(testAssemblyLocation);
            var traitConstructor =
                testModule.Definition.Import(
                    typeof(TraitAttribute).GetConstructor(new[] { typeof(string), typeof(string) }));
            var customAttribute = new CustomAttribute(traitConstructor);
            customAttribute.ConstructorArguments.Add(
                new CustomAttributeArgument(
                    testModule.Definition.TypeSystem.String, "NinjaTurtles"));
            customAttribute.ConstructorArguments.Add(
                new CustomAttributeArgument(
                    testModule.Definition.TypeSystem.String, "run"));

            foreach (var module in testModule.Definition.Types)
            {
                foreach (var method in module.Methods)
                {
                    string qualifiedMethodName = module.FullName + "." + method.Name;
                    if (testsToRun.Contains(qualifiedMethodName))
                    {
                        method.CustomAttributes.Add(customAttribute);
                    }
                }
            }
            mutation.TestDirectory.SaveAssembly(testModule);

            string arguments = string.Format("\"{0}\" {{0}}noshadow {{0}}trait \"NinjaTurtles=run\"",
                                 testAssemblyLocation);

            return ConsoleProcessFactory.CreateProcess("xunit.console.clr4.exe", arguments);
        }
    }
}
