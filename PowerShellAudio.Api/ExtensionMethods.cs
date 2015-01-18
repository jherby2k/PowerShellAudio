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

using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace PowerShellAudio
{
    internal static class ExtensionMethods
    {
        internal static void ReadWriteParallel(this ISampleDecoder decoder, ISampleConsumer consumer, CancellationToken cancelToken, bool samplesAreManuallyFreed)
        {
            Contract.Requires(decoder != null);
            Contract.Requires(consumer != null);

            using (var outputQueue = new BlockingCollection<SampleCollection>(10))
            {
                Task decode = Task.Run(() =>
                {
                    SampleCollection samples;
                    do
                    {
                        cancelToken.ThrowIfCancellationRequested();
                        samples = decoder.DecodeSamples();
                        outputQueue.Add(samples);
                    } while (!samples.IsLast);
                }).ContinueWith(task => outputQueue.CompleteAdding());

                foreach (SampleCollection queuedSamples in outputQueue.GetConsumingEnumerable(cancelToken))
                {
                    consumer.Submit(queuedSamples);
                    if (!samplesAreManuallyFreed)
                        SampleCollectionFactory.Instance.Free(queuedSamples);
                }

                // This will re-throw any decoding exceptions:
                decode.Wait();
            }
        }
    }
}
