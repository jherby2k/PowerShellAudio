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

using Id3Lib.Frames;
using System;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Id3
{
    class SoundCheckFrame : FrameFullText
    {
        string _gain;
        string _peak;

        [CanBeNull]
        internal string Gain
        {
            get => _gain;
            set
            {
                _gain = value;
                Text = GetText();
            }
        }

        [CanBeNull]
        internal string Peak
        {
            get => _peak;
            set
            {
                _peak = value;
                Text = GetText();
            }
        }

        internal SoundCheckFrame()
            : base("COMM")
        {
            Description = "iTunNORM";
            FileAlter = true;
        }

        [NotNull]
        string GetText()
        {
            if (!string.IsNullOrEmpty(_gain) && !string.IsNullOrEmpty(_peak))
                return ConvertToSoundCheck(_gain, _peak);
            return string.Empty;
        }

        [NotNull]
        static string ConvertToSoundCheck([NotNull] string gain, [NotNull] string peak)
        {
            float numericGain = float.Parse(gain.Replace(" dB", string.Empty), CultureInfo.InvariantCulture);
            string convertedBase1000 = ConvertGain(numericGain, 1000);
            string convertedBase2500 = ConvertGain(numericGain, 2500);
            string convertedPeak = ConvertPeak(float.Parse(peak, CultureInfo.InvariantCulture));

            var result = new StringBuilder();
            result.Append(' ');
            result.Append(convertedBase1000);
            result.Append(' ');
            result.Append(convertedBase1000);
            result.Append(' ');
            result.Append(convertedBase2500);
            result.Append(' ');
            result.Append(convertedBase2500);
            result.Append(" 00007FFF 00007FFF ");
            result.Append(convertedPeak);
            result.Append(' ');
            result.Append(convertedPeak);
            result.Append(" 00007FFF 00007FFF");
            return result.ToString();
        }

        [NotNull]
        static string ConvertGain(float gain, int reference)
        {
            var numericResult = (int)Math.Round(Math.Pow(10, gain / -10) * reference);
            return ConvertToAsciiHex(numericResult);
        }

        [NotNull]
        static string ConvertPeak(float peak)
        {
            return ConvertToAsciiHex((int)Math.Abs(peak * 0x8000));
        }

        [NotNull]
        static string ConvertToAsciiHex(int value)
        {
            return value.ToString("x8", CultureInfo.InvariantCulture).ToUpperInvariant();
        }
    }
}
