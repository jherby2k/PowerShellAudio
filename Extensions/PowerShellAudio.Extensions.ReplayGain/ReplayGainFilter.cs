/*
 * Copyright © 2014 Jeremy Herbison
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
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading.Tasks;

namespace PowerShellAudio.Extensions.ReplayGain
{
    [SampleFilterExport("ReplayGain")]
    public class ReplayGainFilter : ISampleFilter
    {
        float _scale = 1;

        public SettingsDictionary DefaultSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<SettingsDictionary>() != null);

                var result = new SettingsDictionary();

                result.Add("ApplyGain", bool.FalseString);

                return result;
            }
        }

        public IReadOnlyCollection<string> AvailableSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyCollection<string>>() != null);

                var result = new List<string>(1);

                result.Add("ApplyGain");

                return result;
            }
        }

        public void Initialize(MetadataDictionary metadata, SettingsDictionary settings)
        {
            if (!string.IsNullOrEmpty(settings["ApplyGain"]) && string.Compare(settings["ApplyGain"], bool.FalseString, StringComparison.OrdinalIgnoreCase) != 0)
            {
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
                    throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.ReplayGainSampleFilterBadApplyGain, settings["ApplyGain"]));

                // Adjust the metadata so that it remains valid:
                metadata["AlbumGain"] = AdjustGain(metadata["AlbumGain"], _scale);
                metadata["TrackGain"] = AdjustGain(metadata["TrackGain"], _scale);
                metadata["AlbumPeak"] = AdjustPeak(metadata["AlbumPeak"], _scale);
                metadata["TrackPeak"] = AdjustPeak(metadata["TrackPeak"], _scale);
            }
        }

        public void Submit(SampleCollection samples)
        {
            if (_scale == 1)
                return;

            // Optimization - Faster when channels are processed in parallel:
            Parallel.ForEach(samples, channel =>
            {
                for (int sample = 0; sample < channel.Length; sample++)
                    channel[sample] *= _scale;
            });
        }

        static float CalculateScale(string gain, string peak)
        {
            Contract.Requires(!string.IsNullOrEmpty(gain));
            Contract.Requires(!string.IsNullOrEmpty(peak));

            // Return the desired scale, or the closest possible without clipping:
            return Math.Min((float)Math.Pow(10, float.Parse(gain.Replace(" dB", string.Empty), CultureInfo.InvariantCulture) / 20), 1 / float.Parse(peak, CultureInfo.InvariantCulture));
        }

        static string AdjustGain(string gain, float scale)
        {
            if (!string.IsNullOrEmpty(gain))
                return string.Format(CultureInfo.InvariantCulture, "{0:0.00} dB", float.Parse(gain.Replace(" dB", string.Empty), CultureInfo.InvariantCulture) - Math.Log10(scale) * 20);
            return string.Empty;
        }

        static string AdjustPeak(string peak, float scale)
        {
            if (!string.IsNullOrEmpty(peak))
                return string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", float.Parse(peak, CultureInfo.InvariantCulture) * scale);
            return string.Empty;
        }
    }
}
