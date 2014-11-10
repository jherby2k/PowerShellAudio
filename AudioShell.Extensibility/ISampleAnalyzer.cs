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

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace PowerShellAudio
{
    /// <summary>
    /// Represents an extension capable of analyzing samples and then generating metadata from the result.
    /// </summary>
    /// <remarks>
    /// To add support for a new analyzer, an extension should implement this class, then decorate their implementation
    /// with the <see cref="SampleAnalyzerExportAttribute"/> attribute so that it can be discovered at runtime.
    /// </remarks>
    [ContractClass(typeof(SampleAnalyzerContract))]
    public interface ISampleAnalyzer : IFinalSampleConsumer
    {
        /// <summary>
        /// Initializes the analyzer.
        /// </summary>
        /// <param name="audioInfo">The  audio info.</param>
        /// <param name="groupToken">The group token.</param>
        void Initialize(AudioInfo audioInfo, GroupToken groupToken);

        /// <summary>
        /// Gets the result. This method will block until the last <see cref="SampleCollection"/> has been submitted
        /// and processed.
        /// </summary>
        /// <returns>A <see cref="MetadataDictionary"/> containing the analysis results.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This can block for a long time, so a property isn't appropriate.")]
        MetadataDictionary GetResult();
    }
}
