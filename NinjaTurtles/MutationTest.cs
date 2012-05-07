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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Mono.Cecil;

using NinjaTurtles.Turtles;

namespace NinjaTurtles
{
	internal class MutationTest : IMutationTest
	{
		private IList<Type> _mutationsToApply = new List<Type>();
		private string _testAssemblyLocation;
		private AssemblyDefinition _testAssembly;
		private TypeReference _targetTypeReference;
		private AssemblyDefinition _assembly;
		private string _testList;
		
		internal MutationTest(string testAssemblyLocation, Type targetType, string targetMethod)
		{
			TargetType = targetType;
			TargetMethod = targetMethod;
			_testAssemblyLocation = testAssemblyLocation;
			_testAssembly = AssemblyDefinition.ReadAssembly(testAssemblyLocation);
			_targetTypeReference = _testAssembly.MainModule.Import(targetType);
		}
		
		public Type TargetType { get; private set; }

		public string TargetMethod { get; private set; }
		
		public void Run()
		{
			MethodDefinition method = ValidateMethod();
			IList<string> tests = GetMatchingTestsOrFail(method);
			_testList = Path.GetTempFileName();
			File.WriteAllLines(_testList, tests);
			int count = 0;
			int failures = 0;
			if (_mutationsToApply.Count == 0) PopulateDefaultTurtles();
			foreach (var turtleType in _mutationsToApply)
			{
				var turtle = (IMethodTurtle)Activator.CreateInstance(turtleType);
				Parallel.ForEach(turtle.Mutate(method, _assembly, _testAssemblyLocation),
				                 mutation => RunMutation(turtle, mutation, tests, ref failures, ref count));
			}
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
		
		private void RunMutation(IMethodTurtle turtle, MutationTestMetaData mutation, IList<string> tests, ref int failures, ref int count)
		{
			bool testProcessFailed = CheckTestProcessFails(turtle, mutation, tests);
			if (!testProcessFailed)
			{
				Interlocked.Increment(ref failures);
			}
			Interlocked.Increment(ref count);
		}
		
		private bool CheckTestProcessFails(IMethodTurtle turtle, MutationTestMetaData mutation, IList<string> tests)
		{
			string testAssemblyLocation = Path.Combine(mutation.TestDirectoryName, Path.GetFileName(_testAssemblyLocation));
			
			var processStartInfo = new ProcessStartInfo("mono",
			                                   "--runtime=4.0 /Users/david/Projects/nt2/packages/NUnit.Runners.2.6.0.12051/tools/nunit-console.exe \"" +
			                                   testAssemblyLocation + "\" -runlist=\"" + _testList + "\" -nologo -nodots");
			
			var process = new Process {
				StartInfo = processStartInfo
			};

			process.Start();
			process.WaitForExit();
			turtle.MutantComplete(mutation);
			
			int exitCode = process.ExitCode;
			
			Console.WriteLine("Mutant: {0}. {1}",
			                  mutation.Description,
			                  exitCode == 0
			                  	? "Survived."
			                    : "Killed.");
				
			return exitCode != 0;
		}
				
		private void PopulateDefaultTurtles()
		{
			foreach (var type in GetType().Assembly.GetTypes()
				.Where(t => t.GetInterface("IMethodTurtle") != null))
			{
				_mutationsToApply.Add(type);
			}
		}
		
		private IList<string> GetMatchingTestsOrFail(MethodDefinition targetMethod)
		{
			var tests = new List<string>();
			foreach (var type in _testAssembly.MainModule.Types)
			{
				foreach (var method in type.Methods)
				{
					if (method.CustomAttributes
							.Any(a => a.AttributeType.Name == "MethodTestedAttribute"
						     	&& ((a.ConstructorArguments[0].Value is String
					    && (string)a.ConstructorArguments[0].Value
					     == _targetTypeReference.FullName)
					    || (a.ConstructorArguments[0].Value is TypeReference
						&& ((TypeReference)a.ConstructorArguments[0].Value).FullName
						     		== _targetTypeReference.FullName))
						     	&& ((string)a.ConstructorArguments[1].Value)
					     			== targetMethod.Name))
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
		
		private MethodDefinition ValidateMethod()
		{
			try
			{
				_assembly = AssemblyDefinition.ReadAssembly(TargetType.Assembly.Location);
				var type = _assembly.MainModule.Types
					.Single(t => t.FullName == TargetType.FullName);
				var method = type.Methods
					.First(m => m.Name == TargetMethod);
				return method;
			}
			catch (Exception)
			{
				throw new MutationTestFailureException(
					string.Format("Method '{0}' was not recognised.", TargetMethod));
			}
		}

		public NinjaTurtles.IMutationTest With<T>() where T : IMethodTurtle
		{
			_mutationsToApply.Add(typeof(T));
			return this;
		}
	}
}

