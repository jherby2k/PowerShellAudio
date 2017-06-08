/*
 * Copyright © 2014-2017 Jeremy Herbison
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

using PowerShellAudio.Extensions.Flac.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Flac
{
    [SampleEncoderExport("FLAC")]
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Loaded via reflection")]
    sealed class FlacSampleEncoder : ISampleEncoder, IDisposable
    {
        static readonly SampleEncoderInfo _encoderInfo = new FlacSampleEncoderInfo();

        NativeStreamEncoder _encoder;
        List<NativeMetadataBlock> _metadataBlocks;
        float _multiplier;
        int[] _buffer;

        [NotNull]
        public SampleEncoderInfo EncoderInfo => _encoderInfo;

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Native blocks are added to a collection for disposal later.")]
        public void Initialize(
            [NotNull] Stream stream,
            [NotNull] AudioInfo audioInfo,
            [NotNull] MetadataDictionary metadata,
            [NotNull] SettingsDictionary settings)
        {
            _encoder = InitializeEncoder(audioInfo, stream);
            _metadataBlocks = new List<NativeMetadataBlock>(3); // Assumes one metadata block, one picture and one seek table
            _multiplier = (float)Math.Pow(2, audioInfo.BitsPerSample - 1);

            uint compressionLevel;
            if (string.IsNullOrEmpty(settings["CompressionLevel"]))
                compressionLevel = 5;
            else if (!uint.TryParse(settings["CompressionLevel"], out compressionLevel) || compressionLevel > 8)
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture,
                    Resources.SampleEncoderBadCompressionLevel, settings["CompressionLevel"]));
            _encoder.SetCompressionLevel(compressionLevel);

            var vorbisCommentBlock = new NativeVorbisCommentBlock();
            foreach (var field in new MetadataToVorbisCommentAdapter(metadata))
                vorbisCommentBlock.Append(field.Key, field.Value);
            _metadataBlocks.Add(vorbisCommentBlock);

            // Add a seek table, unless SeekPointInterval = 0:
            uint seekPointInterval;
            if (string.IsNullOrEmpty(settings["SeekPointInterval"]))
                seekPointInterval = 10;
            else if (!uint.TryParse(settings["SeekPointInterval"], out seekPointInterval))
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture,
                    Resources.SampleEncoderBadSeekPointInterval, settings["SeekPointInterval"]));
            if (seekPointInterval > 0)
                _metadataBlocks.Add(new NativeSeekTableBlock(
                    (int)Math.Ceiling(audioInfo.SampleCount / (double)audioInfo.SampleRate / seekPointInterval),
                    audioInfo.SampleCount));

            // Add a picture block, if cover art is available:
            if (metadata.CoverArt != null)
                _metadataBlocks.Add(new CoverArtToPictureBlockAdapter(metadata.CoverArt));

            _encoder.SetMetadata(_metadataBlocks);

            EncoderInitStatus initStatus = _encoder.Initialize();
            if (initStatus != EncoderInitStatus.Ok)
                throw new IOException(string.Format(CultureInfo.CurrentCulture,
                    Resources.SampleEncoderInitializationError, initStatus));
        }

        public bool ManuallyFreesSamples => false;

        public void Submit([NotNull] SampleCollection samples)
        {
            if (!samples.IsLast)
            {
                if (_buffer == null)
                    _buffer = new int[samples.Channels * samples.SampleCount];

                // Interlace the samples in integer format, and store them in the input buffer:
                var index = 0;
                for (var sample = 0; sample < samples.SampleCount; sample++)
                    for (var channel = 0; channel < samples.Channels; channel++)
                        _buffer[index++] = (int)Math.Round(samples[channel][sample] * _multiplier);

                if (!_encoder.ProcessInterleaved(_buffer, (uint)samples.SampleCount))
                    throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderEncodingError, _encoder.GetState()));
            }
            else
            {
                _encoder.Finish();

                foreach (NativeMetadataBlock metadataBlock in _metadataBlocks)
                    metadataBlock.Dispose();
                _metadataBlocks.Clear();
            }
        }

        public void Dispose()
        {
            _encoder?.Dispose();

            if (_metadataBlocks == null)
                return;

            foreach (NativeMetadataBlock metadataBlock in _metadataBlocks)
                metadataBlock.Dispose();
        }

        [NotNull]
        static NativeStreamEncoder InitializeEncoder([NotNull] AudioInfo audioInfo, [NotNull] Stream output)
        {
            var result = new NativeStreamEncoder(output);

            result.SetChannels((uint)audioInfo.Channels);
            result.SetBitsPerSample((uint)audioInfo.BitsPerSample);
            result.SetSampleRate((uint)audioInfo.SampleRate);
            result.SetTotalSamplesEstimate((ulong)audioInfo.SampleCount);

            return result;
        }
    }
}
