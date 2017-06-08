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

using System.Diagnostics.CodeAnalysis;
using PowerShellAudio.Extensions.Flac.Properties;
using System.Globalization;
using System.IO;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Flac
{
    [AudioInfoDecoderExport(".flac")]
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Loaded via reflection")]
    class FlacAudioInfoDecoder : IAudioInfoDecoder
    {
        [NotNull]
        public AudioInfo ReadAudioInfo([NotNull] Stream stream)
        {
            using (var decoder = new NativeStreamAudioInfoDecoder(stream))
            {
                DecoderInitStatus initStatus = decoder.Initialize();
                if (initStatus != DecoderInitStatus.Ok)
                    if (initStatus == DecoderInitStatus.UnsupportedContainer)
                        throw new UnsupportedAudioException(Resources.AudioInfoDecoderUnsupportedContainerError);
                    else
                        throw new IOException(string.Format(CultureInfo.CurrentCulture,
                            Resources.AudioInfoDecoderInitializationError, initStatus));

                if (!decoder.ProcessMetadata())
                    throw new IOException(string.Format(CultureInfo.CurrentCulture,
                        Resources.AudioInfoDecoderDecodingError, decoder.GetState()));

                decoder.Finish();

                return decoder.AudioInfo;
            }
        }
    }
}
