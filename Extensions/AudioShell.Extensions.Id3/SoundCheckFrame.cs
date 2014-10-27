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
using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;

namespace AudioShell.Extensions.Id3
{
    class SoundCheckFrame : FrameFullText
    {
        string _gain;
        string _peak;

        internal string Gain
        {
            get { return _gain; }
            set
            {
                Contract.Requires(!string.IsNullOrEmpty(value));

                _gain = value;
                Text = GetText();
            }
        }

        internal string Peak
        {
            get { return _peak; }
            set
            {
                Contract.Requires(!string.IsNullOrEmpty(value));

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

        string GetText()
        {
            Contract.Ensures(Contract.Result<string>() != null);

            if (!string.IsNullOrEmpty(_gain) && !string.IsNullOrEmpty(_peak))
                return ConvertToSoundCheck(_gain, _peak);
            return string.Empty;
        }

        static string ConvertToSoundCheck(string gain, string peak)
        {
            Contract.Requires(!string.IsNullOrEmpty(gain));
            Contract.Requires(!string.IsNullOrEmpty(peak));
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

            float numericGain = float.Parse(gain.Replace(" dB", string.Empty), CultureInfo.InvariantCulture);
            string convertedBase1000 = ConvertGain(numericGain, 1000);
            string convertedBase2500 = ConvertGain(numericGain, 2500);
            string convertedPeak = ConvertPeak(float.Parse(peak, CultureInfo.InvariantCulture));

            StringBuilder result = new StringBuilder();
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

        static string ConvertGain(float gain, int reference)
        {
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

            int numericResult = (int)Math.Round(Math.Pow(10, gain / -10) * reference);
            return ConvertToAsciiHex(numericResult);
        }

        static string ConvertPeak(float peak)
        {
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

            return ConvertToAsciiHex((int)Math.Abs(peak * 0x8000));
        }

        static string ConvertToAsciiHex(int value)
        {
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

            return value.ToString("x8", CultureInfo.InvariantCulture).ToUpperInvariant();
        }
    }
}
