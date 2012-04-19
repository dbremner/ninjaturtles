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

namespace NinjaTurtles.Fluent
{
    internal class MutationTest : IMutationTest
    {
        private readonly AssemblyDefinition _assembly;
        private readonly IDictionary<Type, string> _expectedInvariantMethodMutators = new Dictionary<Type, string>();
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
                bool thisTurtleAllFailed = true;
                var turtle = (IMethodTurtle)Activator.CreateInstance(methodTurtle);
                Console.WriteLine(turtle.Description);
                bool isExpectedInvariant = _expectedInvariantMethodMutators.ContainsKey(methodTurtle);
                if (isExpectedInvariant)
                {
                    Console.WriteLine("*** This mutation is expected to be invariant! ***");
                    if (!string.IsNullOrEmpty(_expectedInvariantMethodMutators[methodTurtle]))
                    {
                        Console.WriteLine("*** Reason: {0} ***", _expectedInvariantMethodMutators[methodTurtle]);
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
                                thisTurtleAllFailed &= (result != 0) ^ isExpectedInvariant;
                            }
                        }
                        if (!mutationsFound)
                        {
                            Console.WriteLine("\tNo valid mutations found (this is fine)");
                        }
                    }
                }
                allFailed &= thisTurtleAllFailed;
            }

            if (!allFailed) throw new MutationTestFailureException();
        }

        public IMutationTest With<T>() where T : IMethodTurtle
        {
            _methodTurtles.Add(typeof(T));
            return this;
        }

        public IMutationTest ExpectedInvariantFor<T>() where T : IMethodTurtle
        {
            return ExpectedInvariantFor<T>(string.Empty);
        }

        public IMutationTest ExpectedInvariantFor<T>(string reason) where T : IMethodTurtle
        {
            if (!_expectedInvariantMethodMutators.ContainsKey(typeof(T)))
            {
                _expectedInvariantMethodMutators.Add(typeof(T), reason);
            }
            else
            {
                _expectedInvariantMethodMutators[typeof(T)] = reason;
            }
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
                    Console.WriteLine("Passed (this is {0})", isExpectedInvariant ? "good" : "bad");
                    break;
                case -1:
                    Console.WriteLine("No valid tests found to run");
                    break;
                default:
                    Console.WriteLine("Failed (this is {0})", isExpectedInvariant ? "bad" : "good");
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