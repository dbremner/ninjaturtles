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
using System.IO;
using System.Linq;
using System.Reflection;

using Mono.Cecil;

using NinjaTurtles.Console.Options;

namespace NinjaTurtles.Console.Commands
{
    internal class Run : Command
    {
        private string _testAssemblyLocation;
        private string _message;

        protected override string HelpText
        {
            get { return @"usage: NinjaTurtles.Console run [-c|-cm] TEST_ASSEMBLY

Runs all mutation tests found in the specified test assembly.

Options:
   --class [-c]       : Specifies the full type name of the class for which
                        mutation testing should be applied.
   --method [-m]      : Specifies the name of the method for which mutation
                        testing should be applied. Will be ignored if not used
                        in conjunction with the --class option.

Arguments:
   TEST_ASSEMBLY      : The file name of the test assembly to inspect, which
                        should be in the current working directory."; }
        }

        public override bool Validate()
        {
            if (!base.Validate())
            {
                return false;
            }
            if (Options.Arguments.Count == 0)
            {
                using (new OutputWriterErrorHighlight())
                {
                    OutputWriter.WriteLine(
                        OutputVerbosity.Quiet,
                        @"The 'run' command must take the path to a test assembly as an argument.");
                }
                return false;
            }
            _testAssemblyLocation = Path.Combine(Environment.CurrentDirectory, Options.Arguments[0]);
            if (!File.Exists(_testAssemblyLocation))
            {
                using (new OutputWriterErrorHighlight())
                {
                    OutputWriter.WriteLine(
                        OutputVerbosity.Quiet,
                        @"Test assembly '{0}' does not exist in the current working directory.",
                        Options.Arguments[0]);
                    return false;
                }
            }
            return true;
        }

        public override bool Execute()
        {
            bool result;
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                var originalOut = System.Console.Out;
                if (!Options.Options.Any(o => o is Verbose))
                {
                    System.Console.SetOut(writer);
                }
                if (Options.Options.Any(o => o is TargetClass))
                {
                    if (Options.Options.Any(o => o is TargetMethod))
                    {
                        result = RunMutationTestsForClassAndMethod();
                    }
                    else
                    {
                        result = RunMutationTestsForClass();
                    }
                }
                else
                {
                    result = RunAllMutationTestsInAssembly();
                }
                if (!Options.Options.Any(o => o is Verbose))
                {
                    System.Console.SetOut(originalOut);
                }
                OutputWriter.WriteLine();
                var statusColor = result
                                      ? ConsoleColor.Green
                                      : ConsoleColor.Red;
                using (new OutputWriterHighlight(statusColor))
                {
                    OutputWriter.WriteLine(_message);
                }
                return result;
            }
        }

        private bool RunMutationTestsForClass()
        {
            string targetClass = Options.Options.OfType<TargetClass>().Single().ClassName;
            var testAssembly = Assembly.LoadFrom(_testAssemblyLocation);
            var matchedType = TypeResolver.ResolveTypeFromReferences(testAssembly, targetClass);
            if (matchedType == null)
            {
                _message = string.Format(@"Unknown type '{0}'", targetClass);
                return false;
            }
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(matchedType.Assembly.Location);
            var targetType = assemblyDefinition.MainModule.Types.FirstOrDefault(t => t.FullName == targetClass);
            bool result = true;
            foreach (var methodInfo in targetType.Methods
                .Where(m => m.HasBody))
            {
                string targetMethod = methodInfo.Name;
                result &= RunTests(targetClass, targetMethod);
            }
            _message = string.Format(
                @"Mutation testing {0}",
                result ? "passed" : "failed");
            return result;
        }

        private bool RunMutationTestsForClassAndMethod()
        {
            string targetClass = Options.Options.OfType<TargetClass>().Single().ClassName;
            string targetMethod = Options.Options.OfType<TargetMethod>().Single().MethodName;
            var result = RunTests(targetClass, targetMethod);
            _message = string.Format(
                @"Mutation testing {0}",
                result ? "passed" : "failed");
            return result;
        }

        private bool RunTests(string targetClass, string targetMethod)
        {
            OutputWriter.WriteLine(
                @"Running mutation tests for {0}.{1}",
                targetClass,
                targetMethod);
            var mutationTest = (MutationTest)MutationTestBuilder.For(targetClass, targetMethod);
            mutationTest.TestAssemblyLocation = _testAssemblyLocation;
            bool result = false;
            try
            {
                mutationTest.Run();
                result = true;
            }
            catch (MutationTestFailureException)
            {
            }
            return result;
        }

        private bool RunAllMutationTestsInAssembly()
        {
            var testAssembly = Assembly.LoadFrom(_testAssemblyLocation);
            int tests = 0;
            int failures = 0;
            foreach (var type in testAssembly.GetTypes())
            {
                if (!type.IsPublic) continue;
                var mutationTests = type.GetMethods()
                    .Where(m => m.GetCustomAttributes(true).Any(a => a.GetType() == typeof(MutationTestAttribute)));
                if (mutationTests.Any())
                {
                    var testFixtureInstance = Activator.CreateInstance(type);
                    foreach (var mutationTest in mutationTests)
                    {
                        tests++;
                        try
                        {
                            mutationTest.Invoke(testFixtureInstance, null);
                        }
                        catch (MutationTestFailureException)
                        {
                            failures++;
                        }
                        OutputWriter.Write(".");
                    }
                }
            }
            OutputWriter.WriteLine();
            _message = string.Format(
                @"Mutation test summary: {0} passed, {1} failed",
                tests - failures,
                failures);
            return tests > 0 && failures == 0;
        }
    }
}
