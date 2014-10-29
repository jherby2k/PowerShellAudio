/*
 * Copyright © 2014 Jeremy Herbison
 * 
 * This file is part of AudioShell.
 * 
 * AudioShell is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option)
 * any later version.
 * 
 * AudioShell is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for more
 * details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with AudioShell.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace AudioShell
{
    /// <summary>
    /// Represents an extension capable of adjusting samples.
    /// </summary>
    /// <remarks>
    /// To add support for a new filter, an extension should implement this class, then decorate their implementation
    /// with the <see cref="SampleFilterExportAttribute"/> attribute so that it can be discovered at runtime.
    /// </remarks>
    [ContractClass(typeof(SampleFilterContract))]
    public interface ISampleFilter : ISampleConsumer
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
        /// Initializes the sample filter using the specified metadata and settings.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="settings">The settings.</param>
        void Initialize(MetadataDictionary metadata, SettingsDictionary settings);
    }
}
