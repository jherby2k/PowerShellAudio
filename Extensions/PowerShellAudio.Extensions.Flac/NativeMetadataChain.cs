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
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices;

namespace PowerShellAudio.Extensions.Flac
{
    class NativeMetadataChain : IDisposable
    {
        readonly IoCallbacks _callbacks;

        internal NativeMetadataChainHandle Handle { get; }

        internal NativeMetadataChain(Stream stream)
        {
            Contract.Requires(stream != null);
            Contract.Requires(stream.CanRead);
            Contract.Requires(stream.CanWrite);
            Contract.Requires(stream.CanSeek);
            Contract.Ensures(Handle != null);
            Contract.Ensures(!Handle.IsClosed);
            Contract.Ensures(GetStatus() == MetadataChainStatus.Ok);

            _callbacks = InitializeCallbacks(stream);
            Handle = SafeNativeMethods.MetadataChainNew();
        }

        internal bool Read()
        {
            Contract.Requires(!Handle.IsClosed);

            return SafeNativeMethods.MetadataChainRead(Handle, IntPtr.Zero, _callbacks);
        }

        internal bool CheckIfTempFileNeeded(bool usePadding)
        {
            Contract.Requires(!Handle.IsClosed);

            return SafeNativeMethods.MetadataChainCheckIfTempFileNeeded(Handle, usePadding);
        }

        internal bool WriteWithTempFile(bool usePadding, Stream tempStream)
        {
            Contract.Requires(tempStream != null);
            Contract.Requires(tempStream.CanRead);
            Contract.Requires(tempStream.CanWrite);
            Contract.Requires(tempStream.CanSeek);
            Contract.Requires(!Handle.IsClosed);

            return SafeNativeMethods.MetadataChainWriteWithTempFile(Handle, usePadding, IntPtr.Zero, _callbacks,
                IntPtr.Zero, InitializeCallbacks(tempStream));
        }

        internal bool Write(bool usePadding)
        {
            Contract.Requires(!Handle.IsClosed);

            return SafeNativeMethods.MetadataChainWrite(Handle, usePadding, IntPtr.Zero, _callbacks);
        }

        [Pure]
        internal MetadataChainStatus GetStatus()
        {
            Contract.Requires(!Handle.IsClosed);

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

        static IoCallbacks InitializeCallbacks(Stream stream)
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

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(Handle != null);
            Contract.Invariant(!Handle.IsInvalid);
            Contract.Invariant(GetStatus() == MetadataChainStatus.Ok);
        }
    }
}
