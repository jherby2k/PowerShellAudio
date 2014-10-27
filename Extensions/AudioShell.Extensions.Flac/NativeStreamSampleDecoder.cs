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
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices;

namespace AudioShell.Extensions.Flac
{
    class NativeStreamSampleDecoder : NativeStreamAudioInfoDecoder
    {
        float _divisor;
        int[][] _managedBuffer;

        internal SampleCollection Samples { get; set; }

        internal NativeStreamSampleDecoder(Stream input)
            : base(input)
        {
            Contract.Requires(input != null);
            Contract.Requires(input.CanRead);
            Contract.Requires(input.CanSeek);
            Contract.Requires(input.Length > 0);
            Contract.Requires(input.Position == 0);
        }

        protected override DecoderWriteStatus WriteCallback(IntPtr decoderHandle, ref Frame frame, IntPtr buffer, IntPtr userData)
        {
            Contract.Ensures(_divisor > 0);
            Contract.Ensures(_managedBuffer != null);
            Contract.Ensures(_managedBuffer.Length > 0);
            Contract.Ensures(_managedBuffer[0].Length > 0);
            Contract.Ensures(Samples != null);

            Contract.Assume(frame.Header.Channels > 0);
            Contract.Assume(frame.Header.BlockSize > 0);

            // Initialize the divisor:
            if (_divisor == 0)
                _divisor = (float)Math.Pow(2, frame.Header.BitsPerSample - 1);

            // Initialize the output buffer:
            if (_managedBuffer == null)
            {
                _managedBuffer = new int[frame.Header.Channels][];
                for (int channel = 0; channel < frame.Header.Channels; channel++)
                    _managedBuffer[channel] = new int[frame.Header.BlockSize];
            }

            // Copy the samples from unmanaged memory into the output buffer:
            for (int channel = 0; channel < frame.Header.Channels; channel++)
            {
                IntPtr channelPtr = Marshal.ReadIntPtr(buffer, channel * Marshal.SizeOf(buffer));
                Marshal.Copy(channelPtr, _managedBuffer[channel], 0, (int)frame.Header.BlockSize);
            }

            Samples = SampleCollectionFactory.Instance.Create((int)frame.Header.Channels, (int)frame.Header.BlockSize);

            // Copy the output buffer into a new sample block, converting to floating point values:
            for (int channel = 0; channel < (int)frame.Header.Channels; channel++)
                for (int sample = 0; sample < (int)frame.Header.BlockSize; sample++)
                    Samples[channel][sample] = _managedBuffer[channel][sample] / _divisor;

            return DecoderWriteStatus.Continue;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_divisor >= 0);
        }
    }
}
