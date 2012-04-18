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
// Copyright (C) 2012 David Musgrove.

#endregion

using System.Collections;
using System.Collections.Generic;

namespace NinjaTurtles.Utilities
{
    /// <summary>
    /// An enumeration of all the permuations of a set of items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AllPermutationsEnumerable<T> : IEnumerable<IEnumerable<T>>
    {
        /// <summary>The set of items over which all permutations are generated.</summary>
        private IEnumerable<T> items;

        /// <summary>
        /// Creates and initializes a new instance of the <see cref="AllPermutationsEnumerable{T}"/> type.
        /// </summary>
        /// <param name="items">The original set of items over which all permutations are generated.</param>
        public AllPermutationsEnumerable(IEnumerable<T> items)
            : base()
        {
            this.items = items;
        }

        /// <summary>
        /// Gets an enumerator to enumerate over the items in this enumeration.
        /// </summary>
        /// <returns>An instance of <see cref="IEnumerator{T}"/>.</returns>
        public IEnumerator<IEnumerable<T>> GetEnumerator()
        {
            return new AllPermutationsEnumerator<T>(this.items);
        }

        /// <summary>
        /// Gets an enumerator to enumerate over the items in this enumeration.
        /// </summary>
        /// <returns>An instance of <see cref="IEnumerator{T}"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new AllPermutationsEnumerator<T>(this.items);
        }
    }
}
