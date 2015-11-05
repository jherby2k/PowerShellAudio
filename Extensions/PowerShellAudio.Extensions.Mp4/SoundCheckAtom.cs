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
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;

namespace PowerShellAudio.Extensions.Mp4
{
    class SoundCheckAtom : WritableAtom
    {
        internal string Gain { get; set; }

        internal string Peak { get; set; }

        internal override byte[] GetBytes()
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == 0 || Contract.Result<byte[]>().Length == 162);

            string soundCheckValue;
            if (!string.IsNullOrEmpty(Gain) && !string.IsNullOrEmpty(Peak))
                soundCheckValue = ConvertToSoundCheck(Gain, Peak);
            else
                return new byte[0];

            var result = new byte[162];

            // Write the atom header:
            GetBytesBigEndian((uint)result.Length).CopyTo(result, 0);
            BitConverter.GetBytes(0x2d2d2d2d).CopyTo(result, 4); // '----'

            // Write the mean atom header:
            GetBytesBigEndian(28).CopyTo(result, 8);
            BitConverter.GetBytes(0x6e61656d).CopyTo(result, 12); // 'naem'
            Encoding.ASCII.GetBytes("com.apple.iTunes").CopyTo(result, 20);

            // Write the name atom header:
            GetBytesBigEndian(20).CopyTo(result, 36);
            BitConverter.GetBytes(0x656d616e).CopyTo(result, 40); // 'eman'
            Encoding.ASCII.GetBytes("iTunNORM").CopyTo(result, 48);

            // Write the data atom header:
            GetBytesBigEndian(106).CopyTo(result, 56);
            BitConverter.GetBytes(0x61746164).CopyTo(result, 60); // 'atad'

            // Set the type flag:
            result[67] = 1;

            // Set the atom contents:
            Encoding.ASCII.GetBytes(soundCheckValue).CopyTo(result, 72);

            return result;
        }

        static byte[] GetBytesBigEndian(uint value)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == 4);

            byte[] result = BitConverter.GetBytes(value);
            Array.Reverse(result);
            return result;
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

        static string ConvertGain(float gain, int reference)
        {
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

            return ConvertToAsciiHex((int)Math.Round(Math.Pow(10, gain / -10) * reference));
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
