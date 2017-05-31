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
using System.Runtime.InteropServices;

#pragma warning disable 0649

namespace PowerShellAudio.Extensions.Vorbis
{
    struct OggStreamState
    {
        internal IntPtr BodyData;

        internal int BodyStorage;

        internal int BodyFill;

        internal int BodyReturned;

        internal IntPtr LacingValues;

        internal IntPtr GranuleValues;

        internal int LacingStorage;

        internal int LacingFill;

        internal int LacingPacket;

        internal int LacingReturned;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 282)]
        internal char[] Header;

        internal int HeaderFill;

        internal int EndOfStream;

        internal int BeginningOfStream;

        internal int SerialNumber;

        internal int PageNumber;

        internal long PacketNumber;

        internal long GranulePosition;
    }
}

#pragma warning restore 0649