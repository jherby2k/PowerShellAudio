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
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace PowerShellAudio
{
    /// <summary>
    /// Contains information about an <see cref="IMetadataEncoder"/> implementation.
    /// </summary>
    [Serializable, ContractClass(typeof(MetadataEncoderInfoContract))]
    public abstract class MetadataEncoderInfo
    {
        /// <summary>
        /// Gets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public abstract string Format { get; }

        /// <summary>
        /// Gets the file extension supported by the encoder.
        /// </summary>
        /// <value>
        /// The file extension.
        /// </value>
        public abstract string FileExtension { get; }

        /// <summary>
        /// Gets the default settings.
        /// </summary>
        /// <value>
        /// The default settings.
        /// </value>
        public virtual SettingsDictionary DefaultSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<SettingsDictionary>() != null);

                return new SettingsDictionary();
            }
        }

        /// <summary>
        /// Gets the available settings.
        /// </summary>
        /// <value>
        /// The available settings.
        /// </value>
        public virtual IReadOnlyCollection<string> AvailableSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyCollection<string>>() != null);

                return new List<string>(0).AsReadOnly();
            }
        }
    }
}
