/*
 * Copyright © 2014 Jeremy Herbison
 * 
 * This file is part of AudioShell.
 * 
 * AudioShell is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option)
 * any later version.
 * 
 * AudioShell is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for more
 * details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with AudioShell.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

using AudioShell.Extensions.Id3.Properties;
using Id3Lib;
using Id3Lib.Exceptions;
using Id3Lib.Frames;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AudioShell.Extensions.Id3
{
    [MetadataEncoderExport(".mp3")]
    public class Id3MetadataEncoder : IMetadataEncoder
    {
        public SettingsDictionary DefaultSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<SettingsDictionary>() != null);

                var result = new SettingsDictionary();

                result.Add("AddSoundCheck", bool.FalseString);
                result.Add("ID3Version", "2.3");
                result.Add("PaddingSize", "0");
                result.Add("UsePadding", bool.FalseString);

                return result;
            }
        }

        public IReadOnlyCollection<string> AvailableSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyCollection<string>>() != null);

                var result = new List<string>(4);

                result.Add("AddSoundCheck");
                result.Add("ID3Version");
                result.Add("PaddingSize");
                result.Add("UsePadding");

                return result.AsReadOnly();
            }
        }

        public void WriteMetadata(Stream stream, MetadataDictionary metadata, SettingsDictionary settings)
        {
            TagModel currentTag = GetCurrentTag(stream);
            uint currentTagSizeWithPadding = currentTag != null ? currentTag.Header.TagSizeWithHeaderFooter: 0;

            TagModel newTag = GetNewTag(currentTag, metadata, settings);
            if (newTag != null)
            {
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
                        throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.MetadataEncoderBadUsePadding, settings["UsePadding"]));

                    // If UsePadding = True and the new tag is smaller than the current one, or the new tag is the same size, there is no need to rewrite the whole stream:
                    if (usePadding && newTagSizeWithPadding <= currentTagSizeWithPadding || newTagSizeWithPadding == currentTagSizeWithPadding)
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
        }

        static TagModel GetCurrentTag(Stream stream)
        {
            Contract.Requires(stream != null);
            Contract.Requires(stream.CanSeek);

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

        static TagModel GetNewTag(TagModel currentTag, MetadataDictionary metadata, SettingsDictionary settings)
        {
            Contract.Requires(metadata != null);
            Contract.Requires(settings != null);

            var result = new MetadataToTagModelAdapter(metadata, settings);

            // Preserve an existing iTunNORM frame if a new one isn't provided, and AddSoundCheck isn't explicitly False:
            FrameBase currentSoundCheckFrame = currentTag == null ? null : currentTag.SingleOrDefault(frame =>
            {
                var fullTextFrame = frame as FrameFullText;
                if (fullTextFrame != null && fullTextFrame.Description == "iTunNORM")
                    return true;
                return false;
            });
            if (currentSoundCheckFrame != null && !result.IncludesSoundCheck && string.Compare(settings["AddSoundCheck"], bool.FalseString, StringComparison.OrdinalIgnoreCase) != 0)
                result.Add(currentSoundCheckFrame);

            if (result.Count == 0)
                return null;

            // If there is no current tag, and no requested ID3 version, default to 2.3:
            if (string.IsNullOrEmpty(settings["ID3Version"]))
                result.Header.Version = currentTag != null ? currentTag.Header.Version : (byte)3;
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
                        throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.MetadataEncoderBadID3Version, settings["ID3Version"]));
                }
            }

            uint paddingSize;
            if (string.IsNullOrEmpty(settings["PaddingSize"]))
                paddingSize = 0;
            else if (!uint.TryParse(settings["PaddingSize"], out paddingSize))
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.MetadataEncoderBadPaddingSize, settings["Quality"]));
            result.Header.PaddingSize = paddingSize;

            result.UpdateSize();

            return result;
        }
    }
}
