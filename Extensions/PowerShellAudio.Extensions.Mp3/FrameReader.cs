/*
 * Copyright © 2014 Jeremy Herbison
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
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

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
                // A frame begins with the first 11 bits set:
                byte currentByte;
                while (true)
                {
                    currentByte = ReadByte();

                    if (currentByte == 0xFF)
                    {
                        ReadByte();
                        if (currentByte >= 0xE0)
                        {
                            BaseStream.Seek(-2, SeekOrigin.Current);
                            return;
                        }
                    }
                }
            }
            catch (EndOfStreamException e)
            {
                throw new UnsupportedAudioException(Resources.FrameReaderSyncError, e);
            }
        }
        
        internal uint ReadBEUInt32()
        {
            Contract.Ensures(_buffer != null);
            Contract.Ensures(_buffer.Length == 4);

            Read(_buffer, 0, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(_buffer);
            return BitConverter.ToUInt32(_buffer, 0);
        }

        internal XingHeader ReadXingHeader()
        {
            XingHeader result = new XingHeader();

            string headerID = new string(ReadChars(4));
            if (headerID == "Xing" || headerID == "Info")
            {
                // The flags DWORD indicates whether the frame and byte counts are present:
                uint flags = ReadBEUInt32();

                if ((flags & 0x1) == 1)
                    result.FrameCount = ReadBEUInt32();

                if ((flags >> 1 & 0x1) == 1)
                    result.ByteCount = ReadBEUInt32();
            }

            return result;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_buffer != null);
            Contract.Invariant(_buffer.Length == 4);
        }
    }
}
