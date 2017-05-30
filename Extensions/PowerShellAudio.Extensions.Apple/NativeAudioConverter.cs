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

using PowerShellAudio.Extensions.Apple.Properties;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Apple
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

        internal NativeAudioConverter(
            ref AudioStreamBasicDescription inputDescription,
            ref AudioStreamBasicDescription outputDescription,
            [NotNull] NativeAudioFile audioFile)
        {
            AudioConverterStatus status = SafeNativeMethods.AudioConverterNew(ref inputDescription,
                ref outputDescription, out _handle);
            if (status != AudioConverterStatus.Ok)
                throw new IOException(string.Format(CultureInfo.CurrentCulture,
                    Resources.NativeAudioConverterInitializationError, status));

            _inputCallback = InputCallback;

            _audioFile = audioFile;
        }

        internal AudioConverterStatus FillBuffer(
            ref uint packetSize,
            ref AudioBufferList outputBuffer,
            [NotNull] AudioStreamPacketDescription[] packetDescriptions)
        {
            return SafeNativeMethods.AudioConverterFillComplexBuffer(_handle, _inputCallback, IntPtr.Zero,
                ref packetSize, ref outputBuffer, packetDescriptions);
        }

        internal AudioConverterStatus SetProperty(AudioConverterPropertyId propertyId, uint size, IntPtr data)
        {
            return SafeNativeMethods.AudioConverterSetProperty(_handle, propertyId, size, data);
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
                _buffer = new byte[numberPackets * _audioFile.GetProperty<uint>(AudioFilePropertyId.PacketSizeUpperBound)];
                _bufferHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
            }

            if (_descriptionsHandle.IsAllocated)
                _descriptionsHandle.Free();

            var inputDescriptions = new AudioStreamPacketDescription[numberPackets];
            AudioFileStatus status = _audioFile.ReadPackets(out uint numBytes, inputDescriptions, _packetIndex,
                ref numberPackets, _bufferHandle.AddrOfPinnedObject());
            if (status != AudioFileStatus.Ok)
                throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.NativeAudioConverterReadError,
                    status));

            _packetIndex += numberPackets;

            data.Buffers[0].DataByteSize = numBytes;
            data.Buffers[0].Data = _bufferHandle.AddrOfPinnedObject();

            // If this conversion requires packet descriptions, provide them:
            if (packetDescriptions != IntPtr.Zero)
            {
                _descriptionsHandle = GCHandle.Alloc(inputDescriptions, GCHandleType.Pinned);
                Marshal.WriteIntPtr(packetDescriptions, _descriptionsHandle.AddrOfPinnedObject());
            }

            return AudioConverterStatus.Ok;
        }
    }
}
