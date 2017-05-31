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

using Id3Lib;
using Id3Lib.Exceptions;
using Id3Lib.Frames;
using PowerShellAudio.Extensions.Id3.Properties;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Id3
{
    [MetadataEncoderExport(".mp3")]
    public class Id3MetadataEncoder : IMetadataEncoder
    {
        static readonly MetadataEncoderInfo _encoderInfo = new Id3MetadataEncoderInfo();

        [NotNull]
        public MetadataEncoderInfo EncoderInfo => _encoderInfo;

        public void WriteMetadata(
            [NotNull] Stream stream, 
            [NotNull] MetadataDictionary metadata,
            [NotNull] SettingsDictionary settings)
        {
            TagModel currentTag = GetCurrentTag(stream);
            uint currentTagSizeWithPadding = currentTag?.Header.TagSizeWithHeaderFooter ?? 0;

            TagModel newTag = GetNewTag(currentTag, metadata, settings);
            if (newTag == null)
                return;

            uint newTagSizeWithPadding = newTag.Header.TagSizeWithHeaderFooter + newTag.Header.PaddingSize;

            // If this is a new stream, just write the tag:
            if (stream.Length == 0)
                TagManager.Serialize(newTag, stream);
            else
            {
                bool usePadding;
                if (string.IsNullOrEmpty(settings["UsePadding"]))
                    usePadding = false;
                else if (!bool.TryParse(settings["UsePadding"], out usePadding))
                    throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture,
                        Resources.MetadataEncoderBadUsePadding, settings["UsePadding"]));

                // If UsePadding = True and the new tag is smaller than the current one, or the new tag is the same size, there is no need to rewrite the whole stream:
                if (usePadding && newTagSizeWithPadding <= currentTagSizeWithPadding ||
                    newTagSizeWithPadding == currentTagSizeWithPadding)
                {
                    newTag.Header.PaddingSize += currentTagSizeWithPadding - newTagSizeWithPadding;
                    TagManager.Serialize(newTag, stream);
                }
                else
                {
                    // If the new tag is larger, rewrite the whole stream:
                    using (var tempStream = new MemoryStream())
                    {
                        stream.Position = currentTagSizeWithPadding;
                        stream.CopyTo(tempStream);

                        stream.SetLength(newTagSizeWithPadding + tempStream.Length);

                        stream.Position = 0;
                        TagManager.Serialize(newTag, stream);
                        tempStream.Position = 0;
                        tempStream.CopyTo(stream);
                    }
                }
            }
        }

        [CanBeNull]
        static TagModel GetCurrentTag([NotNull] Stream stream)
        {
            try
            {
                if (stream.Length == 0)
                    return null;

                TagModel result = TagManager.Deserialize(stream);
                stream.Position = 0;
                return result;
            }
            catch (TagNotFoundException)
            {
                return null;
            }
        }

        static TagModel GetNewTag(
            [CanBeNull] TagModel currentTag,
            [NotNull] MetadataDictionary metadata,
            [NotNull] SettingsDictionary settings)
        {
            var result = new MetadataToTagModelAdapter(metadata, settings);

            // Preserve an existing iTunNORM frame if a new one isn't provided, and AddSoundCheck isn't explicitly False:
            FrameBase currentSoundCheckFrame = currentTag?.SingleOrDefault(frame =>
            {
                var fullTextFrame = frame as FrameFullText;
                return fullTextFrame != null && fullTextFrame.Description == "iTunNORM";
            });
            if (currentSoundCheckFrame != null && !result.IncludesSoundCheck &&
                string.Compare(settings["AddSoundCheck"], bool.FalseString, StringComparison.OrdinalIgnoreCase) != 0)
                result.Add(currentSoundCheckFrame);

            if (result.Count == 0)
                return null;

            // If there is no current tag, and no requested ID3 version, default to 2.3:
            if (string.IsNullOrEmpty(settings["ID3Version"]))
                result.Header.Version = currentTag?.Header.Version ?? 3;
            else
            {
                switch (settings["ID3Version"])
                {
                    case "2.2":
                        result.Header.Version = 2;
                        break;
                    case "2.3":
                        result.Header.Version = 3;
                        break;
                    case "2.4":
                        result.Header.Version = 4;
                        break;
                    default:
                        throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture,
                            Resources.MetadataEncoderBadID3Version, settings["ID3Version"]));
                }
            }

            uint paddingSize;
            if (string.IsNullOrEmpty(settings["PaddingSize"]))
                paddingSize = 0;
            else if (!uint.TryParse(settings["PaddingSize"], out paddingSize))
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture,
                    Resources.MetadataEncoderBadPaddingSize, settings["Quality"]));
            result.Header.PaddingSize = paddingSize;

            result.UpdateSize();

            return result;
        }
    }
}
