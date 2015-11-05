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

using PowerShellAudio.Extensions.Mp3.Properties;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;

namespace PowerShellAudio.Extensions.Mp3
{
    class FrameHeader
    {
        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member", Justification = "Does not waste space")] static readonly int[,] _bitRates =
        {
            { 0, 32, 64, 96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448 },
            { 0, 32, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 384 },
            { 0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320 },
            { 0, 32, 48, 56, 64, 80, 96, 112, 128, 144, 160, 176, 192, 224, 256 },
            { 0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160 }
        };

        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member", Justification = "Does not waste space")] static readonly int[,] _sampleRates =
        {
            { 44100, 48000, 32000 },
            { 22050, 24000, 16000 },
            { 11025, 12000, 8000 }
        };

        readonly byte[] _headerBytes;

        internal string MpegVersion
        {
            get
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

                switch ((_headerBytes[1] >> 3) & 0x3)
                {
                    case 0:
                        return "2.5";
                    case 2:
                        return "2";
                    case 3:
                        return "1";
                    default:
                        throw new UnsupportedAudioException(Resources.FrameHeaderVersionError);
                }
            }
        }

        internal string Layer
        {
            get
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

                switch ((_headerBytes[1] >> 1) & 0x3)
                {
                    case 1:
                        return "III";
                    case 2:
                        return "II";
                    case 3:
                        return "I";
                    default:
                        throw new UnsupportedAudioException(Resources.FrameHeaderLayerError);
                }
            }
        }

        internal bool HasCrc => (_headerBytes[1] & 0x1) == 0;

        internal int BitRate
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);

                int column = (_headerBytes[2] >> 4) & 0xf;
                if (column == 15)
                    throw new IOException(Resources.FrameHeaderBitRateError);

                int row;
                if (MpegVersion == "1")
                    switch (Layer)
                    {
                        case "I":
                            row = 0;
                            break;
                        case "II":
                            row = 1;
                            break;
                        default:
                            row = 2;
                            break;
                    }
                else
                    if (Layer == "I")
                        row = 3;
                    else
                        row = 4;

                return _bitRates[row, column];
            }
        }

        internal int SampleRate
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);

                int column = (_headerBytes[2] >> 2) & 0x3;
                int row;

                if (column == 3)
                    throw new IOException(Resources.FrameHeaderSampleRateError);

                switch (MpegVersion)
                {
                    case "1":
                        row = 0;
                        break;
                    case "2":
                        row = 1;
                        break;
                    default:
                        row = 2;
                        break;
                }

                return _sampleRates[row, column];
            }
        }

        internal int Padding => (_headerBytes[2] >> 1) & 0x1;

        internal string ChannelMode
        {
            get
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

                switch ((_headerBytes[3] >> 6) & 0x3)
                {
                    case 0:
                        return "Stereo";
                    case 1:
                        return "Joint Stereo";
                    case 2:
                        return "Dual Channel";
                    default:
                        return "Mono";
                }
            }
        }

        internal int SamplesPerFrame
        {
            get
            {
                if (Layer == "I")
                    return 384;
                if (Layer == "II" || MpegVersion == "1")
                    return 1152;
                return 576;
            }
        }

        internal FrameHeader(byte[] headerBytes)
        {
            Contract.Requires(headerBytes != null);
            Contract.Requires(headerBytes.Length == 4);
            Contract.Ensures(_headerBytes == headerBytes);

            _headerBytes = headerBytes;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_headerBytes != null);
            Contract.Invariant(_headerBytes.Length == 4);
        }
    }
}
