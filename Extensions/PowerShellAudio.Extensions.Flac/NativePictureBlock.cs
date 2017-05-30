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

using PowerShellAudio.Extensions.Flac.Properties;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Flac
{
    class NativePictureBlock : NativeMetadataBlock
    {
        internal NativePictureBlock()
            : base(MetadataType.Picture)
        {
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification = "There is no native method for setting this, so the structure has to be modified directly")]
        internal void SetType(PictureType type)
        {
            Marshal.WriteInt32(IntPtr.Add(Handle.DangerousGetHandle(),
                Marshal.OffsetOf<PictureMetadataBlock>("Picture").ToInt32() +
                Marshal.OffsetOf<Picture>("Type").ToInt32()), (int)type);
        }

        internal void SetMimeType([NotNull] string mimeType)
        {
            if (!SafeNativeMethods.PictureSetMimeType(Handle, mimeType, true))
                throw new IOException(Resources.NativePictureBlockMemoryError);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification = "There is no native method for setting this, so the structure has to be modified directly")]
        internal void SetWidth(int width)
        {
            Marshal.WriteInt32(IntPtr.Add(Handle.DangerousGetHandle(),
                Marshal.OffsetOf<PictureMetadataBlock>("Picture").ToInt32() +
                Marshal.OffsetOf<Picture>("Width").ToInt32()), width);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification = "There is no native method for setting this, so the structure has to be modified directly")]
        internal void SetHeight(int height)
        {
            Marshal.WriteInt32(IntPtr.Add(Handle.DangerousGetHandle(),
                Marshal.OffsetOf<PictureMetadataBlock>("Picture").ToInt32() +
                Marshal.OffsetOf<Picture>("Height").ToInt32()), height);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification = "There is no native method for setting this, so the structure has to be modified directly")]
        internal void SetColorDepth(int depth)
        {
            Marshal.WriteInt32(IntPtr.Add(Handle.DangerousGetHandle(),
                Marshal.OffsetOf<PictureMetadataBlock>("Picture").ToInt32() +
                Marshal.OffsetOf<Picture>("ColorDepth").ToInt32()), depth);
        }

        internal void SetData([NotNull] byte[] data)
        {
            if (!SafeNativeMethods.PictureSetData(Handle, data, (uint)data.Length, true))
                throw new IOException(Resources.NativePictureBlockMemoryError);
        }
    }
}
