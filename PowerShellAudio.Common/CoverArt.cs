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

using PowerShellAudio.Properties;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PowerShellAudio
{
    /// <summary>
    /// The cover art for an audio file.
    /// </summary>
    public class CoverArt
    {
        readonly byte[] _data;

        /// <summary>
        /// Gets the MIME type for this image format.
        /// </summary>
        /// <value>
        /// The MIME type.
        /// </value>
        public string MimeType { get; private set; }

        /// <summary>
        /// Gets the file extension for this image format.
        /// </summary>
        /// <value>
        /// The file extension.
        /// </value>
        public string Extension { get; private set; }

        /// <summary>
        /// Gets the width, in pixels.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height, in pixels.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int Height { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverArt" /> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="data" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="data" /> is an empty array.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Thrown if <paramref name="data" /> does not have a valid image format.
        /// </exception>
        /// <exception cref="UnsupportedCoverArtException">
        /// Thrown if <paramref name="data" /> is not in a supported image format.
        /// </exception>
        public CoverArt(byte[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null);
            Contract.Requires<ArgumentOutOfRangeException>(data.Length > 0);
            Contract.Ensures(_data != null);
            Contract.Ensures(_data == data);
            Contract.Ensures(!string.IsNullOrEmpty(MimeType));
            Contract.Ensures(!string.IsNullOrEmpty(Extension));
            Contract.Ensures(Width > 0);
            Contract.Ensures(Height > 0);

            // This will throw an exception if it isn't a valid image:
            using (var memoryStream = new MemoryStream(data))
            using (Image image = Image.FromStream(memoryStream))
            {
                // Convert bitmaps to PNG format:
                if (image.RawFormat.Guid == ImageFormat.Bmp.Guid)
                    using (MemoryStream pngStream = new MemoryStream())
                    {
                        image.Save(pngStream, ImageFormat.Png);
                        _data = pngStream.ToArray();

                        MimeType = "image/png";
                        Extension = ".png";
                    }
                else
                {
                    if (image.RawFormat.Guid == ImageFormat.Png.Guid)
                    {
                        MimeType = "image/png";
                        Extension = ".png";
                    }
                    else if (image.RawFormat.Guid == ImageFormat.Jpeg.Guid)
                    {
                        MimeType = "image/jpeg";
                        Extension = ".jpg";
                    }
                    else
                        throw new UnsupportedCoverArtException(Resources.CoverArtUnsupportedImageFormat);


                    _data = data;
                }

                Width = image.Width;
                Height = image.Height;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverArt" /> class.
        /// </summary>
        /// <param name="fileInfo">The file information.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="fileInfo" /> is null.</exception>
        /// <exception cref="FileNotFoundException">Thrown if <paramref name="fileInfo" /> does not exist.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="fileInfo" /> is an empty file.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Thrown if <paramref name="fileInfo" /> does not have a valid image format.
        /// </exception>
        /// <exception cref="UnsupportedCoverArtException">
        /// Thrown if <paramref name="fileInfo" /> is not in a supported image format.
        /// </exception>
        public CoverArt(FileInfo fileInfo)
            : this(File.ReadAllBytes(fileInfo.FullName))
        {
            Contract.Requires<ArgumentNullException>(fileInfo != null);
            Contract.Requires<FileNotFoundException>(fileInfo.Exists);
            Contract.Requires<ArgumentOutOfRangeException>(fileInfo.Length > 0);
            Contract.Ensures(_data != null);
            Contract.Ensures(_data.Length > 0);
        }

        /// <summary>
        /// Gets a copy of the raw image data.
        /// </summary>
        /// <returns>A copy of the data.</returns>
        public byte[] GetData()
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == _data.Length);

            return (byte[])_data.Clone();
        }

        /// <summary>
        /// Exports the cover art to the specified directory, using the specified file name.
        /// </summary>
        /// <param name="directory">The output directory.</param>
        /// <param name="fileName">The name of the file, without extension.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="directory" /> is null, or <paramref name="fileName" /> is null or empty.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// Thrown if <paramref name="directory" /> does not exist.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Only a directory object makes sense here")]
        public void Export(DirectoryInfo directory, string fileName)
        {
            Contract.Requires<ArgumentNullException>(directory != null);
            Contract.Requires<DirectoryNotFoundException>(directory.Exists);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(fileName));

            File.WriteAllBytes(Path.Combine(directory.FullName, fileName + Extension), _data);
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_data != null);
            Contract.Invariant(_data.Length > 0);
            Contract.Invariant(!string.IsNullOrEmpty(MimeType));
            Contract.Invariant(!string.IsNullOrEmpty(Extension));
            Contract.Invariant(Width > 0);
            Contract.Invariant(Height > 0);
        }
    }
}
