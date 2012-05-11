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

using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;

namespace NinjaTurtles
{
    public class Module
    {
        public Module(string assemblyLocation)
        {
            AssemblyLocation = assemblyLocation;
            var readerParameters = new ReaderParameters(ReadingMode.Immediate);
            AssemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyLocation, readerParameters);
            Definition = AssemblyDefinition.MainModule;
            SourceFiles = new Dictionary<string, string[]>();
        }

        public string AssemblyLocation { get; private set; }
        public AssemblyDefinition AssemblyDefinition { get; private set; }
        public ModuleDefinition Definition { get; private set; }
        public IDictionary<string, string[]> SourceFiles { get; private set; } 

        public void LoadDebugInformation()
        {
            var reader = ResolveSymbolReader();
            if (reader == null) return;

            Definition.ReadSymbols(reader);

            foreach (var method in Definition.Types
                .SelectMany(t => t.Methods)
                .Where(m => m.HasBody))
            {
                reader.Read(method.Body,
                    o => method.Body.Instructions.FirstOrDefault(i => i.Offset == o));
                var sourceFiles = method.Body.Instructions.Where(i => i.SequencePoint != null)
                    .Select(i => i.SequencePoint.Document.Url)
                    .Distinct();
                foreach (var sourceFile in sourceFiles)
                {
                    if (!SourceFiles.ContainsKey(sourceFile))
                    {
                        SourceFiles.Add(sourceFile, File.ReadAllLines(sourceFile));
                    }
                }
            }
        }

        private static Instruction FirstOrDefault(int o, MethodDefinition method)
        {
            return method.Body.Instructions.FirstOrDefault(i => i.Offset == o);
        }

        private ISymbolReader ResolveSymbolReader()
        {
            string symbolLocation = null;
            string pdbLocation = Path.ChangeExtension(AssemblyLocation, "pdb");
            string mdbLocation = AssemblyLocation + ".mdb";
            ISymbolReaderProvider provider = null;
            if (File.Exists(pdbLocation))
            {
                symbolLocation = pdbLocation;
                provider = new PdbReaderProvider();
            }
            else if (File.Exists(mdbLocation))
            {
                symbolLocation = AssemblyLocation;
                provider = new MdbReaderProvider();
            }
            if (provider == null) return null;
            var reader = provider.GetSymbolReader(Definition, symbolLocation);
            return reader;
        }
    }
}
