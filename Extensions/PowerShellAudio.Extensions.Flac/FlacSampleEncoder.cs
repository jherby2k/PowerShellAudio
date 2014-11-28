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

using PowerShellAudio.Extensions.Flac.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;

namespace PowerShellAudio.Extensions.Flac
{
    [SampleEncoderExport("FLAC")]
    public class FlacSampleEncoder : ISampleEncoder, IDisposable
    {
        static readonly SampleEncoderInfo _encoderInfo = new FlacSampleEncoderInfo();

        NativeStreamEncoder _encoder;
        List<NativeMetadataBlock> _metadataBlocks;
        float _multiplier;
        int[] _buffer;

        public SampleEncoderInfo EncoderInfo
        {
            get
            {
                Contract.Ensures(Contract.Result<SampleEncoderInfo>() != null);

                return _encoderInfo;
            }
        }

        public void Initialize(Stream stream, AudioInfo audioInfo, MetadataDictionary metadata, SettingsDictionary settings)
        {
            Contract.Ensures(_encoder != null);
            Contract.Ensures(_encoder.GetState() == EncoderState.OK);
            Contract.Ensures(_multiplier > 0);

            _encoder = InitializeEncoder(audioInfo, stream);
            _metadataBlocks = new List<NativeMetadataBlock>(2);
            _multiplier = (float)Math.Pow(2, audioInfo.BitsPerSample - 1);

            uint compressionLevel;
            if (string.IsNullOrEmpty(settings["CompressionLevel"]))
                compressionLevel = 5;
            else if (!uint.TryParse(settings["CompressionLevel"], out compressionLevel) || compressionLevel > 8)
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderBadCompressionLevel, settings["CompressionLevel"]));
            _encoder.SetCompressionLevel(compressionLevel);

            if (string.IsNullOrEmpty(settings["AddMetadata"]) || string.Compare(settings["AddMetadata"], bool.TrueString, StringComparison.OrdinalIgnoreCase) == 0)
            {
                var adaptedMetadata = new MetadataToVorbisCommentAdapter(metadata);
                if (adaptedMetadata.Count > 0)
                {
                    var vorbisCommentBlock = new NativeVorbisCommentBlock();
                    _metadataBlocks.Add(vorbisCommentBlock);

                    foreach (var field in adaptedMetadata)
                        vorbisCommentBlock.Append(field.Key, field.Value);
                }
            }
            else if (string.Compare(settings["AddMetadata"], bool.FalseString, StringComparison.OrdinalIgnoreCase) != 0)
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderBadAddMetadata, settings["AddMetadata"]));

            // Add a seek table, unless SeekPointInterval = 0:
            uint seekPointInterval;
            if (string.IsNullOrEmpty(settings["SeekPointInterval"]))
                seekPointInterval = 10;
            else if (!uint.TryParse(settings["SeekPointInterval"], out seekPointInterval))
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderBadSeekPointInterval, settings["SeekPointInterval"]));
            if (seekPointInterval > 0)
            {
                NativeSeekTableBlock seekTableBlock = new NativeSeekTableBlock((int)Math.Ceiling(audioInfo.SampleCount / audioInfo.SampleRate / (double)seekPointInterval), audioInfo.SampleCount);
                _metadataBlocks.Add(seekTableBlock);
            }

            _encoder.SetMetadata(_metadataBlocks);

            EncoderInitStatus initStatus = _encoder.Initialize();
            if (initStatus != EncoderInitStatus.OK)
                throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderInitializationError, initStatus));
        }

        public bool ManuallyFreesSamples
        {
            get { return false; }
        }

        public void Submit(SampleCollection samples)
        {
            Contract.Ensures(_buffer != null);

            if (!samples.IsLast)
            {
                if (_buffer == null)
                    _buffer = new int[samples.Channels * samples.SampleCount];

                // Interlace the samples in integer format, and store them in the input buffer:
                int index = 0;
                for (int sample = 0; sample < samples.SampleCount; sample++)
                    for (int channel = 0; channel < samples.Channels; channel++)
                        _buffer[index++] = (int)Math.Round(samples[channel][sample] * _multiplier);

                if (!_encoder.ProcessInterleaved(_buffer, (uint)samples.SampleCount))
                    throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderEncodingError, _encoder.GetState()));
            }
            else
            {
                _encoder.Finish();

                foreach (var metadataBlock in _metadataBlocks)
                    metadataBlock.Dispose();
                _metadataBlocks.Clear();
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

                if (_metadataBlocks != null)
                    foreach (var metadataBlock in _metadataBlocks)
                        metadataBlock.Dispose();
            }
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_multiplier >= 0);
        }

        static NativeStreamEncoder InitializeEncoder(AudioInfo audioInfo, Stream output)
        {
            Contract.Requires(audioInfo != null);
            Contract.Requires(output != null);
            Contract.Requires(output.CanWrite);
            Contract.Requires(output.CanSeek);

            var result = new NativeStreamEncoder(output);

            result.SetChannels((uint)audioInfo.Channels);
            result.SetBitsPerSample((uint)audioInfo.BitsPerSample);
            result.SetSampleRate((uint)audioInfo.SampleRate);
            result.SetTotalSamplesEstimate((ulong)audioInfo.SampleCount);

            return result;
        }
    }
}
