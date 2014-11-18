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
        ExportLifetimeContext<ISampleFilter> _replayGainFilterLifetime;

        public SampleEncoderInfo EncoderInfo
        {
            get
            {
                Contract.Ensures(Contract.Result<SampleEncoderInfo>() != null);

                return new LameEncoderInfo();
            }
        }

        public void Initialize(Stream stream, AudioInfo audioInfo, MetadataDictionary metadata, SettingsDictionary settings)
        {
            Contract.Ensures(_encoder != null);

            // Load the external gain filter:
            var sampleFilterFactory = ExtensionProvider.GetFactories<ISampleFilter>().Where(factory => string.Compare((string)factory.Metadata["Name"], "ReplayGain", StringComparison.OrdinalIgnoreCase) == 0).SingleOrDefault();
            if (sampleFilterFactory == null)
                throw new ExtensionInitializationException(Resources.SampleEncoderReplayGainFilterError);
            _replayGainFilterLifetime = sampleFilterFactory.CreateExport();
            _replayGainFilterLifetime.Value.Initialize(metadata, settings);

            if (string.IsNullOrEmpty(settings["AddMetadata"]) || string.Compare(settings["AddMetadata"], bool.TrueString, StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Call the external ID3 encoder:
                var metadataEncoderFactory = ExtensionProvider.GetFactories<IMetadataEncoder>().Where(factory => string.Compare((string)factory.Metadata["Extension"], EncoderInfo.FileExtension, StringComparison.OrdinalIgnoreCase) == 0).Single();
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
                // Filter by ReplayGain, depending on settings:
                _replayGainFilterLifetime.Value.Submit(samples);

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
            if (disposing)
            {
                if (_encoder != null)
                    _encoder.Dispose();
                if (_replayGainFilterLifetime != null)
                    _replayGainFilterLifetime.Dispose();
            }
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
    }
}
