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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace PowerShellAudio.Extensions.Vorbis
{
    class VorbisCommentToMetadataAdapter : MetadataDictionary
    {
        static readonly Dictionary<string, string> _map =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "ALBUM", "Album" },
                { "REPLAYGAIN_ALBUM_PEAK", "AlbumPeak" },
                { "REPLAYGAIN_ALBUM_GAIN", "AlbumGain" },
                { "ARTIST", "Artist" },
                { "DESCRIPTION", "Comment" },
                { "COMMENT", "Comment" },
                { "GENRE", "Genre" },
                { "TITLE", "Title" },
                { "TOTALTRACKS", "TrackCount" },
                { "TRACKCOUNT", "TrackCount" },
                { "REPLAYGAIN_TRACK_GAIN", "TrackGain" },
                { "REPLAYGAIN_TRACK_PEAK", "TrackPeak" }
            };

        internal VorbisCommentToMetadataAdapter(VorbisComment vorbisComment)
        {
            var commentLengths = new int[vorbisComment.Comments];
            Marshal.Copy(vorbisComment.CommentLengths, commentLengths, 0, commentLengths.Length);

            var commentPtrs = new IntPtr[vorbisComment.Comments];
            Marshal.Copy(vorbisComment.UserComments, commentPtrs, 0, commentPtrs.Length);

            for (var i = 0; i < vorbisComment.Comments; i++)
            {
                var commentBytes = new byte[commentLengths[i]];
                Marshal.Copy(commentPtrs[i], commentBytes, 0, commentLengths[i]);

                string[] comment = Encoding.UTF8.GetString(commentBytes).Split(new[] { '=' }, 2);

                // The track number and count may be packed into the same comment:
                switch (comment[0])
                {
                    case "TRACKNUMBER":
                        string[] segments = comment[1].Split('/');
                        base["TrackNumber"] = segments[0];
                        if (segments.Length > 1)
                            base["TrackCount"] = segments[1];
                        break;

                    case "DATE":
                    case "YEAR":
                        // The DATE comment may contain a full date, or only the year:
                        DateTime result;
                        if (DateTime.TryParse(comment[1], CultureInfo.CurrentCulture,
                            DateTimeStyles.NoCurrentDateDefault, out result) && result.Year >= 1000)
                        {
                            base["Day"] = result.Day.ToString(CultureInfo.InvariantCulture);
                            base["Month"] = result.Month.ToString(CultureInfo.InvariantCulture);
                            base["Year"] = result.Year.ToString(CultureInfo.InvariantCulture);
                        }
                        else
                            base["Year"] = comment[1];
                        break;

                    case "METADATA_BLOCK_PICTURE":
                        var picture = new MetadataBlockPicture(comment[1]);
                        if (picture.Type == PictureType.CoverFront || picture.Type == PictureType.Other)
                        {
                            try
                            {
                                CoverArt = new CoverArt(picture.Data);
                            }
                            catch (UnsupportedCoverArtException)
                            { }
                        }
                        break;

                    case "COVERART":
                        try
                        {
                            // Deprecated way to store cover art:
                            CoverArt = new CoverArt(Convert.FromBase64String(comment[1]));
                        }
                        catch (UnsupportedCoverArtException)
                        { }
                        break;

                    default:
                        string mappedKey;
                        if (_map.TryGetValue(comment[0], out mappedKey))
                            base[mappedKey] = comment[1];
                        break;
                }
            }
        }
    }
}
