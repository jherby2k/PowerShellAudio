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

using PowerShellAudio.Extensions.Id3.Properties;
using Id3Lib;
using Id3Lib.Frames;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Id3
{
    class MetadataToTagModelAdapter : TagModel
    {
        static readonly Dictionary<string, string> _map = new Dictionary<string, string>
        {
            { "Album", "TALB" },
            { "Artist", "TPE1" },
            { "Genre", "TCON" },
            { "Title", "TIT2" },
            { "Year", "TYER" }
        };

        internal MetadataToTagModelAdapter([NotNull] MetadataDictionary metadata, [NotNull] SettingsDictionary settings)
        {
            var trckFrame = new TrckFrame();
            var tdatFrame = new TdatFrame();
            var trackSoundCheckFrame = new SoundCheckFrame();
            var albumSoundCheckFrame = new SoundCheckFrame();

            foreach (var item in metadata)
            {
                switch (item.Key)
                {
                    case "TrackNumber":
                        trckFrame.TrackNumber = item.Value;
                        break;

                    case "TrackCount":
                        trckFrame.TrackCount = item.Value;
                        break;

                    case "Day":
                        tdatFrame.Day = item.Value;
                        break;

                    case "Month":
                        tdatFrame.Month = item.Value;
                        break;

                    // The standard comment field has a blank description:
                    case "Comment":
                        var commentFrame = (FrameFullText)FrameFactory.Build("COMM");
                        commentFrame.Text = item.Value;
                        Add(commentFrame);
                        break;

                    case "TrackGain":
                        trackSoundCheckFrame.Gain = item.Value;
                        var trackGainFrame = (FrameTextUserDef)FrameFactory.Build("TXXX");
                        trackGainFrame.Description = "REPLAYGAIN_TRACK_GAIN";
                        trackGainFrame.Text = item.Value;
                        trackGainFrame.FileAlter = true;
                        Add(trackGainFrame);
                        break;

                    case "TrackPeak":
                        trackSoundCheckFrame.Peak = item.Value;
                        var trackPeakFrame = (FrameTextUserDef)FrameFactory.Build("TXXX");
                        trackPeakFrame.Description = "REPLAYGAIN_TRACK_PEAK";
                        trackPeakFrame.Text = item.Value;
                        trackPeakFrame.FileAlter = true;
                        Add(trackPeakFrame);
                        break;

                    case "AlbumGain":
                        albumSoundCheckFrame.Gain = item.Value;
                        var albumGainFrame = (FrameTextUserDef)FrameFactory.Build("TXXX");
                        albumGainFrame.Description = "REPLAYGAIN_ALBUM_GAIN";
                        albumGainFrame.Text = item.Value;
                        albumGainFrame.FileAlter = true;
                        Add(albumGainFrame);
                        break;

                    case "AlbumPeak":
                        albumSoundCheckFrame.Peak = item.Value;
                        var albumPeakFrame = (FrameTextUserDef)FrameFactory.Build("TXXX");
                        albumPeakFrame.Description = "REPLAYGAIN_ALBUM_PEAK";
                        albumPeakFrame.Text = item.Value;
                        albumPeakFrame.FileAlter = true;
                        Add(albumPeakFrame);
                        break;

                    // Every other field should be treated as text:
                    default:
                        string mappedKey;
                        if (_map.TryGetValue(item.Key, out mappedKey))
                        {
                            var textFrame = (FrameText)FrameFactory.Build(mappedKey);
                            textFrame.Text = item.Value;
                            Add(textFrame);
                        }
                        break;
                }
            }

            if (!string.IsNullOrEmpty(trckFrame.Text))
                Add(trckFrame);

            if (!string.IsNullOrEmpty(tdatFrame.Text))
                Add(tdatFrame);

            if (!string.IsNullOrEmpty(settings["AddSoundCheck"]) &&
                string.Compare(settings["AddSoundCheck"], bool.FalseString, StringComparison.OrdinalIgnoreCase) != 0)
            {
                if (string.Compare(settings["AddSoundCheck"], "Album", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (string.IsNullOrEmpty(albumSoundCheckFrame.Gain))
                        throw new InvalidSettingException(Resources.MetadataToTagModelAdapterMissingAlbumGain);
                    if (string.IsNullOrEmpty(albumSoundCheckFrame.Peak))
                        throw new InvalidSettingException(Resources.MetadataToTagModelAdapterMissingAlbumPeak);

                    Add(albumSoundCheckFrame);
                }
                else if (string.Compare(settings["AddSoundCheck"], "Track", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (string.IsNullOrEmpty(trackSoundCheckFrame.Gain))
                        throw new InvalidSettingException(Resources.MetadataToTagModelAdapterMissingTrackGain);
                    if (string.IsNullOrEmpty(trackSoundCheckFrame.Peak))
                        throw new InvalidSettingException(Resources.MetadataToTagModelAdapterMissingTrackPeak);

                    Add(trackSoundCheckFrame);
                }
                else
                    throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture,
                        Resources.MetadataToTagModelAdapterBadAddSoundCheck, settings["AddSoundCheck"]));
            }

            if (metadata.CoverArt != null)
                Add(new CoverArtToFrameAdapter(metadata.CoverArt));
        }

        internal bool IncludesSoundCheck
        {
            get
            {
                return this.Any(frame =>
                {
                    var fullTextFrame = frame as FrameFullText;
                    return fullTextFrame != null && fullTextFrame.Description == "iTunNORM";
                });
            }
        }
    }
}
