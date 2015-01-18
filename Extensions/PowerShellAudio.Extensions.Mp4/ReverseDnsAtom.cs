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

using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace PowerShellAudio.Extensions.Mp4
{
    class ReverseDnsAtom : IWritableAtom
    {
        readonly byte[] _data;

        internal string Name
        {
            get { return ConvertToString(_data.Skip(48).Take(8).ToArray()); }
        }

        internal ReverseDnsAtom(byte[] data)
        {
            Contract.Requires(data != null);
            Contract.Requires(data.Length >= 56);

            _data = data;
        }

        internal override byte[] GetBytes()
        {
            return _data;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_data != null);
            Contract.Invariant(_data.Length >= 56);
        }

        static string ConvertToString(byte[] value)
        {
            Contract.Requires(value != null);
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
            Contract.Ensures(Contract.Result<string>().Length == value.Length);

            return new string(Encoding.GetEncoding(1252).GetChars(value));
        }
    }
}
