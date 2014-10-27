/*
 * Copyright © 2014 Jeremy Herbison
 * 
 * This file is part of AudioShell.
 * 
 * AudioShell is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option)
 * any later version.
 * 
 * AudioShell is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for more
 * details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with AudioShell.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace AudioShell.Extensions.ReplayGain
{
    class SampleCountFilter
    {
        SampleCollection _buffer;
        int _bufferedSampleCount;

        internal SampleCountFilter(int channels, int sampleCount)
        {
            Contract.Requires(channels > 0);
            Contract.Requires(sampleCount > 0);
            Contract.Ensures(_buffer != null);
            Contract.Ensures(_buffer.Channels == channels);
            Contract.Ensures(_buffer.SampleCount == sampleCount);

            _buffer = SampleCollectionFactory.Instance.Create(channels, sampleCount);
        }

        internal SampleCollection[] Process(SampleCollection input)
        {
            Contract.Requires(input != null);
            Contract.Ensures(Contract.Result<SampleCollection[]>() != null);

            // If the size matches the input (and the buffer is empty), just pass the input through:
            if (input.SampleCount == _buffer.SampleCount && _bufferedSampleCount == 0)
                return new[] { input };

            // If input is an empty (last) sample collection, return the buffer, then the empty one:
            if (input.IsLast)
            {
                if (_bufferedSampleCount > 0)
                {
                    SampleCollectionFactory.Instance.Resize(_buffer, _bufferedSampleCount);
                    return new[] { _buffer, input };
                }
                else
                    return new[] { input };
            }

            // Otherwise, the result has to be built and then returned:
            var results = new List<SampleCollection>();
            int inputIndex = 0;

            // While there are enough samples available to return at least one result:
            while (_bufferedSampleCount + input.SampleCount - inputIndex > _buffer.SampleCount)
            {
                // Copy as much of the input into the buffer as possible:
                int bufferToFill = _buffer.SampleCount - _bufferedSampleCount;
                for (int channel = 0; channel < _buffer.Channels; channel++)
                    Array.Copy(input[channel], inputIndex, _buffer[channel], _bufferedSampleCount, bufferToFill);

                inputIndex += bufferToFill;
                _bufferedSampleCount = 0;

                // Return the buffer itself, then create a new one:
                results.Add(_buffer);
                _buffer = SampleCollectionFactory.Instance.Create(_buffer.Channels, _buffer.SampleCount);
            }

            // Copy any remaining samples into the buffer:
            int remainingInputSamples = input.SampleCount - inputIndex;
            for (int channel = 0; channel < _buffer.Channels; channel++)
                Array.Copy(input[channel], inputIndex, _buffer[channel], _bufferedSampleCount, remainingInputSamples);
            _bufferedSampleCount += remainingInputSamples;

            // Free the input:
            SampleCollectionFactory.Instance.Free(input);

            return results.ToArray();
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_buffer != null);
            Contract.Invariant(!_buffer.IsLast);
            Contract.Invariant(_bufferedSampleCount >= 0);
        }
    }
}
