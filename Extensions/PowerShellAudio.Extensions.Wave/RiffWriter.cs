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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace PowerShellAudio.Extensions.Wave
{
    class RiffWriter : BinaryWriter
    {
        readonly Stack<Tuple<bool, uint>> _chunkSizePositions = new Stack<Tuple<bool, uint>>();
        readonly string _fourCC;

        internal RiffWriter(Stream output, string fourCC)
            : base(output, Encoding.ASCII, true)
        {
            Contract.Requires(output != null);
            Contract.Requires(output.CanWrite);
            Contract.Requires(output.CanSeek);
            Contract.Requires(!string.IsNullOrEmpty(fourCC));
            Contract.Requires(fourCC.Length == 4);
            Contract.Ensures(!string.IsNullOrEmpty(_fourCC));
            Contract.Ensures(_fourCC == fourCC);

            _fourCC = fourCC;
        }

        internal void Initialize()
        {
            BeginChunk("RIFF");
            Write(_fourCC.ToCharArray());
        }

        internal void BeginChunk(string chunkId)
        {
            Contract.Requires(!string.IsNullOrEmpty(chunkId));

            Write(chunkId.ToCharArray());
            _chunkSizePositions.Push(Tuple.Create(false, (uint)BaseStream.Position));
            Write((uint)0);
        }

        internal void BeginChunk(string chunkId, uint chunkSize)
        {
            Contract.Requires(!string.IsNullOrEmpty(chunkId));

            Write(chunkId.ToCharArray());
            _chunkSizePositions.Push(Tuple.Create(true, (uint)BaseStream.Position));
            Write(chunkSize);
        }

        internal void FinishChunk()
        {
            Tuple<bool, uint> chunkSizePosition = _chunkSizePositions.Pop();

            // If the chunk size wasn't known at the beginning, update it now:
            if (!chunkSizePosition.Item1)
            {
                var currentPosition = (uint)BaseStream.Position;
                BaseStream.Position = chunkSizePosition.Item2;
                Write((uint)(currentPosition - BaseStream.Position - 4));
                BaseStream.Position = currentPosition;
            }

            // Chunks should be word-aligned:
            if (BaseStream.Position % 2 == 1)
                Write((byte)0);
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_chunkSizePositions != null);
            Contract.Invariant(!string.IsNullOrEmpty(_fourCC));
            Contract.Invariant(_fourCC.Length == 4);
        }
    }
}
