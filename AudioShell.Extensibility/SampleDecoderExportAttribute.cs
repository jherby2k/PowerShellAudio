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
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

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
    public sealed class SampleDecoderExportAttribute : ExportAttribute
    {
        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <value>
        /// The file extension.
        /// </value>
        public string Extension { get; private set; }

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
        public SampleDecoderExportAttribute(string extension)
            : base(typeof(ISampleDecoder))
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(extension));
            Contract.Requires<ArgumentException>(extension.StartsWith(".", StringComparison.OrdinalIgnoreCase));
            Contract.Requires<ArgumentException>(!extension.Any(character => char.IsWhiteSpace(character)));
            Contract.Requires<ArgumentException>(!extension.Any(character => Path.GetInvalidFileNameChars().Contains(character)));
            Contract.Requires<ArgumentException>(!extension.Any(character => char.IsUpper(character)));
            Contract.Ensures(Extension == extension);

            Extension = extension;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(!string.IsNullOrEmpty(Extension));
            Contract.Invariant(Extension.StartsWith(".", StringComparison.OrdinalIgnoreCase));
            Contract.Invariant(!Extension.Any(character => char.IsWhiteSpace(character)));
            Contract.Invariant(!Extension.Any(character => Path.GetInvalidFileNameChars().Contains(character)));
            Contract.Invariant(!Extension.Any(character => char.IsUpper(character)));
        }
    }
}
