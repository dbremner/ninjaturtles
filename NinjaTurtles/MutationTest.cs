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
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Microsoft.Win32;

using Mono.Cecil;

using NinjaTurtles.Reporting;
using NinjaTurtles.TestRunners;
using NinjaTurtles.Turtles;

namespace NinjaTurtles
{
	internal class MutationTest : IMutationTest
	{
	    private const string ERROR_REPORTING_KEY = @"SOFTWARE\Microsoft\Windows\Windows Error Reporting";
	    private const string ERROR_REPORTING_VALUE = "DontShowUI";

	    private readonly IList<Type> _mutationsToApply = new List<Type>();
		private readonly string _testAssemblyLocation;
	    private readonly Type[] _parameterTypes;
	    private readonly AssemblyDefinition _testAssembly;
		private readonly TypeReference _targetTypeReference;
	    private Module _module;
	    private IEnumerable<string> _testsToRun;
	    private ITestRunner _runner;
	    private MutationTestingReport _report;
        private ReportingStrategy _reportingStrategy = new NullReportingStrategy();
	    private string _reportFileName;

	    internal MutationTest(string testAssemblyLocation, Type targetType, string targetMethod, Type[] parameterTypes)
		{
			TargetType = targetType;
			TargetMethod = targetMethod;
			_testAssemblyLocation = testAssemblyLocation;
		    _parameterTypes = parameterTypes;
		    _testAssembly = AssemblyDefinition.ReadAssembly(testAssemblyLocation);
			_targetTypeReference = _testAssembly.MainModule.Import(targetType);
		}
		
		public Type TargetType { get; private set; }

		public string TargetMethod { get; private set; }
		
		public void Run()
		{
		    var key = Registry.LocalMachine.OpenSubKey(ERROR_REPORTING_KEY, RegistryKeyPermissionCheck.ReadWriteSubTree);
            var errorReportingValue = key.GetValue(ERROR_REPORTING_VALUE, null);
            key.SetValue(ERROR_REPORTING_VALUE, 1, RegistryValueKind.DWord);
		    key.Close();

			MethodDefinition method = ValidateMethod();
            _module.LoadDebugInformation();
            int[] originalOffsets = method.Body.Instructions.Select(i => i.Offset).ToArray();
		    _report = new MutationTestingReport();
            _testsToRun = GetMatchingTestsOrFail(method);
			int count = 0;
			int failures = 0;
			if (_mutationsToApply.Count == 0) PopulateDefaultTurtles();
			foreach (var turtleType in _mutationsToApply)
			{
                var turtle = (MethodTurtleBase)Activator.CreateInstance(turtleType);
                Parallel.ForEach(turtle.Mutate(method, _module, originalOffsets),
        		    mutation => RunMutation(turtle, mutation, ref failures, ref count));
			}

            _report.RegisterMethod(method);
            _reportingStrategy.WriteReport(_report, _reportFileName);

            key = Registry.LocalMachine.OpenSubKey(ERROR_REPORTING_KEY, RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (errorReportingValue == null)
            {
                key.DeleteValue(ERROR_REPORTING_VALUE);
            }
            else
            {
                key.SetValue(ERROR_REPORTING_VALUE, errorReportingValue, RegistryValueKind.DWord);
            }
            key.Close();

			if (count == 0)
			{
				Console.WriteLine("No valid mutations found (this is fine).");
				return;
			}
			if (failures > 0)
			{
				throw new MutationTestFailureException();
			}
		}

        private void RunMutation(MethodTurtleBase turtle, MutantMetaData mutation, ref int failures, ref int count)
		{
			bool testProcessFailed = CheckTestProcessFails(turtle, mutation);
			if (!testProcessFailed)
			{
				Interlocked.Increment(ref failures);
			}
			Interlocked.Increment(ref count);
		}
		
		private bool CheckTestProcessFails(MethodTurtleBase turtle, MutantMetaData mutation)
		{
            if (_runner == null) _runner = (ITestRunner)Activator.CreateInstance(MutationTestBuilder.TestRunner);
		    var process = _runner.GetRunnerProcess(mutation, _testAssemblyLocation, _testsToRun);

            process.Start();
			bool exitedInTime = process.WaitForExit(30000);
			int exitCode = -1;

			try
			{
				if (!exitedInTime)
				{
					KillProcessAndChildren(process.Id);
				}
				exitCode = process.ExitCode;
			}
			catch {}

            bool testSuitePassed = exitCode == 0 && exitedInTime;
            
            string result = string.Format("Mutant: {0}. {1}",
			                  mutation.Description,
			                  testSuitePassed
			                  	? "Survived."
			                    : "Killed.");
            _report.AddResult(turtle.GetCurrentSequencePoint(mutation.ILIndex), mutation, !testSuitePassed);

            if (testSuitePassed)
            {
                mutation.TestDirectory.DoNotDelete = true;
                result = string.Format("{0}\nOriginal source code around surviving mutant (in {1}):\n{2}\nFiles left for inspection in: {3}",
                    result,
                    turtle.GetOriginalSourceFileName(mutation.ILIndex),
                    turtle.GetOriginalSourceCode(mutation.ILIndex),
                    mutation.TestDirectoryName);
            }

            Console.WriteLine(result);

            turtle.MutantComplete(mutation);
            return !testSuitePassed;
		}

        private void KillProcessAndChildren(int pid)
        {
            using (var searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid))
            using (ManagementObjectCollection moc = searcher.Get())
            {
                foreach (ManagementObject mo in moc)
                {
                    KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
                }
                try
                {
                    Process proc = Process.GetProcessById(pid);
					proc.Kill();
                }
				catch (ArgumentException) {}
            }
        }

				
		private void PopulateDefaultTurtles()
		{
            foreach (var type in GetType().Assembly.GetTypes()
                .Where(t => t.GetInterface("IMethodTurtle") != null
                && !t.IsAbstract))
            {
                _mutationsToApply.Add(type);
            }
		}

