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

using PowerShellAudio.Extensions.Lame.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace PowerShellAudio.Extensions.Lame
{
    class LameSampleEncoderInfo : SampleEncoderInfo
    {
        public override string Name => "Lame MP3";

        public override string FileExtension => ".mp3";

        public override bool IsLossless => false;

        public override string ExternalLibrary
        {
            get
            {
                try
                {
                    return string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderDescription,
                        Marshal.PtrToStringAnsi(SafeNativeMethods.GetLameVersion()));
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
                var result = new SettingsDictionary { { "Quality", "3" }, { "VBRQuality", "2" } };

                // Call the external ReplayGain filter for scaling the input:
                ExportFactory<ISampleFilter> replayGainFilterFactory =
                    ExtensionProvider.GetFactories<ISampleFilter>("Name", "ReplayGain").SingleOrDefault();
                if (replayGainFilterFactory != null)
                    using (ExportLifetimeContext<ISampleFilter> replayGainFilterLifetime = replayGainFilterFactory.CreateExport())
                        replayGainFilterLifetime.Value.DefaultSettings.CopyTo(result);

                // Call the external ID3 encoder:
                ExportFactory<IMetadataEncoder> metadataEncoderFactory =
                    ExtensionProvider.GetFactories<IMetadataEncoder>("Extension", "FileExtension").SingleOrDefault();
                if (metadataEncoderFactory != null)
                    using (ExportLifetimeContext<IMetadataEncoder> metadataEncoderLifetime = metadataEncoderFactory.CreateExport())
                        metadataEncoderLifetime.Value.EncoderInfo.DefaultSettings.CopyTo(result);

                return result;
            }
        }

        public override IReadOnlyCollection<string> AvailableSettings
        {
            get
            {
                var partialResult = new List<string> { "BitRate", "ForceCBR", "Quality", "VBRQuality" };

                // Call the external ReplayGain filter for scaling the input:
                ExportFactory<ISampleFilter> replayGainFilterFactory =
                    ExtensionProvider.GetFactories<ISampleFilter>("Name", "ReplayGain").SingleOrDefault();
                if (replayGainFilterFactory != null)
                    using (ExportLifetimeContext<ISampleFilter> replayGainFilterLifetime = replayGainFilterFactory.CreateExport())
                        partialResult = partialResult.Concat(replayGainFilterLifetime.Value.AvailableSettings).ToList();

                // Call the external ID3 encoder:
                ExportFactory<IMetadataEncoder> metadataEncoderFactory =
                    ExtensionProvider.GetFactories<IMetadataEncoder>("Extension", FileExtension).SingleOrDefault();
                if (metadataEncoderFactory != null)
                    using (ExportLifetimeContext<IMetadataEncoder> metadataEncoderLifetime = metadataEncoderFactory.CreateExport())
                        partialResult = partialResult.Concat(metadataEncoderLifetime.Value.EncoderInfo.AvailableSettings).ToList();

                return partialResult;
            }
        }
    }
}
