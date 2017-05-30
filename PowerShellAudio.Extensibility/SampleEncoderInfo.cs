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
using System.Collections.Generic;
using JetBrains.Annotations;

namespace PowerShellAudio
{
    /// <summary>
    /// Contains information about an <see cref="ISampleEncoder"/> implementation.
    /// </summary>
    [Serializable]
    public abstract class SampleEncoderInfo
    {
        /// <summary>
        /// Gets the name of the encoder.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [NotNull]
        public abstract string Name { get; }

        /// <summary>
        /// Gets the file extension output by the encoder.
        /// </summary>
        /// <value>
        /// The file extension.
        /// </value>
        [NotNull]
        public abstract string FileExtension { get; }

        /// <summary>
        /// Gets a value indicating whether this encoder generates lossless output.
        /// </summary>
        /// <value>
        /// <c>true</c> if this encoder is lossless; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsLossless { get; }

        /// <summary>
        /// Gets the external library's version string, if relevant.
        /// </summary>
        /// <value>
        /// The external library.
        /// </value>
        [NotNull]
        public virtual string ExternalLibrary => string.Empty;

        /// <summary>
        /// Gets the default settings.
        /// </summary>
        /// <value>
        /// The default settings.
        /// </value>
        [NotNull]
        public virtual SettingsDictionary DefaultSettings => new SettingsDictionary();

        /// <summary>
        /// Gets the available settings.
        /// </summary>
        /// <value>
        /// The available settings.
        /// </value>
        [NotNull, ItemNotNull]
        public virtual IReadOnlyCollection<string> AvailableSettings => new List<string>(0);
    }
}
