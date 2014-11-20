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

using PowerShellAudio.Extensions.Apple.Properties;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace PowerShellAudio.Extensions.Apple
{
    [SampleEncoderExport("Apple Lossless")]
    public class LosslessSampleEncoder : ISampleEncoder, IDisposable
    {
        static readonly SampleEncoderInfo _encoderInfo = new LosslessSampleEncoderInfo();

        Stream _stream;
        float _multiplier;
        MetadataDictionary _metadata;
        SettingsDictionary _settings;
        NativeExtendedAudioFile _audioFile;
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
            Contract.Ensures(_stream != null);
            Contract.Ensures(_stream == stream);
            Contract.Ensures(_metadata != null);
            Contract.Ensures(_metadata == metadata);
            Contract.Ensures(_settings != null);
            Contract.Ensures(_settings == settings);
            Contract.Ensures(_audioFile != null);

            _stream = stream;
            _multiplier = (float)Math.Pow(2, audioInfo.BitsPerSample - 1);
            _metadata = metadata;
            _settings = settings;

            AudioStreamBasicDescription inputDescription = GetInputDescription(audioInfo);
            AudioStreamBasicDescription outputDescription = GetOutputDescription(inputDescription);

            try
            {
                _audioFile = new NativeExtendedAudioFile(outputDescription, AudioFileType.M4a, stream);

                ExtendedAudioFileStatus status = _audioFile.SetProperty<AudioStreamBasicDescription>(ExtendedAudioFilePropertyID.ClientDataFormat, inputDescription);
                if (status != ExtendedAudioFileStatus.OK)
                    throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderInitializationError, status));
            }
            catch (TypeInitializationException e)
            {
                if (e.InnerException != null && e.InnerException.GetType() == typeof(ExtensionInitializationException))
                    throw e.InnerException;
                throw;
            }
        }

        public bool ManuallyFreesSamples
        {
            get { return false; }
        }

        public void Submit(SampleCollection samples)
        {
            Contract.Ensures(_buffer != null);

            if (_buffer == null)
                _buffer = new int[samples.SampleCount * samples.Channels];

            if (!samples.IsLast)
            {
                int index = 0;
                for (int sample = 0; sample < samples.SampleCount; sample++)
                    for (int channel = 0; channel < samples.Channels; channel++)
                        _buffer[index++] = (int)Math.Round(samples[channel][sample] * _multiplier);

                GCHandle handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);

                try
                {
                    var bufferList = new AudioBufferList() { NumberBuffers = 1 };
                    bufferList.Buffers = new AudioBuffer[1];
                    bufferList.Buffers[0].NumberChannels = (uint)samples.Channels;
                    bufferList.Buffers[0].DataByteSize = (uint)(index * Marshal.SizeOf<int>());
                    bufferList.Buffers[0].Data = handle.AddrOfPinnedObject();

                    ExtendedAudioFileStatus status = _audioFile.Write(bufferList, (uint)samples.SampleCount);
                    if (status != ExtendedAudioFileStatus.OK)
                        throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderWriteError, status));
                }
                finally
                {
                    handle.Free();
                }
            }
            else
            {
                _audioFile.Dispose();

                // Call an external MP4 encoder for writing iTunes-compatible atoms:
                _stream.Position = 0;
                var metadataEncoderFactory = ExtensionProvider.GetFactories<IMetadataEncoder>("Extension", EncoderInfo.FileExtension).SingleOrDefault();
                if (metadataEncoderFactory == null)
                    throw new ExtensionInitializationException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderMetadataEncoderError, EncoderInfo.FileExtension));
                using (ExportLifetimeContext<IMetadataEncoder> metadataEncoderLifetime = metadataEncoderFactory.CreateExport())
                    metadataEncoderLifetime.Value.WriteMetadata(_stream, _metadata, _settings);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _audioFile != null)
                _audioFile.Dispose();
        }

        static AudioStreamBasicDescription GetInputDescription(AudioInfo audioInfo)
        {
            Contract.Requires(audioInfo != null);

            return new AudioStreamBasicDescription() { SampleRate = audioInfo.SampleRate, AudioFormat = AudioFormat.LinearPcm, Flags = AudioFormatFlags.PcmIsSignedInteger, BytesPerPacket = 4 * (uint)audioInfo.Channels, FramesPerPacket = 1, BytesPerFrame = 4 * (uint)audioInfo.Channels, ChannelsPerFrame = (uint)audioInfo.Channels, BitsPerChannel = (uint)audioInfo.BitsPerSample };
        }

        static AudioStreamBasicDescription GetOutputDescription(AudioStreamBasicDescription inputDescription)
        {
            var result = new AudioStreamBasicDescription() { SampleRate = inputDescription.SampleRate, FramesPerPacket = 4096, AudioFormat = AudioFormat.AppleLossless, ChannelsPerFrame = inputDescription.ChannelsPerFrame };

            // Some sample rates aren't supported on output, so a best match should be made:
            switch ((int)inputDescription.BitsPerChannel)
            {
                case 16:
                    result.Flags = AudioFormatFlags.Alac16BitSourceData;
                    break;
                case 20:
                    result.Flags = AudioFormatFlags.Alac20BitSourceData;
                    break;
                case 24:
                    result.Flags = AudioFormatFlags.Alac24BitSourceData;
                    break;
                case 32:
                    result.Flags = AudioFormatFlags.Alac32BitSourceData;
                    break;
                default:
                    throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.LosslessSampleEncoderBitRateError, inputDescription.BitsPerChannel));
            }

            return result;
        }
    }
}
