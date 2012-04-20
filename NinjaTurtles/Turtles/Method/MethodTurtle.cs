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

using System.Collections.Generic;
using System.IO;
using System.Threading;

using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace NinjaTurtles.Turtles.Method
{
    public abstract class MethodTurtle : IMethodTurtle
    {
        protected string _originalFileName;

        public abstract string Description { get; }

        public bool IsExpectedInvariant { get; set; }

        public IEnumerable<string> Mutate(MethodDefinition method, AssemblyDefinition assembly, string fileName)
        {
            if (!method.HasBody) yield break;
            method.Body.SimplifyMacros(); 
            _originalFileName = fileName.Replace(".dll", ".ninjaoriginal.dll");
            if (File.Exists(_originalFileName)) File.Delete(_originalFileName);
            foreach (var line in DoMutate(method, assembly, fileName))
            {
                yield return line;
            }
        }

        protected abstract IEnumerable<string> DoMutate(MethodDefinition method, AssemblyDefinition assembly, string fileName); 

        protected IEnumerable<string> PlaceFileAndYield(AssemblyDefinition assembly, string fileName, string output)
        {
            File.Move(fileName, _originalFileName);
            assembly.Write(fileName);
            yield return output;
            File.Delete(fileName);
            // HACKTAG: Wait for file to be deletable...
            while (File.Exists(fileName))
            {
                Thread.Sleep(100);
                File.Delete(fileName);
            }
            File.Move(_originalFileName, fileName);
        }
    }
}
