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

namespace PowerShellAudio.Extensions.Mp4
{
    class AtomToMetadataAdapter : MetadataDictionary
    {
        static readonly Dictionary<string, string> _map =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "©alb", "Album" },
                { "©ART", "Artist" },
                { "©cmt", "Comment" },
                { "©gen", "Genre" },
                { "©nam", "Title" }
            };

        internal AtomToMetadataAdapter(Mp4 mp4, AtomInfo[] atoms)
        {
            Contract.Requires(mp4 != null);
            Contract.Requires(atoms != null);

            foreach (AtomInfo atom in atoms)
            {
                byte[] atomData = mp4.ReadAtom(atom);

                switch (atom.FourCC)
                {
                    case "trkn":
                        var trackNumberAtom = new TrackNumberAtom(atomData);
                        Add("TrackNumber", trackNumberAtom.TrackNumber.ToString(CultureInfo.InvariantCulture));
                        if (trackNumberAtom.TrackCount > 0)
                            Add("TrackCount", trackNumberAtom.TrackCount.ToString(CultureInfo.InvariantCulture));
                        break;

                    case "©day":
                        // The ©day atom may contain a full date, or only the year:
                        var dayAtom = new TextAtom(atomData);
                        DateTime result;
                        if (DateTime.TryParse(dayAtom.Value, CultureInfo.CurrentCulture, DateTimeStyles.NoCurrentDateDefault, out result) && result.Year >= 1000)
                        {
                            base["Day"] = result.Day.ToString(CultureInfo.InvariantCulture);
                            base["Month"] = result.Month.ToString(CultureInfo.InvariantCulture);
                            base["Year"] = result.Year.ToString(CultureInfo.InvariantCulture);
                        }
                        else
                            base["Year"] = dayAtom.Value;
                        break;

                    case "covr":
                        try
                        {
                            CoverArt = new CoverArt(new CovrAtom(atomData).Value);
                        }
                        catch (UnsupportedCoverArtException)
                        { }
                        break;

                    default:
                        string mappedKey;
                        if (_map.TryGetValue(atom.FourCC, out mappedKey))
                            base[mappedKey] = new TextAtom(atomData).Value;
                        break;
                }
            }
        }
    }
}
