/*
 * Copyright © 2014-2017 Jeremy Herbison
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
using JetBrains.Annotations;

namespace PowerShellAudio.Commands
{
    [Cmdlet(VerbsCommon.Get, "AudioAnalyzerInfo"), OutputType(typeof(SampleAnalyzerInfo))]
    [PublicAPI]
    public class GetAudioAnalyzerInfoCommand : Cmdlet
    {
        [Parameter(Position = 0)]
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            foreach (ExportFactory<ISampleAnalyzer> encoderFactory in string.IsNullOrEmpty(Name)
                ? ExtensionProvider.GetFactories<ISampleAnalyzer>()
                : ExtensionProvider.GetFactories<ISampleAnalyzer>("Name", Name))
                using (ExportLifetimeContext<ISampleAnalyzer> encoderLifetime = encoderFactory.CreateExport())
                    WriteObject(encoderLifetime.Value.AnalyzerInfo);
        }
    }
}
