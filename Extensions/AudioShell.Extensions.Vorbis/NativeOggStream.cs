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

using AudioShell.Extensions.Vorbis.Properties;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices;

namespace AudioShell.Extensions.Vorbis
{
    class NativeOggStream : IDisposable
    {
        [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = "Reference to a structure, not a handle.")]
        readonly IntPtr _state;

        internal int SerialNumber
        {
            get { return Marshal.PtrToStructure<OggStreamState>(_state).SerialNumber; }
        }

        internal NativeOggStream(int serialNumber)
        {
            Contract.Ensures(_state != IntPtr.Zero);

            _state = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(OggStreamState)));
            if (SafeNativeMethods.OggStreamInitialize(_state, serialNumber) != 0)
                throw new IOException(Resources.NativeOggStreamInitializationError);
        }

        internal void PageIn(ref OggPage page)
        {
            if (SafeNativeMethods.OggStreamPageIn(_state, ref page) != 0)
                throw new IOException(Resources.NativeOggStreamPageInError);
        }

        internal bool PageOut(out OggPage page)
        {
            if (SafeNativeMethods.OggStreamPageOut(_state, out page) != 0)
                return true;
            return false;
        }

        internal bool Flush(out OggPage page)
        {
            if (SafeNativeMethods.OggStreamFlush(_state, out page) != 0)
                return true;
            return false;
        }

        internal void PacketIn(ref OggPacket packet)
        {
            if (SafeNativeMethods.OggStreamPacketIn(_state, ref packet) != 0)
                throw new IOException(Resources.NativeOggStreamPacketInError);
        }

        internal int PacketOut(out OggPacket packet)
        {
            return SafeNativeMethods.OggStreamPacketOut(_state, out packet);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            SafeNativeMethods.OggStreamClear(_state);
            Marshal.FreeHGlobal(_state);
        }

        ~NativeOggStream()
        {
            Dispose(false);
        }
    }
}
