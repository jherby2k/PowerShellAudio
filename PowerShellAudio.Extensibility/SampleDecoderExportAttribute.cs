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
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using PowerShellAudio.Properties;

namespace PowerShellAudio
{
    /// <summary>
    /// Specifies that an <see cref="ISampleDecoder"/> implemented in an extension assembly should be automatically
    /// imported and used by PowerShellAudio at runtime.
    /// </summary>
    /// <remarks>
    /// The attributed <see cref="ISampleDecoder"/> will be used to decode metadata from files with the provided file
    /// extension.
    /// </remarks>
    [MetadataAttribute, AttributeUsage(AttributeTargets.Class)]
    [PublicAPI, MeansImplicitUse, BaseTypeRequired(typeof(ISampleDecoder))]
    public sealed class SampleDecoderExportAttribute : ExportAttribute
    {
        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <value>
        /// The file extension.
        /// </value>
        [NotNull]
        public string Extension { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleDecoderExportAttribute"/> class.
        /// </summary>
        /// <param name="extension">
        /// The file extension supported by the attributed <see cref="ISampleDecoder"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="extension"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="extension"/> is not a valid file extension.
        /// </exception>
        public SampleDecoderExportAttribute([NotNull] string extension)
            : base(typeof(ISampleDecoder))
        {
            if (extension == null) throw new ArgumentNullException(nameof(extension));
            if (!extension.StartsWith(".", StringComparison.OrdinalIgnoreCase)
                || extension.Any(char.IsWhiteSpace)
                || extension.Any(character => Path.GetInvalidFileNameChars().Contains(character)))
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture,
                        Resources.AudioInfoDecoderExportAttributeExtensionIsInvalidError, extension),
                    nameof(extension));

            Extension = extension;
        }
    }
}
