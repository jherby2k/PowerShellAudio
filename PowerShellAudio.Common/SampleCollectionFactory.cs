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
using System.Globalization;
using JetBrains.Annotations;
using PowerShellAudio.Properties;

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
        [NotNull]
        public static SampleCollectionFactory Instance => _lazyInstance.Value;

        readonly ConcurrentDictionary<int, ConcurrentBag<WeakReference<float[]>>> _cachedArrayDictionary =
            new ConcurrentDictionary<int, ConcurrentBag<WeakReference<float[]>>>();

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
        [NotNull]
        public SampleCollection Create(int channels, int sampleCount)
        {
            if (channels <= 0)
                throw new ArgumentOutOfRangeException(nameof(channels), channels,
                    string.Format(CultureInfo.CurrentCulture, Resources.SampleCollectionFactoryCreateChannelsIsOutOfRangeError, channels));
            if (sampleCount < 0)
                throw new ArgumentOutOfRangeException(nameof(sampleCount), sampleCount,
                    string.Format(CultureInfo.CurrentCulture, Resources.SampleCollectionFactoryCreateSampleCountIsOutOfRangeError, sampleCount));

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
        public void Free([NotNull] SampleCollection samples)
        {
            if (samples == null) throw new ArgumentNullException(nameof(samples));

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
        public void Resize([NotNull] SampleCollection samples, int sampleCount)
        {
            if (samples == null) throw new ArgumentNullException(nameof(samples));
            if (sampleCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleCount), sampleCount,
                    string.Format(CultureInfo.CurrentCulture, Resources.SampleCollectionFactoryResizeSampleCountIsOutOfRangeError, sampleCount));

            for (var channel = 0; channel < samples.Channels; channel++)
            {
                float[] newArray = CreateOrGetCachedArray(sampleCount);
                Array.Copy(samples[channel], newArray, Math.Min(sampleCount, samples.SampleCount));
                samples[channel] = newArray;
                CacheArray(samples[channel]);
            }
        }

        [NotNull]
        float[] CreateOrGetCachedArray(int sampleCount)
        {
            if (sampleCount == 0)
                return new float[0];

            // Check the dictionary for arrays of the same length:
            if (_cachedArrayDictionary.TryGetValue(sampleCount, out ConcurrentBag<WeakReference<float[]>> cachedArrays))
            {
                // Check arrays one at a time, until we get one that hasn't been disposed yet:
                while (cachedArrays.TryTake(out WeakReference<float[]> weakReference))
                {
                    if (weakReference.TryGetTarget(out float[] target))
                        return target;
                }
            }

            // If no cached arrays are available, create a new one:
            return new float[sampleCount];
        }

        void CacheArray([NotNull] float[] array)
        {
            _cachedArrayDictionary.AddOrUpdate(array.Length, new ConcurrentBag<WeakReference<float[]>>(), (i, bag) =>
            {
                bag.Add(new WeakReference<float[]>(array));
                return bag;
            });
        }
    }
}
