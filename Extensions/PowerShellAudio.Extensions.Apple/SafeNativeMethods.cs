/*
 * Copyright © 2014 Jeremy Herbison
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
using Microsoft.Win32;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Diagnostics;

namespace PowerShellAudio.Extensions.Apple
{
    [SuppressUnmanagedCodeSecurity]
    static class SafeNativeMethods
    {
        const string _coreAudioToolboxLibrary = "CoreAudioToolbox.dll";
        static readonly string _coreAudioInstallDir;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Static initialization must be guaranteed to occur before a static method of the type is called.")]
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "This type is unusable if accessed from a 64-bit process, if the unmanaged DLL can't be loaded")]
        static SafeNativeMethods()
        {
            if (Environment.Is64BitProcess)
                throw new ExtensionInitializationException(Resources.SafeNativeMethods64BitError);

            try
            {
                _coreAudioInstallDir = (string)Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Apple Inc.").OpenSubKey("Apple Application Support").GetValue("InstallDir");

                // Prefix the PATH variable with the Apple Application Support installation directory:
                var newPath = new StringBuilder(_coreAudioInstallDir);
                newPath.Append(Path.PathSeparator);
                newPath.Append(Environment.GetEnvironmentVariable("PATH"));
                Environment.SetEnvironmentVariable("PATH", newPath.ToString());
            }
            catch (NullReferenceException e)
            {
                throw new ExtensionInitializationException(Resources.SafeNativeMethodsDllsMissing, e);
            }
        }

        internal static string GetCoreAudioToolboxVersion()
        {
            return FileVersionInfo.GetVersionInfo(Path.Combine(_coreAudioInstallDir, _coreAudioToolboxLibrary)).FileVersion;
        }

        [DllImport(_coreAudioToolboxLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern AudioFileStatus AudioFileOpenWithCallbacks(IntPtr userData, AudioFileReadCallback readCallback, AudioFileWriteCallback writeCallback, AudioFileGetSizeCallback getSizeCallback, AudioFileSetSizeCallback setSizeCallback, AudioFileType fileType, out NativeAudioFileHandle handle);

        [DllImport(_coreAudioToolboxLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern AudioFileStatus AudioFileInitializeWithCallbacks(IntPtr userData, AudioFileReadCallback readCallback, AudioFileWriteCallback writeCallback, AudioFileGetSizeCallback getSizeCallback, AudioFileSetSizeCallback setSizeCallback, AudioFileType fileType, ref AudioStreamBasicDescription description, uint flags, out NativeAudioFileHandle handle);

        [DllImport(_coreAudioToolboxLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern AudioFileStatus AudioFileGetProperty(NativeAudioFileHandle handle, AudioFilePropertyID id, ref uint size, [Out]IntPtr data);

        [DllImport(_coreAudioToolboxLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern AudioFileStatus AudioFileGetPropertyInfo(NativeAudioFileHandle handle, AudioFilePropertyID id, out uint dataSize, out uint isWritable);

        [DllImport(_coreAudioToolboxLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern AudioFileStatus AudioFileReadPackets(NativeAudioFileHandle handle, [MarshalAs(UnmanagedType.Bool)]bool useCache, out uint numBytes, [In, Out]AudioStreamPacketDescription[] packetDescriptions, long startingPacket, ref uint packets, IntPtr data);

        [DllImport(_coreAudioToolboxLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern AudioFileStatus AudioFileClose(IntPtr handle);

        [DllImport(_coreAudioToolboxLibrary, EntryPoint = "ExtAudioFileWrapAudioFileID", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ExtendedAudioFileStatus ExtAudioFileWrapAudioFile(NativeAudioFileHandle audioFileHandle, [MarshalAs(UnmanagedType.Bool)]bool forWriting, out NativeExtendedAudioFileHandle handle);

        [DllImport(_coreAudioToolboxLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ExtendedAudioFileStatus ExtAudioFileGetProperty(NativeExtendedAudioFileHandle handle, ExtendedAudioFilePropertyID id, ref uint size, [Out]IntPtr data);

        [DllImport(_coreAudioToolboxLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ExtendedAudioFileStatus ExtAudioFileSetProperty(NativeExtendedAudioFileHandle handle, ExtendedAudioFilePropertyID id, UInt32 size, IntPtr data);

        [DllImport(_coreAudioToolboxLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ExtendedAudioFileStatus ExtAudioFileWrite(NativeExtendedAudioFileHandle handle, uint frames, ref AudioBufferList data);

        [DllImport(_coreAudioToolboxLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ExtendedAudioFileStatus ExtAudioFileDispose(IntPtr handle);

        [DllImport(_coreAudioToolboxLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern AudioConverterStatus AudioConverterNew(ref AudioStreamBasicDescription sourceFormat, ref AudioStreamBasicDescription destinationFormat, out NativeAudioConverterHandle handle);

        [DllImport(_coreAudioToolboxLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern AudioConverterStatus AudioConverterSetProperty(IntPtr handle, AudioConverterPropertyID id, uint size, IntPtr data);

        [DllImport(_coreAudioToolboxLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern AudioConverterStatus AudioConverterSetProperty(NativeAudioConverterHandle handle, AudioConverterPropertyID id, uint size, IntPtr data);

        [DllImport(_coreAudioToolboxLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern AudioConverterStatus AudioConverterFillComplexBuffer(NativeAudioConverterHandle handle, AudioConverterComplexInputCallback inputCallback, IntPtr userData, ref uint packetSize, ref AudioBufferList outputData, [In, Out]AudioStreamPacketDescription[] packetDescriptions);

        [DllImport(_coreAudioToolboxLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern AudioConverterStatus AudioConverterDispose(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr EncoderFactory();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate AudioFileStatus AudioFileReadCallback(IntPtr userData, long position, uint requestCount, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]byte[] buffer, out uint actualCount);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate AudioFileStatus AudioFileWriteCallback(IntPtr userData, long position, uint requestCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]byte[] buffer, out uint actualCount);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate long AudioFileGetSizeCallback(IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate AudioFileStatus AudioFileSetSizeCallback(IntPtr userData, long size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate AudioConverterStatus AudioConverterComplexInputCallback(IntPtr handle, ref uint numberPackets, ref AudioBufferList data, IntPtr packetDescriptions, IntPtr userData);
    }
}
