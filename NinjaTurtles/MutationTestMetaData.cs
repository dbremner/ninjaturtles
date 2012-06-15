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

using Mono.Cecil;

namespace NinjaTurtles
{
    /// <summary>
    /// A class containing metadata for a mutation test.
    /// </summary>
    public class MutationTestMetaData
	{
        /// <summary>
        /// Gets or sets the description of the mutation test being run.
        /// </summary>
        public string Description { get; internal set; }

		internal MethodDefinition MethodDefinition { get; set; }
        internal int ILIndex { get; set; }
        internal TestDirectory TestDirectory { get; set; }

        /// <summary>
        /// Gets the name of the target directory for the mutation test, to
        /// which the test DLLs and mutated assembly have been copied.
        /// </summary>
        public string TestDirectoryName
		{
			get { return TestDirectory.FullName; }
		}
	}
}

