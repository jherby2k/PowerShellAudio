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

using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace AudioShell.Commands
{
    [Cmdlet(VerbsCommon.Rename, "AudioFile", SupportsShouldProcess = true), OutputType(typeof(TaggedAudioFile))]
    public class RenameAudioFileCommand : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Name { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true)]
        public AudioFile AudioFile { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            if (ShouldProcess(AudioFile.FileInfo.FullName))
            {
                var taggedAudioFile = AudioFile as TaggedAudioFile;
                if (taggedAudioFile == null)
                    taggedAudioFile = new TaggedAudioFile(AudioFile);

                char[] invalidChars = Path.GetInvalidFileNameChars();

                // Replace all instances of {Key} with the value of metadata["Key"], while omitting any invalid characters:
                taggedAudioFile.Rename(Regex.Replace(Name, @"\{[^{]+\}", match => new string(taggedAudioFile.Metadata[match.Value.Substring(1, match.Value.Length - 2)].Where(character => !invalidChars.Contains(character)).ToArray())));
                if (PassThru)
                    WriteObject(taggedAudioFile);
            }
        }
    }
}
