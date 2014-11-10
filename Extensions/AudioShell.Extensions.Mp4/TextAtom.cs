/*
 * Copyright © 2014 Jeremy Herbison
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
using System.Text;

namespace PowerShellAudio.Extensions.Mp4
{
    class TextAtom : IWritableAtom
    {
        readonly string _fourCC;

        internal string Value { get; set; }

        internal TextAtom(string fourCC, string value)
        {
            Contract.Requires(!string.IsNullOrEmpty(fourCC));
            Contract.Requires(fourCC.Length == 4);
            Contract.Requires(value != null);
            Contract.Ensures(_fourCC != null);
            Contract.Ensures(_fourCC == fourCC);
            Contract.Ensures(Value != null);
            Contract.Ensures(Value == value);

            _fourCC = fourCC;
            Value = value;
        }

        internal TextAtom(byte[] data)
        {
            Contract.Requires(data != null);
            Contract.Requires(data.Length >= 24);

            _fourCC = ConvertToString(data.Take(4).ToArray());
            Value = new string(Encoding.UTF8.GetChars(data.Skip(24).Take(data.Length - 24).ToArray()));
        }

        internal override byte[] GetBytes()
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length >= 24);

            byte[] contents = Encoding.UTF8.GetBytes(Value);

            var result = new byte[contents.Length + 24];

            // Write the atom header:
            ConvertToBigEndianBytes((uint)result.Length).CopyTo(result, 0);
            Encoding.GetEncoding(1252).GetBytes(_fourCC).CopyTo(result, 4);

            // Write the data atom header:
            ConvertToBigEndianBytes((uint)result.Length - 8).CopyTo(result, 8);
            BitConverter.GetBytes(0x61746164).CopyTo(result, 12); // 'atad'

            // Set the type flag:
            result[19] = 1;

            // Set the atom contents:
            contents.CopyTo(result, 24);

            return result;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(!string.IsNullOrEmpty(_fourCC));
            Contract.Invariant(_fourCC.Length == 4);
            Contract.Invariant(Value != null);
        }

        static string ConvertToString(byte[] value)
        {
            Contract.Requires(value != null);
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
            Contract.Ensures(Contract.Result<string>().Length == value.Length);

            Array.Reverse(value);
            return new string(Encoding.GetEncoding(1252).GetChars(value));
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
