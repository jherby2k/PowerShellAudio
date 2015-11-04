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

using PowerShellAudio.Properties;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Security;

namespace PowerShellAudio
{
    /// <summary>
    /// The cover art for an audio file.
    /// </summary>
    public class CoverArt
    {
        WeakReference<byte[]> _dataReference;
        FileInfo _tempFile;

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
        /// Gets the color depth, in bits.
        /// </summary>
        /// <value>
        /// The color depth.
        /// </value>
        public int ColorDepth { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverArt"/> class from an existing <see cref="CoverArt"/>
        /// object.
        /// </summary>
        /// <param name="coverArt">The cover art.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="coverArt" /> is null.</exception>
        public CoverArt(CoverArt coverArt)
        {
            Contract.Requires<ArgumentNullException>(coverArt != null);
            Contract.Ensures(_dataReference != null);
            Contract.Ensures(!string.IsNullOrEmpty(MimeType));
            Contract.Ensures(!string.IsNullOrEmpty(Extension));

            Initialize(coverArt.GetData(), false);
        }

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
            Contract.Ensures(_dataReference != null);
            Contract.Ensures(!string.IsNullOrEmpty(MimeType));
            Contract.Ensures(!string.IsNullOrEmpty(Extension));

            Initialize(data, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverArt" /> class.
        /// </summary>
        /// <param name="fileInfo">The file information.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="fileInfo" /> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown if<paramref name="fileInfo" />'s FullName property is an empty string.
        /// </exception>
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
        {
            Contract.Requires<ArgumentNullException>(fileInfo != null);
            Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(fileInfo.FullName));
            Contract.Requires<FileNotFoundException>(fileInfo.Exists);
            Contract.Requires<ArgumentOutOfRangeException>(fileInfo.Length > 0);
            Contract.Ensures(_dataReference != null);

            Initialize(File.ReadAllBytes(fileInfo.FullName), false);
        }

        /// <summary>
        /// Gets a copy of the raw image data.
        /// </summary>
        /// <returns>A copy of the data.</returns>
        public byte[] GetData()
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);

            byte[] result;

            // If the reference is still valid, return a clone of the data:
            if (_dataReference.TryGetTarget(out result))
                return (byte[])result.Clone();

            // Otherwise, load the temporary file and update the reference:
            result = File.ReadAllBytes(_tempFile.FullName);
            if (_dataReference == null)
                _dataReference = new WeakReference<byte[]>(result);
            else
                _dataReference.SetTarget(result);
            return result;
        }

        /// <summary>
        /// Exports the cover art to the specified directory, using the specified file name.
        /// </summary>
        /// <param name="directory">The output directory.</param>
        /// <param name="fileName">The name of the file, without extension.</param>
        /// <param name="replaceExisting">if set to <c>true</c>, replace the file if it already exists.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="directory" /> is null, or <paramref name="fileName" /> is null or empty.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// Thrown if <paramref name="directory" /> does not exist.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Only a directory object makes sense here")]
        public void Export(DirectoryInfo directory, string fileName, bool replaceExisting = false)
        {
            Contract.Requires<ArgumentNullException>(directory != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(fileName));

            string outputName = Path.Combine(directory.FullName, fileName + Extension);

            if (!replaceExisting && File.Exists(outputName))
                throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.CoverArtFileExistsError, outputName));

            File.WriteAllBytes(outputName, GetData());
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="CoverArt"/> class.
        /// </summary>
        ~CoverArt()
        {
            try
            {
                _tempFile.Delete();
            }
            catch (IOException)
            { }
            catch (SecurityException)
            { }
        }

        void Initialize(byte[] data, bool copy)
        {
            Contract.Requires(data != null);
            Contract.Requires(data.Length > 0);
            Contract.Ensures(_dataReference != null);
            Contract.Ensures(!string.IsNullOrEmpty(MimeType));
            Contract.Ensures(!string.IsNullOrEmpty(Extension));

            // This will throw an exception if it isn't a valid image:
            using (var memoryStream = new MemoryStream(data))
            using (Image image = Image.FromStream(memoryStream))
            {
                // Convert bitmaps to PNG format:
                if (image.RawFormat.Guid == ImageFormat.Bmp.Guid)
                    using (var pngStream = new MemoryStream())
                    {
                        image.Save(pngStream, ImageFormat.Png);
                        
                        SetData(pngStream.ToArray());
                        MimeType = "image/png";
                        Extension = ".png";
                    }
                else
                {
                    if (copy)
                        SetData((byte[])data.Clone());
                    else
                        SetData(data);

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
                }

                Width = image.Width;
                Height = image.Height;

                switch (image.PixelFormat)
                {
                    case PixelFormat.Format16bppRgb555:
                    case PixelFormat.Format16bppRgb565:
                    case PixelFormat.Format16bppArgb1555:
                    case PixelFormat.Format16bppGrayScale:
                        ColorDepth = 16;
                        break;

                    case PixelFormat.Format24bppRgb:
                        ColorDepth = 24;
                        break;

                    case PixelFormat.Format32bppRgb:
                    case PixelFormat.Format32bppPArgb:
                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Canonical:
                        ColorDepth = 32;
                        break;

                    case PixelFormat.Format48bppRgb:
                        ColorDepth = 48;
                        break;

                    case PixelFormat.Format64bppPArgb:
                    case PixelFormat.Format64bppArgb:
                        ColorDepth = 64;
                        break;
                }
            }
        }

        void SetData(byte[] data)
        {
            Contract.Requires(data != null);
            Contract.Ensures(_dataReference != null);
            Contract.Ensures(_tempFile != null);
            Contract.Ensures(_tempFile.Exists);

            // To limit memory usage store a weak reference, and cache the data in a temporary file:
            if (_dataReference == null)
                _dataReference = new WeakReference<byte[]>(data);
            else
                _dataReference.SetTarget(data);
            _tempFile = new FileInfo(Path.Combine(Path.GetTempPath(), "AudioShell", Path.GetRandomFileName()));
            _tempFile.Directory?.Create();
            File.WriteAllBytes(_tempFile.FullName, data);
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_dataReference != null);
            Contract.Invariant(!string.IsNullOrEmpty(MimeType));
            Contract.Invariant(!string.IsNullOrEmpty(Extension));
        }
    }
}
