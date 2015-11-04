/*
 * Copyright © 2014, 2015 Jeremy Herbison
 * 
 * This file is part of PowerShell Audio.
 * 
 * PowerShell Audio is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser
 * General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 * 
 * PowerShell Audio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License
 * for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with PowerShell Audio.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;

namespace PowerShellAudio
{
    /// <summary>
    /// A singleton helper class for managing <see cref="SampleCollection"/> lifetime.
    /// </summary>
    /// <remarks>
    /// <see cref="SampleCollection"/> objects are created by calling Create on the singleton instance of this class.
    /// Since <see cref="SampleCollection"/>s can take up a lot of memory, frequently creating them could cause an
    /// excessive number of garbage collection cycles. By calling the Free method once a <see cref="SampleCollection"/>
    /// is no longer required, the internal arrays can be reused by the next call to the Create method.
    /// </remarks>
    public class SampleCollectionFactory
    {
        static readonly Lazy<SampleCollectionFactory> _lazyInstance = new Lazy<SampleCollectionFactory>(() => new SampleCollectionFactory());

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        /// <value>
        /// The singleton instance.
        /// </value>
        public static SampleCollectionFactory Instance
        {
            get
            {
                Contract.Ensures(Contract.Result<SampleCollectionFactory>() != null);

                return _lazyInstance.Value;
            }
        }

        readonly ConcurrentDictionary<int, ConcurrentBag<WeakReference<float[]>>> _cachedArrayDictionary = new ConcurrentDictionary<int, ConcurrentBag<WeakReference<float[]>>>();

        SampleCollectionFactory()
        { }

        /// <summary>
        /// Creates a new <see cref="SampleCollection"/> with the specified channels and sample count.
        /// </summary>
        /// <param name="channels">The number of channels.</param>
        /// <param name="sampleCount">The sample count.</param>
        /// <returns>A new <see cref="SampleCollection"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if either <paramref name="channels"/> is less than 1, or <paramref name="sampleCount"/> is negative.
        /// </exception>
        public SampleCollection Create(int channels, int sampleCount)
        {
            Contract.Requires<ArgumentOutOfRangeException>(channels > 0);
            Contract.Requires<ArgumentOutOfRangeException>(sampleCount >= 0);
            Contract.Ensures(Contract.Result<SampleCollection>() != null);
            Contract.Ensures(Contract.Result<SampleCollection>().SampleCount == sampleCount);

            var samples = new float[channels][];
            for (var channel = 0; channel < channels; channel++)
                samples[channel] = CreateOrGetCachedArray(sampleCount);

            return new SampleCollection(samples);
        }

        /// <summary>
        /// Frees the internal arrays used by the specified <see cref="SampleCollection"/>, so they can be reallocated.
        /// </summary>
        /// <param name="samples">The <see cref="SampleCollection"/> to free.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="samples"/> is null.</exception>
        public void Free(SampleCollection samples)
        {
            Contract.Requires<ArgumentNullException>(samples != null);

            if (samples.SampleCount == 0)
                return;

            foreach (float[] channel in samples)
                CacheArray(channel);
        }

        /// <summary>
        /// Resizes the specified <see cref="SampleCollection"/>. This frees the existing arrays after copying the
        /// values.
        /// </summary>
        /// <param name="samples">The <see cref="SampleCollection"/> to resize.</param>
        /// <param name="sampleCount">The new sample count.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="samples"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="sampleCount"/> is less than 1.</exception>
        public void Resize(SampleCollection samples, int sampleCount)
        {
            Contract.Requires<ArgumentNullException>(samples != null);
            Contract.Requires<ArgumentOutOfRangeException>(sampleCount > 0);
            Contract.Ensures(samples.SampleCount == sampleCount);

            for (var channel = 0; channel < samples.Channels; channel++)
            {
                float[] newArray = CreateOrGetCachedArray(sampleCount);
                Array.Copy(samples[channel], newArray, Math.Min(sampleCount, samples.SampleCount));
                samples[channel] = newArray;
                CacheArray(samples[channel]);
            }
        }

        float[] CreateOrGetCachedArray(int sampleCount)
        {
            Contract.Requires(sampleCount >= 0);
            Contract.Ensures(Contract.Result<float[]>() != null);
            Contract.Ensures(Contract.Result<float[]>().Length == sampleCount);

            if (sampleCount == 0)
                return new float[0];

            // Check the dictionary for arrays of the same length:
            ConcurrentBag<WeakReference<float[]>> cachedArrays;
            if (_cachedArrayDictionary.TryGetValue(sampleCount, out cachedArrays))
            {
                // Check arrays one at a time, until we get one that hasn't been disposed yet:
                WeakReference<float[]> weakReference;
                while (cachedArrays.TryTake(out weakReference))
                {
                    float[] target;
                    if (weakReference.TryGetTarget(out target))
                        return target;
                }
            }

            // If no cached arrays are available, create a new one:
            return new float[sampleCount];
        }

        void CacheArray(float[] array)
        {
            Contract.Requires(array != null);
            Contract.Requires(array.Length > 0);

            _cachedArrayDictionary.AddOrUpdate(array.Length, new ConcurrentBag<WeakReference<float[]>>(), (i, bag) =>
            {
                bag.Add(new WeakReference<float[]>(array));
                return bag;
            });
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_cachedArrayDictionary != null);
        }
    }
}
