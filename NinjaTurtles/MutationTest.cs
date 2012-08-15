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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Microsoft.Win32;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

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
		private  string _testAssemblyLocation;
	    private readonly Type[] _parameterTypes;
	    private AssemblyDefinition _testAssembly;
		private readonly TypeReference _targetTypeReference;
	    private Module _module;
	    private IEnumerable<string> _testsToRun;
	    private ITestRunner _runner;
	    private MutationTestingReport _report;
        private ReportingStrategy _reportingStrategy = new NullReportingStrategy();
	    private string _reportFileName;
	    private MethodReferenceComparer _comparer;
	    private static Regex _automaticallyGeneratedNestedClassMatcher = new Regex("^\\<([A-Za-z0-9@_]+)\\>");
	    //private TimeSpan _benchmark;

	    internal MutationTest(string testAssemblyLocation, Type targetType, string targetMethod, Type[] parameterTypes)
		{
			TargetType = targetType;
			TargetMethod = targetMethod;
			TestAssemblyLocation = testAssemblyLocation;
		    _parameterTypes = parameterTypes;
			_targetTypeReference = _testAssembly.MainModule.Import(targetType);
		}
		
		public Type TargetType { get; private set; }

		public string TargetMethod { get; private set; }

	    public string TestAssemblyLocation
	    {
	        get { return _testAssemblyLocation; }
	        set
	        {
	            _testAssemblyLocation = value;
	            _testAssembly = AssemblyDefinition.ReadAssembly(_testAssemblyLocation);
	        }
	    }

	    public void Run()
		{
		    var key = Registry.LocalMachine.OpenSubKey(ERROR_REPORTING_KEY, RegistryKeyPermissionCheck.ReadWriteSubTree);
            var errorReportingValue = key.GetValue(ERROR_REPORTING_VALUE, null);
            key.SetValue(ERROR_REPORTING_VALUE, 1, RegistryValueKind.DWord);
		    key.Close();

			MethodDefinition method = ValidateMethod();
            _module.LoadDebugInformation();

		    _comparer = new MethodReferenceComparer();
            var matchingMethods = new List<MethodReference>();
            AddMethod(method, matchingMethods);

            int[] originalOffsets = method.Body.Instructions.Select(i => i.Offset).ToArray();
		    _report = new MutationTestingReport();
            _testsToRun = GetMatchingTestsFromTree(method, matchingMethods);

		    Console.WriteLine(
                "Suite of {0} tests identified for {1}.{2}",
                _testsToRun.Count(),
                TargetType.FullName,
                TargetMethod);

            //_benchmark = BenchmarkTestSuite();

			int count = 0;
			int failures = 0;
			if (_mutationsToApply.Count == 0) PopulateDefaultTurtles();
			foreach (var turtleType in _mutationsToApply)
			{
                var turtle = (MethodTurtleBase)Activator.CreateInstance(turtleType);
                Console.WriteLine(turtle.Description);
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

        private void AddMethod(MethodDefinition targetMethod, List<MethodReference> matchingMethods)
        {
            if (matchingMethods.Contains(targetMethod, _comparer)) return;
            matchingMethods.Add(targetMethod);
            AddCallingMethods(targetMethod, matchingMethods);
            var declaringType = targetMethod.DeclaringType;
            TypeDefinition type = declaringType;
            while (type != null && type.BaseType != null)
            {
                var thisModule = type.BaseType.Module;
                if (type.BaseType.Scope.Name != type.BaseType.Module.Assembly.Name.Name
                    && type.BaseType.Scope is AssemblyNameReference)
                {
                    thisModule =
                        type.BaseType.Module.AssemblyResolver.Resolve((AssemblyNameReference)type.BaseType.Scope).
                            MainModule;
                }
                type = thisModule.Types.SingleOrDefault(t => t.FullName == type.BaseType.FullName);
                if (type != null)
                {
                    var baseMethod = type.Methods
                        .SingleOrDefault(m => MethodsMatch(m, targetMethod));
                    if (baseMethod != null)
                    {
                        AddMethod(baseMethod, matchingMethods);
                        break;
                    }
                    AddMethodsForInterfaces(targetMethod, matchingMethods, type);
                }
            }
            AddMethodsForInterfaces(targetMethod, matchingMethods, declaringType);
        }

	    private void AddMethodsForInterfaces(MethodDefinition targetMethod, List<MethodReference> matchingMethods, TypeDefinition type)
	    {
	        foreach (var interfaceReference in type.Interfaces)
	        {
	            var thisModule = type.Module;
	            if (interfaceReference.Scope.Name != type.Module.Assembly.Name.Name
	                && interfaceReference.Scope is AssemblyNameReference)
	            {
	                thisModule =
	                    interfaceReference.Module.AssemblyResolver.Resolve((AssemblyNameReference)interfaceReference.Scope).
	                        MainModule;
	            }
	            var interfaceDefinition =
	                thisModule.Types.SingleOrDefault(t => t.FullName == interfaceReference.FullName);
	            if (interfaceDefinition != null)
	            {
	                var interfaceMethod = interfaceDefinition.Methods
	                    .SingleOrDefault(m => MethodsMatch(m, targetMethod));
	                if (interfaceMethod != null)
	                {
	                    AddMethod(interfaceMethod, matchingMethods);
	                }
	            }
	        }
	    }

	    private bool MethodsMatch(MethodDefinition first, MethodDefinition second)
        {
            return first.Name == second.Name
                   && Enumerable.SequenceEqual(first.Parameters.Select(p => p.ParameterType.Name),
                                               second.Parameters.Select(p => p.ParameterType.Name))
                   && first.GenericParameters.Count == second.GenericParameters.Count;
        }

        private ISet<string> GetMatchingTestsFromTree(MethodDefinition targetmethod, IList<MethodReference> matchingMethods, bool force = false)
        {
            ISet<string> result = new HashSet<string>();
            foreach (var type in _testAssembly.MainModule.Types)
            AddTestsForType(targetmethod, matchingMethods, force, type, result);
            if (!force && result.Count == 0)
            {
                result = GetMatchingTestsFromTree(targetmethod, matchingMethods, true);
            }
            if (result.Count == 0)
            {
                throw new MutationTestFailureException(
                    "No matching tests were found to run.");
            }
            return result;
	    }

	    private void AddTestsForType(MethodDefinition targetmethod, IList<MethodReference> matchingMethods, bool force, TypeDefinition type,
	                                 ISet<string> result)
	    {
	        foreach (var method in type.Methods.Where(m => m.HasBody))
	        {
	            var targetType = targetmethod.DeclaringType.FullName;
	            if (!force && !DoesMethodReferenceType(method, targetType)) continue;
	            foreach (var instruction in method.Body.Instructions)
	            {
	                if (instruction.OpCode == OpCodes.Call
	                    || instruction.OpCode == OpCodes.Callvirt
	                    || instruction.OpCode == OpCodes.Newobj
	                    || instruction.OpCode == OpCodes.Ldftn)
	                {
	                    var reference = (MethodReference)instruction.Operand;
	                    if (matchingMethods.Any(m => _comparer.Equals(m, reference))
	                        && !method.CustomAttributes.Any(a => a.AttributeType.Name == "MutationTestAttribute"))
	                    {
	                        result.Add(string.Format("{0}.{1}", type.FullName.Replace("/", "+"), method.Name));
	                        break;
	                    }
	                }
	            }
	        }
            if (type.NestedTypes != null)
            {
                foreach (var typeDefinition in type.NestedTypes)
                {
                    AddTestsForType(targetmethod, matchingMethods, force, typeDefinition, result);
                }
            }
	    }

	    private static bool DoesMethodReferenceType(MethodDefinition method, string targetType)
	    {
	        bool typeUsed = false;
	        foreach (var instruction in method.Body.Instructions)
	        {
	            if (instruction.OpCode == OpCodes.Call
	                || instruction.OpCode == OpCodes.Callvirt
	                || instruction.OpCode == OpCodes.Newobj
	                || instruction.OpCode == OpCodes.Ldftn)
	            {
	                var reference = (MethodReference)instruction.Operand;
	                var declaringType = reference.DeclaringType;
	                if (declaringType.FullName == targetType)
	                {
	                    typeUsed = true;
	                    break;
	                }
	                var genericType = declaringType as GenericInstanceType;
	                if (genericType != null)
	                {
	                    if (genericType.GenericArguments.Any(a => a.FullName == targetType))
	                    {
	                        typeUsed = true;
	                        break;
	                    }
	                }
	                var genericMethod = reference as GenericInstanceMethod;
	                if (genericMethod != null)
	                {
	                    if (genericMethod.GenericArguments.Any(a => a.FullName == targetType))
	                    {
	                        typeUsed = true;
	                        break;
	                    }
	                }
	            }
	            if (instruction.OpCode == OpCodes.Newobj)
	            {
	                var constructor = (MemberReference)instruction.Operand;
	                var declaringType = constructor.DeclaringType;
	                if (declaringType.FullName == targetType)
	                {
	                    typeUsed = true;
	                    break;
	                }
	            }
	        }
	        return typeUsed;
	    }

	    private void AddCallingMethods(MethodReference targetMethod, List<MethodReference> matchingMethods)
        {
            foreach (var type in _module.Definition.Types)
            AddCallingMethodsForType(targetMethod, matchingMethods, type);
        }

	    private void AddCallingMethodsForType(MethodReference targetMethod, List<MethodReference> matchingMethods, TypeDefinition type)
	    {
	        foreach (var method in type.Methods.Where(m => m.HasBody))
	        foreach (var instruction in method.Body.Instructions)
	        {
	            if (instruction.OpCode == OpCodes.Call
	                || instruction.OpCode == OpCodes.Callvirt
                    || instruction.OpCode == OpCodes.Newobj
                    || instruction.OpCode == OpCodes.Ldftn)
	            {
	                var reference = (MethodReference)instruction.Operand;
	                if (_comparer.Equals(reference, targetMethod)
	                    && !matchingMethods.Contains(method, _comparer))
	                {
                        AddMethod(method, matchingMethods);
                        var match = _automaticallyGeneratedNestedClassMatcher.Match(type.Name);
                        if (match.Success && type.DeclaringType != null)
                        {
                            if (type.DeclaringType.Methods.Any(m => m.Name == match.Groups[1].Value))
                            {
                                AddMethod(type.DeclaringType.Methods.First(m => m.Name == match.Groups[1].Value), matchingMethods);
                            }
                        }
	                }
	            }
	        }
            foreach (var nestedType in type.NestedTypes)
            {
                AddCallingMethodsForType(targetMethod, matchingMethods, nestedType);
            }
	    }

	    private class MethodReferenceComparer : IEqualityComparer<MethodReference>
        {
            public bool Equals(MethodReference x, MethodReference y)
            {
                if (x.Name != y.Name) return false;
                        return x.DeclaringType.FullName == y.DeclaringType.FullName
                        && Enumerable.SequenceEqual(x.Parameters.Select(p => p.ParameterType.Name),
                                                 y.Parameters.Select(p => p.ParameterType.Name))
                        && x.GenericParameters.Count == y.GenericParameters.Count;
            }

            public int GetHashCode(MethodReference obj)
            {
                return obj.Name.GetHashCode();
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

//        private TimeSpan BenchmarkTestSuite()
//        {
//            var testDirectory = new TestDirectory(Path.GetDirectoryName(_testAssemblyLocation));
//
//            var process = GetTestRunnerProcess(testDirectory);
//
//            var stopwatch = new Stopwatch();
//            stopwatch.Start();
//            process.Start();
//            process.WaitForExit();
//            stopwatch.Stop();
//
//            testDirectory.Dispose();
//
//            if (process.ExitCode != 0)
//            {
//                throw new MutationTestFailureException("Test suite does not pass with unmutated code - mutation testing aborted.");
//            }
//            var timespan = stopwatch.Elapsed;
//            Console.WriteLine("Test suite benchmarked at {0:0.000} seconds.", timespan.TotalSeconds);
//
//            return timespan;
//        }

	    private Process GetTestRunnerProcess(TestDirectory testDirectory)
	    {
	        if (_runner == null) _runner = (ITestRunner)Activator.CreateInstance(MutationTestBuilder.TestRunner);
	        return _runner.GetRunnerProcess(testDirectory, TestAssemblyLocation, _testsToRun);
	    }

	    private bool CheckTestProcessFails(MethodTurtleBase turtle, MutantMetaData mutation)
		{
            var process = GetTestRunnerProcess(mutation.TestDirectory);

            process.Start();
	        bool exitedInTime = process.WaitForExit(30000); //Math.Min(30000, (int)(5 * _benchmark.TotalMilliseconds)));
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
            
            string result = string.Format(" Mutant: {0}. {1}",
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

	    private MethodDefinition ValidateMethod()
	    {
            _module = new Module(TargetType.Assembly.Location);

            var type = ResolveFromTypeCollection(_module.Definition.Types);
		    return MethodDefinitionResolver.ResolveMethod(type, TargetMethod, _parameterTypes);
		}

	    private TypeDefinition ResolveFromTypeCollection(Collection<TypeDefinition> types)
	    {
	        var type = types.SingleOrDefault(t => t.FullName == TargetType.FullName.Replace("+", "/"));
            if (type == null)
            {
                foreach (var typeDefinition in types)
                {
                    if (typeDefinition.NestedTypes != null)
                    {
                        type = ResolveFromTypeCollection(typeDefinition.NestedTypes);
                        if (type != null) return type;
                    }
                }
            }
	        return type;
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

