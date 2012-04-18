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

using NinjaTurtles.Mutators;
using NinjaTurtles.TestRunner;

namespace NinjaTurtles.Fluent
{
    internal class MutationTest : IMutationTest
    {
        private readonly AssemblyDefinition _assembly;
        private readonly ISet<Type> _expectedInvariantMethodMutators = new HashSet<Type>();
        private readonly ISet<Type> _methodMutators = new HashSet<Type>();
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
            if (_methodMutators.Count == 0)
            {
                PopulateDefaultMutators();
            }
            bool allFailed = true;
            foreach (Type methodMutator in _methodMutators)
            {
                bool thisMutatorAllFailed = true;
                var mutator = (IMethodMutator)Activator.CreateInstance(methodMutator);
                Console.WriteLine(mutator.Description);
                bool isExpectedInvariant = _expectedInvariantMethodMutators.Contains(methodMutator);
                if (isExpectedInvariant)
                {
                    Console.WriteLine("*** This mutation is expected to be invariant! ***");
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
                        foreach (string mutation in mutator.Mutate(method, _assembly, fileName))
                        {
                            mutationsFound = true;
                            Console.Write("\t{0}: ", mutation);
                            int result = runner.RunTestsWithMutations(method, fileName, _testAssemblyLocation);
                            OutputResultToConsole(isExpectedInvariant, result);
                            if (result != -1)
                            {
                                thisMutatorAllFailed &= result != 0 ^ !isExpectedInvariant;
                            }
                        }
                        if (!mutationsFound)
                        {
                            Console.WriteLine("\tNo valid mutations found (this is fine)");
                        }
                    }
                }
                allFailed &= thisMutatorAllFailed;
            }

            if (allFailed) throw new MutationTestFailureException();
        }

        public IMutationTest With<T>() where T : IMethodMutator
        {
            _methodMutators.Add(typeof(T));
            return this;
        }

        public IMutationTest ExpectedInvariantFor<T>() where T : IMethodMutator
        {
            _expectedInvariantMethodMutators.Add(typeof(T));
            return this;
        }

        public IMutationTest CombineWith<T>() where T : IMethodMutator
        {
            PopulateDefaultMutators();
            _methodMutators.Add(typeof(T));
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

        private void PopulateDefaultMutators()
        {
            foreach (Type type in GetType().Assembly.GetTypes()
                .Where(t => !t.IsAbstract)
                .Where(t => t.GetInterface(typeof(IMethodMutator).Name) != null))
            {
                _methodMutators.Add(type);
            }
        }
    }
}