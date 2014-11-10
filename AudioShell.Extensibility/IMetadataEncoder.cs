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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace PowerShellAudio
{
    /// <summary>
    /// Represents an extension capable of encoding metadata to a particular format.
    /// </summary>
    /// <remarks>
    /// To add support for a new metadata format, an extension should implement this class, then decorate their
    /// implementation with the <see cref="MetadataEncoderExportAttribute"/> attribute so that it can be discovered at
    /// runtime.
    /// </remarks>
    [ContractClass(typeof(MetadataEncoderContract))]
    public interface IMetadataEncoder
    {
        /// <summary>
        /// Gets the default settings.
        /// </summary>
        /// <value>
        /// The default settings.
        /// </value>
        SettingsDictionary DefaultSettings { get; }

        /// <summary>
        /// Gets the available settings.
        /// </summary>
        /// <value>
        /// The available settings.
        /// </value>
        IReadOnlyCollection<string> AvailableSettings { get; }

        /// <summary>
        /// Writes the metadata.
        /// </summary>
        /// <param name="stream">The stream for writing.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="settings">The settings.</param>
        void WriteMetadata(Stream stream, MetadataDictionary metadata, SettingsDictionary settings);
    }
}
