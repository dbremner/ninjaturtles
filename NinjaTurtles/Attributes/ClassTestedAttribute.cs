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
    /// When applied to a class containing unit tests, specifies that the class
    /// contains tests for the type passed in the constructor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ClassTestedAttribute : Attribute
    {
        private readonly string _className;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ClassTestedAttribute" /> class.
        /// </summary>
        /// <param name="targetClass">
        /// The type for which the attributed class contains unit tests.
        /// </param>
        public ClassTestedAttribute(Type targetClass)
        {
            _className = targetClass.Name;
        }

        internal string ClassName
        {
            get { return _className; }
        }
    }
}
