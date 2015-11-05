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

namespace PowerShellAudio.Extensions.Mp4
{
    class TrackNumberAtom : WritableAtom
    {
        internal byte TrackNumber { get; set; }

        internal byte TrackCount { get; set; }

        internal bool IsValid => TrackNumber > 0;

        internal TrackNumberAtom()
        { }

        internal TrackNumberAtom(byte[] data)
        {
            Contract.Requires(data != null);
            Contract.Requires(data.Length == 32);

            TrackNumber = data[27];
            TrackCount = data[29];
        }

        internal override byte[] GetBytes()
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == 0 || Contract.Result<byte[]>().Length == 32);

            if (TrackNumber == 0)
                return new byte[0];

            var result = new byte[32];

            // Write the atom header:
            ConvertToBigEndianBytes((uint)result.Length).CopyTo(result, 0);
            BitConverter.GetBytes(0x6e6b7274).CopyTo(result, 4); // 'nkrt'

            // Write the data atom header:
            ConvertToBigEndianBytes((uint)result.Length - 8).CopyTo(result, 8);
            BitConverter.GetBytes(0x61746164).CopyTo(result, 12); // 'atad'

            // Set the track number (the rest of the bytes are set to 0):
            result[27] = TrackNumber;
            result[29] = TrackCount;

            return result;
        }

        static byte[] ConvertToBigEndianBytes(uint value)
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == 4);

            byte[] result = BitConverter.GetBytes(value);
            Array.Reverse(result);
            return result;
        }
    }
}
