﻿/*
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

using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace PowerShellAudio.Extensions.Wave
{
    [SampleEncoderExport("Wave")]
    public class WaveSampleEncoder : ISampleEncoder, IDisposable
    {
        static readonly SampleEncoderInfo _encoderInfo = new WaveSampleEncoderInfo();

        readonly byte[] _buffer = new byte[4];
        RiffWriter _writer;
        int _channels;
        int _bytesPerSample;
        float _multiplier;

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
            Contract.Ensures(_writer != null);
            Contract.Ensures(_writer.BaseStream == stream);
            Contract.Ensures(_channels == audioInfo.Channels);
            Contract.Ensures(_bytesPerSample > 0);
            Contract.Ensures(_multiplier > 0);

            _writer = new RiffWriter(stream, "WAVE");
            _channels = audioInfo.Channels;
            _bytesPerSample = (int)Math.Ceiling(audioInfo.BitsPerSample / (double)8);
            _multiplier = (float)Math.Pow(2, audioInfo.BitsPerSample - 1);

            _writer.Initialize();
            WriteFmtChunk(_writer, audioInfo, _bytesPerSample);
            _writer.BeginChunk("data");
        }

        public bool ManuallyFreesSamples => false;

        public void Submit(SampleCollection samples)
        {
            if (!samples.IsLast)
            {
                if (_bytesPerSample == 1)
                {
                    // 1-8 bit samples are unsigned:
                    for (var sample = 0; sample < samples.SampleCount; sample++)
                        for (var channel = 0; channel < _channels; channel++)
                            _writer.Write((byte)Math.Round(samples[channel][sample] * _multiplier + 128));
                }
                else
                {
                    for (var sample = 0; sample < samples.SampleCount; sample++)
                        for (var channel = 0; channel < _channels; channel++)
                        {
                            // Optimization - BitConverter wastes memory because you can't reuse the array:
                            var int32Value = (int)Math.Round(samples[channel][sample] * _multiplier);
                            ConvertInt32ToBytes(int32Value, _buffer);
                            _writer.Write(_buffer, 0, _bytesPerSample);
                        }
                }
            }
            else
            {
                // Finish the data and RIFF chunks:
                _writer.FinishChunk();
                _writer.FinishChunk();
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
            try
            {
                _writer?.Dispose();
            }
            catch (ObjectDisposedException)
            {
                //TODO not sure this is necessary
            }
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_buffer != null);
            Contract.Invariant(_buffer.Length == 4);
        }

        static void WriteFmtChunk(RiffWriter writer, AudioInfo audioInfo, int bytesPerSample)
        {
            Contract.Requires(writer != null);
            Contract.Requires(audioInfo != null);
            Contract.Requires(bytesPerSample > 0);

            writer.BeginChunk("fmt ", 16);
            writer.Write((ushort)1);
            writer.Write((ushort)audioInfo.Channels);
            writer.Write((uint)audioInfo.SampleRate);
            writer.Write((uint)(bytesPerSample * audioInfo.Channels * audioInfo.SampleRate));
            writer.Write((ushort)(bytesPerSample * audioInfo.Channels));
            writer.Write((ushort)audioInfo.BitsPerSample);
            writer.FinishChunk();
        }

        static void ConvertInt32ToBytes(int value, byte[] buffer)
        {
            Contract.Requires(buffer != null);
            Contract.Requires(buffer.Length == 4);

            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
        }
    }
}
