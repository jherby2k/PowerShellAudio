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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace PowerShellAudio.Extensions.Flac
{
    [StructLayout(LayoutKind.Explicit)]
    struct FrameHeader
    {
        [FieldOffset(0)]
        internal readonly uint BlockSize;

        [FieldOffset(4)]
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "P/Invoke signature")]
        readonly uint SampleRate;

        [FieldOffset(8)]
        internal readonly uint Channels;

        [FieldOffset(12)]
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "P/Invoke signature")]
        readonly ChannelAssignment ChannelAssignment;

        [FieldOffset(16)]
        internal readonly uint BitsPerSample;

        [FieldOffset(20)]
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "P/Invoke signature")]
        readonly FrameNumberType NumberType;

        [FieldOffset(24)]
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "P/Invoke signature")]
        readonly uint FrameNumber;

        [FieldOffset(24)]
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "P/Invoke signature")]
        readonly ulong SampleNumber;

        [FieldOffset(32)]
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "P/Invoke signature")]
        readonly byte Crc;
    }
}
