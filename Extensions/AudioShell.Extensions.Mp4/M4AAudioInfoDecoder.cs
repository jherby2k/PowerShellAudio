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

using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PowerShellAudio.Extensions.Mp4
{
    [AudioInfoDecoderExport(".m4a")]
    public class M4AAudioInfoDecoder : IAudioInfoDecoder
    {
        public AudioInfo ReadAudioInfo(Stream stream)
        {
            Contract.Ensures(Contract.Result<AudioInfo>() != null);

            var mp4 = new Mp4(stream);

            uint dataSize = mp4.GetChildAtomInfo().Single(atom => atom.FourCC == "mdat").Size;
            
            mp4.DescendToAtom("moov", "trak", "mdia", "minf", "stbl", "stts");
            var stts = new SttsAtom(mp4.ReadAtom(mp4.CurrentAtom));

            uint sampleCount = stts.PacketCount * stts.PacketSize;

            mp4.DescendToAtom("moov", "trak", "mdia", "minf", "stbl", "stsd", "mp4a", "esds");
            var esds = new EsdsAtom(mp4.ReadAtom(mp4.CurrentAtom));
            if (esds.SampleRate > 0)
                return new AudioInfo(string.Format(CultureInfo.CurrentCulture, "{0}kbps MPEG 4 AAC", esds.AverageBitrate > 0 ? (int)esds.AverageBitrate / 1000 : CalculateBitRate(dataSize, sampleCount, esds.SampleRate)), esds.Channels, 0, (int)esds.SampleRate, sampleCount);
            else
            {
                // Apple Lossless files have their own atom for storing audio info:
                mp4.DescendToAtom("moov", "trak", "mdia", "minf", "stbl", "stsd", "alac");
                var alac = new AlacAtom(mp4.ReadAtom(mp4.CurrentAtom));
                return new AudioInfo("Apple Lossless", alac.Channels, alac.BitsPerSample, (int)alac.SampleRate, sampleCount);
            }
        }

        static int CalculateBitRate(uint byteCount, uint sampleCount, uint sampleRate)
        {
            Contract.Requires(sampleCount > 0);
            Contract.Requires(sampleRate > 0);
            Contract.Ensures(Contract.Result<int>() >= 0);

            return (int)Math.Round(byteCount * 8 / (sampleCount / (double)sampleRate) / 1000);
        }
    }
}
