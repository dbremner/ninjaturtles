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
    /// When applied to a unit test method, specifies that the method is a test
    /// for the method named in the constructor parameter. This is interpreted
    /// as a method in the class defined in a
    /// <see cref="ClassTestedAttribute" /> applied to the containing test
    /// class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MethodTestedAttribute : Attribute
    {
        private readonly string _methodName;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="MethodTestedAttribute" /> class.
        /// </summary>
        /// <param name="methodName">
        /// The name of a method which is tested by the attributed unit test.
        /// </param>
        public MethodTestedAttribute(string methodName)
        {
            _methodName = methodName;
        }

        internal string MethodName
        {
            get { return _methodName; }
        }
    }
}
