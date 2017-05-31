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

using System;
using System.Globalization;
using System.Threading;
using PowerShellAudio.Properties;

namespace PowerShellAudio
{
    /// <summary>
    /// An object used to indicate membership in a collection (An album or compilation).
    /// </summary>
    public sealed class GroupToken : IDisposable
    {
        readonly ManualResetEventSlim _resetEvent = new ManualResetEventSlim();
        int _remainingMembers;

        /// <summary>
        /// Gets the member count.
        /// </summary>
        /// <value>
        /// The member count.
        /// </value>
        public int Count { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupToken"/> class.
        /// </summary>
        /// <param name="count">The member count.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is less than 1.</exception>
        public GroupToken(int count)
        {
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), count,
                string.Format(CultureInfo.CurrentCulture, Resources.GroupTokenCountIsOutOfRangeError, count));

            _remainingMembers = count;
            Count = count;
        }

        /// <summary>
        /// Signals that one of the members has completed. Once the final member completes, WaitForMembers will no
        /// longer block waiting threads.
        /// </summary>
        public void CompleteMember()
        {
            if (Interlocked.Decrement(ref _remainingMembers) <= 0)
                _resetEvent.Set();
        }

        /// <summary>
        /// Blocks the current thread until the last member completes.
        /// </summary>
        public void WaitForMembers()
        {
            _resetEvent.Wait();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _resetEvent.Dispose();
        }
    }
}
