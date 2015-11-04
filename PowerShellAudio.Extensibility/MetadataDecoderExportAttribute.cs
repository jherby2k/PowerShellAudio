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
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace PowerShellAudio
{
    /// <summary>
    /// Specifies that an <see cref="IMetadataDecoder"/> implemented in an extension assembly should be automatically
    /// imported and used by PowerShellAudio at runtime.
    /// </summary>
    /// <remarks>
    /// The attributed <see cref="IMetadataDecoder"/> will be used to create <see cref="MetadataDictionary"/> objects
    /// which describe a specified stream.
    /// </remarks>
    [MetadataAttribute, AttributeUsage(AttributeTargets.Class)]
    public sealed class MetadataDecoderExportAttribute : ExportAttribute
    {
        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <value>
        /// The file extension.
        /// </value>
        public string Extension { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataDecoderExportAttribute"/> class.
        /// </summary>
        /// <param name="extension">
        /// The file extension supported by the attributed <see cref="IMetadataDecoder"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="extension"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="extension"/> is not a valid file extension.
        /// </exception>
        public MetadataDecoderExportAttribute(string extension)
            : base(typeof(IMetadataDecoder))
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(extension));
            Contract.Requires<ArgumentException>(extension.StartsWith(".", StringComparison.OrdinalIgnoreCase));
            Contract.Requires<ArgumentException>(!extension.Any(char.IsWhiteSpace));
            Contract.Requires<ArgumentException>(!extension.Any(character => Path.GetInvalidFileNameChars().Contains(character)));
            Contract.Requires<ArgumentException>(!extension.Any(char.IsUpper));
            Contract.Ensures(Extension == extension);

            Extension = extension;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(!string.IsNullOrEmpty(Extension));
            Contract.Invariant(Extension.StartsWith(".", StringComparison.OrdinalIgnoreCase));
            Contract.Invariant(!Extension.Any(char.IsWhiteSpace));
            Contract.Invariant(!Extension.Any(character => Path.GetInvalidFileNameChars().Contains(character)));
            Contract.Invariant(!Extension.Any(char.IsUpper));
        }
    }
}
