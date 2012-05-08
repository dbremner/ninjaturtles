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
using System.Text.RegularExpressions;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;
using Mono.Cecil.Rocks;

namespace NinjaTurtles.Turtles
{
    public abstract class MethodTurtleBase : IMethodTurtle
    {
        private int[] _originalOffsets;
        private IDictionary<string, string[]> _sourceFiles = new Dictionary<string, string[]>();
        private AssemblyDefinition _assembly;
        private MethodDefinition _method;

        public void MutantComplete(MutationTestMetaData metaData)
        {
            metaData.TestDirectory.Dispose();
        }

        public IEnumerable<MutationTestMetaData> Mutate(MethodDefinition method, AssemblyDefinition assembly, string assemblyLocation)
        {
            _assembly = assembly;
            _method = method;
            _originalOffsets = method.Body.Instructions.Select(i => i.Offset).ToArray();
            LoadDebugSymbolsAndSourceCode(assemblyLocation);
            method.Body.SimplifyMacros();
            foreach (var mutation in DoMutate(method, assembly, assemblyLocation))
            {
                yield return mutation;
            }
        }

        private void LoadDebugSymbolsAndSourceCode(string assemblyLocation)
        {
            string pdbLocation = Path.ChangeExtension(assemblyLocation, "pdb");
            string mdbLocation = Path.ChangeExtension(assemblyLocation, "mdb");
            ISymbolReaderProvider provider = null;
            if (File.Exists(pdbLocation))
            {
                provider = new PdbReaderProvider();
            }
            else if (File.Exists(mdbLocation))
            {
                provider = new MdbReaderProvider();
            }
            if (provider == null) return;
            var reader = provider.GetSymbolReader(_assembly.MainModule, pdbLocation);
            _assembly.MainModule.ReadSymbols(reader);
            reader.Read(_method.Body, o => _method.Body.Instructions.FirstOrDefault(i => i.Offset == o));
            var sourceFiles =
                _method.Body.Instructions.Where(i => i.SequencePoint != null).Select(i => i.SequencePoint.Document.Url).
                    Distinct();
            foreach (var sourceFile in sourceFiles)
            {
                _sourceFiles.Add(sourceFile, File.ReadAllLines(sourceFile));
            }
        }

        protected abstract IEnumerable<MutationTestMetaData> DoMutate(MethodDefinition method, AssemblyDefinition assembly, string assemblyLocation);

        protected MutationTestMetaData DoYield(MethodDefinition method, AssemblyDefinition assembly, string assemblyLocation, string description, int index)
        {
            var testDirectory = new TestDirectory(Path.GetDirectoryName(assemblyLocation));
            testDirectory.SaveAssembly(assembly, Path.GetFileName(assemblyLocation));
            return new MutationTestMetaData
            {
                Description = description,
                MethodDefinition = method,
                TestDirectory = testDirectory,
                ILIndex = index
            };
        }

        public int GetOriginalOffset(int index)
        {
            return _originalOffsets[index];
        }

        public string GetOriginalSourceFileName(int index)
        {
            var sequencePoint = GetCurrentSequencePoint(index);
            return Path.GetFileName(sequencePoint.Document.Url);
        }

        private SequencePoint GetCurrentSequencePoint(int index)
        {
            var instruction = _method.Body.Instructions[index];
            while (instruction.SequencePoint == null)
            {
                index--;
                instruction = _method.Body.Instructions[index];
            }
            var sequencePoint = instruction.SequencePoint;
            return sequencePoint;
        }

        public string GetOriginalSourceCode(int index)
        {
            var sequencePoint = GetCurrentSequencePoint(index);
            string result = "";
            string[] sourceCode = _sourceFiles[sequencePoint.Document.Url];
            int upperBound = Math.Min(sequencePoint.EndLine + 2, sourceCode.Length);
            for (int line = Math.Max(sequencePoint.StartLine - 2, 1); line <= upperBound; line++)
            {
                string sourceLine = sourceCode[line - 1].Replace("\t", "    ");
                result += line.ToString().PadLeft(4, ' ') + ": " + sourceLine.TrimEnd(' ', '\t');
                if (line < upperBound) result += Environment.NewLine;
            }
            return result;
        }
    }
}
