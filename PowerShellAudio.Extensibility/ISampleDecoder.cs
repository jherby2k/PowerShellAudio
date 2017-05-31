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

using System.IO;

namespace PowerShellAudio
{
    /// <summary>
    /// Represents an extension capable of decoding samples from a particular format.
    /// </summary>
    /// <remarks>
    /// To add support for decoding a new format, an extension should implement this class, then decorate their
    /// implementation with the <see cref="SampleDecoderExportAttribute"/> attribute so that it can be discovered at
    /// runtime.
    /// </remarks>
    public interface ISampleDecoder
    {
        /// <summary>
        /// Initializes the decoder using the specified stream.
        /// </summary>
        /// <param name="stream">The stream for reading.</param>
        void Initialize(Stream stream);

        /// <summary>
        /// Reads the next <see cref="SampleCollection"/> in sequence.
        /// </summary>
        /// <returns>The next <see cref="SampleCollection"/> object constructed from the stream.</returns>
        SampleCollection DecodeSamples();
    }
}
