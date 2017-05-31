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

using PowerShellAudio.Extensions.Vorbis.Properties;
using System;
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Vorbis
{
    [MetadataDecoderExport(".ogg")]
    public class VorbisMetadataDecoder : IMetadataDecoder
    {
        public MetadataDictionary ReadMetadata([NotNull] Stream stream)
        {
            var buffer = new byte[4096];

            using (var decoder = new NativeVorbisDecoder())
            {
                NativeOggStream oggStream = null;
                var vorbisComment = new VorbisComment();

                try
                {
                    SafeNativeMethods.VorbisCommentInitialize(out vorbisComment);

                    using (var sync = new NativeOggSync())
                    {
                        OggPage page;

                        do
                        {
                            // Read from the buffer into a page:
                            while (sync.PageOut(out page) != 1)
                            {
                                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                                if (bytesRead == 0)
                                    throw new IOException(Resources.ReadError);

                                IntPtr nativeBuffer = sync.Buffer(bytesRead);
                                Marshal.Copy(buffer, 0, nativeBuffer, bytesRead);

                                sync.Wrote(bytesRead);
                            }

                            if (oggStream == null)
                                oggStream = new NativeOggStream(SafeNativeMethods.OggPageGetSerialNumber(ref page));

                            oggStream.PageIn(ref page);

                            OggPacket packet;
                            while (oggStream.PacketOut(out packet) == 1)
                            {
                                decoder.HeaderIn(ref vorbisComment, ref packet);

                                if (packet.PacketNumber == 1)
                                    return new VorbisCommentToMetadataAdapter(vorbisComment);
                            }
                        } while (SafeNativeMethods.OggPageEndOfStream(ref page) == 0);

                        throw new IOException(Resources.EndOfStreamError);
                    }
                }
                finally
                {
                    oggStream?.Dispose();

                    SafeNativeMethods.VorbisCommentClear(ref vorbisComment);
                }
            }
        }
    }
}
