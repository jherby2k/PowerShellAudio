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
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.ReplayGain
{
    class PeakDetector
    {
        readonly object _syncRoot = new object();

        internal float Peak { get; private set; }

        internal void Submit([NotNull] SampleCollection input)
        {
            // Optimization - Faster when channels are calculated in parallel:
            Parallel.For(0, input.Channels, () => 0, (int channel, ParallelLoopState loopState, float channelMax) =>
            {
                return input[channel].Aggregate(channelMax, (current, sample) => CompareAbsolute(sample, current));
            }, Submit);
        }

        internal void Submit(float input)
        {
            lock (_syncRoot)
                Peak = CompareAbsolute(input, Peak);
        }

        static float CompareAbsolute(float relative, float absolute)
        {
            float relativeAsAbsolute = Math.Abs(relative);
            return relativeAsAbsolute > absolute ? relativeAsAbsolute : absolute;
        }
    }
}
