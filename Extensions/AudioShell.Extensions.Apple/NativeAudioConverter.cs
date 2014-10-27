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

using AudioShell.Extensions.Apple.Properties;
using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace AudioShell.Extensions.Apple
{
    class NativeAudioConverter : IDisposable
    {
        readonly NativeAudioConverterHandle _handle;
        readonly SafeNativeMethods.AudioConverterComplexInputCallback _inputCallback;
        readonly NativeAudioFile _audioFile;
        long _packetIndex;
        byte[] _buffer;
        GCHandle _bufferHandle;
        GCHandle _descriptionsHandle;

        internal NativeAudioConverter(ref AudioStreamBasicDescription inputDescription, ref AudioStreamBasicDescription outputDescription, NativeAudioFile audioFile)
        {
            Contract.Requires(audioFile != null);
            Contract.Ensures(_handle != null);
            Contract.Ensures(!_handle.IsClosed);
            Contract.Ensures(_inputCallback != null);
            Contract.Ensures(_audioFile != null);
            Contract.Ensures(_audioFile == audioFile);

            AudioConverterStatus status = SafeNativeMethods.AudioConverterNew(ref inputDescription, ref outputDescription, out _handle);
            if (status != AudioConverterStatus.OK)
                throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.NativeAudioConverterInitializationError, status));

            _inputCallback = new SafeNativeMethods.AudioConverterComplexInputCallback(InputCallback);

            _audioFile = audioFile;
        }

        internal AudioConverterStatus FillBuffer(ref uint packetSize, ref AudioBufferList outputBuffer, AudioStreamPacketDescription[] packetDescriptions)
        {
            return SafeNativeMethods.AudioConverterFillComplexBuffer(_handle, _inputCallback, IntPtr.Zero, ref packetSize, ref outputBuffer, packetDescriptions);
        }

        internal AudioConverterStatus SetProperty(AudioConverterPropertyID propertyID, uint size, IntPtr data)
        {
            return SafeNativeMethods.AudioConverterSetProperty(_handle, propertyID, size, data);
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
                if (_bufferHandle.IsAllocated)
                    _bufferHandle.Free();
                if (_descriptionsHandle.IsAllocated)
                    _descriptionsHandle.Free();
                _handle.Dispose();
                _audioFile.Dispose();
            }
        }

        AudioConverterStatus InputCallback(IntPtr handle, ref uint numberPackets, ref AudioBufferList data, IntPtr packetDescriptions, IntPtr userData)
        {
            if (_buffer == null)
            {
                _buffer = new byte[numberPackets * _audioFile.GetProperty<uint>(AudioFilePropertyID.PacketSizeUpperBound)];
                _bufferHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
            }

            if (_descriptionsHandle.IsAllocated)
                _descriptionsHandle.Free();

            uint numBytes;
            var inputDescriptions = new AudioStreamPacketDescription[numberPackets];
            AudioFileStatus status = _audioFile.ReadPackets(out numBytes, inputDescriptions, _packetIndex, ref numberPackets, _bufferHandle.AddrOfPinnedObject());
            if (status != AudioFileStatus.OK)
                throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.NativeAudioConverterReadError, status));

            _packetIndex += numberPackets;

            data.Buffers[0].DataByteSize = numBytes;
            data.Buffers[0].Data = _bufferHandle.AddrOfPinnedObject();

            // If this conversion requires packet descriptions, provide them:
            if (packetDescriptions != IntPtr.Zero)
            {
                _descriptionsHandle = GCHandle.Alloc(inputDescriptions, GCHandleType.Pinned);
                Marshal.WriteIntPtr(packetDescriptions, _descriptionsHandle.AddrOfPinnedObject());
            }

            return AudioConverterStatus.OK;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(!_handle.IsInvalid);
            Contract.Invariant(_inputCallback != null);
            Contract.Invariant(_audioFile != null);
        }
    }
}
