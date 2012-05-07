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
	public sealed class MutationTestBuilder<T>
	{
		public static IMutationTest For(string targetMethod)
		{
			var callingAssembly = Assembly.GetCallingAssembly();
			return MutationTestBuilder.For(callingAssembly.Location, typeof(T), targetMethod);
		}
	}
	
	public sealed class MutationTestBuilder
	{
		public static IMutationTest For(string targetClass, string targetMethod)
		{
			var callingAssembly = Assembly.GetCallingAssembly();
            Type resolvedType = TypeResolver.ResolveTypeFromReferences(callingAssembly, targetClass);

			return For(callingAssembly.Location, resolvedType, targetMethod);
		}
		
		internal static IMutationTest For(string callingAssemblyLocation, Type targetType, string targetMethod)
		{
			return new MutationTest(callingAssemblyLocation, targetType, targetMethod);
		}
	}
}

