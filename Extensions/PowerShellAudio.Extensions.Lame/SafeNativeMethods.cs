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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace PowerShellAudio.Extensions.Lame
{
    [SuppressUnmanagedCodeSecurity]
    static class SafeNativeMethods
    {
        const string _lameLibrary = "libmp3lame.dll";

        static SafeNativeMethods()
        {
            // Select an architecture-appropriate libmp3lame.dll by prefixing the PATH variable:
            var newPath = new StringBuilder(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            newPath.Append(Path.DirectorySeparatorChar);
            newPath.Append(Environment.Is64BitProcess ? "x64" : "x86");
            newPath.Append(Path.PathSeparator);
            newPath.Append(Environment.GetEnvironmentVariable("PATH"));

            Environment.SetEnvironmentVariable("PATH", newPath.ToString());
        }

        [DllImport(_lameLibrary, EntryPoint = "get_lame_version", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr GetLameVersion();

        [DllImport(_lameLibrary, EntryPoint = "lame_init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NativeEncoderHandle Initialize();

        [DllImport(_lameLibrary, EntryPoint = "lame_set_num_samples", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetSampleCount(NativeEncoderHandle handle, uint samples);

        [DllImport(_lameLibrary, EntryPoint = "lame_set_in_samplerate", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetSampleRate(NativeEncoderHandle handle, int sampleRate);

        [DllImport(_lameLibrary, EntryPoint = "lame_set_num_channels", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetChannels(NativeEncoderHandle handle, int channels);

        [DllImport(_lameLibrary, EntryPoint = "lame_set_quality", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetQuality(NativeEncoderHandle handle, int quality);

        [DllImport(_lameLibrary, EntryPoint = "lame_set_brate", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetBitRate(NativeEncoderHandle handle, int bitRate);

        [DllImport(_lameLibrary, EntryPoint = "lame_set_VBR", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetVbr(NativeEncoderHandle handle, VbrMode vbrMode);

        [DllImport(_lameLibrary, EntryPoint = "lame_set_VBR_quality", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetVbrQuality(NativeEncoderHandle handle, float quality);

        [DllImport(_lameLibrary, EntryPoint = "lame_set_VBR_mean_bitrate_kbps", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetMeanBitRate(NativeEncoderHandle handle, int bitRate);

        [DllImport(_lameLibrary, EntryPoint = "lame_init_params", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int InitializeParams(NativeEncoderHandle handle);

        [DllImport(_lameLibrary, EntryPoint = "lame_encode_buffer_ieee_float", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int EncodeBuffer(NativeEncoderHandle handle, float[] leftSamples, float[] rightSamples, int sampleCount, [In, Out]byte[] buffer, int bufferSize);

        [DllImport(_lameLibrary, EntryPoint = "lame_encode_flush", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Flush(NativeEncoderHandle handle, [In, Out]byte[] buffer, int bufferSize);

        [DllImport(_lameLibrary, EntryPoint = "lame_get_lametag_frame", CallingConvention = CallingConvention.Cdecl)]
        internal static extern UIntPtr GetLameTagFrame(NativeEncoderHandle handle, [In, Out]byte[] buffer, UIntPtr bufferSize);

        [DllImport(_lameLibrary, EntryPoint = "lame_close", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Close(IntPtr handle);
    }
}
