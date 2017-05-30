/*
 * Copyright © 2014, 2015 Jeremy Herbison
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

using System;
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Flac
{
    class NativeStreamAudioInfoDecoder : NativeStreamDecoder
    {
        internal AudioInfo AudioInfo { get; private set; }

        internal NativeStreamAudioInfoDecoder([NotNull] Stream input)
            : base(input)
        {
        }

        protected override void MetadataCallback(IntPtr handle, IntPtr metadata, IntPtr userData)
        {
            if ((MetadataType)Marshal.ReadInt32(metadata) != MetadataType.StreamInfo)
                return;

            StreamInfo streamInfo = Marshal.PtrToStructure<StreamInfoMetadataBlock>(metadata).StreamInfo;
            AudioInfo = new AudioInfo("FLAC", (int)streamInfo.Channels, (int)streamInfo.BitsPerSample,
                (int)streamInfo.SampleRate, (long)streamInfo.TotalSamples);
        }
    }
}
