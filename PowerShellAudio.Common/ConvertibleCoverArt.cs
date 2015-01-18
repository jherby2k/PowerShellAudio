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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace PowerShellAudio
{
    /// <summary>
    /// Represents cover art that can be resized and/or converted to lossy (JPEG) format.
    /// </summary>
    public class ConvertibleCoverArt : CoverArt
    {
        const int _defaultMaxWidth = 65535;
        const bool _defaultConvertToLossy = false;
        const int _defaultQuality = 75;

        /// <summary>
        /// Gets the default maximum width.
        /// </summary>
        /// <value>
        /// The default maximum width.
        /// </value>
        public static int DefaultMaxWidth
        {
            get { return _defaultMaxWidth; }
        }

        /// <summary>
        /// Gets a value indicating whether to convert to lossy by default.
        /// </summary>
        /// <value>
        /// <c>true</c> if lossy conversion is the default; otherwise, <c>false</c>.
        /// </value>
        public static bool DefaultConvertToLossy
        {
            get { return _defaultConvertToLossy; }
        }

        /// <summary>
        /// Gets the default lossy compression quality.
        /// </summary>
        /// <value>
        /// The default quality.
        /// </value>
        public static int DefaultQuality
        {
            get { return _defaultQuality; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertibleCoverArt"/> class from an existing
        /// <see cref="CoverArt" /> object.
        /// </summary>
        /// <param name="coverArt">The cover art.</param>
        public ConvertibleCoverArt(CoverArt coverArt)
            : base(coverArt)
        {
            Contract.Requires<ArgumentNullException>(coverArt != null);
        }

        /// <summary>
        /// Converts the cover art to a new image.
        /// </summary>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="convertToLossy">if set to <c>true</c>, the image will be convert to JPEG format.</param>
        /// <param name="quality">The compression quality, if the image is in lossy (JPEG) format.</param>
        /// <returns>
        /// The new <see cref="CoverArt" /> object.
        /// </returns>
        public CoverArt Convert(int maxWidth = _defaultMaxWidth, bool convertToLossy = _defaultConvertToLossy, int quality = _defaultQuality)
        {
            Contract.Requires<ArgumentOutOfRangeException>(maxWidth > 0);
            Contract.Requires<ArgumentOutOfRangeException>(maxWidth <= 65535);
            Contract.Requires<ArgumentOutOfRangeException>(quality >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(quality <= 100);

            using (var outputStream = new MemoryStream())
            using (var sourceStream = new MemoryStream(GetData()))
            using (Image image = GetResizedImage(maxWidth, sourceStream))
            {
                if (MimeType == "image/jpeg" || convertToLossy)
                {
                    using (var parameters = new EncoderParameters(1))
                    {
                        parameters.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);
                        image.Save(outputStream, ImageCodecInfo.GetImageEncoders().Where(codecInfo => string.Compare(codecInfo.MimeType, "image/jpeg", StringComparison.OrdinalIgnoreCase) == 0).First(), parameters);
                    }
                }
                else
                    image.Save(outputStream, ImageFormat.Png);

                return new CoverArt(outputStream.ToArray());
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Result should be disposed by the calling method")]
        Image GetResizedImage(int maxWidth, MemoryStream sourceStream)
        {
            Contract.Requires(maxWidth > 0);

            // If maxWidth isn't exceeded, just return the original image:
            if (Width <= maxWidth)
                return Image.FromStream(sourceStream);

            // Preserve the aspect ratio:
            var result = new Bitmap(maxWidth, (int)(Height / (float)Width * maxWidth));

            result.SetResolution(Width, Height);

            using (var sourceImage = Image.FromStream(sourceStream))
            using (var graphics = Graphics.FromImage(result))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(sourceImage, 0, 0, result.Width, result.Height);
            }

            return result;
        }
    }
}
