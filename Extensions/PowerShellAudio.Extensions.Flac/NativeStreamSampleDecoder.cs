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
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Flac
{
    class NativeStreamSampleDecoder : NativeStreamAudioInfoDecoder
    {
        float _divisor;
        int[][] _managedBuffer;

        [CanBeNull]
        internal SampleCollection Samples { get; set; }

        internal NativeStreamSampleDecoder([NotNull] Stream input)
            : base(input)
        {
        }

        protected override DecoderWriteStatus WriteCallback(IntPtr decoderHandle, ref Frame frame, IntPtr buffer, IntPtr userData)
        {
            // Initialize the divisor:
            if (_divisor < 1)
                _divisor = (float)Math.Pow(2, frame.Header.BitsPerSample - 1);

            // Initialize the output buffer:
            if (_managedBuffer == null)
            {
                _managedBuffer = new int[frame.Header.Channels][];
                for (var channel = 0; channel < frame.Header.Channels; channel++)
                    _managedBuffer[channel] = new int[frame.Header.BlockSize];
            }

            // Copy the samples from unmanaged memory into the output buffer:
            for (var channel = 0; channel < frame.Header.Channels; channel++)
            {
                IntPtr channelPtr = Marshal.ReadIntPtr(buffer, channel * Marshal.SizeOf(buffer));
                Marshal.Copy(channelPtr, _managedBuffer[channel], 0, (int)frame.Header.BlockSize);
            }

            Samples = SampleCollectionFactory.Instance.Create((int)frame.Header.Channels, (int)frame.Header.BlockSize);

            // Copy the output buffer into a new sample block, converting to floating point values:
            for (var channel = 0; channel < (int)frame.Header.Channels; channel++)
                for (var sample = 0; sample < (int)frame.Header.BlockSize; sample++)
                    Samples[channel][sample] = _managedBuffer[channel][sample] / _divisor;

            return DecoderWriteStatus.Continue;
        }
    }
}
