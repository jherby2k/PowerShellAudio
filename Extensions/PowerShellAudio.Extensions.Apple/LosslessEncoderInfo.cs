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

using PowerShellAudio.Extensions.Apple.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

namespace PowerShellAudio.Extensions.Apple
{
    class LosslessEncoderInfo : SampleEncoderInfo
    {
        public override string Description
        {
            get
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

                return string.Format(CultureInfo.CurrentCulture, Resources.LosslessSampleEncoderDescription, SafeNativeMethods.GetCoreAudioToolboxVersion());
            }
        }

        public override string Extension
        {
            get
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

                return ".m4a";
            }
        }

        public override SettingsDictionary DefaultSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<SettingsDictionary>() != null);

                // Call the external MP4 encoder for writing iTunes-compatible atoms:
                var metadataEncoderFactory = ExtensionProvider.GetFactories<IMetadataEncoder>().Where(factory => string.Compare((string)factory.Metadata["Extension"], Extension, StringComparison.OrdinalIgnoreCase) == 0).SingleOrDefault();
                if (metadataEncoderFactory != null)
                    using (ExportLifetimeContext<IMetadataEncoder> metadataEncoderLifetime = metadataEncoderFactory.CreateExport())
                        return metadataEncoderLifetime.Value.DefaultSettings;

                return new SettingsDictionary();
            }
        }

        public override IReadOnlyCollection<string> AvailableSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyCollection<string>>() != null);

                // Call the external MP4 encoder for writing iTunes-compatible atoms:
                var metadataEncoderFactory = ExtensionProvider.GetFactories<IMetadataEncoder>().Where(factory => string.Compare((string)factory.Metadata["Extension"], Extension, StringComparison.OrdinalIgnoreCase) == 0).SingleOrDefault();
                if (metadataEncoderFactory != null)
                    using (ExportLifetimeContext<IMetadataEncoder> metadataEncoderLifetime = metadataEncoderFactory.CreateExport())
                        return metadataEncoderLifetime.Value.AvailableSettings;

                return new List<string>(1).AsReadOnly();
            }
        }
    }
}
