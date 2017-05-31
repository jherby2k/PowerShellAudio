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
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.ReplayGain
{
    [SampleAnalyzerExport("ReplayGain 2.0")]
    public class ReplayGain2Analyzer : ISampleAnalyzer, IDisposable
    {
        const int _referenceLevel = -18;
        static readonly SampleAnalyzerInfo _analyzerInfo = new ReplayGain2SampleAnalyzerInfo();

        GroupToken _groupToken;
        NativeR128Analyzer _analyzer;
        float[] _buffer;

        [NotNull]
        public SampleAnalyzerInfo AnalyzerInfo => _analyzerInfo;

        public void Initialize([NotNull] AudioInfo audioInfo, [NotNull] GroupToken groupToken)
        {
            _groupToken = groupToken;
            _analyzer = new NativeR128Analyzer((uint)audioInfo.Channels, (uint)audioInfo.SampleRate, groupToken);
        }

        [NotNull]
        public MetadataDictionary GetResult()
        {
            var result = new MetadataDictionary
            {
                ["TrackPeak"] = ConvertPeakToString(_analyzer.GetSamplePeak()),
                ["TrackGain"] = ConvertGainToString(_referenceLevel - _analyzer.GetLoudness())
            };

            _groupToken.CompleteMember();
            _groupToken.WaitForMembers();
            
            result["AlbumPeak"] = ConvertPeakToString(_analyzer.GetSamplePeakMultiple());
            result["AlbumGain"] = ConvertGainToString(_referenceLevel - _analyzer.GetLoudnessMultiple());

            return result;
        }

        public bool ManuallyFreesSamples => false;

        public void Submit([NotNull] SampleCollection samples)
        {
            if (_buffer == null)
                _buffer = new float[samples.Channels * samples.SampleCount];

            // Interlace the samples, and store them in the buffer:
            var index = 0;
            for (var sample = 0; sample < samples.SampleCount; sample++)
                for (var channel = 0; channel < samples.Channels; channel++)
                    _buffer[index++] = samples[channel][sample];

            _analyzer.AddFrames(_buffer);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _analyzer?.Dispose();
        }

        [NotNull]
        static string ConvertGainToString(double gain)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.00} dB", gain);
        }

        [NotNull]
        static string ConvertPeakToString(double peak)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", peak);
        }
    }
}
