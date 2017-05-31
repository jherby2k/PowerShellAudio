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

using System.IO;
using System.Management.Automation;
using JetBrains.Annotations;

namespace PowerShellAudio.Commands
{
    [Cmdlet(VerbsCommon.Get, "AudioCoverArt", DefaultParameterSetName = "ByPath"), OutputType(typeof(CoverArt))]
    [PublicAPI]
    public class GetAudioCoverArtCommand : PSCmdlet
    {
        [Parameter(ParameterSetName = "ByPath", Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public string Path { get; set; }

        [Parameter(ParameterSetName = "ByLiteralPath", Mandatory = true, Position = 0), Alias("PSPath")]
        public string LiteralPath { get; set; }

        [Parameter(ParameterSetName = "ByFileInfo", Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public FileInfo FileInfo { get; set; }

        protected override void ProcessRecord()
        {
            foreach (string path in this.GetFileSystemPaths(Path, LiteralPath))
                WriteObject(new CoverArt(new FileInfo(path)));

            if (FileInfo != null)
                WriteObject(new CoverArt(FileInfo));
        }
    }
}
