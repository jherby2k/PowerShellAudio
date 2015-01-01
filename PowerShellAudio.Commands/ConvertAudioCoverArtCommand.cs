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
    [Cmdlet(VerbsData.Convert, "AudioCoverArt"), OutputType(typeof(CoverArt), typeof(TaggedAudioFile))]
    public class ConvertAudioCoverArtCommand : Cmdlet
    {
        int _maxWidth = ConvertibleCoverArt.DefaultMaxWidth;
        bool _convertToLossy = ConvertibleCoverArt.DefaultConvertToLossy;
        int _quality = ConvertibleCoverArt.DefaultQuality;

        [Parameter(ParameterSetName = "ByCoverArt", Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public CoverArt CoverArt { get; set; }

        [Parameter(ParameterSetName = "ByAudioFile", Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public AudioFile AudioFile { get; set; }

        [Parameter]
        public int MaxWidth
        {
            get { return _maxWidth; }
            set { _maxWidth = value; }
        }

        [Parameter]
        public SwitchParameter ConvertToLossy
        {
            get { return _convertToLossy; }
            set { _convertToLossy = value; }
        }

        [Parameter]
        public int Quality
        {
            get { return _quality; }
            set { _quality = value; }
        }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            if (CoverArt != null)
                WriteObject(new ConvertibleCoverArt(CoverArt).Convert(_maxWidth, _convertToLossy, _quality));
            else if (AudioFile != null)
            {
                var taggedAudioFile = new TaggedAudioFile(AudioFile);
                taggedAudioFile.Metadata.CoverArt = new ConvertibleCoverArt(taggedAudioFile.Metadata.CoverArt).Convert(_maxWidth, _convertToLossy, _quality);
                if (PassThru)
                    WriteObject(taggedAudioFile);
            }
        }
    }
}
