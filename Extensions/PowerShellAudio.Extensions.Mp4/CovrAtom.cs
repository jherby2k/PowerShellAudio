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
using System.Linq;

namespace PowerShellAudio.Extensions.Mp4
{
    class CovrAtom : IWritableAtom
    {
        internal CoverType CoverType { get; set; }

        internal byte[] Value { get; set; }

        internal CovrAtom(CoverArt coverArt)
        {
            Contract.Requires(coverArt != null);
            Contract.Ensures(Value != null);

            CoverType = coverArt.MimeType == "image/png" ? CoverType.Png : CoverType.Jpeg;
            Value = coverArt.GetData();
        }

        internal CovrAtom(byte[] data)
        {
            Contract.Requires(data != null);
            Contract.Requires(data.Length >= 24);
            Contract.Ensures(Value != null);

            // There could be more than one data atom. Ignore all but the first:
            CoverType = (CoverType)data[19];
            Value = new byte[BitConverter.ToUInt32(data.Skip(8).Take(4).Reverse().ToArray(), 0) - 16];
            Array.Copy(data, 24, Value, 0, Value.Length);
        }

        internal override byte[] GetBytes()
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length >= 24);

            var result = new byte[Value.Length + 24];

            // Write the atom header:
            ConvertToBigEndianBytes((uint)result.Length).CopyTo(result, 0);
            BitConverter.GetBytes(0x72766f63).CopyTo(result, 4); // 'rvoc'

            // Write the data atom header:
            ConvertToBigEndianBytes((uint)result.Length - 8).CopyTo(result, 8);
            BitConverter.GetBytes(0x61746164).CopyTo(result, 12); // 'atad'

            // Set the type flag:
            result[19] = (byte)CoverType;

            // Set the atom contents:
            Value.CopyTo(result, 24);

            return result;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(Value != null);
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
