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

using PowerShellAudio.Extensions.Vorbis.Properties;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PowerShellAudio.Extensions.Vorbis
{
    [MetadataEncoderExport(".ogg")]
    public class VorbisMetadataEncoder : IMetadataEncoder
    {
        static readonly MetadataEncoderInfo _encoderInfo = new VorbisMetadataEncoderInfo();

        public MetadataEncoderInfo EncoderInfo
        {
            get
            {
                Contract.Ensures(Contract.Result<MetadataEncoderInfo>() != null);

                return _encoderInfo;
            }
        }

        public void WriteMetadata(Stream stream, MetadataDictionary metadata, SettingsDictionary settings)
        {
            // This buffer is used for both reading and writing:
            var buffer = new byte[4096];

            using (var tempStream = new MemoryStream())
            {
                NativeOggStream inputOggStream = null;
                NativeOggStream outputOggStream = null;

                try
                {
                    using (var sync = new NativeOggSync())
                    {
                        OggPage inPage;

                        do
                        {
                            // Read from the buffer into a page:
                            while (sync.PageOut(out inPage) != 1)
                            {
                                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                                if (bytesRead == 0)
                                    throw new IOException(Resources.ReadError);

                                IntPtr nativeBuffer = sync.Buffer(bytesRead);
                                Marshal.Copy(buffer, 0, nativeBuffer, bytesRead);

                                sync.Wrote(bytesRead);
                            }

                            // Initialize the input and output streams, if necessary:
                            if (inputOggStream == null)
                                inputOggStream = new NativeOggStream(SafeNativeMethods.OggPageGetSerialNumber(ref inPage));
                            if (outputOggStream == null)
                                outputOggStream = new NativeOggStream(inputOggStream.SerialNumber);

                            inputOggStream.PageIn(ref inPage);

                            OggPacket packet;
                            while (inputOggStream.PacketOut(out packet) == 1)
                            {
                                // Substitute the new comment packet:
                                if (packet.PacketNumber == 1)
                                {
                                    OggPacket commentPacket = GetCommentPacket(metadata);
                                    outputOggStream.PacketIn(ref commentPacket);
                                }
                                else
                                    outputOggStream.PacketIn(ref packet);

                                // Page out each packet, flushing at the end of the header:
                                OggPage outPage;
                                if (packet.PacketNumber == 2)
                                    while (outputOggStream.Flush(out outPage))
                                        WritePage(outPage, tempStream, buffer);
                                else
                                    while (outputOggStream.PageOut(out outPage))
                                        WritePage(outPage, tempStream, buffer);
                            }
                        } while (SafeNativeMethods.OggPageEndOfStream(ref inPage) == 0);

                        // If the end of the stream is reached, overwrite the original file and return:
                        Overwrite(stream, tempStream);
                    }
                }
                finally
                {
                    inputOggStream?.Dispose();
                    outputOggStream?.Dispose();
                }
            }
        }

        static OggPacket GetCommentPacket(MetadataDictionary metadata)
        {
            Contract.Requires(metadata != null);

            var comment = new VorbisComment();
            try
            {
                SafeNativeMethods.VorbisCommentInitialize(out comment);

                foreach (var item in new MetadataToVorbisCommentAdapter(metadata))
                {
                    // The key and value need to be marshaled as null-terminated UTF-8 strings:
                    var keyBytes = new byte[Encoding.UTF8.GetByteCount(item.Key) + 1];
                    Encoding.UTF8.GetBytes(item.Key, 0, item.Key.Length, keyBytes, 0);

                    var valueBytes = new byte[Encoding.UTF8.GetByteCount(item.Value) + 1];
                    Encoding.UTF8.GetBytes(item.Value, 0, item.Value.Length, valueBytes, 0);

                    SafeNativeMethods.VorbisCommentAddTag(ref comment, keyBytes, valueBytes);
                }

                OggPacket result;
                if (SafeNativeMethods.VorbisCommentHeaderOut(ref comment, out result) != 0)
                    throw new IOException(Resources.MetadataEncoderHeaderOutError);

                return result;
            }
            finally
            {
                SafeNativeMethods.VorbisCommentClear(ref comment);
            }
        }

        static void WritePage(OggPage page, Stream stream, byte[] buffer)
        {
            Contract.Requires(stream != null);
            Contract.Requires(stream.CanWrite);
            Contract.Requires(buffer != null);

            WritePointer(page.Header, page.HeaderLength, stream, buffer);
            WritePointer(page.Body, page.BodyLength, stream, buffer);
        }

        static void WritePointer(IntPtr location, int length, Stream stream, byte[] buffer)
        {
            Contract.Requires(location != IntPtr.Zero);
            Contract.Requires(stream != null);
            Contract.Requires(stream.CanWrite);
            Contract.Requires(buffer != null);

            var offset = 0;
            while (offset < length)
            {
                int bytesCopied = Math.Min(length - offset, buffer.Length);
                Marshal.Copy(IntPtr.Add(location, offset), buffer, 0, bytesCopied);
                stream.Write(buffer, 0, bytesCopied);
                offset += bytesCopied;
            }
        }

        static void Overwrite(Stream originalStream, Stream newStream)
        {
            Contract.Requires(originalStream != null);
            Contract.Requires(originalStream.CanWrite);
            Contract.Requires(originalStream.CanSeek);
            Contract.Requires(newStream != null);
            Contract.Requires(newStream.CanRead);
            Contract.Requires(newStream.CanSeek);
            Contract.Ensures(originalStream.Length == newStream.Length);

            originalStream.SetLength(newStream.Length);
            originalStream.Position = 0;
            newStream.Position = 0;
            newStream.CopyTo(originalStream);
        }
    }
}
