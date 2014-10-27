/*
 * Copyright © 2014 Jeremy Herbison
 * 
 * This file is part of AudioShell.
 * 
 * AudioShell is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option)
 * any later version.
 * 
 * AudioShell is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for more
 * details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with AudioShell.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

using AudioShell.Extensions.Flac.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;

namespace AudioShell.Extensions.Flac
{
    [SampleEncoderExport("FLAC")]
    public class FlacSampleEncoder : ISampleEncoder, IDisposable
    {
        readonly List<NativeMetadataBlock> _metadataBlocks = new List<NativeMetadataBlock>(2);
        NativeStreamEncoder _encoder;
        float _multiplier;
        int[] _buffer;

        public string Extension
        {
            get { return ".flac"; }
        }

        public SettingsDictionary DefaultSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<SettingsDictionary>() != null);

                var result = new SettingsDictionary();

                result.Add("AddMetadata", bool.TrueString);
                result.Add("CompressionLevel", "5");
                result.Add("SeekPointInterval", "10");

                return result;
            }
        }

        public IReadOnlyCollection<string> AvailableSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyCollection<string>>() != null);

                var result = new List<string>(3);

                result.Add("AddMetadata");
                result.Add("CompressionLevel");
                result.Add("SeekPointInterval");

                return result.AsReadOnly();
            }
        }

        public void Initialize(Stream stream, AudioInfo audioInfo, MetadataDictionary metadata, SettingsDictionary settings)
        {
            Contract.Ensures(_encoder != null);
            Contract.Ensures(_encoder.GetState() == EncoderState.OK);

            _encoder = new NativeStreamEncoder(stream);

            InitializeAudioInfo(audioInfo);

            uint compressionLevel;
            if (string.IsNullOrEmpty(settings["CompressionLevel"]))
                compressionLevel = 5;
            else if (!uint.TryParse(settings["CompressionLevel"], out compressionLevel) || compressionLevel > 8)
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderBadCompressionLevel, settings["CompressionLevel"]));
            _encoder.SetCompressionLevel(compressionLevel);

            // FLAC should default to a block size of 4096 for compression levels >= 3, but it doesn't:
            if (compressionLevel >= 3)
                _encoder.SetBlockSize(4096);

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

                foreach (var metadataBlock in _metadataBlocks)
                    metadataBlock.Dispose();
            }
        }

        void InitializeAudioInfo(AudioInfo audioInfo)
        {
            Contract.Requires(audioInfo != null);
            Contract.Ensures(_multiplier > 0);

            _multiplier = (float)Math.Pow(2, audioInfo.BitsPerSample - 1);

            _encoder.SetChannels((uint)audioInfo.Channels);
            _encoder.SetBitsPerSample((uint)audioInfo.BitsPerSample);
            _encoder.SetSampleRate((uint)audioInfo.SampleRate);
            _encoder.SetTotalSamplesEstimate((ulong)audioInfo.SampleCount);
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_metadataBlocks != null);
            Contract.Invariant(_multiplier >= 0);
        }

    }
}
