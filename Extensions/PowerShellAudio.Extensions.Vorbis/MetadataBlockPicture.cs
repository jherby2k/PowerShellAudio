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
using System.Text;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Vorbis
{
    class MetadataBlockPicture
    {
        readonly string _mimeType;
        readonly string _description;
        readonly uint _width;
        readonly uint _height;
        readonly uint _colorDepth;

        internal PictureType Type { get; }

        [NotNull]
        internal byte[] Data { get; }

        internal MetadataBlockPicture([NotNull] string encodedData)
        {
            Stream stream = null;
            try
            {
                stream = new MemoryStream(Convert.FromBase64String(encodedData));
                using (var reader = new BinaryReader(stream))
                {
                    stream = null;

                    Type = (PictureType)reader.ReadUInt32BigEndian();
                    _mimeType = Encoding.ASCII.GetString(reader.ReadBytes((int)reader.ReadUInt32BigEndian()));
                    _description = Encoding.UTF8.GetString(reader.ReadBytes((int)reader.ReadUInt32BigEndian()));
                    _width = reader.ReadUInt32BigEndian();
                    _height = reader.ReadUInt32BigEndian();
                    _colorDepth = reader.ReadUInt32BigEndian();
                    reader.BaseStream.Seek(4, SeekOrigin.Current); // Always 0 for PNG and JPEG
                    Data = reader.ReadBytes((int)reader.ReadUInt32BigEndian());
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }

        internal MetadataBlockPicture([NotNull] CoverArt coverArt)
        {
            Type = PictureType.CoverFront;
            _mimeType = coverArt.MimeType;
            _description = string.Empty;
            _width = (uint)coverArt.Width;
            _height = (uint)coverArt.Height;
            _colorDepth = (uint)coverArt.ColorDepth;
            Data = coverArt.GetData();
        }

        public override string ToString()
        {
            Stream stream = null;
            try
            {
                stream = new MemoryStream();
                using (var writer = new BinaryWriter(stream))
                {
                    stream = null;

                    writer.WriteBigEndian((uint)Type);

                    byte[] mimeBytes = Encoding.ASCII.GetBytes(_mimeType);
                    writer.WriteBigEndian((uint)mimeBytes.Length);
                    writer.Write(mimeBytes);

                    byte[] descriptionBytes = Encoding.UTF8.GetBytes(_description);
                    writer.WriteBigEndian((uint)descriptionBytes.Length);
                    writer.Write(descriptionBytes);

                    writer.WriteBigEndian(_width);
                    writer.WriteBigEndian(_height);
                    writer.WriteBigEndian(_colorDepth);
                    writer.WriteBigEndian(0); // Always 0 for PNG and JPEG
                    writer.WriteBigEndian((uint)Data.Length);
                    writer.Write(Data);

                    return Convert.ToBase64String(((MemoryStream)writer.BaseStream).ToArray());
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }
    }
}
