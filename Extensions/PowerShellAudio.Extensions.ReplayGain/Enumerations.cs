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

namespace PowerShellAudio.Extensions.ReplayGain
{
    enum Ebur128Error
    {
        Success,
        NoMemory,
        InvalidMode,
        InvalidChannelIndex,
        NoChange
    }

    [Flags]
    enum Mode
    {
        Momentary = 0x1,    // (1 << 0)
        ShortTerm = 0x3,    // (1 << 1) | Mode.Momentary
        Global = 0x5,       // (1 << 2) | Mode.Momentary
        Range = 0xb,        // (1 << 3) | Mode.ShortTerm
        SamplePeak = 0x11,  // (1 << 4) | Mode.Momentary
        TruePeak = 0x31,    // (1 << 5) | Mode.SamplePeak
        Histogram = 0x40    // (1 << 6)
    }
}
