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

namespace NinjaTurtles
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class MethodTestedAttribute : Attribute
	{
		public MethodTestedAttribute(Type targetType, string targetMethod)
		{
			TargetType = targetType;
			TargetMethod = targetMethod;
		}
		
		public MethodTestedAttribute(string targetType, string targetMethod)
		{
            Type resolvedType = TypeResolver.ResolveTypeFromReferences(Assembly.GetCallingAssembly(), targetType);
			TargetType = resolvedType;
			TargetMethod = targetMethod;
		}
		
		public Type TargetType { get; private set; }
		
		public string TargetMethod { get; private set; }
	}
}

