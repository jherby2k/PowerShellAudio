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

using PowerShellAudio.Extensions.Mp4.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace PowerShellAudio.Extensions.Mp4
{
    class Mp4
    {
        readonly Stream _stream;
        readonly Stack<AtomInfo> _atomInfoStack = new Stack<AtomInfo>();

        public AtomInfo CurrentAtom
        {
            get { return _atomInfoStack.Peek(); }
        }

        internal Mp4(Stream stream)
        {
            Contract.Requires(stream != null);
            Contract.Requires(stream.CanRead);
            Contract.Requires(stream.CanSeek);
            Contract.Ensures(_stream != null);
            Contract.Ensures(_stream == stream);

            _stream = stream;
        }

        internal void DescendToAtom(params string[] hierarchy)
        {
            Contract.Requires(hierarchy != null);
            Contract.Requires(Contract.ForAll(hierarchy, fourCC => !string.IsNullOrEmpty(fourCC)));
            Contract.Requires(Contract.ForAll(hierarchy, fourCC => fourCC.Length == 4));

            _stream.Position = 0;
            _atomInfoStack.Clear();

            using (var reader = new BinaryReader(_stream, Encoding.GetEncoding(1252), true))
            {
                foreach (string fourCC in hierarchy)
                {
                    do
                    {
                        var subAtom = new AtomInfo((uint)_stream.Position, reader.ReadUInt32BigEndian(), reader.ReadFourCC());
                        if (subAtom.Size == 0)
                            throw new IOException(Resources.Mp4AtomNotFoundError);

                        if (subAtom.FourCC == fourCC)
                        {
                            _atomInfoStack.Push(subAtom);

                            // Some containers also contain data, which needs to be skipped:
                            switch (fourCC)
                            {
                                case "meta":
                                    _stream.Seek(4, SeekOrigin.Current);
                                    break;
                                case "stsd":
                                    _stream.Seek(8, SeekOrigin.Current);
                                    break;
                                case "mp4a":
                                    _stream.Seek(28, SeekOrigin.Current);
                                    break;
                            }

                            break;
                        }

                        _stream.Position = subAtom.End;

                    } while (_stream.Position < (_atomInfoStack.Count == 0 ? _stream.Length : _atomInfoStack.Peek().End));
                }
            }
        }

        internal AtomInfo[] GetChildAtomInfo()
        {
            Contract.Ensures(Contract.Result<AtomInfo[]>() != null);

            List<AtomInfo> result = new List<AtomInfo>();

            using (var reader = new BinaryReader(_stream, Encoding.GetEncoding(1252), true))
            {
                _stream.Position = _atomInfoStack.Count == 0 ? 0 : _atomInfoStack.Peek().Start + 8;

                while (_stream.Position < (_atomInfoStack.Count == 0 ? _stream.Length : _atomInfoStack.Peek().End))
                {
                    var childAtom = new AtomInfo((uint)_stream.Position, reader.ReadUInt32BigEndian(), reader.ReadFourCC());
                    result.Add(childAtom);
                    _stream.Position = childAtom.End;
                }
            }

            return result.ToArray();
        }

        internal byte[] ReadAtom(AtomInfo atom)
        {
            Contract.Requires(atom != null);
            Contract.Ensures(Contract.Result<byte[]>() != null);

            _stream.Position = atom.Start;

            using (var reader = new BinaryReader(_stream, Encoding.Default, true))
                return reader.ReadBytes((int)atom.Size);
        }

        internal void CopyAtom(AtomInfo atom, Stream output)
        {
            Contract.Requires(atom != null);
            Contract.Requires(output != null);
            Contract.Requires(output.CanWrite);

            _stream.Position = atom.Start;
            _stream.CopyRangeTo(output, atom.Size);
        }

        internal void UpdateAtomSizes(uint increase)
        {
            if (_atomInfoStack.Count > 0)
                using (var writer = new BinaryWriter(_stream, Encoding.Default, true))
                {
                    do
                    {
                        var currentAtom = _atomInfoStack.Pop();
                        _stream.Position = currentAtom.Start;
                        writer.WriteBigEndian(currentAtom.Size + increase);
                    } while (_atomInfoStack.Count > 0);
                }
        }

        internal void UpdateMvhd(DateTime creation, DateTime modification)
        {
            DescendToAtom("moov", "mvhd");
            int version = _stream.ReadByte();
            _stream.Seek(3, SeekOrigin.Current);

            var epoch = new DateTime(1904, 1, 1);
            double creationSeconds = creation.Subtract(epoch).TotalSeconds;
            double modificationSeconds = modification.Subtract(epoch).TotalSeconds;

            using (var writer = new BinaryWriter(_stream, Encoding.Default, true))
            {
                if (version == 0)
                {
                    writer.WriteBigEndian((uint)creationSeconds);
                    writer.WriteBigEndian((uint)modificationSeconds);
                }
                else
                {
                    writer.WriteBigEndian((ulong)creationSeconds);
                    writer.WriteBigEndian((ulong)modificationSeconds);
                }
            }
        }

        internal void UpdateTkhd(DateTime creation, DateTime modification)
        {
            DescendToAtom("moov", "trak", "tkhd");
            int version = _stream.ReadByte();
            _stream.Seek(3, SeekOrigin.Current);

            var epoch = new DateTime(1904, 1, 1);
            double creationSeconds = creation.Subtract(epoch).TotalSeconds;
            double modificationSeconds = modification.Subtract(epoch).TotalSeconds;

            using (var writer = new BinaryWriter(_stream, Encoding.Default, true))
            {
                if (version == 0)
                {
                    writer.WriteBigEndian((uint)creationSeconds);
                    writer.WriteBigEndian((uint)modificationSeconds);
                }
                else
                {
                    writer.WriteBigEndian((ulong)creationSeconds);
                    writer.WriteBigEndian((ulong)modificationSeconds);
                }
            }
        }

        internal void UpdateMdhd(DateTime creation, DateTime modification)
        {
            DescendToAtom("moov", "trak", "mdia", "mdhd");
            int version = _stream.ReadByte();
            _stream.Seek(3, SeekOrigin.Current);

            var epoch = new DateTime(1904, 1, 1);
            double creationSeconds = creation.Subtract(epoch).TotalSeconds;
            double modificationSeconds = modification.Subtract(epoch).TotalSeconds;

            using (var writer = new BinaryWriter(_stream, Encoding.Default, true))
            {
                if (version == 0)
                {
                    writer.WriteBigEndian((uint)creationSeconds);
                    writer.WriteBigEndian((uint)modificationSeconds);
                }
                else
                {
                    writer.WriteBigEndian((ulong)creationSeconds);
                    writer.WriteBigEndian((ulong)modificationSeconds);
                }
            }
        }

        internal void UpdateStco(int offset)
        {
            if (offset != 0)
            {
                DescendToAtom("moov", "trak", "mdia", "minf", "stbl", "stco");
                _stream.Seek(4, SeekOrigin.Current);

                using (var reader = new BinaryReader(_stream, Encoding.Default, true))
                using (var writer = new BinaryWriter(_stream, Encoding.Default, true))
                {
                    uint count = reader.ReadUInt32BigEndian();
                    long dataStart = _stream.Position;

                    for (int i = 0; i < count; i++)
                    {
                        _stream.Position = dataStart + i * 4;
                        int value = (int)reader.ReadUInt32BigEndian();
                        _stream.Seek(-4, SeekOrigin.Current);
                        writer.WriteBigEndian((uint)(value += offset));
                    }
                }
            }
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_stream != null);
            Contract.Invariant(_stream.CanRead);
            Contract.Invariant(_stream.CanSeek);
            Contract.Invariant(_atomInfoStack != null);
        }
    }
}
