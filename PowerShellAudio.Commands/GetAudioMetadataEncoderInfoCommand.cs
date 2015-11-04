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

using System.ComponentModel.Composition;
using System.Management.Automation;

namespace PowerShellAudio.Commands
{
    [Cmdlet(VerbsCommon.Get, "AudioMetadataEncoderInfo"), OutputType(typeof(MetadataEncoderInfo))]
    public class GetAudioMetadataEncoderInfoCommand : Cmdlet
    {
        [Parameter(Position = 0)]
        public string Extension { get; set; }

        protected override void ProcessRecord()
        {
            foreach (ExportFactory<IMetadataEncoder> factory in string.IsNullOrEmpty(Extension)
                ? ExtensionProvider.GetFactories<IMetadataEncoder>() 
                : ExtensionProvider.GetFactories<IMetadataEncoder>("Extension", Extension))
                using (ExportLifetimeContext<IMetadataEncoder> encoderLifetime = factory.CreateExport())
                    WriteObject(encoderLifetime.Value.EncoderInfo);
        }
    }
}
