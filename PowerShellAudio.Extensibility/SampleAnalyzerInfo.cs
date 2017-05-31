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
using JetBrains.Annotations;

namespace PowerShellAudio
{
    /// <summary>
    /// Contains information about an <see cref="ISampleAnalyzer"/> implementation.
    /// </summary>
    [Serializable]
    [PublicAPI]
    public abstract class SampleAnalyzerInfo
    {
        /// <summary>
        /// Gets the name of the analyzer.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [NotNull]
        public abstract string Name { get; }

        /// <summary>
        /// Gets the external library's version string, if relevant.
        /// </summary>
        /// <value>
        /// The external library.
        /// </value>
        [NotNull]
        public virtual string ExternalLibrary => string.Empty;
    }
}
