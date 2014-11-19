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

using System.Management.Automation;

namespace PowerShellAudio.Commands
{
    [Cmdlet(VerbsCommon.Get, "AudioEncoderInfo"), OutputType(typeof(SampleEncoderInfo))]
    public class GetAudioEncoderInfoCommand : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Encoder { get; set; }

        protected override void ProcessRecord()
        {
            foreach (var encoderFactory in ExtensionProvider.GetFactories<ISampleEncoder>("Name", Encoder))
                using (var encoderLifetime = encoderFactory.CreateExport())
                    WriteObject(encoderLifetime.Value.EncoderInfo);
        }
    }
}
