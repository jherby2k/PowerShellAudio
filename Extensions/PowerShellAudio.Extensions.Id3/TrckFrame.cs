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

using Id3Lib.Frames;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Id3
{
    class TrckFrame : FrameText
    {
        string _trackNumber;
        string _trackCount;

        [CanBeNull]
        internal string TrackNumber
        {
            set
            {
                _trackNumber = value;
                Text = GetText();
            }
        }

        [CanBeNull]
        internal string TrackCount
        {
            set
            {
                _trackCount = value;
                Text = GetText();
            }
        }

        internal TrckFrame()
            : base("TRCK")
        { }

        [NotNull]
        string GetText()
        {
            return !string.IsNullOrEmpty(_trackCount) ? _trackNumber + '/' + _trackCount : _trackNumber;
        }
    }
}
