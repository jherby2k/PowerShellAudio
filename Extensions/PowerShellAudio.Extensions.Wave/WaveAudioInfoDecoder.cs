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

using PowerShellAudio.Extensions.Wave.Properties;
using System.Diagnostics.Contracts;
using System.IO;

namespace PowerShellAudio.Extensions.Wave
{
    [AudioInfoDecoderExport(".wav")]
    public class WaveAudioInfoDecoder : IAudioInfoDecoder
    {
        public AudioInfo ReadAudioInfo(Stream stream)
        {
            Contract.Ensures(stream.CanRead);
            Contract.Ensures(stream.CanSeek);
            Contract.Ensures(Contract.Result<AudioInfo>() != null);

            if (stream.Length < 45) // 12 byte RIFF descriptor + 24 byte fmt chunk + 9 byte data chunk
                throw new IOException(Resources.AudioInfoDecoderLengthError);

            using (RiffReader reader = new RiffReader(stream))
            {
                if (!reader.Validate() || reader.ReadFourCC() != "WAVE")
                    throw new UnsupportedAudioException(Resources.AudioInfoDecoderNotWaveError);

                uint fmtChunkSize = reader.SeekToChunk("fmt ");
                if (fmtChunkSize == 0)
                    throw new IOException(Resources.AudioInfoDecoderMissingFmtError);
                if (fmtChunkSize < 16)
                    throw new IOException(Resources.AudioInfoDecoderFmtLengthError);

                Format format = (Format)reader.ReadUInt16();
                if (format != Format.Pcm && format != Format.Extensible)
                    throw new UnsupportedAudioException(Resources.AudioInfoDecoderUnsupportedError);

                // WAVEFORMATEXTENSIBLE headers are 40 bytes long:
                if (format == Format.Extensible && fmtChunkSize < 40)
                    throw new IOException(Resources.AudioInfoDecoderFmtLengthError);

                ushort channels = reader.ReadUInt16();
                uint sampleRate = reader.ReadUInt32();
                stream.Seek(4, SeekOrigin.Current); // Ignore bytesPerSecond
                ushort blockAlign = reader.ReadUInt16();
                uint bitsPerSample = reader.ReadUInt16();

                // Read the WAVEFORMATEXTENSIBLE extended header, if present:
                if (format == Format.Extensible)
                {
                    if (reader.ReadUInt16() < 22)
                        throw new UnsupportedAudioException(Resources.AudioInfoDecoderFmtExtensionLengthError);

                    if (bitsPerSample % 8 != 0)
                        throw new UnsupportedAudioException(Resources.AudioInfoDecoderBitsPerSampleError);
                    bitsPerSample = reader.ReadUInt16();

                    // Ignore the channel mask for now:
                    stream.Seek(4, SeekOrigin.Current);

                    if ((Format)reader.ReadUInt16() != Format.Pcm)
                        throw new UnsupportedAudioException(Resources.AudioInfoDecoderUnsupportedError);
                }

                uint dataChunkSize = reader.SeekToChunk("data");
                if (dataChunkSize == 0)
                    throw new IOException(Resources.AudioInfoDecoderMissingSamplesError);

                return new AudioInfo("LPCM", (int)channels, (int)bitsPerSample, (int)sampleRate, dataChunkSize / blockAlign);
            }
        }
    }
}
