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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioShell.Extensions.Mp4
{
    [MetadataDecoderExport(".m4a")]
    public class ItunesMetadataDecoder : IMetadataDecoder
    {
        public MetadataDictionary ReadMetadata(Stream stream)
        {
            var result = new MetadataDictionary();

            var mp4 = new Mp4(stream);
            mp4.DescendToAtom("moov", "udta", "meta", "ilst");

            var adaptedMetadata = new AtomToMetadataAdapter(mp4.GetChildAtomInfo());
            foreach (var item in adaptedMetadata)
            {
                byte[] atomData = mp4.ReadAtom(item.Value);

                if (item.Key == "TrackNumber")
                    result.Add(item.Key, new TrackNumberAtom(atomData).TrackNumber.ToString(CultureInfo.InvariantCulture));
                else
                    result.Add(item.Key, new TextAtom(atomData).Value);
            }

            return result;
        }
    }
}
