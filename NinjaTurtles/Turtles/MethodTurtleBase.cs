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
using Mono.Cecil.Rocks;

namespace NinjaTurtles.Turtles
{
    public abstract class MethodTurtleBase : IMethodTurtle
    {
        private int[] _originalOffsets;

        public void MutantComplete(MutationTestMetaData metaData)
        {
            metaData.TestDirectory.Dispose();
        }

        public IEnumerable<MutationTestMetaData> Mutate(MethodDefinition method, AssemblyDefinition assembly, string assemblyLocation)
        {
            _originalOffsets = method.Body.Instructions.Select(i => i.Offset).ToArray();
            method.Body.SimplifyMacros();
            foreach (var mutation in DoMutate(method, assembly, assemblyLocation))
            {
                yield return mutation;
            }
        }

        protected abstract IEnumerable<MutationTestMetaData> DoMutate(MethodDefinition method, AssemblyDefinition assembly, string assemblyLocation);

        protected MutationTestMetaData DoYield(MethodDefinition method, AssemblyDefinition assembly, string assemblyLocation, string description)
       {
           var testDirectory = new TestDirectory(Path.GetDirectoryName(assemblyLocation));
           testDirectory.SaveAssembly(assembly, Path.GetFileName(assemblyLocation));
           return new MutationTestMetaData
           {
               Description = description,
               MethodDefinition = method,
               TestDirectory = testDirectory
           };
       }

        protected int GetOriginalOffset(int index)
        {
            return _originalOffsets[index];
        }
    }
}
