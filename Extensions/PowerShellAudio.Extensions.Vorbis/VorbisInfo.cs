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

#pragma warning disable 0649

namespace PowerShellAudio.Extensions.Vorbis
{
    struct VorbisInfo
    {
        internal int Version;
        
        internal int Channels;

        internal int Rate;

        internal int BitrateUpper;

        internal int BitrateNominal;

        internal int BitrateLower;

        internal int BitrateWindow;

        internal IntPtr CodecSetup;
    }
}

#pragma warning restore 0649
