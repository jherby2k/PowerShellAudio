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

using PowerShellAudio.Extensions.ReplayGain.Properties;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace PowerShellAudio.Extensions.ReplayGain
{
    abstract class EqualLoudnessFilter : IirFilter
    {
        internal EqualLoudnessFilter(int sampleRate, float[,] a, float[,] b)
            : base(GetCoefficients(a, sampleRate), GetCoefficients(b, sampleRate))
        {
            Contract.Requires(a != null);
            Contract.Requires(a.Length > 0);
            Contract.Requires(b != null);
            Contract.Requires(b.Length > 0);
        }

        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "0#")]
        static float[] GetCoefficients(float[,] coefficientMap, int sampleRate)
        {
            Contract.Requires(coefficientMap != null);
            Contract.Requires(coefficientMap.GetLength(1) > 0);
            Contract.Ensures(Contract.Result<float[]>() != null);
            Contract.Ensures(Contract.Result<float[]>().Length > 0);

            int sampleRateIndex = GetSampleRateIndex(sampleRate);
            int coefficientCount = coefficientMap.GetLength(1);
            var result = new float[coefficientCount];

            for (var coefficient = 0; coefficient < coefficientCount; coefficient++)
                result[coefficient] = coefficientMap[sampleRateIndex, coefficient];

            return result;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Switch statement is simple and easy to maintain.")]
        static int GetSampleRateIndex(int sampleRate)
        {
            Contract.Ensures(Contract.Result<int>() >= 0);

            switch (sampleRate)
            {
                case 192000:
                case 96000:
                case 48000:
                    return 0;

                case 176400:
                case 88200:
                case 44100:
                    return 1;

                case 37800:
                    return 2;

                case 144000:
                case 36000:
                    return 3;

                case 128000:
                case 64000:
                case 32000:
                    return 4;

                case 28000:
                    return 5;

                case 24000:
                    return 6;

                case 22050:
                    return 7;

                case 18900:
                    return 8;

                case 16000:
                    return 9;

                case 12000:
                    return 10;

                case 11025:
                    return 11;

                case 8000:
                    return 12;

                default:
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                        Resources.EqualLoudnessFilterSampleRateError, sampleRate));
            }
        }
    }
}
