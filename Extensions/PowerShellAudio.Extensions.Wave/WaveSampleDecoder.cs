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

using PowerShellAudio.Extensions.Wave.Properties;
using System;
using System.IO;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Wave
{
    [SampleDecoderExport(".wav")]
    public class WaveSampleDecoder : ISampleDecoder, IDisposable
    {
        const int _samplesPerResult = 4096;

        readonly byte[] _buffer = new byte[4];
        RiffReader _reader;
        int _channels;
        long _samplesRemaining;
        int _bytesPerSample;
        float _divisor;

        public void Initialize([NotNull] Stream stream)
        {
            _reader = new RiffReader(stream);

            if (!_reader.Validate() || _reader.ReadFourCC() != "WAVE")
                throw new UnsupportedAudioException(Resources.SampleDecoderNotWaveError);

            uint fmtChunkSize = _reader.SeekToChunk("fmt ");
            if (fmtChunkSize == 0)
                throw new IOException(Resources.SampleDecoderMissingFmtError);
            if (fmtChunkSize < 16)
                throw new IOException(Resources.SampleDecoderFmtLengthError);

            var format = (Format)_reader.ReadUInt16();
            if (format != Format.Pcm && format != Format.Extensible)
                throw new UnsupportedAudioException(Resources.SampleDecoderUnsupportedError);

            // WAVEFORMATEXTENSIBLE headers are 40 bytes long:
            if (format == Format.Extensible && fmtChunkSize < 40)
                throw new IOException(Resources.SampleDecoderFmtLengthError);

            _channels = _reader.ReadUInt16();
            if (_channels == 0 || _channels > 2)
                throw new UnsupportedAudioException(Resources.SampleDecoderChannelsError);

            // Ignore sampleRate and bytesPerSecond:
            stream.Seek(8, SeekOrigin.Current);

            ushort blockAlign = _reader.ReadUInt16();
            uint bitsPerSample = _reader.ReadUInt16();

            // Read the WAVEFORMATEXTENSIBLE extended header, if present:
            if (format == Format.Extensible)
            {
                if (_reader.ReadUInt16() < 22)
                    throw new UnsupportedAudioException(Resources.SampleDecoderFmtExtensionLengthError);

                if (bitsPerSample % 8 != 0)
                    throw new UnsupportedAudioException(Resources.SampleDecoderBitsPerSampleError);
                bitsPerSample = _reader.ReadUInt16();

                // Ignore the channel mask for now:
                stream.Seek(4, SeekOrigin.Current);

                if ((Format)_reader.ReadUInt16() != Format.Pcm)
                    throw new UnsupportedAudioException(Resources.SampleDecoderUnsupportedError);
            }

            _bytesPerSample = (int)Math.Ceiling(bitsPerSample / (double)8);
            _divisor = (float)Math.Pow(2, bitsPerSample - 1);

            uint dataChunkSize = _reader.SeekToChunk("data");

            if (dataChunkSize == 0)
                _samplesRemaining = 0;
            else
                _samplesRemaining = dataChunkSize / blockAlign;
        }

        [NotNull]
        public SampleCollection DecodeSamples()
        {
            if (_samplesRemaining == 0)
                return SampleCollectionFactory.Instance.Create(_channels, 0);

            SampleCollection result = SampleCollectionFactory.Instance.Create(_channels,
                (int)Math.Min(_samplesRemaining, _samplesPerResult));

            if (_bytesPerSample == 1)
            {
                // 1-8 bit samples are unsigned:
                for (var sample = 0; sample < result.SampleCount; sample++)
                    for (var channel = 0; channel < _channels; channel++)
                        result[channel][sample] = (_reader.ReadByte() - 128) / _divisor;
            }
            else
            {
                for (var sample = 0; sample < result.SampleCount; sample++)
                    for (var channel = 0; channel < _channels; channel++)
                    {
                        if (_reader.Read(_buffer, 4 - _bytesPerSample, _bytesPerSample) != _bytesPerSample)
                            throw new IOException(Resources.SampleDecoderEndOfStreamError);
                        int intValue = BitConverter.ToInt32(_buffer, 0) >> (4 - _bytesPerSample) * 8;
                        result[channel][sample] = intValue / _divisor;
                    }
            }

            _samplesRemaining -= result.SampleCount;
            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _reader?.Dispose();
        }
    }
}
