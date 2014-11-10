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

using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace PowerShellAudio.Extensions.Mp4
{
    static class ExtensionMethods
    {
        internal static void CopyRangeTo(this Stream input, Stream output, long count)
        {
            Contract.Requires(input != null);
            Contract.Requires(input.CanRead);
            Contract.Requires(input.Length - input.Position >= count);
            Contract.Requires(output != null);
            Contract.Requires(output.CanSeek);
            Contract.Requires(count > 0);
            Contract.Ensures(output.Length >= Contract.OldValue<long>(count));

            byte[] buffer = new byte[1024];
            int read;
            do
            {
                read = input.Read(buffer, 0, (int)Math.Min(buffer.Length, count));
                output.Write(buffer, 0, read);
                count -= read;
            } while (count > 0);
        }

        internal static uint ReadUInt32BigEndian(this BinaryReader reader)
        {
            Contract.Requires(reader != null);

            byte[] buffer = reader.ReadBytes(4);
            if (buffer.Length < 4)
                throw new EndOfStreamException();

            Array.Reverse(buffer);
            return BitConverter.ToUInt32(buffer, 0);
        }

        internal static string ReadFourCC(this BinaryReader reader)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
            Contract.Ensures(Contract.Result<string>().Length == 4);
            
            char[] buffer = reader.ReadChars(4);
            if (buffer.Length < 4)
                throw new EndOfStreamException();

            return new string(buffer);
        }

        internal static uint ReadDescriptorLength(this BinaryReader reader)
        {
            Contract.Requires(reader != null);

            uint result = 0;

            byte currentByte;
            do
            {
                currentByte = reader.ReadByte();
                result = (result << 7) | (uint)(currentByte & 0x7f);
            } while ((currentByte & 0x80) == 0x80);

            return result;
        }

        internal static void WriteBigEndian(this BinaryWriter writer, uint value)
        {
            Contract.Requires(writer != null);

            var buffer = BitConverter.GetBytes(value);
            Array.Reverse(buffer);
            writer.Write(buffer);
        }

        internal static void WriteBigEndian(this BinaryWriter writer, ulong value)
        {
            Contract.Requires(writer != null);

            var buffer = BitConverter.GetBytes(value);
            Array.Reverse(buffer);
            writer.Write(buffer);
        }
    }
}
