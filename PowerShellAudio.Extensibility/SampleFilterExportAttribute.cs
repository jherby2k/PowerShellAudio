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
using JetBrains.Annotations;
using PowerShellAudio.Properties;

namespace PowerShellAudio
{
    /// <summary>
    /// Specifies that an <see cref="ISampleFilter"/> implemented in an extension assembly should be automatically
    /// imported and used by PowerShellAudio at runtime.
    /// </summary>
    /// <remarks>
    /// The attributed <see cref="ISampleFilter"/> with the specified name will be used to analyze the audio stream.
    /// </remarks>
    [MetadataAttribute, AttributeUsage(AttributeTargets.Class)]
    public sealed class SampleFilterExportAttribute : ExportAttribute
    {
        /// <summary>
        /// Gets the name of the algorithm.
        /// </summary>
        /// <value>
        /// The name of the algorithm.
        /// </value>
        [NotNull]
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleFilterExportAttribute"/> class.
        /// </summary>
        /// <param name="name">
        /// The type of filtering performed by the attributed <see cref="ISampleFilter"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="name"/> is null or empty.
        /// </exception>
        public SampleFilterExportAttribute([NotNull] string name)
            : base(typeof(ISampleFilter))
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(Resources.SampleAnalyzerExportAttributeNameIsEmptyError, nameof(name));

            Name = name;
        }
    }
}
