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
    sealed class NativeOggSync : IDisposable
    {
        [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = "Reference to a structure, not a handle.")]
        readonly IntPtr _state;

        internal NativeOggSync()
        {
            _state = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(OggSyncState)));
            if (SafeNativeMethods.OggSyncInitialize(_state) != 0)
                throw new IOException(Resources.NativeOggSyncInitializationError);
        }

        internal int PageOut(out OggPage page)
        {
            return SafeNativeMethods.OggSyncPageOut(_state, out page);
        }

        internal IntPtr Buffer(int size)
        {
            return SafeNativeMethods.OggSyncBuffer(_state, size);
        }

        internal void Wrote(int bytes)
        {
            if (SafeNativeMethods.OggSyncWrote(_state, bytes) != 0)
                throw new IOException(Resources.NativeOggSyncWroteError);
        }

        public void Dispose()
        {
            SafeNativeMethods.OggSyncClear(_state);
            Marshal.FreeHGlobal(_state);

            GC.SuppressFinalize(this);
        }

        ~NativeOggSync()
        {
            Dispose();
        }
    }
}
