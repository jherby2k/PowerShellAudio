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

using PowerShellAudio.Extensions.ReplayGain.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.ReplayGain
{
    [SampleFilterExport("ReplayGain")]
    public class ReplayGainFilter : ISampleFilter
    {
        float _scale = 1;

        [NotNull]
        public SettingsDictionary DefaultSettings => new SettingsDictionary
        {
            { "ApplyGain", bool.FalseString }
        };

        [NotNull]
        public IReadOnlyCollection<string> AvailableSettings => new List<string>
        {
            "ApplyGain"
        };

        public void Initialize([NotNull] MetadataDictionary metadata, [NotNull] SettingsDictionary settings)
        {
            if (string.IsNullOrEmpty(settings["ApplyGain"]) ||
                string.Compare(settings["ApplyGain"], bool.FalseString, StringComparison.OrdinalIgnoreCase) == 0)
                return;

            if (string.Compare(settings["ApplyGain"], "Album", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (string.IsNullOrEmpty(metadata["AlbumGain"]))
                    throw new InvalidSettingException(Resources.ReplayGainSampleFilterMissingAlbumGain);
                if (string.IsNullOrEmpty(metadata["AlbumPeak"]))
                    throw new InvalidSettingException(Resources.ReplayGainSampleFilterMissingAlbumPeak);

                _scale = CalculateScale(metadata["AlbumGain"], metadata["AlbumPeak"]);
            }
            else if (string.Compare(settings["ApplyGain"], "Track", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (string.IsNullOrEmpty(metadata["TrackGain"]))
                    throw new InvalidSettingException(Resources.ReplayGainSampleFilterMissingTrackGain);
                if (string.IsNullOrEmpty(metadata["TrackPeak"]))
                    throw new InvalidSettingException(Resources.ReplayGainSampleFilterMissingTrackPeak);

                _scale = CalculateScale(metadata["TrackGain"], metadata["TrackPeak"]);
            }
            else
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture,
                    Resources.ReplayGainSampleFilterBadApplyGain, settings["ApplyGain"]));

            // Adjust the metadata so that it remains valid:
            metadata["AlbumGain"] = AdjustGain(metadata["AlbumGain"], _scale);
            metadata["TrackGain"] = AdjustGain(metadata["TrackGain"], _scale);
            metadata["AlbumPeak"] = AdjustPeak(metadata["AlbumPeak"], _scale);
            metadata["TrackPeak"] = AdjustPeak(metadata["TrackPeak"], _scale);
        }

        public void Submit([NotNull] SampleCollection samples)
        {
            if (Math.Abs(_scale - 1) < 0.001)
                return;

            // Optimization - Faster when channels are processed in parallel:
            Parallel.ForEach(samples, channel =>
            {
                for (var sample = 0; sample < channel.Length; sample++)
                    channel[sample] *= _scale;
            });
        }

        static float CalculateScale([NotNull] string gain, [NotNull] string peak)
        {
            // Return the desired scale, or the closest possible without clipping:
            return Math.Min((float)Math.Pow(10, float.Parse(gain.Replace(" dB", string.Empty),
                CultureInfo.InvariantCulture) / 20), 1 / float.Parse(peak, CultureInfo.InvariantCulture));
        }

        [NotNull]
        static string AdjustGain([NotNull] string gain, float scale)
        {
            return !string.IsNullOrEmpty(gain)
                ? string.Format(CultureInfo.InvariantCulture, "{0:0.00} dB",
                    float.Parse(gain.Replace(" dB", string.Empty), CultureInfo.InvariantCulture) -
                    Math.Log10(scale) * 20)
                : string.Empty;
        }

        static string AdjustPeak([NotNull] string peak, float scale)
        {
            return !string.IsNullOrEmpty(peak)
                ? string.Format(CultureInfo.InvariantCulture, "{0:0.000000}",
                    float.Parse(peak, CultureInfo.InvariantCulture) * scale)
                : string.Empty;
        }
    }
}
