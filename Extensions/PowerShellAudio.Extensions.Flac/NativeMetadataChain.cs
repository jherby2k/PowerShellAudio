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

using System;
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Flac
{
    class NativeMetadataChain : IDisposable
    {
        readonly IoCallbacks _callbacks;

        [NotNull]
        internal NativeMetadataChainHandle Handle { get; }

        internal NativeMetadataChain([NotNull] Stream stream)
        {
            _callbacks = InitializeCallbacks(stream);
            Handle = SafeNativeMethods.MetadataChainNew();
        }

        internal bool Read()
        {
            return SafeNativeMethods.MetadataChainRead(Handle, IntPtr.Zero, _callbacks);
        }

        internal bool CheckIfTempFileNeeded(bool usePadding)
        {
            return SafeNativeMethods.MetadataChainCheckIfTempFileNeeded(Handle, usePadding);
        }

        internal bool WriteWithTempFile(bool usePadding, [NotNull] Stream tempStream)
        {
            return SafeNativeMethods.MetadataChainWriteWithTempFile(Handle, usePadding, IntPtr.Zero, _callbacks,
                IntPtr.Zero, InitializeCallbacks(tempStream));
        }

        internal bool Write(bool usePadding)
        {
            return SafeNativeMethods.MetadataChainWrite(Handle, usePadding, IntPtr.Zero, _callbacks);
        }

        [Pure]
        internal MetadataChainStatus GetStatus()
        {
            return SafeNativeMethods.MetadataChainGetStatus(Handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Handle.Dispose();
        }

        static IoCallbacks InitializeCallbacks([NotNull] Stream stream)
        {
            return new IoCallbacks
            {
                Read = (readBuffer, bufferSize, numberOfRecords, handle) =>
                {
                    ulong totalBufferSize = (ulong)bufferSize.ToInt64() * (ulong)numberOfRecords.ToInt64();
                    var managedBuffer = new byte[totalBufferSize];
                    int bytesRead = stream.Read(managedBuffer, 0, (int)totalBufferSize);

                    Marshal.Copy(managedBuffer, 0, readBuffer, (int)totalBufferSize);
                    return new IntPtr(bytesRead);
                },
                Write = (writeBuffer, bufferSize, numberOfRecords, handle) =>
                {
                    var castNumberOfRecords = (ulong)numberOfRecords.ToInt64();
                    ulong totalBufferSize = (ulong)bufferSize.ToInt64() * castNumberOfRecords;
                    var managedBuffer = new byte[totalBufferSize];
                    Marshal.Copy(writeBuffer, managedBuffer, 0, (int)totalBufferSize);

                    stream.Write(managedBuffer, 0, (int)totalBufferSize);
                    return new IntPtr((long)castNumberOfRecords);
                },
                Seek = (handle, offset, whence) =>
                {
                    stream.Seek(offset, whence);
                    return 0;
                },
                Tell = handle => stream.Position,
                Eof = handle => stream.Position < stream.Length ? 0 : 1
            };
        }
    }
}
