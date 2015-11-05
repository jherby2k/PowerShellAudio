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
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace PowerShellAudio.Extensions.Vorbis
{
    class MetadataBlockPicture
    {
        internal PictureType Type { get; set; }

        internal string MimeType { get; set; }

        internal string Description { get; set; }

        internal uint Width { get; set; }

        internal uint Height { get; set; }

        internal uint ColorDepth { get; set; }

        internal byte[] Data { get; set; }

        internal MetadataBlockPicture(string encodedData)
        {
            Contract.Requires(!string.IsNullOrEmpty(encodedData));
            Contract.Ensures(MimeType != null);
            Contract.Ensures(Description != null);
            Contract.Ensures(Data != null);

            Stream stream = null;
            try
            {
                stream = new MemoryStream(Convert.FromBase64String(encodedData));
                using (var reader = new BinaryReader(stream))
                {
                    stream = null;

                    Type = (PictureType)reader.ReadUInt32BigEndian();
                    MimeType = Encoding.ASCII.GetString(reader.ReadBytes((int)reader.ReadUInt32BigEndian()));
                    Description = Encoding.UTF8.GetString(reader.ReadBytes((int)reader.ReadUInt32BigEndian()));
                    Width = reader.ReadUInt32BigEndian();
                    Height = reader.ReadUInt32BigEndian();
                    ColorDepth = reader.ReadUInt32BigEndian();
                    reader.BaseStream.Seek(4, SeekOrigin.Current); // Always 0 for PNG and JPEG
                    Data = reader.ReadBytes((int)reader.ReadUInt32BigEndian());
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }

        internal MetadataBlockPicture(CoverArt coverArt)
        {
            Type = PictureType.CoverFront;
            MimeType = coverArt.MimeType;
            Description = string.Empty;
            Width = (uint)coverArt.Width;
            Height = (uint)coverArt.Height;
            ColorDepth = (uint)coverArt.ColorDepth;
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

                    byte[] mimeBytes = Encoding.ASCII.GetBytes(MimeType);
                    writer.WriteBigEndian((uint)mimeBytes.Length);
                    writer.Write(mimeBytes);

                    byte[] descriptionBytes = Encoding.UTF8.GetBytes(Description);
                    writer.WriteBigEndian((uint)descriptionBytes.Length);
                    writer.Write(descriptionBytes);

                    writer.WriteBigEndian(Width);
                    writer.WriteBigEndian(Height);
                    writer.WriteBigEndian(ColorDepth);
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
