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
		private readonly IList<Type> _mutationsToApply = new List<Type>();
		private readonly string _testAssemblyLocation;
	    private readonly Type[] _parameterTypes;
	    private readonly AssemblyDefinition _testAssembly;
		private readonly TypeReference _targetTypeReference;
		private AssemblyDefinition _assembly;
		private string _testList;
		
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
			MethodDefinition method = ValidateMethod();
			IEnumerable<string> tests = GetMatchingTestsOrFail(method);
			_testList = Path.GetTempFileName();
			File.WriteAllLines(_testList, tests);
			int count = 0;
			int failures = 0;
			if (_mutationsToApply.Count == 0) PopulateDefaultTurtles();
			foreach (var turtleType in _mutationsToApply)
			{
				var turtle = (IMethodTurtle)Activator.CreateInstance(turtleType);
				Parallel.ForEach(turtle.Mutate(method, _assembly, _testAssemblyLocation),
				                 mutation => RunMutation(turtle, mutation, ref failures, ref count));
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
		
		private void RunMutation(IMethodTurtle turtle, MutationTestMetaData mutation, ref int failures, ref int count)
		{
			bool testProcessFailed = CheckTestProcessFails(turtle, mutation);
			if (!testProcessFailed)
			{
				Interlocked.Increment(ref failures);
			}
			Interlocked.Increment(ref count);
		}
		
		private bool CheckTestProcessFails(IMethodTurtle turtle, MutationTestMetaData mutation)
		{
			string testAssemblyLocation = Path.Combine(mutation.TestDirectoryName, Path.GetFileName(_testAssemblyLocation));

		    string arguments = string.Format("\"{0}\" {{0}}runlist=\"{1}\" {{0}}nologo {{0}}nodots", testAssemblyLocation, _testList);

            var process = ConsoleProcessFactory.CreateProcess("nunit-console.exe", arguments);

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
                && !Enumerable.SequenceEqual(_parameterTypes, (Type[])attribute.Properties.Single(p => p.Name == "ParameterTypes").Argument.Value))
            {
                return false;
            }
            return true;
        }

	    private MethodDefinition ValidateMethod()
		{
		    _assembly = AssemblyDefinition.ReadAssembly(TargetType.Assembly.Location);
		    var type = _assembly.MainModule.Types
		        .Single(t => t.FullName == TargetType.FullName);
		    var method = MethodDefinitionResolver.ResolveMethod(type, TargetMethod, _parameterTypes);
		    if (method == null)
		    {
		        throw new MutationTestFailureException(
		            string.Format("Method '{0}' was not recognised.", TargetMethod));
		    }
		    return method;
		}

	    public IMutationTest With<T>() where T : IMethodTurtle
		{
			_mutationsToApply.Add(typeof(T));
			return this;
		}
	}
}

