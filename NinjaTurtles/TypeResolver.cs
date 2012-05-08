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
using System.Reflection;

using Mono.Cecil;

namespace NinjaTurtles
{
	internal class TypeResolver
	{
		internal static Type ResolveTypeFromReferences(Assembly callingAssembly, string className)
		{
			return ResolveTypeFromReferences(callingAssembly, className, new List<string>());
		}
		
		private static Type ResolveTypeFromReferences(Assembly assembly, string className, IList<string> consideredAssemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.FullName == className) return type;
            }
            foreach (var reference in assembly.GetReferencedAssemblies())
            {
				if (consideredAssemblies.Contains(reference.Name)) continue;
				consideredAssemblies.Add(reference.Name);
                var type = ResolveTypeFromReferences(Assembly.Load(reference), className, consideredAssemblies);
                if (type != null) return type;
            }
            return null;
        }
	}
}

