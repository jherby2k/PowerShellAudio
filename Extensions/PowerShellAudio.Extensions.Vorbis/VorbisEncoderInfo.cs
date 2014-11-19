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

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;

namespace PowerShellAudio.Extensions.Vorbis
{
    class VorbisEncoderInfo : SampleEncoderInfo
    {
        public override string Name
        {
            get
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

                return "Ogg Vorbis";
            }
        }

        public override string FileExtension
        {
            get
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

                return ".ogg";
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

                return SafeNativeMethods.VorbisVersion();
            }
        }

        public override SettingsDictionary DefaultSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<SettingsDictionary>() != null);

                var result = new SettingsDictionary();

                result.Add("AddMetadata", bool.TrueString);
                result.Add("ControlMode", "Variable");
                result.Add("VBRQuality", "5");

                // Call the external ReplayGain filter for scaling the input:
                var replayGainFilterFactory = ExtensionProvider.GetFactories<ISampleFilter>("Name", "ReplayGain").SingleOrDefault();
                if (replayGainFilterFactory != null)
                    using (ExportLifetimeContext<ISampleFilter> replayGainFilterLifetime = replayGainFilterFactory.CreateExport())
                        replayGainFilterLifetime.Value.DefaultSettings.CopyTo(result);

                return result;
            }
        }

        public override IReadOnlyCollection<string> AvailableSettings
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyCollection<string>>() != null);

                var partialResult = new List<string>();

                partialResult.Add("AddMetadata");
                partialResult.Add("BitRate");
                partialResult.Add("ControlMode");
                partialResult.Add("SerialNumber");
                partialResult.Add("VBRQuality");

                // Call the external ReplayGain filter for scaling the input:
                var replayGainFilterFactory = ExtensionProvider.GetFactories<ISampleFilter>("Name", "ReplayGain").SingleOrDefault();
                if (replayGainFilterFactory != null)
                    using (ExportLifetimeContext<ISampleFilter> replayGainFilterLifetime = replayGainFilterFactory.CreateExport())
                        partialResult = partialResult.Concat(replayGainFilterLifetime.Value.AvailableSettings).ToList();

                return partialResult.AsReadOnly();
            }
        }
    }
}
