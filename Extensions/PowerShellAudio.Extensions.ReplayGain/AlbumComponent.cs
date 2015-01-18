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
using System.Threading.Tasks.Dataflow;

namespace PowerShellAudio.Extensions.ReplayGain
{
    class AlbumComponent
    {
        int _tracksStillProcessing;
        int _tracksNotDisposed;
        TransformBlock<Tuple<float, float>, Tuple<float, float>> _analyzeAlbumPeaksBlock;
        BroadcastBlock<Tuple<float, float>> _broadcastAlbumResultsBlock;

        internal ITargetBlock<Tuple<float, float>> InputBlock
        {
            get
            {
                Contract.Ensures(Contract.Result<ITargetBlock<Tuple<float, float>>>() != null);

                return _analyzeAlbumPeaksBlock;
            }
        }

        internal ISourceBlock<Tuple<float, float>> OutputBlock
        {
            get
            {
                Contract.Ensures(Contract.Result<ISourceBlock<Tuple<float, float>>>() != null);

                return _broadcastAlbumResultsBlock;
            }
        }

        internal AlbumComponent(int count)
        {
            Contract.Requires(count > 0);
            Contract.Ensures(_tracksStillProcessing == count);
            Contract.Ensures(_tracksNotDisposed == count);

            _tracksStillProcessing = count;
            _tracksNotDisposed = count;
            InitializePipeline(count);
        }

        internal bool SetTrackDisposed()
        {
            return (Interlocked.Decrement(ref _tracksNotDisposed) == 0);
        }

        void InitializePipeline(int trackCount)
        {
            Contract.Ensures(_analyzeAlbumPeaksBlock != null);
            Contract.Ensures(_broadcastAlbumResultsBlock != null);

            var propagateLinkOptions = new DataflowLinkOptions() { PropagateCompletion = true };

            // Calculate the album peak:
            var peakDetector = new PeakDetector();
            _analyzeAlbumPeaksBlock = new TransformBlock<Tuple<float, float>, Tuple<float, float>>(input =>
            {
                // Only need to submit the peak once per track:
                if (float.IsNaN(input.Item2))
                {
                    peakDetector.Submit(input.Item1);
                    return Tuple.Create(peakDetector.Peak, input.Item2);
                }
                return Tuple.Create(input.Item1, input.Item2);
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = trackCount });

            // Calculate the album gain:
            var windowSelector = new WindowSelector();
            var analyzeAlbumGainBlock = new TransformBlock<Tuple<float, float>, Tuple<float, float>>(input =>
            {
                if (float.IsNaN(input.Item2))
                {
                    if (Interlocked.Decrement(ref _tracksStillProcessing) == 0)
                        return Tuple.Create(input.Item1, windowSelector.GetResult());
                }
                else
                    windowSelector.Submit(input.Item2);
                return Tuple.Create(float.NaN, float.NaN);
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = trackCount, SingleProducerConstrained = true });
            _analyzeAlbumPeaksBlock.LinkTo(analyzeAlbumGainBlock, propagateLinkOptions);

            // Broadcast the results:
            _broadcastAlbumResultsBlock = new BroadcastBlock<Tuple<float, float>>(input => Tuple.Create(input.Item1, input.Item2));
            analyzeAlbumGainBlock.LinkTo(DataflowBlock.NullTarget<Tuple<float, float>>(), result => float.IsNaN(result.Item2));
            analyzeAlbumGainBlock.LinkTo(_broadcastAlbumResultsBlock, propagateLinkOptions, result => !float.IsNaN(result.Item2));
        }
    }
}
