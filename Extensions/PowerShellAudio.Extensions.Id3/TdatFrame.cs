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
    class TdatFrame : FrameText
    {
        string _day;
        string _month;

        [CanBeNull]
        internal string Day
        {
            set
            {
                _day = value;
                Text = GetText();
            }
        }

        [CanBeNull]
        internal string Month
        {
            set
            {
                _month = value;
                Text = GetText();
            }
        }

        internal TdatFrame()
            : base("TDAT")
        { }

        [NotNull]
        string GetText()
        {
            if (string.IsNullOrEmpty(_day) || string.IsNullOrEmpty(_month))
                return string.Empty;

            return _day + _month;
        }
    }
}
