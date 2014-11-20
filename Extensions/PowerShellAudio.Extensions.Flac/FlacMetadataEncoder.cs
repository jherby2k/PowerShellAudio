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

using PowerShellAudio.Extensions.Flac.Properties;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace PowerShellAudio.Extensions.Flac
{
    [MetadataEncoderExport(".flac")]
    public class FlacMetadataEncoder : IMetadataEncoder
    {
        static readonly MetadataEncoderInfo _encoderInfo = new FlacMetadataEncoderInfo();

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
            bool usePadding;
            if (string.IsNullOrEmpty(settings["UsePadding"]))
                usePadding = false;
            else if (!bool.TryParse(settings["UsePadding"], out usePadding))
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.MetadataEncoderBadUsePadding, settings["UsePadding"]));

            NativeVorbisCommentBlock vorbisCommentBlock = null;

            try
            {
                using (var chain = new NativeMetadataChain(stream))
                {
                    if (!chain.Read())
                    {
                        MetadataChainStatus chainStatus = chain.GetStatus();
                        if (chainStatus == MetadataChainStatus.NotAFlacFile)
                            throw new UnsupportedAudioException(Resources.MetadataEncoderNotFlacError);
                        else
                            throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.MetadataEncoderReadError, chainStatus));
                    }

                    // Create a Vorbis Comment block, even if empty:
                    vorbisCommentBlock = new NativeVorbisCommentBlock();
                    foreach (var item in new MetadataToVorbisCommentAdapter(metadata))
                        vorbisCommentBlock.Append(item.Key, item.Value);

                    // Iterate over the existing blocks, replacing and deleting as needed:
                    using (var iterator = new NativeMetadataIterator(chain.Handle))
                        UpdateMetadata(iterator, vorbisCommentBlock);

                    if (chain.CheckIfTempFileNeeded(usePadding))
                    {
                        // If FLAC requests a temporary file, use a MemoryStream instead. Then overwrite the original:
                        using (var tempStream = new MemoryStream())
                        {
                            if (!chain.WriteWithTempFile(usePadding, tempStream))
                                throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.MetadataEncoderTempFileError, chain.GetStatus()));

                            // Clear the original stream, and copy the temporary one over it:
                            stream.SetLength(tempStream.Length);
                            stream.Position = 0;
                            tempStream.WriteTo(stream);
                        }
                    }
                    else
                        if (!chain.Write(usePadding))
                            throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.MetadataEncoderWriteError, chain.GetStatus()));
                }
            }
            finally
            {
                if (vorbisCommentBlock != null)
                    vorbisCommentBlock.Dispose();
            }
        }

        static void UpdateMetadata(NativeMetadataIterator iterator, NativeVorbisCommentBlock newComments)
        {
            Contract.Requires(iterator != null);
            Contract.Requires(newComments != null);

            bool metadataInserted = false;

            do
            {
                switch ((MetadataType)Marshal.ReadInt32(iterator.GetBlock()))
                {
                    // Replace the existing Vorbis comment:
                    case MetadataType.VorbisComment:
                        if (!iterator.DeleteBlock(false))
                            throw new IOException(Resources.MetadataEncoderDeleteError);
                        if (!iterator.InsertBlockAfter(newComments.Handle))
                            throw new IOException(Resources.MetadataEncoderInsertBlockError);
                        metadataInserted = true;
                        break;

                    // Delete any padding:
                    case MetadataType.Padding:
                        if (!iterator.DeleteBlock(false))
                            throw new IOException(Resources.MetadataEncoderDeleteError);
                        break;
                    default:
                        break;
                }
            } while (iterator.Next());

            // If there was no existing metadata block to replace, insert it now:
            if (!metadataInserted && !iterator.InsertBlockAfter(newComments.Handle))
                throw new IOException(Resources.MetadataEncoderInsertBlockError);

            newComments.ReleaseHandleOwnership();
        }
    }
}
