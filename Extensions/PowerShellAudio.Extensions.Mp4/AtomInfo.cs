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

using System.Diagnostics.Contracts;

namespace PowerShellAudio.Extensions.Mp4
{
    class AtomInfo
    {
        internal uint Start { get; private set; }

        internal uint Size { get; private set; }

        internal uint End
        {
            get { return Start + Size; }
        }

        internal string FourCC { get; private set; }

        internal AtomInfo(uint start, uint size, string fourCC)
        {
            Contract.Requires(!string.IsNullOrEmpty(fourCC));
            Contract.Requires(fourCC.Length == 4);
            Contract.Ensures(Start == start);
            Contract.Ensures(Size == size);
            Contract.Ensures(FourCC == fourCC);

            Start = start;
            Size = size;
            FourCC = fourCC;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(!string.IsNullOrEmpty(FourCC));
            Contract.Invariant(FourCC.Length == 4);
        }
    }
}
