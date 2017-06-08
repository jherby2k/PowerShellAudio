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

using PowerShellAudio.Extensions.Mp4.Properties;
using System.IO;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Mp4
{
    class EsdsAtom
    {
        static readonly uint[] _sampleRates = { 96000, 88200, 64000, 48000, 44100, 32000, 24000, 22050, 16000, 12000, 11025, 8000, 7350, 0 };

        internal uint AverageBitrate { get; }

        internal uint SampleRate { get; }

        internal ushort Channels { get; }

        internal EsdsAtom([NotNull] byte[] data)
        {
            Stream stream = null;
            try
            {
                stream = new MemoryStream(data);
                using (var reader = new BinaryReader(stream))
                {
                    stream = null;

                    // Seek past the header, version and flags fields:
                    reader.BaseStream.Position = 12;

                    // This appears to be 0 for Apple Lossless files: 
                    if (reader.ReadByte() != 0x3) return;

                    reader.SkipDescriptorLength();
                    reader.BaseStream.Seek(2, SeekOrigin.Current); // Ignore the ES ID
                    reader.BaseStream.Seek(1, SeekOrigin.Current); // TODO check these flags for possible additional data!

                    if (reader.ReadByte() != 0x4)
                        throw new IOException(Resources.EsdsAtomDcdError);
                    reader.SkipDescriptorLength();
                    if (reader.ReadByte() != 0x40)
                        throw new UnsupportedAudioException(Resources.EsdsAtomTypeError);
                    reader.BaseStream.Seek(8, SeekOrigin.Current); // Seek past the stream type, buffer size and max bitrate
                    AverageBitrate = reader.ReadUInt32BigEndian();

                    if (reader.ReadByte() != 0x5)
                        throw new IOException(Resources.EsdsAtomDsiError);
                    reader.SkipDescriptorLength();
                    byte[] dsiBytes = reader.ReadBytes(2);

                    SampleRate = _sampleRates[(dsiBytes[0] << 1) & 0xe | (dsiBytes[1] >> 7) & 0x1];
                    Channels = (ushort)((dsiBytes[1] >> 3) & 0xf);
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }
    }
}
