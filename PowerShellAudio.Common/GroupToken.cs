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
using System.Threading;

namespace PowerShellAudio
{
    /// <summary>
    /// An object used to indicate membership in a collection (An album or compilation).
    /// </summary>
    public class GroupToken : IDisposable
    {
        readonly ManualResetEventSlim resetEvent = new ManualResetEventSlim();
        int _remainingMembers;

        /// <summary>
        /// Gets the member count.
        /// </summary>
        /// <value>
        /// The member count.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is less than 1.
        /// </exception>
        public int Count { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupToken"/> class.
        /// </summary>
        /// <param name="count">The member count.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is less than 1.</exception>
        public GroupToken(int count)
        {
            Contract.Requires<ArgumentOutOfRangeException>(count > 0);
            Contract.Ensures(_remainingMembers == count);
            Contract.Ensures(Count == count);

            _remainingMembers = count;
            Count = count;
        }

        /// <summary>
        /// Signals that one of the members has completed. Once the final member completes, WaitForMembers will no
        /// longer block waiting threads.
        /// </summary>
        public void CompleteMember()
        {
            Contract.Ensures(_remainingMembers == Contract.OldValue<int>(_remainingMembers) - 1);

            if (Interlocked.Decrement(ref _remainingMembers) <= 0)
                resetEvent.Set();
        }

        /// <summary>
        /// Blocks the current thread until the last member completes.
        /// </summary>
        public void WaitForMembers()
        {
            resetEvent.Wait();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged
        /// resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            resetEvent.Dispose();
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(Count > 0);
        }
    }
}
