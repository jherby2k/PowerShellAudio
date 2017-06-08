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

#pragma warning disable 169, 649

namespace PowerShellAudio.Extensions.Vorbis
{
    struct VorbisDspState
    {
        internal int AnalysisP;

        internal IntPtr VorbisInfo;

        internal IntPtr Pcm;

        internal IntPtr PcmRet;

        internal int PcmStorage;

        internal int PcmCurrent;

        internal int PcmReturned;

        internal int PreExtrapolate;

        internal int EofFlag;

        internal int Lw;

        internal int W;

        internal int Nw;

        internal int CenterW;

        internal long GranulePosition;

        internal long Sequence;

        internal long GlueBits;

        internal long TimeBits;

        internal long FloorBits;

        internal long ResBits;

        internal IntPtr BackendState;
    }
}

#pragma warning restore 169, 649