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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace AudioShell.Extensions.Flac
{
    class VorbisCommentToMetadataAdapter : MetadataDictionary
    {
        static readonly Dictionary<string, string> _map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "ALBUM", "Album"                     },
            { "REPLAYGAIN_ALBUM_PEAK", "AlbumPeak" },
            { "REPLAYGAIN_ALBUM_GAIN", "AlbumGain" },
            { "ARTIST", "Artist"                   },
            { "DESCRIPTION", "Comment"             },
            { "COMMENT", "Comment"                 },
            { "GENRE", "Genre"                     },
            { "TITLE", "Title"                     },
            { "TOTALTRACKS", "TrackCount"          },
            { "TRACKCOUNT", "TrackCount"           },
            { "REPLAYGAIN_TRACK_GAIN", "TrackGain" },
            { "REPLAYGAIN_TRACK_PEAK", "TrackPeak" },
            { "DATE", "Year"                       },
            { "YEAR", "Year"                       }
        };

        internal VorbisCommentToMetadataAdapter(IEnumerable<KeyValuePair<string, string>> vorbisComments)
        {
            Contract.Requires<ArgumentNullException>(vorbisComments != null);

            foreach (KeyValuePair<string, string> item in vorbisComments)
            {
                // The track number and count may be packed into the same comment:
                if (item.Key == "TRACKNUMBER")
                {
                    string[] segments = item.Value.Split('/');
                    base["TrackNumber"] = segments[0];
                    if (segments.Length > 1)
                        base["TrackCount"] = segments[1];
                }
                else
                {
                    string mappedKey;
                    if (_map.TryGetValue(item.Key, out mappedKey))
                        base[mappedKey] = item.Value;
                }
            }
        }
    }
}
