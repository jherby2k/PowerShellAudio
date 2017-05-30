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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using PowerShellAudio.Properties;

namespace PowerShellAudio
{
    /// <summary>
    /// Describes the audio format of a file.
    /// </summary>
    /// <remarks>
    /// Instances of this class are normally created by extensions, in the PowerShellAudio.Extensibility namespace.
    /// </remarks>
    [Serializable]
    public class AudioInfo
    {
        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> of supported channel configurations.
        /// </summary>
        /// <value>An <see cref="IReadOnlyCollection{T}"/> of supported channel configurations.</value>
        [NotNull]
        public static IReadOnlyCollection<int> SupportedChannels { get; } = new[] { 1, 2 };

        /// <summary>
        /// Gets an <see cref="IReadOnlyCollection{T}"/> of supported sample rates.
        /// </summary>
        /// <value>An <see cref="IReadOnlyCollection{T}"/> of supported sample rates.</value>
        [NotNull]
        public static IReadOnlyCollection<int> SupportedSampleRates { get; } = new[]
        {
            8000,
            11025,
            12000,
            16000,
            18900,
            22050,
            24000,
            28000,
            32000,
            36000,
            37800,
            44100,
            48000,
            64000,
            88200,
            96000,
            128000,
            144000,
            176400,
            192000
        };

        /// <summary>
        /// Gets a description of the audio stream format.
        /// </summary>
        /// <value>
        /// A description of the audio stream format.
        /// </value>
        [NotNull]
        public string Format { get; }

        /// <summary>
        /// Gets the number of channels.
        /// </summary>
        /// <value>The number of channels.</value>
        public int Channels { get; }

        /// <summary>
        /// Gets the number of bits per sample, or 0 if the audio is compressed in a lossy fashion.
        /// </summary>
        /// <value>The number of bits per sample.</value>
        public int BitsPerSample { get; }

        /// <summary>
        /// Gets the sample rate.
        /// </summary>
        /// <value>The sample rate.</value>
        public int SampleRate { get; }

        /// <summary>
        /// Gets the total sample count, or 0 if it is unavailable.
        /// </summary>
        /// <value>The total sample count.</value>
        public long SampleCount { get; }

        /// <summary>
        /// Gets the length of the audio. Returns 0 if the total sample count is unavailable.
        /// </summary>
        /// <value>The length of the audio.</value>
        public TimeSpan Length { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioInfo"/> class.
        /// </summary>
        /// <param name="format">A description of the audio stream format.</param>
        /// <param name="channels">The number of channels.</param>
        /// <param name="bitsPerSample">
        /// The number of bits per sample, or 0 if the audio is compressed in a lossy fashion.
        /// </param>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="sampleCount">The total sample count, or 0 if it cannot be determined.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="format"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if one or more parameters are outside the supported range.
        /// </exception>
        public AudioInfo([NotNull] string format, int channels, int bitsPerSample, int sampleRate, long sampleCount)
        {
            if (string.IsNullOrEmpty(format))
                throw new ArgumentException(Resources.AudioInfoFormatIsEmptyError, nameof(format));
            if (!SupportedChannels.Contains(channels))
                throw new ArgumentOutOfRangeException(nameof(channels), channels,
                    string.Format(CultureInfo.CurrentCulture, Resources.AudioInfoChannelsOutOfRangeError, channels));
            if (bitsPerSample < 0 || bitsPerSample > 32)
                throw new ArgumentOutOfRangeException(nameof(bitsPerSample), bitsPerSample,
                    string.Format(CultureInfo.CurrentCulture, Resources.AudioInfoBitsPerSampleIsOutOfRangeError, bitsPerSample));
            if (!SupportedSampleRates.Contains(sampleRate))
                throw new ArgumentOutOfRangeException(nameof(sampleRate), sampleRate,
                    string.Format(CultureInfo.CurrentCulture, Resources.AudioInfoSampleRateIsOutOfRangeError, sampleRate));
            if (sampleCount < 0)
                throw new ArgumentOutOfRangeException(nameof(sampleCount), sampleCount,
                    string.Format(CultureInfo.CurrentCulture, Resources.AudioInfoSampleCountIsOutOfRangeError, sampleCount));

            Format = format;
            Channels = channels;
            BitsPerSample = bitsPerSample;
            SampleRate = sampleRate;
            SampleCount = sampleCount;
            if (sampleCount > 0)
                Length = new TimeSpan(0, 0, (int)Math.Round(sampleCount / (double)sampleRate));
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents this instance.</returns>
        public override string ToString()
        {
            var result = new StringBuilder();

            if (BitsPerSample > 0)
            {
                result.Append(BitsPerSample);
                result.Append("-bit ");
            }
            result.Append((SampleRate / 1000f).ToString("G", CultureInfo.CurrentCulture));
            result.Append("kHz ");
            switch (Channels)
            {
                case 1:
                    result.Append("Mono ");
                    break;
                case 2:
                    result.Append("Stereo ");
                    break;
            }
            result.Append(Format);

            if (Length.TotalSeconds < 1)
                return result.ToString();

            result.Append(" [");
            result.Append(Length.Hours < 1
                ? Length.ToString(@"%m\:ss", CultureInfo.CurrentCulture)
                : Length.ToString(@"%h\:mm\:ss", CultureInfo.CurrentCulture));
            result.Append("]");

            return result.ToString();
        }
    }
}