        private IEnumerable<string> GetMatchingTestsOrFail(MethodDefinition targetMethod)
		{
			var tests = new List<string>();
			foreach (var type in _testAssembly.MainModule.Types)
			{
				foreach (var method in type.Methods)
				{
					if (method.CustomAttributes
							.Any(a => HasMatchingMethodTestedAttribute(targetMethod, a)))
					{
						tests.Add(string.Format ("{0}.{1}", type.FullName, method.Name));
					}
						
				}
			}
			if (!tests.Any())
			{
				throw new MutationTestFailureException(
					"No matching tests were found to run.");
			}
			return tests;
		}

        private bool HasMatchingMethodTestedAttribute(MethodDefinition targetMethod, CustomAttribute attribute)
        {
            if (attribute.AttributeType.Name != "MethodTestedAttribute") return false;
            if ((string)attribute.ConstructorArguments[1].Value != targetMethod.Name) return false;
            if (attribute.ConstructorArguments[0].Value is string
                && (string)attribute.ConstructorArguments[0].Value != _targetTypeReference.FullName)
            {
                return false;
            }
            if (attribute.ConstructorArguments[0].Value is TypeReference
                && ((TypeReference)attribute.ConstructorArguments[0].Value).FullName != _targetTypeReference.FullName)
            {
                return false;
            }
            if (_parameterTypes != null
                && attribute.HasProperties
                && attribute.Properties.Any(p => p.Name == "ParameterTypes")
                && !Enumerable.SequenceEqual(
                    _parameterTypes.Select(t => t.Name),
                    Array.ConvertAll((CustomAttributeArgument[])attribute.Properties.Single(p => p.Name == "ParameterTypes").Argument.Value, a => (TypeReference)a.Value)
                        .Select(t => t.Name)))
            {
                return false;
            }
            return true;
        }

	    private MethodDefinition ValidateMethod()
	    {
            _module = new Module(TargetType.Assembly.Location);
            var type = _module.Definition.Types
		        .Single(t => t.FullName == TargetType.FullName);
		    return MethodDefinitionResolver.ResolveMethod(type, TargetMethod, _parameterTypes);
		}

	    public IMutationTest With<T>() where T : IMethodTurtle
		{
			_mutationsToApply.Add(typeof(T));
			return this;
		}

	    /// <summary>
	    /// Sets the unit test runner to be used, which is an implementation of
	    /// <see cref="ITestRunner" />. If none is specified, then the default
	    /// is to use <see cref="NUnitTestRunner" />.
	    /// </summary>
	    /// <typeparam name="T">
	    /// A type that implements <see cref="ITestRunner" />.
	    /// </typeparam>
	    /// <returns>
	    /// The original <see cref="IMutationTest" /> instance to allow fluent
	    /// method chaining.
	    /// </returns>
	    public IMutationTest UsingRunner<T>() where T : ITestRunner, new()
	    {
	        _runner = new T();
	        return this;
	    }

	    public IMutationTest WriteReportTo(string fileName)
	    {
	        _reportingStrategy = new OverwriteReportingStrategy();
	        _reportFileName = fileName;
	        return this;
	    }

	    public IMutationTest MergeReportTo(string fileName)
	    {
	        _reportingStrategy = new MergeReportingStrategy();
            _reportFileName = fileName;
            return this;
	    }

        private abstract class ReportingStrategy
        {
            public abstract void WriteReport(MutationTestingReport report, string fileName);
        }

        private class NullReportingStrategy : ReportingStrategy
        {
            public override void WriteReport(MutationTestingReport report, string fileName) { }
        }

        private class OverwriteReportingStrategy : ReportingStrategy
        {
            public override void WriteReport(MutationTestingReport report, string fileName)
            {
                using (var streamWriter = File.CreateText(fileName))
                {
                    new XmlSerializer(typeof(MutationTestingReport)).Serialize(streamWriter, report);
                }
            }
        }

        private class MergeReportingStrategy : ReportingStrategy
        {
            public override void WriteReport(MutationTestingReport report, string fileName)
            {
                report.MergeFromFile(fileName);
                using (var streamWriter = File.CreateText(fileName))
                {
                    new XmlSerializer(typeof(MutationTestingReport)).Serialize(streamWriter, report);
                }
            }
        }
    }
}

