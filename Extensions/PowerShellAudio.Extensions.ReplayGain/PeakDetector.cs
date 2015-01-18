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
using System.Threading;
using System.Threading.Tasks;

namespace PowerShellAudio.Extensions.ReplayGain
{
    class PeakDetector
    {
        readonly object _syncRoot = new object();

        internal float Peak { get; private set; }

        internal void Submit(SampleCollection input)
        {
            Contract.Requires(input != null);
            Contract.Ensures(Peak >= 0);
            Contract.Ensures(Peak >= Contract.OldValue<float>(Peak));

            // Optimization - Faster when channels are calculated in parallel:
            Parallel.For<float>(0, input.Channels, () => 0, (int channel, ParallelLoopState loopState, float channelMax) =>
            {
                foreach (float sample in input[channel])
                    channelMax = CompareAbsolute(sample, channelMax);
                return channelMax;
            }, channelMax => Submit(channelMax));
        }

        internal void Submit(float input)
        {
            Contract.Ensures(Peak >= Contract.OldValue<float>(Peak));

            lock (_syncRoot)
                Peak = CompareAbsolute(input, Peak);
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_syncRoot != null);
            Contract.Invariant(Peak >= 0);
        }

        static float CompareAbsolute(float relative, float absolute)
        {
            float relativeAsAbsolute = Math.Abs(relative);
            return relativeAsAbsolute > absolute ? relativeAsAbsolute : absolute;
        }
    }
}
