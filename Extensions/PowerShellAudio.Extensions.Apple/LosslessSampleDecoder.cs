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

using PowerShellAudio.Extensions.Apple.Properties;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Apple
{
    [SampleDecoderExport(".m4a")]
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Loaded via reflection")]
    sealed class LosslessSampleDecoder : ISampleDecoder, IDisposable
    {
        AudioStreamBasicDescription _inputDescription;
        float _divisor;
        NativeAudioConverter _converter;
        IntPtr _magicCookie;
        int[] _buffer;

        public void Initialize([NotNull] Stream stream)
        {
            try
            {
                var audioFile = new NativeAudioFile(AudioFileType.M4A, stream);

                _inputDescription = audioFile.GetProperty<AudioStreamBasicDescription>(AudioFilePropertyId.DataFormat);
                if (_inputDescription.AudioFormat != AudioFormat.AppleLossless)
                    throw new UnsupportedAudioException(Resources.LosslessSampleDecoderFormatError);

                AudioStreamBasicDescription outputDescription = InitializeOutputDescription(_inputDescription);

                _divisor = (float)Math.Pow(2, outputDescription.BitsPerChannel - 1);
                _converter = new NativeAudioConverter(ref _inputDescription, ref outputDescription, audioFile);
                _magicCookie = InitializeMagicCookie(audioFile, _converter);
            }
            catch (TypeInitializationException e)
            {
                if (e.InnerException != null && e.InnerException.GetType() == typeof(ExtensionInitializationException))
                    throw e.InnerException;
                throw;
            }
        }

        [NotNull]
        public SampleCollection DecodeSamples()
        {
            uint sampleCount = 4096;

            if (_buffer == null)
                _buffer = new int[sampleCount * _inputDescription.ChannelsPerFrame];

            GCHandle handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);

            try
            {
                var bufferList = new AudioBufferList
                {
                    NumberBuffers = 1,
                    Buffers = new AudioBuffer[1]
                };
                bufferList.Buffers[0].NumberChannels = _inputDescription.ChannelsPerFrame;
                bufferList.Buffers[0].DataByteSize = (uint)_buffer.Length;
                bufferList.Buffers[0].Data = handle.AddrOfPinnedObject();

                AudioConverterStatus status = _converter.FillBuffer(ref sampleCount, ref bufferList, null);
                if (status != AudioConverterStatus.Ok)
                    throw new IOException(string.Format(CultureInfo.CurrentCulture,
                        Resources.LosslessSampleDecoderFillBufferError, status));

                SampleCollection result =
                    SampleCollectionFactory.Instance.Create((int)_inputDescription.ChannelsPerFrame, (int)sampleCount);

                // De-interlace the output buffer into the new sample collection, converting to floating point values:
                var index = 0;
                for (var sample = 0; sample < result.SampleCount; sample++)
                    for (var channel = 0; channel < result.Channels; channel++)
                        result[channel][sample] = _buffer[index++] / _divisor;

                return result;
            }
            finally
            {
                handle.Free();
            }
        }

        public void Dispose()
        {
            _converter?.Dispose();
            Marshal.FreeHGlobal(_magicCookie);
        }

        static AudioStreamBasicDescription InitializeOutputDescription(AudioStreamBasicDescription inputDescription)
        {
            uint bitsPerSample;
            switch (inputDescription.Flags)
            {
                case AudioFormatFlags.Alac16BitSourceData:
                    bitsPerSample = 16;
                    break;
                case AudioFormatFlags.Alac20BitSourceData:
                    bitsPerSample = 20;
                    break;
                case AudioFormatFlags.Alac24BitSourceData:
                    bitsPerSample = 24;
                    break;
                case AudioFormatFlags.Alac32BitSourceData:
                    bitsPerSample = 32;
                    break;
                default:
                    throw new IOException(string.Format(CultureInfo.CurrentCulture,
                        Resources.LosslessSampleDecoderFlagsError, inputDescription.Flags));
            }

            return new AudioStreamBasicDescription
            {
                AudioFormat = AudioFormat.LinearPcm,
                Flags = AudioFormatFlags.PcmIsSignedInteger,
                BytesPerPacket = sizeof(int) * inputDescription.ChannelsPerFrame,
                FramesPerPacket = 1,
                BytesPerFrame = sizeof(int) * inputDescription.ChannelsPerFrame,
                ChannelsPerFrame = inputDescription.ChannelsPerFrame,
                BitsPerChannel = bitsPerSample,
                SampleRate = inputDescription.SampleRate
            };
        }

        static IntPtr InitializeMagicCookie([NotNull] NativeAudioFile audioFile, [NotNull] NativeAudioConverter converter)
        {
            AudioFileStatus getStatus = audioFile.GetPropertyInfo(AudioFilePropertyId.MagicCookieData,
                out uint dataSize, out _);
            if (getStatus != AudioFileStatus.Ok)
                throw new IOException(string.Format(CultureInfo.CurrentCulture,
                    Resources.LosslessSampleDecoderGetCookieInfoError, getStatus));

            if (dataSize == 0)
                return IntPtr.Zero;

            IntPtr cookie = audioFile.GetProperty(AudioFilePropertyId.MagicCookieData, dataSize);

            AudioConverterStatus setStatus = converter.SetProperty(AudioConverterPropertyId.DecompressionMagicCookie,
                dataSize, cookie);
            if (setStatus != AudioConverterStatus.Ok)
                throw new IOException(string.Format(CultureInfo.CurrentCulture,
                    Resources.LosslessSampleDecoderSetCookieError, setStatus));

            return cookie;
        }
    }
}
