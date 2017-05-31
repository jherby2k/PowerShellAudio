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

using System;
using System.IO;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Vorbis
{
    static class ExtensionMethods
    {
        internal static uint ReadUInt32BigEndian([NotNull] this BinaryReader reader)
        {
            return ((uint)reader.ReadByte() << 24)
                   + ((uint)reader.ReadByte() << 16)
                   + ((uint)reader.ReadByte() << 8)
                   + reader.ReadByte();
        }

        internal static void WriteBigEndian([NotNull] this BinaryWriter writer, uint value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            Array.Reverse(buffer);
            writer.Write(buffer);
        }
    }
}
