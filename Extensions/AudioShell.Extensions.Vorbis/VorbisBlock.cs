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

using System;

#pragma warning disable 0649

namespace AudioShell.Extensions.Vorbis
{
    struct VorbisBlock
    {
        internal IntPtr Pcm;

        internal OggPackBuffer OggPackBuffer;

        internal int LW;

        internal int W;

        internal int NW;

        internal int PcmEnd;

        internal int Mode;

        internal int EoffLag;

        internal long GranulePosition;

        internal long Sequence;

        internal IntPtr DspState;

        internal IntPtr LocalStore;

        internal int LocalTop;

        internal int LocalAlloc;

        internal int TotalUse;

        internal IntPtr Reap;

        internal int GlueBits;

        internal int TimeBits;

        internal int FloorBits;

        internal int ResBits;

        internal IntPtr Internal;
    }
}

#pragma warning restore 0649