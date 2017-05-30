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
using System.IO;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Lame
{
    class NativeEncoder : IDisposable
    {
        readonly NativeEncoderHandle _handle = SafeNativeMethods.Initialize();
        readonly Stream _output;
        long _beginning;
        byte[] _buffer;

        internal NativeEncoder([NotNull] Stream output)
        {
            _output = output;
        }

        internal void SetSampleCount(uint sampleCount)
        {
            SafeNativeMethods.SetSampleCount(_handle, sampleCount);
        }

        internal void SetSampleRate(int sampleRate)
        {
            SafeNativeMethods.SetSampleRate(_handle, sampleRate);
        }

        internal void SetChannels(int channels)
        {
            SafeNativeMethods.SetChannels(_handle, channels);
        }

        internal void SetQuality(int quality)
        {
            SafeNativeMethods.SetQuality(_handle, quality);
        }

        internal void SetBitRate(int bitRate)
        {
            SafeNativeMethods.SetBitRate(_handle, bitRate);
        }

        internal void SetVbr(VbrMode mode)
        {
            SafeNativeMethods.SetVbr(_handle, mode);
        }

        internal void SetMeanBitRate(int bitRate)
        {
            SafeNativeMethods.SetMeanBitRate(_handle, bitRate);
        }

        internal void SetVbrQuality(float quality)
        {
            SafeNativeMethods.SetVbrQuality(_handle, quality);
        }

        internal int InitializeParams()
        {
            _beginning = _output.Position;

            return SafeNativeMethods.InitializeParams(_handle);
        }

        internal void Encode([NotNull] float[] leftSamples, [CanBeNull] float[] rightSamples)
        {
            if (_buffer == null)
                _buffer = new byte[(int)Math.Ceiling(1.25 * leftSamples.Length) + 7200];

            int bytesEncoded = SafeNativeMethods.EncodeBuffer(_handle, leftSamples, rightSamples, leftSamples.Length, _buffer, _buffer.Length);
            switch (bytesEncoded)
            {
                case -1:
                    throw new IOException(Resources.NativeEncoderBufferError);
                case -2:
                    throw new IOException(Resources.NativeEncoderMemoryError);
                case -4:
                    throw new IOException(Resources.NativeEncoderPsychoacousticError);
                default:
                    _output.Write(_buffer, 0, bytesEncoded);
                    break;
            }
        }

        internal void Flush()
        {
            int bytesFlushed = SafeNativeMethods.Flush(_handle, _buffer, _buffer.Length);
            if (bytesFlushed > 0)
                _output.Write(_buffer, 0, bytesFlushed);
        }

        internal void UpdateLameTag()
        {
            _output.Position = _beginning;
            _output.Write(_buffer, 0, (int)SafeNativeMethods.GetLameTagFrame(_handle, _buffer, new UIntPtr((uint)_buffer.Length)).ToUInt32());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _handle.Dispose();
        }
    }
}
