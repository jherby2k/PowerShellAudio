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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

namespace PowerShellAudio.Extensions.Apple
{
    class AacEncoderInfo : SampleEncoderInfo
    {
        public override string Name
        {
            get
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

                return "Apple AAC";
            }
        }

        public override string FileExtension
        {
            get
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

                return ".m4a";
            }
        }

        public override bool IsLossless
        {
            get { return false; }
        }

        public override string ExternalLibrary
        {
            get
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

                return string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderDescription, SafeNativeMethods.GetCoreAudioToolboxVersion());
            }
        }

        public override SettingsDictionary DefaultSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<SettingsDictionary>() != null);

                var result = new SettingsDictionary();

                result.Add("ControlMode", "Variable");
                result.Add("Quality", "High");
                result.Add("VBRQuality", "9");

                // Call the external ReplayGain filter for scaling the input:
                var replayGainFilterFactory = ExtensionProvider.GetFactories<ISampleFilter>("Name", "ReplayGain").SingleOrDefault();
                if (replayGainFilterFactory != null)
                    using (ExportLifetimeContext<ISampleFilter> replayGainFilterLifetime = replayGainFilterFactory.CreateExport())
                        replayGainFilterLifetime.Value.DefaultSettings.CopyTo(result);

                // Call the external MP4 encoder for writing iTunes-compatible atoms:
                var metadataEncoderFactory = ExtensionProvider.GetFactories<IMetadataEncoder>("Extension", FileExtension).SingleOrDefault();
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
                Contract.Ensures(Contract.Result<IReadOnlyCollection<string>>() != null);

                var partialResult = new List<string>();

                partialResult.Add("BitRate");
                partialResult.Add("ControlMode");
                partialResult.Add("Quality");
                partialResult.Add("VBRQuality");

                // Call the external ReplayGain filter for scaling the input:
                var replayGainFilterFactory = ExtensionProvider.GetFactories<ISampleFilter>("Name", "ReplayGain").SingleOrDefault();
                if (replayGainFilterFactory != null)
                    using (ExportLifetimeContext<ISampleFilter> replayGainFilterLifetime = replayGainFilterFactory.CreateExport())
                        partialResult = partialResult.Concat(replayGainFilterLifetime.Value.AvailableSettings).ToList();

                // Call the external MP4 encoder for writing iTunes-compatible atoms:
                var metadataEncoderFactory = ExtensionProvider.GetFactories<IMetadataEncoder>("Extension", FileExtension).SingleOrDefault();
                if (metadataEncoderFactory != null)
                    using (ExportLifetimeContext<IMetadataEncoder> metadataEncoderLifetime = metadataEncoderFactory.CreateExport())
                        partialResult = partialResult.Concat(metadataEncoderLifetime.Value.EncoderInfo.AvailableSettings).ToList();

                return partialResult.AsReadOnly();
            }
        }
    }
}
