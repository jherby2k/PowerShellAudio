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

using PowerShellAudio.Extensions.Mp3.Properties;
using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Mp3
{
    class FrameReader : BinaryReader
    {
        readonly byte[] _buffer = new byte[4];

        internal FrameReader(Stream input)
            : base(input, Encoding.ASCII, true)
        { }

        internal void SeekToNextFrame()
        {
            try
            {
                // First, check if there in an ID3v2 tag present, and skip it:
                if (BaseStream.Position == 0)
                {
                    if (new string(ReadChars(3)) == "ID3")
                    {
                        BaseStream.Seek(3, SeekOrigin.Current);
                        BaseStream.Seek(this.ReadInt32SyncSafe(), SeekOrigin.Current);
                    }
                    else
                        BaseStream.Position = 0;
                }

                // A frame begins with the first 11 bits set:
                while (true)
                {
                    if (ReadByte() != 0xff || ReadByte() < 0xe0) continue;
                    BaseStream.Seek(-2, SeekOrigin.Current);
                    return;
                }
            }
            catch (EndOfStreamException e)
            {
                throw new UnsupportedAudioException(Resources.FrameReaderSyncError, e);
            }
        }

        internal bool VerifyFrameSync([NotNull] FrameHeader header)
        {
            try
            {
                int frameLength = header.SamplesPerFrame / 8 * header.BitRate * 1000 / header.SampleRate +
                                  header.Padding;

                // Seek to where the next frame should start, assuming the current position is just past the header:
                long initialPosition = BaseStream.Position;
                BaseStream.Seek(frameLength - 4, SeekOrigin.Current);
                byte firstByte = ReadByte();
                byte secondByte = ReadByte();
                BaseStream.Position = initialPosition;

                // If another sync is detected, return success:
                return firstByte == 0xff && secondByte >= 0xe0;
            }
            catch (Exception e) when (e is UnsupportedAudioException || e is IOException)
            {
                // Treat a bad header as an invalid sync:
                return false;
            }
        }

        internal XingHeader ReadXingHeader()
        {
            var result = new XingHeader();

            var headerId = new string(ReadChars(4));
            if (headerId != "Xing" && headerId != "Info")
                return result;

            // The flags DWORD indicates whether the frame and byte counts are present:
            uint flags = ReadUInt32BigEndian();

            if ((flags & 0x1) == 1)
                result.FrameCount = ReadUInt32BigEndian();

            if ((flags >> 1 & 0x1) == 1)
                result.ByteCount = ReadUInt32BigEndian();

            return result;
        }

        uint ReadUInt32BigEndian()
        {
            Read(_buffer, 0, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(_buffer);
            return BitConverter.ToUInt32(_buffer, 0);
        }
    }
}
