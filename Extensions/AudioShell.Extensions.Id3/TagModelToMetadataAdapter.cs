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

using Id3Lib;
using Id3Lib.Frames;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace PowerShellAudio.Extensions.Id3
{
    class TagModelToMetadataAdapter : MetadataDictionary
    {
        static readonly Dictionary<string, string> _map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "TALB", "Album"  },
            { "TPE1", "Artist" },
            { "TCON", "Genre"  },
            { "TIT2", "Title"  },
            { "TYER", "Year"   }
        };

        internal TagModelToMetadataAdapter(TagModel tagModel)
        {
            Contract.Requires(tagModel != null);

            foreach (FrameBase frame in tagModel)
            {
                var frameText = frame as FrameText;
                if (frameText != null)
                {
                    switch (frameText.FrameId)
                    {
                        // The TRCK frame contains the track number and (optionally) the track count:
                        case "TRCK":
                            string[] segments = frameText.Text.Split('/');
                            base["TrackNumber"] = segments[0];
                            if (segments.Length > 1)
                                base["TrackCount"] = segments[1];
                            break;

                        default:
                            string mappedKey;
                            if (_map.TryGetValue(frameText.FrameId, out mappedKey))
                                base[mappedKey] = frameText.Text;
                            break;
                    }
                }
                else
                {
                    var frameFullText = frame as FrameFullText;
                    if (frameFullText != null && frameFullText.FrameId == "COMM" && frameFullText.Description == null)
                        base["Comment"] = frameFullText.Text;
                    else
                    {
                        var frameTextUserDef = frame as FrameTextUserDef;
                        if (frameTextUserDef != null && frameTextUserDef.FrameId == "TXXX")
                            switch (frameTextUserDef.Description)
                            {
                                case "REPLAYGAIN_TRACK_GAIN":
                                    base["TrackGain"] = frameTextUserDef.Text;
                                    break;
                                case "REPLAYGAIN_TRACK_PEAK":
                                    base["TrackPeak"] = frameTextUserDef.Text;
                                    break;
                                case "REPLAYGAIN_ALBUM_GAIN":
                                    base["AlbumGain"] = frameTextUserDef.Text;
                                    break;
                                case "REPLAYGAIN_ALBUM_PEAK":
                                    base["AlbumPeak"] = frameTextUserDef.Text;
                                    break;
                                default:
                                    break;
                            }
                    }
                }
            }
        }
    }
}
