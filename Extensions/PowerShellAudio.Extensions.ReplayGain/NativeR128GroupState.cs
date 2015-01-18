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

using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;

namespace PowerShellAudio.Extensions.ReplayGain
{
    class NativeR128GroupState
    {
        int _membersDisposed;

        internal int MembersDisposed
        {
            get { return _membersDisposed; }
        }

        internal ConcurrentBag<NativeStateHandle> Handles { get; private set; }

        internal NativeR128GroupState()
        {
            Contract.Ensures(Handles != null);

            Handles = new ConcurrentBag<NativeStateHandle>();
        }

        internal void MemberDisposed()
        {
            Contract.Ensures(_membersDisposed == Contract.OldValue<int>(_membersDisposed) + 1);

            Interlocked.Increment(ref _membersDisposed);
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_membersDisposed >= 0);
            Contract.Invariant(Handles != null);
        }
    }
}
