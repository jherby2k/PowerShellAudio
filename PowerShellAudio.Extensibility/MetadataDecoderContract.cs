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
using System.Diagnostics.Contracts;
using System.IO;

namespace PowerShellAudio
{
    [ContractClassFor(typeof(IMetadataDecoder))]
    abstract class MetadataDecoderContract : IMetadataDecoder
    {
        public MetadataDictionary ReadMetadata(Stream stream)
        {
            Contract.Requires<ArgumentNullException>(stream != null);
            Contract.Requires<ArgumentException>(stream.CanRead);
            Contract.Requires<ArgumentException>(stream.CanSeek);
            Contract.Requires<ArgumentException>(stream.Position == 0);
            Contract.Ensures(stream.CanRead);
            Contract.Ensures(stream.CanSeek);
            Contract.Ensures(Contract.OldValue<Stream>(stream).Length == stream.Length);
            Contract.Ensures(Contract.Result<MetadataDictionary>() != null);

            return default(MetadataDictionary);
        }
    }
}
