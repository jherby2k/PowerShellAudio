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

using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Wave
{
    class RiffReader : BinaryReader
    {
        uint _riffChunkSize;

        internal RiffReader([NotNull] Stream input)
            : base(input, Encoding.ASCII, true)
        {
        }

        internal bool Validate()
        {
            var result = false;

            BaseStream.Position = 0;
            if (new string(ReadChars(4)) == "RIFF")
                result = true;

            // Initialize riffChunkSize now, since it is stored adjacent:
            _riffChunkSize = ReadUInt32();

            return result;
        }

        [NotNull]
        internal string ReadFourCC()
        {
            BaseStream.Position = 8;
            var result = new string(ReadChars(4));

            return result;
        }

        internal uint SeekToChunk([NotNull] string chunkId)
        {
            BaseStream.Position = 12;

            var currentChunkId = new string(ReadChars(4));
            uint currentChunkLength = ReadUInt32();

            // If riffChunkSize hasn't been initialized, do so:
            if (_riffChunkSize == 0)
            {
                BaseStream.Position = 4;
                _riffChunkSize = ReadUInt32();
            }

            while (currentChunkId != chunkId)
            {
                // Chunks are word-aligned:
                BaseStream.Seek(currentChunkLength + currentChunkLength % 2, SeekOrigin.Current);

                if (BaseStream.Position >= _riffChunkSize + 8)
                    return 0;

                currentChunkId = new string(ReadChars(4));
                currentChunkLength = ReadUInt32();
            }

            return currentChunkLength;
        }
    }
}
