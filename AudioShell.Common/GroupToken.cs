/*
 * Copyright © 2014 Jeremy Herbison
 * 
 * This file is part of AudioShell.
 * 
 * AudioShell is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option)
 * any later version.
 * 
 * AudioShell is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for more
 * details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with AudioShell.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

using System;
using System.Diagnostics.Contracts;

namespace AudioShell
{
    /// <summary>
    /// An object used to indicate membership in a collection (An album or compilation).
    /// </summary>
    public class GroupToken
    {
        /// <summary>
        /// Gets the member count.
        /// </summary>
        /// <value>
        /// The member count.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is less than 1.
        /// </exception>
        public int Count { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupToken"/> class.
        /// </summary>
        /// <param name="count">The member count.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is less than 1.</exception>
        public GroupToken(int count)
        {
            Contract.Requires<ArgumentOutOfRangeException>(count > 0);
            Contract.Ensures(Count == count);

            Count = count;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(Count > 0);
        }
    }
}
