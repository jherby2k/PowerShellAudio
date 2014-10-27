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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace AudioShell
{
    /// <summary>
    /// A collection of 32-bit floating-point audio samples.
    /// </summary>
    /// <remarks>
    /// Samples should be normalized to between 1.0 and -1.0, otherwise clipping may occur.
    /// This class is a light-weight wrapper around a two-dimensional array. New instances should be created by using
    /// the <see cref="SampleCollectionFactory"/> helper class. To avoid frequent garbage collection cycles when
    /// allocating a large number of <see cref="SampleCollection"/> objects, always call the
    /// <see cref="SampleCollectionFactory"/>'s Free method once the instance is no longer needed, so that the arrays
    /// can be reused.
    /// </remarks>
    public class SampleCollection : IEnumerable<float[]>
    {
        readonly float[][] _samples;

        internal SampleCollection(float[][] samples)
        {
            Contract.Requires(samples != null);
            Contract.Requires(samples.Length > 0);
            Contract.Requires(Contract.ForAll(samples, channel => channel != null));
            Contract.Ensures(_samples != null);
            Contract.Ensures(_samples == samples);

            _samples = samples;
        }

        /// <summary>
        /// Gets the array of <see cref="Single"/>s containing samples for the specified channel.
        /// </summary>
        /// <remarks>
        /// This property does not return a copy of the array, but the original. It may be modified in-place.
        /// </remarks>
        /// <value>
        /// The array of <see cref="Single"/>s containing samples for the specified channel.
        /// </value>
        /// <param name="channel">The channel index.</param>
        /// <returns>The array of <see cref="Single"/>s containing samples for the specified channel.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throw if <paramref name="channel"/> is negative.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "The array is intended to be modifiable, and cannot be wrapped in a collection for performance reasons.")]
        public float[] this[int channel]
        {
            get
            {
                Contract.Requires(channel >= 0);
                Contract.Ensures(Contract.Result<float[]>() != null);

                return _samples[channel];
            }
            internal set
            {
                Contract.Requires(channel >= 0);
                Contract.Requires(value != null);
                Contract.Ensures(_samples[channel] == value);

                _samples[channel] = value;
            }
        }

        /// <summary>
        /// Gets the number of channels.
        /// </summary>
        /// <value>
        /// The number of channels.
        /// </value>
        public int Channels
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);

                return _samples.Length;
            }
        }

        /// <summary>
        /// Gets the sample count.
        /// </summary>
        /// <value>
        /// The sample count.
        /// </value>
        public int SampleCount
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);

                return _samples[0].Length;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this is the last <see cref="SampleCollection"/> in the stream. This is
        /// equivalent to checking if SampleCount equals 0.
        /// </summary>
        /// <value>True if this instance is last; otherwise, false.</value>
        public bool IsLast
        {
            get
            {
                return _samples[0].Length == 0 ? true : false;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<float[]> GetEnumerator()
        {
            return ((IEnumerable<float[]>)_samples).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _samples.GetEnumerator();
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_samples.Length > 0);
            Contract.Invariant(Contract.ForAll(_samples, channel => channel != null));
        }
    }
}
