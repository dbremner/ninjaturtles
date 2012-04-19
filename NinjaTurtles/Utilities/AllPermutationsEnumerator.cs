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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTurtles.Utilities
{
    /// <summary>
    /// Enumerators over all possible permutations of a set of items.
    /// </summary>
    /// <typeparam name="T">The type of the items in the original set.</typeparam>
    public class AllPermutationsEnumerator<T> : IEnumerator<IEnumerable<T>>
    {
        /// <summary>The set of items over which all permutations are generated.</summary>
        private T[] _originalItems;

        /// <summary>Flag to indicate if this enumerator is in its initial state.</summary>
        private bool _isFirst;

        /// <summary>Flag to indicate if this enumerator has been disposed of.</summary>
        private bool _isDisposed;

        /// <summary>The current permutation.</summary>
        private T[] _current;

        /// <summary>The indicates of the items in the original set that are selected for the current permutation.</summary>
        private int[] _indices;

        /// <summary>A state variable used by the permutation algorithm.</summary>
        private bool _even;

        /// <summary>A state variable used by the permutation algorithm to track whether this enumerator is finished.</summary>
        private bool _isFinished;

        /// <summary>
        /// Creates and initializes an instance of the <see cref="AllPermutationsEnumerator{T}"/> type.
        /// </summary>
        /// <param name="items">The set of items over which permutations are to be created.</param>
        public AllPermutationsEnumerator(IEnumerable<T> items)
        {
            if (null == items)
            {
                throw new ArgumentNullException("items");
            }

            _originalItems = items.ToArray();
            _indices = new int[_originalItems.Length];
            for (int i = 0; i < _indices.Length; ++i)
            {
                _indices[i] = i + 1;
            }
            _current = null;
            _even = true;
            _isDisposed = false;
            _isFirst = true;
            _isFinished = false;
        }

        /// <summary>
        /// Gets the current item referred to by this enumerator.
        /// </summary>
        public IEnumerable<T> Current
        {
            get
            {
                CheckIfDisposed();
                return _current;
            }
        }

        /// <summary>
        /// Disposes of this enumerator.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _originalItems = null;
            _current = null;
            _indices = null;
            _even = true;
            _isFirst = true;
            _isFinished = false;
        }

        /// <summary>
        /// Gets the current item referred to by this enumerator.
        /// </summary>
        object IEnumerator.Current
        {
            get { return Current; }
        }

        /// <summary>
        /// Moves this enumerator to refer to the next item in the enumeration.
        /// </summary>
        /// <returns><c>true</c> if a value is available; <c>false</c> if the end has been reached.</returns>
        public bool MoveNext()
        {
            CheckIfDisposed();

            if (_isFinished)
            {
                return false;
            }

            if (_isFirst)
            {
                _even = true;
                _isFirst = false;
                Select();
                return true;
            }
            if (_originalItems.Length == 1)
            {
                _current = null;
                return false;
            }

            if (_even)
            {
                _even = false;
                _indices.Swap(0, 1);
                Select();

                if (!((_indices[_indices.Length - 1] != 1) || (_indices[0] != (2 + _indices.Length % 2))))
                {
                    if (_indices.Length <= 3)
                    {
                        _isFinished = true;
                    }
                    else
                    {
                        _isFinished = true;
                        for (int i = 0; i < (_indices.Length - 3); ++i)
                        {
                            if (_indices[i + 1] != _indices[i] + 1)
                            {
                                _isFinished = false;
                            }
                        }
                    }
                }
                return true;
            }

            int s = 0;
            _even = true;

            for (int k = 1; k < _indices.Length; ++k)
            {
                int d = 0;
                int ia = _indices[k];
                int i = k - 1;

                for (int j = 0; j <= i; ++j)
                {
                    if (_indices[j] > ia)
                    {
                        ++d;
                    }
                }
                s += d;

                if (d != (i + 1) * (s % 2))
                {
                    int l = 0;
                    int m = ((s + 1) % 2) * (_indices.Length + 1);
                    for (int j = 0; j <= i; ++j)
                    {
                        if (Sign(1, _indices[j] - ia) != Sign(1, _indices[j] - m))
                        {
                            m = _indices[j];
                            l = j;
                        }
                    }
                    _indices[l] = ia;
                    _indices[k] = m;
                    _even = true;
                    Select();
                    break;
                }
            }

            return true;
        }

        /// <summary>
        /// Simulates the fortran SIGN function that computes the absolute of the parameter <paramref name="x"/>
        /// and then assigns the sign of the parameter <paramref name="y"/> to <paramref name="x"/> and returns
        /// this new value.
        /// </summary>
        /// <param name="x">The value to sign.</param>
        /// <param name="y">The value that determines the sign.</param>
        /// <returns><paramref name="x"/> signed by <paramref name="y"/>.</returns>
        private int Sign(int x, int y)
        {
            return x * y >= 0 ? 1 : -1;
        }

        /// <summary>
        /// Selects the items in the current permutation based on the computed indicies of the items
        /// to be selected.
        /// </summary>
        private void Select()
        {
            _current = new T[_originalItems.Length];
            for (int i = 0; i < _indices.Length; ++i)
            {
                _current[i] = _originalItems[_indices[i] - 1];
            }
        }

        /// <summary>
        /// Resets this enumerator to the initial state.
        /// </summary>
        public void Reset()
        {
            CheckIfDisposed();

            for (int i = 0; i < _indices.Length; ++i)
            {
                _indices[i] = i + 1;
            }

            _current = null;
            _even = true;
            _isDisposed = false;
            _isFirst = true;
            _isFinished = false;
        }

        /// <summary>
        /// Checks and throws an exception if this enumerator has been disposed.
        /// </summary>
        private void CheckIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
