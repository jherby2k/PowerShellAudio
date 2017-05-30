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

using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.ReplayGain
{
    class SampleRateConverter
    {
        readonly int _divisor;

        internal SampleRateConverter(int sampleRate)
        {
            _divisor = GetDivisor(sampleRate);
        }

        [NotNull]
        internal SampleCollection Convert([NotNull] SampleCollection input)
        {
            if (_divisor == 1 || input.IsLast)
                return input;

            SampleCollection result = SampleCollectionFactory.Instance.Create(input.Channels,
                input.SampleCount / _divisor);

            for (var channel = 0; channel < input.Channels; channel++)
                for (int resultSample = 0, inputSample = 0;
                    resultSample < result.SampleCount;
                    resultSample++, inputSample += _divisor)
                    result[channel][resultSample] = input[channel][inputSample];

            SampleCollectionFactory.Instance.Free(input);

            return result;
        }

        static int GetDivisor(int sampleRate)
        {
            switch (sampleRate)
            {
                case 192000:
                case 176400:
                case 144000:
                case 128000:
                case 112000:
                    return 4;

                case 96000:
                case 88200:
                case 64000:
                case 56000:
                    return 2;

                default:
                    return 1;
            }
        }
    }
}
