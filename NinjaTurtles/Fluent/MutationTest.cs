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
using System.Linq;

using Mono.Cecil;

using NinjaTurtles.TestRunner;
using NinjaTurtles.Turtles;
using NinjaTurtles.Utilities;

namespace NinjaTurtles.Fluent
{
    internal class MutationTest : IMutationTest
    {
        private readonly AssemblyDefinition _assembly;
        private readonly IDictionary<Type, Tuple<int, string>> _expectedInvariantMethodMutators = new Dictionary<Type, Tuple<int, string>>();
        private readonly ISet<Type> _methodTurtles = new HashSet<Type>();
        private readonly string _methodName;
        private readonly Type _targetClass;
        private readonly string _testAssemblyLocation;
        private Type _testRunner = typeof(NUnitTestRunner);

        internal MutationTest(Type targetClass, string methodName, string testAssemblyLocation)
        {
            _targetClass = targetClass;
            _methodName = methodName;
            _testAssemblyLocation = testAssemblyLocation;
            _assembly = AssemblyDefinition.ReadAssembly(targetClass.Assembly.Location);
        }

        #region IMutationTest Members

        public void Run()
        {
            var runner = (ITestRunner)Activator.CreateInstance(_testRunner);
            string fileName = _targetClass.Assembly.Location;
            if (_methodTurtles.Count == 0)
            {
                PopulateDefaultTurtles();
            }
            bool allFailed = true;
            foreach (Type methodTurtle in _methodTurtles)
            {
                var turtle = (IMethodTurtle)Activator.CreateInstance(methodTurtle);
                Console.WriteLine(turtle.Description);
                int passCount = 0;
                int failCount = 0;
                int expectedPassCount = 0;
                bool isExpectedInvariant = _expectedInvariantMethodMutators.ContainsKey(methodTurtle);
                if (isExpectedInvariant)
                {
                    expectedPassCount = _expectedInvariantMethodMutators[methodTurtle].Item1;
                    Console.WriteLine("*** This mutation is expected to be invariant in {0} cases! ***",
                        expectedPassCount);
                    if (!string.IsNullOrEmpty(_expectedInvariantMethodMutators[methodTurtle].Item2))
                    {
                        Console.WriteLine("*** Reason: {0} ***", _expectedInvariantMethodMutators[methodTurtle].Item2);
                    }
                }

                foreach (TypeDefinition type in _assembly.MainModule.Types.Where(t => t.Name == _targetClass.Name))
                {
                    if (!type.Methods.Any(m => m.Name == _methodName))
                    {
                        throw new MutationTestFailureException(
                            string.Format("Unknown method '{0}'", _methodName));
                    }
                    foreach (MethodDefinition method in type.Methods.Where(m => m.Name == _methodName))
                    {
                        bool mutationsFound = false;
                        foreach (string mutation in turtle.Mutate(method, _assembly, fileName))
                        {
                            mutationsFound = true;
                            Console.Write("\t{0}: ", mutation);
                            int result = runner.RunTestsWithMutations(method, fileName, _testAssemblyLocation);
                            OutputResultToConsole(isExpectedInvariant, result);
                            if (result != -1)
                            {
                                if (result == 0)
                                {
                                    passCount++;
                                }
                                else
                                {
                                    failCount++;
                                }
                            }
                        }
                        if (!mutationsFound)
                        {
                            Console.WriteLine("\tNo valid mutations found (this is fine)");
                        }
                    }
                }
                allFailed &= (passCount == expectedPassCount);
            }

            if (!allFailed) throw new MutationTestFailureException();
        }

        public IMutationTest With<T>() where T : IMethodTurtle
        {
            _methodTurtles.Add(typeof(T));
            return this;
        }

        public IMutationTest ExpectedInvariantCasesFor<T>(params int[] interchangeableParameterSetSizes) where T : IMethodTurtle
        {
            return ExpectedInvariantCasesFor<T>(string.Empty, interchangeableParameterSetSizes);
        }

        public IMutationTest ExpectedInvariantCasesFor<T>(string reason, params int[] interchangeableParameterSetSizes) where T : IMethodTurtle
        {
            if (interchangeableParameterSetSizes.Length == 0)
            {
                throw new ArgumentException("You must specify at least one set of interchangeable parameters for this method to work.");
            }
            _expectedInvariantMethodMutators[typeof(T)] = new Tuple<int, string>(interchangeableParameterSetSizes.Select(n => n.Fact() - 1).Sum(), reason);
            return this;
        }

        public IMutationTest CombineWith<T>() where T : IMethodTurtle
        {
            PopulateDefaultTurtles();
            _methodTurtles.Add(typeof(T));
            return this;
        }

        public IMutationTest UsingRunner<T>() where T : ITestRunner
        {
            _testRunner = typeof(T);
            return this;
        }

        #endregion

        private static void OutputResultToConsole(bool isExpectedInvariant, int result)
        {
            switch (result)
            {
                case 0:
                    Console.WriteLine("Passed (this {0})", isExpectedInvariant ? "might be OK" : "is bad");
                    break;
                case -1:
                    Console.WriteLine("No valid tests found to run");
                    break;
                default:
                    Console.WriteLine("Failed (this {0})", isExpectedInvariant ? "might be OK" : "is good");
                    break;
            }
        }

        private void PopulateDefaultTurtles()
        {
            foreach (Type type in GetType().Assembly.GetTypes()
                .Where(t => !t.IsAbstract)
                .Where(t => t.GetInterface(typeof(IMethodTurtle).Name) != null))
            {
                _methodTurtles.Add(type);
            }
        }
    }
}