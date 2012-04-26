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

using NinjaTurtles.TestRunner;
using NinjaTurtles.Turtles;
using NinjaTurtles.Turtles.Method;

namespace NinjaTurtles
{
    internal class MutationTest : IMutationTest
    {
        private readonly AssemblyDefinition _assembly;
        private readonly ISet<Type> _methodTurtles = new HashSet<Type>();
        private readonly string _methodName;
        private readonly Type _targetClass;
        private readonly string _testAssemblyLocation;

        internal MutationTest(Type targetClass, string methodName, string testAssemblyLocation)
        {
            _targetClass = targetClass;
            _methodName = methodName;
            _testAssemblyLocation = testAssemblyLocation;
            _assembly = AssemblyDefinition.ReadAssembly(targetClass.Assembly.Location);
            TestRunner = typeof(NUnitTestRunner);
        }

        internal Type TestRunner { get; set; }

        #region IMutationTest Members

        public void Run()
        {
            using (var runner = (ITestRunner)Activator.CreateInstance(TestRunner))
            {
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
                    foreach (TypeDefinition type in _assembly.MainModule.Types.Where(t => t.Name == _targetClass.Name))
                    {
                        if (type.Methods.All(m => m.Name != _methodName))
                        {
                            throw new MutationTestFailureException(
                                string.Format("Unknown method '{0}'", _methodName));
                        }
                        foreach (MethodDefinition method in type.Methods.Where(m => m.Name == _methodName))
                        {
                            bool mutationsFound = false;
                            var parallelOptions = new ParallelOptions();
                            //parallelOptions.MaxDegreeOfParallelism = 1;
                            Parallel.ForEach(turtle.Mutate(method, _assembly, fileName),
                                             parallelOptions,
                                             mutation =>
                                                 {
                                                     mutationsFound = true;
                                                     string testAssembly = Path.Combine(mutation.TestFolder,
                                                                                        Path.GetFileName(
                                                                                            _testAssemblyLocation));
                                                     bool? result = runner.RunTestsWithMutations(method, testAssembly);
                                                     OutputResultToConsole(mutation.Description, result);
                                                     if (result ?? false) Interlocked.Increment(ref passCount);
                                                     mutation.Dispose();
                                                 });
                            if (!mutationsFound)
                            {
                                Console.WriteLine("\tNo valid mutations found (this is fine)");
                            }
                        }
                    }
                    allFailed &= (passCount == 0);
                }

                if (!allFailed) throw new MutationTestFailureException();
            }
        }

        public IMutationTest With<T>() where T : ITurtle
        {
            _methodTurtles.Add(typeof(T));
            return this;
        }

        public IMutationTest UsingRunner<T>() where T : ITestRunner
        {
            TestRunner = typeof(T);
            return this;
        }

        #endregion

        private static void OutputResultToConsole(string description, bool? result)
        {
            string interpretation;
            if (!result.HasValue)
            {
                interpretation = "No valid tests found to run";
            }
            else if (result.Value)
            {
                interpretation = "Passed (this is bad)";
            }
            else
            {
                interpretation = "Failed (this is good)";
            }
            Console.WriteLine("\t{0}: {1}", description, interpretation);
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