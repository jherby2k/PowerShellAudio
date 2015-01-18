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

using PowerShellAudio.Extensions.Vorbis.Properties;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace PowerShellAudio.Extensions.Vorbis
{
    class NativeVorbisDecoder : IDisposable
    {
        [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = "Reference to a structure, not a handle.")]
        readonly IntPtr _info;

        internal NativeVorbisDecoder()
        {
            Contract.Ensures(_info != IntPtr.Zero);

            _info = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VorbisInfo)));
            SafeNativeMethods.VorbisInfoInitialize(_info);
        }

        internal void HeaderIn(ref VorbisComment comment, ref OggPacket packet)
        {
            Result result = SafeNativeMethods.VorbisSynthesisHeaderIn(_info, ref comment, ref packet);
            switch (result)
            {
                case Result.OK:
                    return;
                case Result.NotVorbisError:
                    throw new UnsupportedAudioException(Resources.NativeVorbisDecoderNotVorbisError);
                default:
                    throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.NativeVorbisDecoderHeaderInError, result));
            }
        }

        internal VorbisInfo GetInfo()
        {
            return Marshal.PtrToStructure<VorbisInfo>(_info);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispoing)
        {
            SafeNativeMethods.VorbisInfoClear(_info);
            Marshal.FreeHGlobal(_info);
        }

        ~NativeVorbisDecoder()
        {
            Dispose(false);
        }
    }
}
