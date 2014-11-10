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

using System.Diagnostics.Contracts;
using System.IO;

namespace PowerShellAudio
{
    /// <summary>
    /// Represents an extension capable of decoding audio from a particular format.
    /// </summary>
    /// <remarks>
    /// To add support for a new audio format, an extension should implement this class, and then decorate their
    /// implementation with the <see cref="AudioInfoDecoderExportAttribute"/> attribute so that it can be discovered at
    /// runtime.
    /// </remarks>
    [ContractClass(typeof(AudioInfoDecoderContract))]
    public interface IAudioInfoDecoder
    {
        /// <summary>
        /// Reads the audio information.
        /// </summary>
        /// <param name="stream">The stream for reading.</param>
        /// <returns>An <see cref="AudioInfo"/> object describing the audio contained in the stream.</returns>
        AudioInfo ReadAudioInfo(Stream stream);
    }
}
