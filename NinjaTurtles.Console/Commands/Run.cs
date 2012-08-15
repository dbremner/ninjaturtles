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
using System.Xml;
using System.Xml.Xsl;

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
            get { return @"usage: NinjaTurtles.Console run [<options>] TEST_ASSEMBLY

Runs all mutation tests found in the specified test assembly.

Options:
   --class [-c]       : Specifies the full type name of the class for which
                        mutation testing should be applied. If no accompanying
                        method name is specified, all methods in the class are
                        identified, and mutation testing applied for each of
                        them.
   --format [f]       : Specifies the format for the output file specified by
                        the --output option. By default, this will be XML, but
                        HTML can be specified here to transform the results
                        into displayable format.
   --method [-m]      : Specifies the name of the method for which mutation
                        testing should be applied. Will be ignored if not used
                        in conjunction with the --class option.
   --output [-o]      : Specifies the name of a file to receive the mutation
                        testing output. This file will be deleted if it already
                        exists.
   --type [-t]        : Specifies the type name of a parameter to the method,
                        used to resolve between overrides of the same method
                        name. Can be specified multiple times, and must be in
                        the same order as the method's parameters. Full type
                        names enclosed in double quotes are accepted.

Arguments:
   TEST_ASSEMBLY      : The file name of the test assembly to inspect, which
                        should be in the current working directory.

Example:
   NinjaTurtles.Console run -c NinjaTurtles.MethodDefinitionResolver
       -m ResolveMethod
       -tt ""Mono.Cecil.TypeDefinition, Mono.Cecil"" System.String
       -o ResolveMethod.html -f HTML
       NinjaTurtles.Tests.dll

   This command will identify all unit tests in NinjaTurtles.Tests.dll that
   exercise the two-parameter override of the ResolveMethod method, and use
   them to perform mutation testing of that method. The resulting output will
   be transformed to HTML and saved to the file ResolveMethod.html.";
            }
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
                string outputPath = "";
                var outputOption = Options.Options.OfType<Output>().FirstOrDefault();
                if (outputOption != null)
                {
                    outputPath = Path.Combine(Environment.CurrentDirectory, outputOption.FileName);
                    if (File.Exists(outputPath)) File.Delete(outputPath);
                }
                var originalOut = System.Console.Out;
                if (!Options.Options.Any(o => o is Verbose))
                {
                    System.Console.SetOut(writer);
                }
                var runnerMethod = Options.Options.Any(o => o is TargetClass)
                                       ? (Options.Options.Any(o => o is TargetMethod)
                                              ? (Func<bool>)RunMutationTestsForClassAndMethod
                                              : RunMutationTestsForClass)
                                       : RunAllMutationTestsInAssembly;
                result = runnerMethod();
                if (!Options.Options.Any(o => o is Verbose))
                {
                    System.Console.SetOut(originalOut);
                }
                var formatOption = Options.Options.OfType<Format>().FirstOrDefault();
                if (outputOption != null && formatOption != null && formatOption.OutputFormat == "HTML")
                {
                    FormatOutput(outputPath);
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

        private static void FormatOutput(string outputPath)
        {
            if (!File.Exists(outputPath)) return;
            string tempPath = Path.GetTempFileName();
            File.Delete(tempPath);
            File.Move(outputPath, tempPath);
            var xslt = new XslCompiledTransform();
            var resolver = new EmbeddedResourceResolver();
            xslt.Load("ReportXslt.xslt", XsltSettings.TrustedXslt, resolver);
            using (var reader = XmlReader.Create(tempPath))
            {                
                xslt.Transform(reader, XmlWriter.Create(outputPath));
            }
            File.Delete(tempPath);
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
                .Where(m => m.HasBody && m.Name != Methods.STATIC_CONSTRUCTOR))
            {
                string targetMethod = methodInfo.Name;
                var parameterTypes = methodInfo.Parameters.Select(p => p.ParameterType.ToSystemType()).ToArray();
                result &= RunTests(targetClass, targetMethod, parameterTypes);
            }
            if (string.IsNullOrEmpty(_message))
            {
                _message = string.Format(
                    @"Mutation testing {0}",
                    result ? "passed" : "failed");
            }
            return result;
        }

        private bool RunMutationTestsForClassAndMethod()
        {
            string targetClass = Options.Options.OfType<TargetClass>().Single().ClassName;
            string targetMethod = Options.Options.OfType<TargetMethod>().Single().MethodName;
            var typeOptions = Options.Options.OfType<ParameterType>().Select(p => p.ResolvedType).ToArray();
            var result = 
                typeOptions.Any()
                ? RunTests(targetClass, targetMethod, typeOptions)
                : RunTests(targetClass, targetMethod);
            if (string.IsNullOrEmpty(_message))
            {
                _message = string.Format(
                    @"Mutation testing {0}",
                    result ? "passed" : "failed");
            }
            return result;
        }

        private bool RunTests(string targetClass, string targetMethod, Type[] parameterTypes = null)
        {
            if (parameterTypes == null || parameterTypes.Length == 0)
            {
                OutputWriter.WriteLine(
                    @"Running mutation tests for {0}.{1}",
                    targetClass,
                    targetMethod);
            }
            else
            {
                OutputWriter.WriteLine(
                    @"Running mutation tests for {0}.{1}({2})",
                    targetClass,
                    targetMethod,
                    string.Join(", ", parameterTypes.Select(t => t.Name).ToArray()));
            }
            MutationTest mutationTest =
                parameterTypes == null
                    ? (MutationTest)MutationTestBuilder.For(targetClass, targetMethod)
                    : (MutationTest)MutationTestBuilder.For(targetClass, targetMethod, parameterTypes);
            mutationTest.TestAssemblyLocation = _testAssemblyLocation;
            var outputOption = Options.Options.OfType<Output>().FirstOrDefault();
            if (outputOption != null)
            {
                string outputPath = Path.Combine(Environment.CurrentDirectory, outputOption.FileName);
                mutationTest.MergeReportTo(outputPath);
            }
            bool result = false;
            try
            {
                mutationTest.Run();
                result = true;
            }
            catch (MutationTestFailureException) {}
            catch (Exception)
            {
                _message =
                    @"An exception was thrown setting up the mutation tests. Please check your
command line parameters and try again.";
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
