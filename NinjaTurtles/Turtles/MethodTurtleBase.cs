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
using System.Globalization;
using System.IO;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace NinjaTurtles.Turtles
{
    /// <summary>
    /// An abstract base class for implementations of
    /// <see cref="IMethodTurtle" />.
    /// </summary>
    public abstract class MethodTurtleBase : IMethodTurtle
    {
        private int[] _originalOffsets;
        private Module _module;
        private MethodDefinition _method;

        internal void MutantComplete(MutantMetaData metaData)
        {
            metaData.TestDirectory.Dispose();
        }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}" /> of detailed descriptions
        /// of mutations, having first carried out the mutation in question and
        /// saved the modified assembly under test to disk.
        /// </summary>
        /// <param name="method">
        /// A <see cref="MethodDefinition" /> for the method on which mutation
        /// testing is to be carried out.
        /// </param>
        /// <param name="module">
        /// A <see cref="Module" /> representing the main module of the
        /// containing assembly.
        /// </param>
        /// <param name="originalOffsets">
        /// An array of the original IL offsets before macros were expanded.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}" /> of
        /// <see cref="MutantMetaData" /> structures.
        /// </returns>
        public IEnumerable<MutantMetaData> Mutate(MethodDefinition method, Module module, int[] originalOffsets)
        {
            _module = module;
            _method = method;
            _originalOffsets = originalOffsets;
            method.Body.SimplifyMacros();
            foreach (var mutation in DoMutate(method, module))
            {
                yield return mutation;
            }
            if (method.ReturnType.Name == "IEnumerable`1")
            {
                var nestedType =
                    method.DeclaringType.NestedTypes.FirstOrDefault(
                        t => t.Name.StartsWith(string.Format("<{0}>", method.Name))
                        && t.Interfaces.Any(i => i.Name == "IEnumerable`1"));
                if (nestedType != null)
                {
                    var nestedMethod = nestedType.Methods.FirstOrDefault(m => m.Name == "MoveNext");
                    if (nestedMethod != null)
                    {
                        _originalOffsets = nestedMethod.Body.Instructions.Select(i => i.Offset).ToArray();
                        _method = nestedMethod;
                        nestedMethod.Body.SimplifyMacros();
                        foreach (var mutation in DoMutate(nestedMethod, module))
                        {
                            yield return mutation;
                        }
                    }
                }
            }
            method.Body.OptimizeMacros();
        }

        /// <summary>
        /// Performs the actual code mutations, returning each with
        /// <c>yield</c> for the calling code to use.
        /// </summary>
        /// <remarks>
        /// Implementing classes should yield the result obtained by calling
        /// the <see mref="DoYield" /> method.
        /// </remarks>
        /// <param name="method">
        /// A <see cref="MethodDefinition" /> for the method on which mutation
        /// testing is to be carried out.
        /// </param>
        /// <param name="module">
        /// A <see cref="Module" /> representing the main module of the
        /// containing assembly.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}" /> of
        /// <see cref="MutantMetaData" /> structures.
        /// </returns>
        protected abstract IEnumerable<MutantMetaData> DoMutate(MethodDefinition method, Module module);

        /// <summary>
        /// A helper method that copies the test folder, and saves the mutated
        /// assembly under test into it before returning an instance of
        /// <see cref="MutantMetaData" />.
        /// </summary>
        /// <param name="method">
        /// A <see cref="MethodDefinition" /> for the method on which mutation
        /// testing is to be carried out.
        /// </param>
        /// <param name="module">
        /// A <see cref="Module" /> representing the main module of the
        /// containing assembly.
        /// </param>
        /// <param name="description">
        /// A description of the mutation that has been applied.
        /// </param>
        /// <param name="index">
        /// The index of the (first) IL instruction at which the mutation was
        /// applied.
        /// </param>
        /// <returns></returns>
        protected MutantMetaData DoYield(MethodDefinition method, Module module, string description, int index)
        {
            var testDirectory = new TestDirectory(Path.GetDirectoryName(module.AssemblyLocation));
            testDirectory.SaveAssembly(module);
            return new MutantMetaData
            {
                Description = description,
                MethodDefinition = method,
                TestDirectory = testDirectory,
                ILIndex = index
            };
        }

        internal int GetOriginalOffset(int index)
        {
            return _originalOffsets[index];
        }

        internal string GetOriginalSourceFileName(int index)
        {
            var sequencePoint = GetCurrentSequencePoint(index);
            return Path.GetFileName(sequencePoint.Document.Url);
        }

        internal SequencePoint GetCurrentSequencePoint(int index)
        {
            var instruction = _method.Body.Instructions[index];
            while ((instruction.SequencePoint == null
                || instruction.SequencePoint.StartLine == 0xfeefee) && index > 0)
            {
                index--;
                instruction = _method.Body.Instructions[index];
            }
            var sequencePoint = instruction.SequencePoint;
            return sequencePoint;
        }

        internal string GetOriginalSourceCode(int index)
        {
            var sequencePoint = GetCurrentSequencePoint(index);
            string result = "";
            string[] sourceCode = _module.SourceFiles[sequencePoint.Document.Url];
            int upperBound = Math.Min(sequencePoint.EndLine + 2, sourceCode.Length);
            for (int line = Math.Max(sequencePoint.StartLine - 2, 1); line <= upperBound; line++)
            {
                string sourceLine = sourceCode[line - 1].Replace("\t", "    ");
                result += line.ToString(CultureInfo.InvariantCulture)
                    .PadLeft(4, ' ') + ": " + sourceLine.TrimEnd(' ', '\t');
                if (line < upperBound) result += Environment.NewLine;
            }
            return result;
        }
    }
}
