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
using System.Reflection;

namespace NinjaTurtles
{
    /// <summary>
    /// When applied to a unit test method, specifies a member of a class that
    /// is tested by that method. This can be applied multiple times for a test
    /// that exercises multiple methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class MethodTestedAttribute : Attribute
	{
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="MethodTestedAttribute" /> class.
        /// </summary>
        /// <param name="targetType">
        /// The type for which the attributed class contains unit tests.
        /// </param>
        /// <param name="targetMethod">
        /// The name of a method which is tested by the attributed unit test.
        /// </param>
        public MethodTestedAttribute(Type targetType, string targetMethod)
		{
			TargetType = targetType;
			TargetMethod = targetMethod;
		}

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="MethodTestedAttribute" /> class. This overload is
        /// designed to allow non-public class's methods to be tested.
        /// </summary>
        /// <param name="targetType">
        /// The namespace-qualified name of the type for which the attributed
        /// class contains unit tests.
        /// </param>
        /// <param name="targetMethod">
        /// The name of a method which is tested by the attributed unit test.
        /// </param>
        public MethodTestedAttribute(string targetType, string targetMethod)
		{
            Type resolvedType = TypeResolver.ResolveTypeFromReferences(Assembly.GetCallingAssembly(), targetType);
			TargetType = resolvedType;
			TargetMethod = targetMethod;
		}
		
		internal Type TargetType { get; private set; }

        internal string TargetMethod { get; private set; }

        /// <summary>
        /// Gets or sets a list of parameter types used to identify a
        /// particular method overload.
        /// </summary>
        public Type[] ParameterTypes { get; set; }
	}
}

