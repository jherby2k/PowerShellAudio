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
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.ReplayGain
{
    [SampleAnalyzerExport("ReplayGain")]
    public class ReplayGainAnalyzer : ISampleAnalyzer, IDisposable
    {
        const int _boundedCapacity = 10;
        const float _rmsWindowTime = 0.05f;

        static readonly SampleAnalyzerInfo _analyzerInfo = new ReplayGainSampleAnalyzerInfo();
        static readonly ConcurrentDictionary<GroupToken, AlbumComponent> _albumComponents =
            new ConcurrentDictionary<GroupToken, AlbumComponent>();

        GroupToken _groupToken;
        AlbumComponent _albumComponent;
        TransformManyBlock<SampleCollection, SampleCollection> _filterSampleCountBlock;
        BufferBlock<MetadataDictionary> _bufferResultsBlock;

        [NotNull]
        public SampleAnalyzerInfo AnalyzerInfo => _analyzerInfo;

        public void Initialize([NotNull] AudioInfo audioInfo, [NotNull] GroupToken groupToken)
        {
            _groupToken = groupToken;
            _albumComponent = _albumComponents.AddOrUpdate(groupToken, token =>
                new AlbumComponent(token.Count), (token, result) => result);
            InitializePipeline(audioInfo.Channels, audioInfo.SampleRate);
        }

        public bool ManuallyFreesSamples => true;

        public void Submit(SampleCollection samples)
        {
            _filterSampleCountBlock.SendAsync(samples).Wait();
        }

        [NotNull]
        public MetadataDictionary GetResult()
        {
            try
            {
                return _bufferResultsBlock.Receive();
            }
            catch (InvalidOperationException)
            {
                if (_bufferResultsBlock.Completion.Exception == null)
                    throw;

                foreach (Exception innerException in _bufferResultsBlock.Completion.Exception.Flatten().InnerExceptions)
                    throw innerException;
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _albumComponent != null && _albumComponent.SetTrackDisposed())
            {
                _albumComponents.TryRemove(_groupToken, out AlbumComponent removedAlbumComponent);
            }
        }

        void InitializePipeline(int channels, int sampleRate)
        {
            var boundedExecutionBlockOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = _boundedCapacity,
                SingleProducerConstrained = true
            };
            var unboundedExecutionBlockOptions = new ExecutionDataflowBlockOptions { SingleProducerConstrained = true };
            var propogateLinkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            // First, resize the sample count collections to the desired window size:
            var sampleCountFilter = new SampleCountFilter(channels, (int)Math.Round(sampleRate * _rmsWindowTime));
            _filterSampleCountBlock =
                new TransformManyBlock<SampleCollection, SampleCollection>(input => sampleCountFilter.Process(input),
                    boundedExecutionBlockOptions);

            // Calculate the track peaks:
            var peakDetector = new PeakDetector();
            var analyzeTrackPeaksBlock = new TransformBlock<SampleCollection, Tuple<SampleCollection, float>>(input =>
            {
                peakDetector.Submit(input);
                return Tuple.Create(input, peakDetector.Peak);
            }, boundedExecutionBlockOptions);
            _filterSampleCountBlock.LinkTo(analyzeTrackPeaksBlock, propogateLinkOptions);

            // Down-convert certain samples rates (easy multiples) that aren't directly supported by ReplayGain:
            var sampleRateConverter = new SampleRateConverter(sampleRate);
            var convertSampleRateBlock =
                new TransformBlock<Tuple<SampleCollection, float>, Tuple<SampleCollection, float>>(input =>
                {
                    SampleCollection result = sampleRateConverter.Convert(input.Item1);
                    return Tuple.Create(result, input.Item2);
                }, boundedExecutionBlockOptions);
            analyzeTrackPeaksBlock.LinkTo(convertSampleRateBlock, propogateLinkOptions);

            // Filter the samples:
            var butterworthFilter = new ButterworthFilter(sampleRate);
            var butterworthFilterBlock =
                new TransformBlock<Tuple<SampleCollection, float>, Tuple<SampleCollection, float>>(input =>
                {
                    butterworthFilter.Process(input.Item1);
                    return input;
                }, boundedExecutionBlockOptions);
            convertSampleRateBlock.LinkTo(butterworthFilterBlock, propogateLinkOptions);

            var yuleWalkFilter = new YuleWalkFilter(sampleRate);
            var yuleWalkFilterBlock =
                new TransformBlock<Tuple<SampleCollection, float>, Tuple<SampleCollection, float>>(input =>
                {
                    yuleWalkFilter.Process(input.Item1);
                    return input;
                }, boundedExecutionBlockOptions);
            butterworthFilterBlock.LinkTo(yuleWalkFilterBlock, propogateLinkOptions);

            // Calculate the root mean square for each filtered window:
            var calculateRmsBlock =
                new TransformBlock<Tuple<SampleCollection, float>, Tuple<SampleCollection, float, float>>(input =>
                    Tuple.Create(input.Item1, input.Item2, input.Item1.IsLast
                        ? float.NaN
                        : CalculateRms(input.Item1)), boundedExecutionBlockOptions);
            yuleWalkFilterBlock.LinkTo(calculateRmsBlock, propogateLinkOptions);

            // Free the sample collections once they are no longer needed:
            var freeSampleCollectionsBlock =
                new TransformBlock<Tuple<SampleCollection, float, float>, Tuple<float, float>>(input =>
                {
                    SampleCollectionFactory.Instance.Free(input.Item1);
                    return Tuple.Create(input.Item2, input.Item3);
                }, boundedExecutionBlockOptions);
            calculateRmsBlock.LinkTo(freeSampleCollectionsBlock, propogateLinkOptions);

            // Broadcast the RMS values:
            var broadcastRmsBlock =
                new BroadcastBlock<Tuple<float, float>>(input => Tuple.Create(input.Item1, input.Item2));
            freeSampleCollectionsBlock.LinkTo(broadcastRmsBlock, propogateLinkOptions);

            // Calculate the album gain:
            broadcastRmsBlock.LinkTo(_albumComponent.InputBlock);

            // Calculate the track gain:
            var windowSelector = new WindowSelector();
            var analyzeTrackGainBlock = new TransformBlock<Tuple<float, float>, Tuple<float, float>>(input =>
            {
                if (float.IsNaN(input.Item2))
                    return Tuple.Create(input.Item1, windowSelector.GetResult());
                windowSelector.Submit(input.Item2);
                return Tuple.Create(input.Item1, float.NaN);
            }, unboundedExecutionBlockOptions);
            broadcastRmsBlock.LinkTo(analyzeTrackGainBlock, propogateLinkOptions);

            // Join the track and album peak and gain values all together:
            var joinResultsBlock = new JoinBlock<Tuple<float, float>, Tuple<float, float>>();
            analyzeTrackGainBlock.LinkTo(DataflowBlock.NullTarget<Tuple<float, float>>(),
                result => float.IsNaN(result.Item2));
            analyzeTrackGainBlock.LinkTo(joinResultsBlock.Target1, propogateLinkOptions,
                result => !float.IsNaN(result.Item2));
            _albumComponent.OutputBlock.LinkTo(joinResultsBlock.Target2, propogateLinkOptions);

            // Convert the results:
            var convertToMetadataBlock =
                new TransformBlock<Tuple<Tuple<float, float>, Tuple<float, float>>, MetadataDictionary>(input =>
                {
                    var result = new MetadataDictionary
                    {
                        ["TrackPeak"] = ConvertPeakToString(input.Item1.Item1),
                        ["TrackGain"] = ConvertGainToString(input.Item1.Item2),
                        ["AlbumPeak"] = ConvertPeakToString(input.Item2.Item1),
                        ["AlbumGain"] = ConvertGainToString(input.Item2.Item2)
                    };
                    return result;
                }, unboundedExecutionBlockOptions);
            joinResultsBlock.LinkTo(convertToMetadataBlock, propogateLinkOptions);

            // Buffer the results:
            _bufferResultsBlock = new BufferBlock<MetadataDictionary>();
            convertToMetadataBlock.LinkTo(_bufferResultsBlock, propogateLinkOptions);
        }

        static float CalculateRms([NotNull] SampleCollection samples)
        {
            float sumOfSquares = samples.SelectMany(channel => channel).Sum(sample => sample * sample);
            return 10 * (float)Math.Log10(sumOfSquares / samples.SampleCount / samples.Channels);
        }

        [NotNull]
        static string ConvertPeakToString(float peak)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.000000}", peak);
        }

        [NotNull]
        static string ConvertGainToString(float gain)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:0.00} dB", gain);
        }
    }
}
