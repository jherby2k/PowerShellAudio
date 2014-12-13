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

using Microsoft.PowerShell.Commands;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace PowerShellAudio.Commands
{
    [Cmdlet(VerbsCommon.Get, "AudioCoverArt", DefaultParameterSetName = "ByPath"), OutputType(typeof(CoverArt))]
    public class GetAudioCoverArtCommand : PSCmdlet
    {
        readonly List<FileInfo> _files = new List<FileInfo>();

        [Parameter(ParameterSetName = "ByFileInfo", Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public FileInfo FileInfo { get; set; }

        [Parameter(ParameterSetName = "ByPath", Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public string Path { get; set; }

        [Parameter(ParameterSetName = "ByLiteralPath", Mandatory = true, Position = 0), Alias("PSPath")]
        public string LiteralPath { get; set; }

        protected override void ProcessRecord()
        {
            ProviderInfo provider;

            if (!string.IsNullOrEmpty(Path))
            {
                var providerPaths = GetResolvedProviderPathFromPSPath(Path, out provider);
                if (provider.ImplementingType == typeof(FileSystemProvider))
                    _files.AddRange(providerPaths.Select(path => new FileInfo(path)));
            }
            else if (!string.IsNullOrEmpty(LiteralPath))
            {
                PSDriveInfo drive;

                string providerPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(LiteralPath, out provider, out drive);
                if (provider.ImplementingType == typeof(FileSystemProvider))
                    _files.Add(new FileInfo(providerPath));
            }
            else
                _files.Add(FileInfo);
        }

        protected override void EndProcessing()
        {
            foreach (var file in _files)
                WriteObject(new CoverArt(file));
        }
    }
}
