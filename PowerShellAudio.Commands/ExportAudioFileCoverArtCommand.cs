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

using System.IO;
using System.Linq;
using System.Management.Automation;

namespace PowerShellAudio.Commands
{
    [Cmdlet(VerbsData.Export, "AudioFileCoverArt", DefaultParameterSetName = "ByPath", SupportsShouldProcess = true), OutputType(typeof(AudioFile))]
    public class ExportAudioFileCoverArtCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Name { get; set; }

        [Parameter(ParameterSetName = "ByPath", Mandatory = true, Position = 1)]
        public string Path { get; set; }

        [Parameter(ParameterSetName = "ByLiteralPath", Mandatory = true, Position = 1), Alias("PSPath")]
        public string LiteralPath { get; set; }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipeline = true)]
        public AudioFile AudioFile { get; set; }

        [Parameter]
        public SwitchParameter Replace { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            var taggedAudioFile = new TaggedAudioFile(AudioFile);
            var substituter = new MetadataSubstituter(taggedAudioFile.Metadata);

            DirectoryInfo outputDirectory;
            try
            {
                outputDirectory = new DirectoryInfo(this.GetFileSystemPaths(substituter.Substitute(Path), substituter.Substitute(LiteralPath)).First());
            }
            catch (ItemNotFoundException e)
            {
                outputDirectory = new DirectoryInfo(e.ItemName);
            }

            if (ShouldProcess(AudioFile.FileInfo.FullName))
                taggedAudioFile.Metadata.CoverArt.Export(outputDirectory, substituter.Substitute(Name), Replace);

            if (PassThru)
                WriteObject(taggedAudioFile);
        }
    }
}
