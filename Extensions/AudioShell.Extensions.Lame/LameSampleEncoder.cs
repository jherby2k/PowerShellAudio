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

using PowerShellAudio.Extensions.Lame.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PowerShellAudio.Extensions.Lame
{
    [SampleEncoderExport("Lame MP3")]
    public class LameSampleEncoder : ISampleEncoder, IDisposable
    {
        NativeEncoder _encoder;

        public string Extension
        {
            get { return ".mp3"; }
        }

        public SettingsDictionary DefaultSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<SettingsDictionary>() != null);

                var result = new SettingsDictionary();

                result.Add("AddMetadata", bool.TrueString);
                result.Add("ApplyGain", bool.FalseString);
                result.Add("Quality", "3");
                result.Add("VBRQuality", "2");

                // Call the external ID3 encoder:
                var metadataEncoderFactory = ExtensionProvider<IMetadataEncoder>.Instance.Factories.Where(factory => string.Compare((string)factory.Metadata["Extension"], Extension, StringComparison.OrdinalIgnoreCase) == 0).SingleOrDefault();
                if (metadataEncoderFactory != null)
                    using (ExportLifetimeContext<IMetadataEncoder> metadataEncoderLifetime = metadataEncoderFactory.CreateExport())
                        metadataEncoderLifetime.Value.DefaultSettings.CopyTo(result);

                return result;
            }
        }

        public IReadOnlyCollection<string> AvailableSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyCollection<string>>() != null);

                var partialResult = new List<string>();

                partialResult.Add("AddMetadata");
                partialResult.Add("ApplyGain");
                partialResult.Add("BitRate");
                partialResult.Add("ForceCBR");
                partialResult.Add("Quality");
                partialResult.Add("VBRQuality");

                // Call the external ID3 encoder:
                var metadataEncoderFactory = ExtensionProvider<IMetadataEncoder>.Instance.Factories.Where(factory => string.Compare((string)factory.Metadata["Extension"], Extension, StringComparison.OrdinalIgnoreCase) == 0).SingleOrDefault();
                if (metadataEncoderFactory != null)
                    using (ExportLifetimeContext<IMetadataEncoder> metadataEncoderLifetime = metadataEncoderFactory.CreateExport())
                        partialResult = partialResult.Concat(metadataEncoderLifetime.Value.AvailableSettings).ToList();

                return partialResult.AsReadOnly();
            }
        }

        public void Initialize(Stream stream, AudioInfo audioInfo, MetadataDictionary metadata, SettingsDictionary settings)
        {
            Contract.Ensures(_encoder != null);

            if (string.IsNullOrEmpty(settings["AddMetadata"]) || string.Compare(settings["AddMetadata"], bool.TrueString, StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Call the external ID3 encoder:
                var metadataEncoderFactory = ExtensionProvider<IMetadataEncoder>.Instance.Factories.Where(factory => string.Compare((string)factory.Metadata["Extension"], Extension, StringComparison.OrdinalIgnoreCase) == 0).Single();
                using (ExportLifetimeContext<IMetadataEncoder> metadataEncoderLifetime = metadataEncoderFactory.CreateExport())
                    metadataEncoderLifetime.Value.WriteMetadata(stream, metadata, settings);
            }
            else if (string.Compare(settings["AddMetadata"], bool.FalseString, StringComparison.OrdinalIgnoreCase) != 0)
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderBadAddMetadata, settings["AddMetadata"]));

            _encoder = InitializeEncoder(audioInfo, stream);
            ConfigureEncoder(settings, metadata, _encoder);
            if (_encoder.InitializeParams() != 0)
                throw new IOException(Resources.SampleEncoderFailedToInitialize);
        }

        public bool ManuallyFreesSamples
        {
            get { return false; }
        }

        public void Submit(SampleCollection samples)
        {
            if (!samples.IsLast)
            {
                // If there is only one channel, set the right channel to null:
                float[] rightSamples = samples.Channels == 1 ? null : rightSamples = samples[1];
                _encoder.Encode(samples[0], rightSamples);
            }
            else
            {
                _encoder.Flush();
                _encoder.UpdateLameTag();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _encoder != null)
                _encoder.Dispose();
        }

        static NativeEncoder InitializeEncoder(AudioInfo audioInfo, Stream output)
        {
            Contract.Requires(audioInfo != null);
            Contract.Requires(output != null);
            Contract.Requires(output.CanWrite);
            Contract.Requires(output.CanSeek);
            Contract.Ensures(Contract.Result<NativeEncoder>() != null);

            var result = new NativeEncoder(output);

            result.SetSampleCount((uint)audioInfo.SampleCount);
            result.SetSampleRate(audioInfo.SampleRate);
            result.SetChannels(audioInfo.Channels);

            return result;
        }

        static void ConfigureEncoder(SettingsDictionary settings, MetadataDictionary metadata, NativeEncoder encoder)
        {
            Contract.Requires(settings != null);
            Contract.Requires(metadata != null);
            Contract.Requires(encoder != null);

            // Set the quality if specified, otherwise select "3":
            uint quality;
            if (string.IsNullOrEmpty(settings["Quality"]))
                quality = 3;
            else if (!uint.TryParse(settings["Quality"], out quality) || quality > 9)
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderBadQuality, settings["Quality"]));
            encoder.SetQuality((int)quality);

            // Set a bitrate only if specified. Otherwise, default to a variable bitrate:
            if (!string.IsNullOrEmpty(settings["BitRate"]))
                ConfigureEncoderForBitRate(settings, encoder);
            else
                ConfigureEncoderForQuality(settings, encoder);

            if (!string.IsNullOrEmpty(settings["ApplyGain"]) && string.Compare(settings["ApplyGain"], bool.FalseString, StringComparison.OrdinalIgnoreCase) != 0)
            {
                float scale;

                if (string.Compare(settings["ApplyGain"], "Album", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (string.IsNullOrEmpty(metadata["AlbumGain"]))
                        throw new InvalidSettingException(Resources.SampleEncoderMissingAlbumGain);
                    if (string.IsNullOrEmpty(metadata["AlbumPeak"]))
                        throw new InvalidSettingException(Resources.SampleEncoderMissingAlbumPeak);

                    scale = CalculateScale(metadata["AlbumGain"], metadata["AlbumPeak"]);
                }
                else if (string.Compare(settings["ApplyGain"], "Track", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (string.IsNullOrEmpty(metadata["TrackGain"]))
                        throw new InvalidSettingException(Resources.SampleEncoderMissingTrackGain);
                    if (string.IsNullOrEmpty(metadata["TrackPeak"]))
                        throw new InvalidSettingException(Resources.SampleEncoderMissingTrackPeak);

                    scale = CalculateScale(metadata["TrackGain"], metadata["TrackPeak"]);
                }
                else
                    throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderBadApplyGain, settings["ApplyGain"]));

                encoder.SetScale(scale);

                // Adjust the metadata so that it remains valid:
                metadata["AlbumGain"] = AdjustGain(metadata["AlbumGain"], scale);
                metadata["TrackGain"] = AdjustGain(metadata["TrackGain"], scale);
                metadata["AlbumPeak"] = AdjustPeak(metadata["AlbumPeak"], scale);
                metadata["TrackPeak"] = AdjustPeak(metadata["TrackPeak"], scale);
            }
        }

        static void ConfigureEncoderForBitRate(SettingsDictionary settings, NativeEncoder encoder)
        {
            Contract.Requires(settings != null);
            Contract.Requires(encoder != null);

            uint bitRate;
            if (!uint.TryParse(settings["BitRate"], out bitRate) || bitRate < 8 || bitRate > 320)
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderBadBitRate, settings["BitRate"]));

            // Default to an average bitrate, unless a constant bitrate is specified:
            if (string.IsNullOrEmpty(settings["ForceCBR"]) || string.Compare(settings["ForceCBR"], bool.FalseString, StringComparison.OrdinalIgnoreCase) == 0)
            {
                encoder.SetVbr(VbrMode.Abr);
                encoder.SetMeanBitRate((int)bitRate);
            }
            else if (string.Compare(settings["ForceCBR"], bool.TrueString, StringComparison.OrdinalIgnoreCase) == 0)
                encoder.SetBitRate((int)bitRate);
            else
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderBadForceCBR, settings["ForceCBR"]));
        }

        static void ConfigureEncoderForQuality(SettingsDictionary settings, NativeEncoder encoder)
        {
            Contract.Requires(settings != null);
            Contract.Requires(encoder != null);

            encoder.SetVbr(VbrMode.Mtrh);

            if (!string.IsNullOrEmpty(settings["ForceCBR"]))
                throw new InvalidSettingException(Resources.SampleEncoderUnexpectedForceCBR);

            float vbrQuality;
            if (string.IsNullOrEmpty(settings["VBRQuality"]))
                vbrQuality = 2;
            else if (!float.TryParse(settings["VBRQuality"], out vbrQuality) || vbrQuality < 0 || vbrQuality >= 10)
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderBadVBRQuality, settings["VBRQuality"]));

            encoder.SetVbrQuality(vbrQuality);
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
            Contract.Requires(!string.IsNullOrEmpty(gain));
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

            return string.Format(CultureInfo.InvariantCulture, "{0:0.00} dB", float.Parse(gain.Replace(" dB", string.Empty), CultureInfo.InvariantCulture) - Math.Log10(scale) * 20);
        }

        static string AdjustPeak(string peak, float scale)
        {
            Contract.Requires(!string.IsNullOrEmpty(peak));
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

            return string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", float.Parse(peak, CultureInfo.InvariantCulture) * scale);
        }
    }
}
