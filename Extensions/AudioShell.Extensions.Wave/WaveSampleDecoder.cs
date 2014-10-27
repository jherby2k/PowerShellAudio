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

using AudioShell.Extensions.Wave.Properties;
using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace AudioShell.Extensions.Wave
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

        public void Initialize(Stream stream)
        {
            Contract.Ensures(_reader != null);
            Contract.Ensures(_reader.BaseStream == stream);
            Contract.Ensures(_channels > 0);
            Contract.Ensures(_samplesRemaining >= 0);
            Contract.Ensures(_bytesPerSample > 0);
            Contract.Ensures(_divisor > 0);

            _reader = new RiffReader(stream);

            if (!_reader.Validate() || _reader.ReadFourCC() != "WAVE")
                throw new UnsupportedAudioException(Resources.SampleDecoderNotWaveError);

            uint fmtChunkSize = _reader.SeekToChunk("fmt ");
            if (fmtChunkSize == 0)
                throw new IOException(Resources.SampleDecoderMissingFmtError);
            if (fmtChunkSize < 16)
                throw new IOException(Resources.SampleDecoderFmtLengthError);

            if (_reader.ReadUInt16() != 1)
                throw new UnsupportedAudioException(Resources.SampleDecoderUnsupportedError);

            _channels = _reader.ReadUInt16();
            if (_channels == 0 || _channels > 2)
                throw new UnsupportedAudioException(Resources.SampleDecoderChannelsError);

            stream.Seek(8, SeekOrigin.Current); // Ignore sampleRate and bytesPerSecond

            ushort blockAlign = _reader.ReadUInt16();

            uint bitsPerSample = _reader.ReadUInt16();

            _bytesPerSample = (int)Math.Ceiling(bitsPerSample / (double)8);
            _divisor = (float)Math.Pow(2, bitsPerSample - 1);

            uint dataChunkSize = _reader.SeekToChunk("data");

            if (dataChunkSize == 0)
                _samplesRemaining = 0;
            else
                _samplesRemaining = dataChunkSize / blockAlign;
        }

        public SampleCollection DecodeSamples()
        {
            Contract.Ensures(Contract.Result<SampleCollection>() != null);
            Contract.Ensures(Contract.Result<SampleCollection>().SampleCount <= _samplesPerResult);

            if (_samplesRemaining == 0)
                return SampleCollectionFactory.Instance.Create(_channels, 0);

            SampleCollection result = SampleCollectionFactory.Instance.Create(_channels, (int)Math.Min(_samplesRemaining, _samplesPerResult));

            if (_bytesPerSample == 1)
            {
                // 1-8 bit samples are unsigned:
                for (int sample = 0; sample < result.SampleCount; sample++)
                    for (int channel = 0; channel < _channels; channel++)
                        result[channel][sample] = (_reader.ReadByte() - 128) / _divisor;
            }
            else
            {
                for (int sample = 0; sample < result.SampleCount; sample++)
                    for (int channel = 0; channel < _channels; channel++)
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
            if (disposing && _reader != null)
                _reader.Dispose();
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_buffer != null);
            Contract.Invariant(_buffer.Length == 4);
            Contract.Invariant(_channels >= 0);
            Contract.Invariant(_samplesRemaining >= 0);
            Contract.Invariant(_bytesPerSample >= 0);
            Contract.Invariant(_divisor >= 0);
        }
    }
}
