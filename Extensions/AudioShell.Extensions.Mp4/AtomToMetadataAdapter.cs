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

namespace AudioShell.Extensions.Mp4
{
    class AtomToMetadataAdapter : Dictionary<string, AtomInfo>
    {
        static readonly Dictionary<string, string> _map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "©alb", "Album"       },
            { "©ART", "Artist"      },
            { "©cmt", "Comment"     },
            { "©gen", "Genre"       },
            { "©nam", "Title"       },
            { "trkn", "TrackNumber" },
            { "©day", "Year"        }
        };

        internal AtomToMetadataAdapter(AtomInfo[] atoms)
        {
            Contract.Requires(atoms != null);

            foreach (AtomInfo atom in atoms)
            {
                string mappedKey;
                if (_map.TryGetValue(atom.FourCC, out mappedKey))
                    base[mappedKey] = atom;
            }
        }
    }
}
