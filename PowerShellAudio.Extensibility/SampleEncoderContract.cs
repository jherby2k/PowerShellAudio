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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace PowerShellAudio
{
    [ContractClassFor(typeof(ISampleEncoder))]
    abstract class SampleEncoderContract : ISampleEncoder
    {
        public string Extension
        {
            get
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
                Contract.Ensures(Contract.Result<string>().StartsWith(".", StringComparison.OrdinalIgnoreCase));
                Contract.Ensures(!Contract.Result<string>().Any(character => char.IsWhiteSpace(character)));
                Contract.Ensures(!Contract.Result<string>().Any(character => Path.GetInvalidFileNameChars().Contains(character)));
                Contract.Ensures(!Contract.Result<string>().Any(character => char.IsUpper(character)));

                return default(string);
            }
        }

        public SettingsDictionary DefaultSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<SettingsDictionary>() != null);

                return default(SettingsDictionary);
            }
        }

        public IReadOnlyCollection<string> AvailableSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyCollection<string>>() != null);

                return default(IReadOnlyCollection<string>);
            }
        }

        public void Initialize(Stream stream, AudioInfo audioInfo, MetadataDictionary metadata, SettingsDictionary settings)
        {
            Contract.Requires<ArgumentNullException>(stream != null);
            Contract.Requires<ArgumentException>(stream.CanRead);
            Contract.Requires<ArgumentException>(stream.CanWrite);
            Contract.Requires<ArgumentException>(stream.CanSeek);
            Contract.Requires<ArgumentException>(stream.Position == 0);
            Contract.Requires(audioInfo != null);
            Contract.Requires(metadata != null);
            Contract.Requires(settings != null);
        }

        public abstract bool ManuallyFreesSamples { get; }

        public abstract void Submit(SampleCollection samples);
    }
}
