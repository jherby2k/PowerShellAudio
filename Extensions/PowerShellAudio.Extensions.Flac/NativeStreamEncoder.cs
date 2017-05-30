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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Flac
{
    class NativeStreamEncoder : IDisposable
    {
        readonly NativeStreamEncoderHandle _handle = SafeNativeMethods.StreamEncoderNew();
        readonly Stream _output;
        readonly SafeNativeMethods.StreamEncoderWriteCallback _writeCallback;
        readonly SafeNativeMethods.StreamEncoderSeekCallback _seekCallback;
        readonly SafeNativeMethods.StreamEncoderTellCallback _tellCallback;

        internal NativeStreamEncoder([NotNull] Stream output)
        {
            _output = output;

            _writeCallback = WriteCallback;
            _seekCallback = SeekCallback;
            _tellCallback = TellCallback;
        }

        internal void SetChannels(uint channels)
        {
            SafeNativeMethods.StreamEncoderSetChannels(_handle, channels);
        }

        internal void SetBitsPerSample(uint bitsPerSample)
        {
            SafeNativeMethods.StreamEncoderSetBitsPerSample(_handle, bitsPerSample);
        }

        internal void SetSampleRate(uint sampleRate)
        {
            SafeNativeMethods.StreamEncoderSetSampleRate(_handle, sampleRate);
        }

        internal void SetTotalSamplesEstimate(ulong sampleCount)
        {
            SafeNativeMethods.StreamEncoderSetTotalSamplesEstimate(_handle, sampleCount);
        }

        internal void SetCompressionLevel(uint compressionLevel)
        {
            SafeNativeMethods.StreamEncoderSetCompressionLevel(_handle, compressionLevel);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification = "Can't pass an array of SafeHandles")]
        internal void SetMetadata([NotNull, ItemNotNull] IEnumerable<NativeMetadataBlock> metadataBlocks)
        {
            IntPtr[] blockPointers = metadataBlocks.Select(block => block.Handle.DangerousGetHandle()).ToArray();
            SafeNativeMethods.StreamEncoderSetMetadata(_handle, blockPointers, (uint)blockPointers.Length);
        }

        internal EncoderInitStatus Initialize()
        {
            return SafeNativeMethods.StreamEncoderInitialize(_handle, _writeCallback, _seekCallback, _tellCallback, null,
                IntPtr.Zero);
        }

        internal bool ProcessInterleaved([NotNull] int[] buffer, uint sampleCount)
        {
            return SafeNativeMethods.StreamEncoderProcessInterleaved(_handle, buffer, sampleCount);
        }

        internal void Finish()
        {
            SafeNativeMethods.StreamEncoderFinish(_handle);
        }

        [Pure]
        internal EncoderState GetState()
        {
            return SafeNativeMethods.StreamEncoderGetState(_handle);
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

        EncoderWriteStatus WriteCallback(IntPtr handle, [NotNull] byte[] buffer, int bytes, uint samples, uint currentFrame, IntPtr userData)
        {
            _output.Write(buffer, 0, bytes);
            return EncoderWriteStatus.Ok;
        }

        EncoderSeekStatus SeekCallback(IntPtr handle, ulong absoluteOffset, IntPtr userData)
        {
            _output.Position = (long)absoluteOffset;
            return EncoderSeekStatus.Ok;
        }

        EncoderTellStatus TellCallback(IntPtr handle, out ulong absoluteOffset, IntPtr userData)
        {
            absoluteOffset = (ulong)_output.Position;
            return EncoderTellStatus.Ok;
        }
    }
}
