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
using System.Threading;

using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace NinjaTurtles.Turtles.Method
{
    /// <summary>
    /// An abstract implementation of <see cref="IMethodTurtle" /> which
    /// handles assembly rewriting for derived classes, requiring them to just
    /// implement their own <see mref="DoMutate" /> method which internally
    /// uses the <see mref="PlaceFileAndYield" /> method.
    /// </summary>
    public abstract class MethodTurtle : IMethodTurtle
    {
        private string _originalFileName;

        /// <summary>
        /// A description for the particular implementation class.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}" /> of detailed descriptions
        /// of mutations, having first carried out the mutation in question and
        /// saved the modified assembly under test to disk.
        /// </summary>
        /// <param name="method">
        /// A <see cref="MethodDefinition" /> for the method on which mutation
        /// testing is to be carried out.
        /// </param>
        /// <param name="assembly">
        /// An <see cref="AssemblyDefinition" /> for the containing assembly.
        /// </param>
        /// <param name="fileName">
        /// The path to the assembly file, so that the turtle can overwrite it
        /// with mutated versions.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}" /> of <see cref="string" />s.
        /// </returns>
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

        /// <summary>
        /// When implemented in a subclass, performs the actual mutations on
        /// the source assembly
        /// </summary>
        /// <param name="method">
        /// A <see cref="MethodDefinition" /> for the method on which mutation
        /// testing is to be carried out.
        /// </param>
        /// <param name="assembly">
        /// An <see cref="AssemblyDefinition" /> for the containing assembly.
        /// </param>
        /// <param name="fileName">
        /// The path to the assembly file, so that the turtle can overwrite it
        /// with mutated versions.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}" /> of <see cref="string" />s.
        /// </returns>
        protected abstract IEnumerable<string> DoMutate(MethodDefinition method, AssemblyDefinition assembly, string fileName); 

        /// <summary>
        /// Moves the original assembly aside, and writes the mutated copy in
        /// its place before yielding to allow the test suite to be run.
        /// Following this it deletes the mutated file and reinstates the
        /// original assembly.
        /// </summary>
        /// <param name="assembly">
        /// An <see cref="AssemblyDefinition" /> for the containing assembly.
        /// </param>
        /// <param name="fileName">
        /// The path to the assembly file, so that the turtle can overwrite it
        /// with mutated versions.
        /// </param>
        /// <param name="output">
        /// The string describing the mutation, returned to calling code with
        /// <c>yield return</c>.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}" /> of <see cref="string" />s with a
        /// single element.
        /// </returns>
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
