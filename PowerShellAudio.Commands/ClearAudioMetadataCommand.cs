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
using System.Linq;

namespace PowerShellAudio.Commands
{
    [Cmdlet(VerbsCommon.Clear, "AudioMetadata"), OutputType(typeof(TaggedAudioFile))]
    public class ClearAudioMetadataCommand : Cmdlet
    {
        [Parameter(Position = 0)]
        public string[] Key { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true)]
        public AudioFile AudioFile { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            var taggedAudioFile = AudioFile as TaggedAudioFile;
            if (taggedAudioFile != null)
            {
                if (Key != null && Key.Length > 0)
                {
                    foreach (var item in Key)
                        taggedAudioFile.Metadata.Remove(item);

                    // Treat CoverArt like a text field:
                    if (Key.Contains("CoverArt"))
                        taggedAudioFile.Metadata.CoverArt = null;
                }
                else
                    taggedAudioFile.Metadata.Clear();
            }

            if (PassThru)
                WriteObject(AudioFile);
        }
    }
}
