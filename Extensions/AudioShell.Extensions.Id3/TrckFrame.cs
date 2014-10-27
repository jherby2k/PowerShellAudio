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

using Id3Lib.Frames;
using System.Diagnostics.Contracts;

namespace AudioShell.Extensions.Id3
{
    class TrckFrame : FrameText
    {
        string _trackNumber;
        string _trackCount;

        internal string TrackNumber
        {
            set
            {
                Contract.Requires(!string.IsNullOrEmpty(value));

                _trackNumber = value;
                Text = GetText();
            }
        }

        internal string TrackCount
        {
            set
            {
                Contract.Requires(!string.IsNullOrEmpty(value));

                _trackCount = value;
                Text = GetText();
            }
        }

        internal TrckFrame()
            : base("TRCK")
        { }

        string GetText()
        {
            return !string.IsNullOrEmpty(_trackCount) ? _trackNumber + '/' + _trackCount : _trackNumber;
        }
    }
}
