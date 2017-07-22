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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace PowerShellAudio.Extensions.ReplayGain
{
    [SuppressUnmanagedCodeSecurity]
    static class SafeNativeMethods
    {
        const string _ebur128Library = "ebur128.dll";

        static SafeNativeMethods()
        {
            // Select an architecture-appropriate ebur128.dll by prefixing the PATH variable:
            var newPath = new StringBuilder(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath));
            newPath.Append(Path.DirectorySeparatorChar);
            newPath.Append(Environment.Is64BitProcess ? "x64" : "x86");
            newPath.Append(Path.PathSeparator);
            newPath.Append(Environment.GetEnvironmentVariable("PATH"));

            Environment.SetEnvironmentVariable("PATH", newPath.ToString());
        }

        [DllImport(_ebur128Library, EntryPoint = "ebur128_get_version", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GetVersion(out int major, out int minor, out int patch);

        [DllImport(_ebur128Library, EntryPoint = "ebur128_init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NativeStateHandle Initialize(uint channels, uint samplerate, Mode mode);

        [DllImport(_ebur128Library, EntryPoint = "ebur128_add_frames_float", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Ebur128Error AddFrames(NativeStateHandle handle, float[] source, UIntPtr count);

        [DllImport(_ebur128Library, EntryPoint = "ebur128_destroy", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Destroy(ref IntPtr handle);

        [DllImport(_ebur128Library, EntryPoint = "ebur128_loudness_global", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Ebur128Error GetLoudness(NativeStateHandle handle, out double result);

        [DllImport(_ebur128Library, EntryPoint = "ebur128_loudness_global_multiple", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Ebur128Error GetLoudnessMultiple(IntPtr[] handle, UIntPtr count, out double result);

        [DllImport(_ebur128Library, EntryPoint = "ebur128_sample_peak", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Ebur128Error GetSamplePeak(NativeStateHandle handle, uint channel, out double result);
    }
}
