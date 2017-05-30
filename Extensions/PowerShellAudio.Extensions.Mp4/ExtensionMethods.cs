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
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Mp4
{
    static class ExtensionMethods
    {
        internal static void CopyRangeTo([NotNull] this Stream input, [NotNull] Stream output, long count)
        {
            var buffer = new byte[1024];
            do
            {
                int read = input.Read(buffer, 0, (int)Math.Min(buffer.Length, count));
                output.Write(buffer, 0, read);
                count -= read;
            } while (count > 0);
        }

        internal static uint ReadUInt32BigEndian([NotNull] this BinaryReader reader)
        {
            return ((uint)reader.ReadByte() << 24) 
                + ((uint)reader.ReadByte() << 16) 
                + ((uint)reader.ReadByte() << 8) 
                + reader.ReadByte();
        }

        internal static string ReadFourCC([NotNull] this BinaryReader reader)
        {
            char[] buffer = reader.ReadChars(4);
            if (buffer.Length < 4)
                throw new EndOfStreamException();

            return new string(buffer);
        }

        internal static uint ReadDescriptorLength([NotNull] this BinaryReader reader)
        {
            uint result = 0;

            byte currentByte;
            do
            {
                currentByte = reader.ReadByte();
                result = (result << 7) | (uint)(currentByte & 0x7f);
            } while ((currentByte & 0x80) == 0x80);

            return result;
        }

        internal static void WriteBigEndian([NotNull] this BinaryWriter writer, uint value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            Array.Reverse(buffer);
            writer.Write(buffer);
        }

        internal static void WriteBigEndian([NotNull] this BinaryWriter writer, ulong value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            Array.Reverse(buffer);
            writer.Write(buffer);
        }
    }
}
