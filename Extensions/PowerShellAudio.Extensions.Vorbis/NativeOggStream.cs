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

using PowerShellAudio.Extensions.Vorbis.Properties;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace PowerShellAudio.Extensions.Vorbis
{
    sealed class NativeOggStream : IDisposable
    {
        [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = "Reference to a structure, not a handle.")]
        readonly IntPtr _state;

        internal int SerialNumber => Marshal.PtrToStructure<OggStreamState>(_state).SerialNumber;

        internal NativeOggStream(int serialNumber)
        {
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
            return SafeNativeMethods.OggStreamPageOut(_state, out page) != 0;
        }

        internal bool Flush(out OggPage page)
        {
            return SafeNativeMethods.OggStreamFlush(_state, out page) != 0;
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
            SafeNativeMethods.OggStreamClear(_state);
            Marshal.FreeHGlobal(_state);

            GC.SuppressFinalize(this);
        }

        ~NativeOggStream()
        {
            Dispose();
        }
    }
}
