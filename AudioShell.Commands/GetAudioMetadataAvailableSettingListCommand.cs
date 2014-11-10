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

using PowerShellAudio.Commands.Properties;
using System;
using System.Globalization;
using System.Linq;
using System.Management.Automation;

namespace PowerShellAudio.Commands
{
    [Cmdlet(VerbsCommon.Get, "AudioMetadataAvailableSettingList"), OutputType(typeof(string))]
    public class GetAudioMetadataAvailableSettingListCommand : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Extension { get; set; }

        protected override void ProcessRecord()
        {
            var encoderFactory = ExtensionProvider<IMetadataEncoder>.Instance.Factories.Where(factory => string.Compare((string)factory.Metadata["Extension"], Extension, StringComparison.OrdinalIgnoreCase) == 0).SingleOrDefault();
            if (encoderFactory == null)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExtensionUnknownError, Extension));

            using (var encoderLifetime = encoderFactory.CreateExport())
                WriteObject(encoderLifetime.Value.AvailableSettings, true);
        }
    }
}
