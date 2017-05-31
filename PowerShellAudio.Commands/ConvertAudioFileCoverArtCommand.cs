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

using System.Management.Automation;
using JetBrains.Annotations;

namespace PowerShellAudio.Commands
{
    [Cmdlet(VerbsData.Convert, "AudioFileCoverArt"), OutputType(typeof(TaggedAudioFile))]
    [PublicAPI]
    public class ConvertAudioFileCoverArtCommand : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public AudioFile AudioFile { get; set; }

        [Parameter]
        public int MaxWidth { get; set; } = ConvertibleCoverArt.DefaultMaxWidth;

        [Parameter]
        public int Quality { get; set; } = ConvertibleCoverArt.DefaultQuality;

        [Parameter]
        public SwitchParameter ConvertToLossy { get; set; } = ConvertibleCoverArt.DefaultConvertToLossy;

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            var taggedAudioFile = new TaggedAudioFile(AudioFile);
            if (taggedAudioFile.Metadata.CoverArt != null)
                taggedAudioFile.Metadata.CoverArt = new ConvertibleCoverArt(taggedAudioFile.Metadata.CoverArt).Convert(MaxWidth, ConvertToLossy, Quality);
            if (PassThru)
                WriteObject(taggedAudioFile);
        }
    }
}
