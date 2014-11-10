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
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Linq;

namespace PowerShellAudio.Extensions.ReplayGain
{
    class WindowSelector
    {
        const float _rmsPercentile = 0.95f;
        const float _pinkNoiseReference = -25.4809818f;

        readonly ConcurrentBag<float> _rmsWindows = new ConcurrentBag<float>();

        internal void Submit(float rmsWindow)
        {
            Contract.Ensures(_rmsWindows.Contains(rmsWindow));

            _rmsWindows.Add(rmsWindow);
        }

        internal float GetResult()
        {
            // Select the best representative value from the 95th percentile:
            var unsortedWindows = _rmsWindows.ToArray();
            float averageEnergy = unsortedWindows.OrderBy(item => item).ElementAt((int)Math.Ceiling(unsortedWindows.Length * _rmsPercentile) - 1);

            // Subtract from the perceived loudness of pink noise at 89dB to get the recommended adjustment:
            return _pinkNoiseReference - averageEnergy;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_rmsWindows != null);
        }
    }
}
