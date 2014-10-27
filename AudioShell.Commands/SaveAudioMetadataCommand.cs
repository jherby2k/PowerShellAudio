/*
 * Copyright © 2014 Jeremy Herbison
 * 
 * This file is part of AudioShell.
 * 
 * AudioShell is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option)
 * any later version.
 * 
 * AudioShell is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for more
 * details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with AudioShell.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

using System.Collections;
using System.Management.Automation;
using System.Linq;
using System.Collections.Generic;

namespace AudioShell.Commands
{
    [Cmdlet(VerbsData.Save, "AudioMetadata", SupportsShouldProcess = true), OutputType(typeof(TaggedAudioFile))]
    public class SaveAudioMetadataCommand : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public AudioFile AudioFile { get; set; }

        [Parameter]
        public Hashtable Setting { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            if (ShouldProcess(AudioFile.FileInfo.FullName))
            {
                var taggedAudioFile = AudioFile as TaggedAudioFile;
                if (taggedAudioFile == null)
                    taggedAudioFile = new TaggedAudioFile(AudioFile);

                taggedAudioFile.SaveMetadata(new HashTableToSettingsDictionaryAdapter(Setting));
                if (PassThru)
                    WriteObject(taggedAudioFile);
            }
        }
    }
}
