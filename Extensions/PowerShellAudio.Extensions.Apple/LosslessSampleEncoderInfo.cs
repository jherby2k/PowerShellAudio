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

using PowerShellAudio.Extensions.Apple.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;

namespace PowerShellAudio.Extensions.Apple
{
    class LosslessSampleEncoderInfo : SampleEncoderInfo
    {
        public override string Name => "Apple Lossless";

        public override string FileExtension => ".m4a";

        public override bool IsLossless => true;

        public override string ExternalLibrary
        {
            get
            {
                try
                {
                    return string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderDescription,
                        SafeNativeMethods.GetCoreAudioToolboxVersion());
                }
                catch (TypeInitializationException e)
                {
                    return e.InnerException?.Message ?? e.Message;
                }
            }
        }

        public override SettingsDictionary DefaultSettings
        {
            get
            {
                // Call the external MP4 encoder for writing iTunes-compatible atoms:
                ExportFactory<IMetadataEncoder> metadataEncoderFactory =
                    ExtensionProvider.GetFactories<IMetadataEncoder>("Extension", FileExtension).SingleOrDefault();
                if (metadataEncoderFactory == null) return new SettingsDictionary();
                using (ExportLifetimeContext<IMetadataEncoder> metadataEncoderLifetime = metadataEncoderFactory.CreateExport())
                    return metadataEncoderLifetime.Value.EncoderInfo.DefaultSettings;
            }
        }

        public override IReadOnlyCollection<string> AvailableSettings
        {
            get
            {
                // Call the external MP4 encoder for writing iTunes-compatible atoms:
                ExportFactory<IMetadataEncoder> metadataEncoderFactory =
                    ExtensionProvider.GetFactories<IMetadataEncoder>("Extension", FileExtension).SingleOrDefault();
                if (metadataEncoderFactory == null) return new List<string>(0);
                using (ExportLifetimeContext<IMetadataEncoder> metadataEncoderLifetime = metadataEncoderFactory.CreateExport())
                    return metadataEncoderLifetime.Value.EncoderInfo.AvailableSettings;
            }
        }
    }
}
