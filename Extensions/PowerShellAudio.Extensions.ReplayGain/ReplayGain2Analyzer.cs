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
using System.Globalization;

namespace PowerShellAudio.Extensions.ReplayGain
{
    [SampleAnalyzerExport("ReplayGain 2.0")]
    public class ReplayGain2Analyzer : ISampleAnalyzer, IDisposable
    {
        const int referenceLevel = -18;
        static readonly SampleAnalyzerInfo _analyzerInfo = new ReplayGain2SampleAnalyzerInfo();

        GroupToken _groupToken;
        NativeR128Analyzer _analyzer;
        float[] _buffer;

        public SampleAnalyzerInfo AnalyzerInfo
        {
            get
            {
                Contract.Ensures(Contract.Result<SampleAnalyzerInfo>() != null);

                return _analyzerInfo;
            }
        }

        public void Initialize(AudioInfo audioInfo, GroupToken groupToken)
        {
            Contract.Ensures(_groupToken != null);
            Contract.Ensures(_groupToken == groupToken);
            Contract.Ensures(_analyzer != null);

            _groupToken = groupToken;
            _analyzer = new NativeR128Analyzer((uint)audioInfo.Channels, (uint)audioInfo.SampleRate, groupToken);
        }

        public MetadataDictionary GetResult()
        {
            Contract.Ensures(Contract.Result<MetadataDictionary>() != null);

            var result = new MetadataDictionary();

            result["TrackPeak"] = ConvertPeakToString(_analyzer.GetSamplePeak());
            result["TrackGain"] = ConvertGainToString(referenceLevel - _analyzer.GetLoudness());

            _groupToken.CompleteMember();
            _groupToken.WaitForMembers();
            
            result["AlbumPeak"] = ConvertPeakToString(_analyzer.GetSamplePeakMultiple());
            result["AlbumGain"] = ConvertGainToString(referenceLevel - _analyzer.GetLoudnessMultiple());

            return result;
        }

        public bool ManuallyFreesSamples
        {
            get { return false; }
        }

        public void Submit(SampleCollection samples)
        {
            Contract.Ensures(_buffer != null);

            if (_buffer == null)
                _buffer = new float[samples.Channels * samples.SampleCount];

            // Interlace the samples, and store them in the buffer:
            int index = 0;
            for (int sample = 0; sample < samples.SampleCount; sample++)
                for (int channel = 0; channel < samples.Channels; channel++)
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
            if (disposing && _analyzer != null)
                _analyzer.Dispose();
        }

        static string ConvertGainToString(double gain)
        {
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

            return string.Format(CultureInfo.InvariantCulture, "{0:0.00} dB", gain);
        }

        static string ConvertPeakToString(double peak)
        {
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

            return string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", peak);
        }
    }
}
