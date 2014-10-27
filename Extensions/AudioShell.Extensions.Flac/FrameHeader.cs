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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace AudioShell.Extensions.Flac
{
    [StructLayout(LayoutKind.Explicit)]
    struct FrameHeader
    {
        [FieldOffset(0), SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal uint BlockSize;

        [FieldOffset(4), SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal uint SampleRate;

        [FieldOffset(8), SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal uint Channels;

        [FieldOffset(12), SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal ChannelAssignment ChannelAssignment;

        [FieldOffset(16), SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal uint BitsPerSample;

        [FieldOffset(20), SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal FrameNumberType NumberType;

        [FieldOffset(24), SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal uint FrameNumber;

        [FieldOffset(24), SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal ulong SampleNumber;

        [FieldOffset(32), SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal byte Crc;
    }
}
