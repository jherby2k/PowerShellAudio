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
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable", Justification = "Callbacks must be pinned for marshalling")]
    class NativeAudioFile : IDisposable
    {
        readonly Stream _stream;
        readonly SafeNativeMethods.AudioFileReadCallback _readCallback;
        readonly SafeNativeMethods.AudioFileWriteCallback _writeCallback;
        readonly SafeNativeMethods.AudioFileGetSizeCallback _getSizeCallback;
        readonly SafeNativeMethods.AudioFileSetSizeCallback _setSizeCallback;
        bool _isDisposed;

        protected NativeAudioFileHandle Handle { get; set; }

        internal NativeAudioFile(AudioStreamBasicDescription description, AudioFileType fileType, [NotNull] Stream stream)
        {
            _readCallback = ReadCallback;
            _writeCallback = WriteCallback;
            _getSizeCallback = GetSizeCallback;
            _setSizeCallback = SetSizeCallback;

            _stream = stream;

            AudioFileStatus status = SafeNativeMethods.AudioFileInitializeWithCallbacks(IntPtr.Zero, _readCallback, _writeCallback,
                _getSizeCallback, _setSizeCallback, fileType, ref description, 0,
                out NativeAudioFileHandle outHandle);
            if (status != AudioFileStatus.Ok)
                throw new IOException(string.Format(CultureInfo.CurrentCulture,
                    Resources.NativeAudioFileInitializationError, status));

            Handle = outHandle;
        }

        internal NativeAudioFile(AudioFileType fileType, [NotNull] Stream stream)
        {
            _readCallback = ReadCallback;
            _getSizeCallback = GetSizeCallback;

            _stream = stream;

            AudioFileStatus status = SafeNativeMethods.AudioFileOpenWithCallbacks(IntPtr.Zero, _readCallback, null,
                _getSizeCallback, null, fileType, out NativeAudioFileHandle outHandle);
            if (status != AudioFileStatus.Ok)
                throw new IOException(string.Format(CultureInfo.CurrentCulture,
                    Resources.NativeAudioFileInitializationError, status));

            Handle = outHandle;
        }

        internal IntPtr GetProperty(AudioFilePropertyId id, uint size)
        {
            // Callers must release this!
            IntPtr unmanagedValue = Marshal.AllocHGlobal((int)size);
            SafeNativeMethods.AudioFileGetProperty(Handle, id, ref size, unmanagedValue);
            return unmanagedValue;
        }

        internal T GetProperty<T>(AudioFilePropertyId id) where T : struct
        {
            var size = (uint)Marshal.SizeOf(typeof(T));
            IntPtr unmanagedValue = Marshal.AllocHGlobal((int)size);
            try
            {
                SafeNativeMethods.AudioFileGetProperty(Handle, id, ref size, unmanagedValue);
                return Marshal.PtrToStructure<T>(unmanagedValue);
            }
            finally
            {
                Marshal.FreeHGlobal(unmanagedValue);
            }
        }

        internal AudioFileStatus GetPropertyInfo(AudioFilePropertyId id, out uint dataSize, out uint isWritable)
        {
            return SafeNativeMethods.AudioFileGetPropertyInfo(Handle, id, out dataSize, out isWritable);
        }

        internal AudioFileStatus ReadPackets(
            out uint numBytes,
            AudioStreamPacketDescription[] packetDescriptions,
            long startingPacket,
            ref uint packets,
            IntPtr data)
        {
            return SafeNativeMethods.AudioFileReadPackets(Handle, false, out numBytes, packetDescriptions,
                startingPacket, ref packets, data);
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

            _isDisposed = true;
            Handle.Dispose();
        }

        AudioFileStatus ReadCallback(IntPtr userData, long position, uint requestCount, [NotNull] byte[] buffer, out uint actualCount)
        {
            _stream.Position = position;
            actualCount = (uint)_stream.Read(buffer, 0, (int)requestCount);

            return AudioFileStatus.Ok;
        }

        AudioFileStatus WriteCallback(IntPtr userData, long position, uint requestCount, [NotNull] byte[] buffer, out uint actualCount)
        {
            _stream.Position = position;
            _stream.Write(buffer, 0, (int)requestCount);
            actualCount = requestCount;

            return AudioFileStatus.Ok;
        }

        long GetSizeCallback(IntPtr userData)
        {
            if (_isDisposed && !_stream.CanRead)
                return 0;

            return _stream.Length;
        }

        AudioFileStatus SetSizeCallback(IntPtr userData, long size)
        {
            _stream.SetLength(size);

            return AudioFileStatus.Ok;
        }
    }
}
