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

using PowerShellAudio.Extensions.Lame.Properties;
using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Lame
{
    [SampleEncoderExport("Lame MP3")]
    public class LameSampleEncoder : ISampleEncoder, IDisposable
    {
        static readonly SampleEncoderInfo _encoderInfo = new LameSampleEncoderInfo();

        NativeEncoder _encoder;
        ExportLifetimeContext<ISampleFilter> _replayGainFilterLifetime;

        [NotNull]
        public SampleEncoderInfo EncoderInfo => _encoderInfo;

        public void Initialize(
            [NotNull] Stream stream, 
            [NotNull] AudioInfo audioInfo, 
            [NotNull] MetadataDictionary metadata,
            [NotNull] SettingsDictionary settings)
        {
            // Load the external gain filter:
            ExportFactory<ISampleFilter> sampleFilterFactory =
                ExtensionProvider.GetFactories<ISampleFilter>("Name", "ReplayGain").SingleOrDefault();
            _replayGainFilterLifetime = sampleFilterFactory?.CreateExport()
                ?? throw new ExtensionInitializationException(Resources.SampleEncoderReplayGainFilterError);
            _replayGainFilterLifetime.Value.Initialize(metadata, settings);

            // Call the external ID3 encoder:
            ExportFactory<IMetadataEncoder> metadataEncoderFactory =
                ExtensionProvider.GetFactories<IMetadataEncoder>("Extension", EncoderInfo.FileExtension).Single();
            if (metadataEncoderFactory == null)
                throw new ExtensionInitializationException(string.Format(CultureInfo.CurrentCulture,
                    Resources.SampleEncoderMetadataEncoderError, EncoderInfo.FileExtension));
            using (ExportLifetimeContext<IMetadataEncoder> metadataEncoderLifetime = metadataEncoderFactory.CreateExport())
                metadataEncoderLifetime.Value.WriteMetadata(stream, metadata, settings);

            _encoder = InitializeEncoder(audioInfo, stream);
            ConfigureEncoder(settings, _encoder);
            if (_encoder.InitializeParams() != 0)
                throw new IOException(Resources.SampleEncoderFailedToInitialize);
        }

        public bool ManuallyFreesSamples => false;

        public void Submit(SampleCollection samples)
        {
            if (!samples.IsLast)
            {
                // Filter by ReplayGain, depending on settings:
                _replayGainFilterLifetime.Value.Submit(samples);

                // If there is only one channel, set the right channel to null:
                float[] rightSamples = samples.Channels == 1 ? null : samples[1];
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
            if (!disposing)
                return;

            _encoder?.Dispose();
            _replayGainFilterLifetime?.Dispose();
        }

        [NotNull]
        static NativeEncoder InitializeEncoder([NotNull] AudioInfo audioInfo, [NotNull] Stream output)
        {
            var result = new NativeEncoder(output);

            result.SetSampleCount((uint)audioInfo.SampleCount);
            result.SetSampleRate(audioInfo.SampleRate);
            result.SetChannels(audioInfo.Channels);

            return result;
        }

        static void ConfigureEncoder([NotNull] SettingsDictionary settings, [NotNull] NativeEncoder encoder)
        {
            // Set the quality if specified, otherwise select "3":
            uint quality;
            if (string.IsNullOrEmpty(settings["Quality"]))
                quality = 3;
            else if (!uint.TryParse(settings["Quality"], out quality) || quality > 9)
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture,
                    Resources.SampleEncoderBadQuality, settings["Quality"]));
            encoder.SetQuality((int)quality);

            // Set a bitrate only if specified. Otherwise, default to a variable bitrate:
            if (!string.IsNullOrEmpty(settings["BitRate"]))
                ConfigureEncoderForBitRate(settings, encoder);
            else
                ConfigureEncoderForQuality(settings, encoder);
        }

        static void ConfigureEncoderForBitRate([NotNull] SettingsDictionary settings, [NotNull] NativeEncoder encoder)
        {
            if (!uint.TryParse(settings["BitRate"], out uint bitRate) || bitRate < 8 || bitRate > 320)
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture,
                    Resources.SampleEncoderBadBitRate, settings["BitRate"]));

            // Default to an average bitrate, unless a constant bitrate is specified:
            if (string.IsNullOrEmpty(settings["ForceCBR"]) ||
                string.Compare(settings["ForceCBR"], bool.FalseString, StringComparison.OrdinalIgnoreCase) == 0)
            {
                encoder.SetVbr(VbrMode.Abr);
                encoder.SetMeanBitRate((int)bitRate);
            }
            else if (string.Compare(settings["ForceCBR"], bool.TrueString, StringComparison.OrdinalIgnoreCase) == 0)
                encoder.SetBitRate((int)bitRate);
            else
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture,
                    Resources.SampleEncoderBadForceCBR, settings["ForceCBR"]));
        }

        static void ConfigureEncoderForQuality([NotNull] SettingsDictionary settings, [NotNull] NativeEncoder encoder)
        {
            encoder.SetVbr(VbrMode.Mtrh);

            if (!string.IsNullOrEmpty(settings["ForceCBR"]))
                throw new InvalidSettingException(Resources.SampleEncoderUnexpectedForceCBR);

            float vbrQuality;
            if (string.IsNullOrEmpty(settings["VBRQuality"]))
                vbrQuality = 2;
            else if (!float.TryParse(settings["VBRQuality"], out vbrQuality) || vbrQuality < 0 || vbrQuality >= 10)
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture,
                    Resources.SampleEncoderBadVBRQuality, settings["VBRQuality"]));

            encoder.SetVbrQuality(vbrQuality);
        }
    }
}
