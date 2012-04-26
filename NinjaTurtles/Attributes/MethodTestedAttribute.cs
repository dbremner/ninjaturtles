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

namespace NinjaTurtles.Attributes
{
    /// <summary>
    /// When applied to a unit test method, specifies a member of a class that
    /// is tested by that method. This can be applied multiple times for a test
    /// that exercises multiple methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class MethodTestedAttribute : Attribute
    {
        private readonly string _className;
        private readonly string _methodName;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="MethodTestedAttribute" /> class.
        /// </summary>
        /// <param name="targetClass">
        /// The type for which the attributed class contains unit tests.
        /// </param>
        /// <param name="methodName">
        /// The name of a method which is tested by the attributed unit test.
        /// </param>
        public MethodTestedAttribute(Type targetClass, string methodName)
        {
            _className = targetClass.Name;
            _methodName = methodName;
        }

        internal string ClassName
        {
            get { return _className; }
        }

        internal string MethodName
        {
            get { return _methodName; }
        }
    }
}
