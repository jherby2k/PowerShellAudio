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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace PowerShellAudio.Extensions.Flac
{
    class MetadataToVorbisCommentAdapter : SortedDictionary<string, string>
    {
        static readonly Dictionary<string, string> _map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "Album", "ALBUM"                     },
            { "AlbumGain", "REPLAYGAIN_ALBUM_GAIN" },
            { "AlbumPeak", "REPLAYGAIN_ALBUM_PEAK" },
            { "Artist", "ARTIST"                   },
            { "Comment", "DESCRIPTION"             },
            { "Genre", "GENRE"                     },
            { "Title", "TITLE"                     },
            { "TrackCount", "TOTALTRACKS"          },
            { "TrackGain", "REPLAYGAIN_TRACK_GAIN" },
            { "TrackNumber", "TRACKNUMBER"         },
            { "TrackPeak", "REPLAYGAIN_TRACK_PEAK" }
        };

        internal MetadataToVorbisCommentAdapter(MetadataDictionary metadata)
        {
            Contract.Requires(metadata != null);

            int day = 0;
            int month = 0;
            int year = 0;

            foreach (var item in metadata)
            {
                if (item.Key == "Day")
                    day = int.Parse(item.Value, CultureInfo.InvariantCulture);
                else if (item.Key == "Month")
                    month = int.Parse(item.Value, CultureInfo.InvariantCulture);
                else if (item.Key == "Year")
                    year = int.Parse(item.Value, CultureInfo.InvariantCulture);
                else
                {
                    string mappedKey;
                    if (_map.TryGetValue(item.Key, out mappedKey))
                        this[mappedKey] = item.Value;
                }
            }

            // The DATE field should contain either a full date, or just the year:
            if (day > 0 && month > 0 && year > 0)
            {
                Contract.Assume(month <= 12);
                this["DATE"] = new DateTime(year, month, day).ToShortDateString();
            }
            else if (year > 0)
                this["DATE"] = year.ToString(CultureInfo.InvariantCulture);
        }
    }
}
