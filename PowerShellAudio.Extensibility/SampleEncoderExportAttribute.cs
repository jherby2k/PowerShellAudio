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

namespace PowerShellAudio
{
    /// <summary>
    /// Specifies that an <see cref="ISampleEncoder"/> implemented in an extension assembly should be automatically
    /// imported and used by PowerShellAudio at runtime.
    /// </summary>
    /// <remarks>
    /// The attributed <see cref="ISampleEncoder"/> with the specified name will be used to analyze the audio stream.
    /// </remarks>
    [MetadataAttribute, AttributeUsage(AttributeTargets.Class)]
    public sealed class SampleEncoderExportAttribute : ExportAttribute
    {
        /// <summary>
        /// Gets the name of the audio format.
        /// </summary>
        /// <value>
        /// The name of the audio format.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleEncoderExportAttribute"/> class.
        /// </summary>
        /// <param name="name">
        /// The name of the audio format encoded to by the attributed <see cref="ISampleEncoder"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="name"/> is null or empty.
        /// </exception>
        public SampleEncoderExportAttribute(string name)
            : base(typeof(ISampleEncoder))
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name));
            Contract.Ensures(Name == name);

            Name = name;
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(!string.IsNullOrEmpty(Name));
        }
    }
}
