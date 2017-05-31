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

using PowerShellAudio.Extensions.Mp4.Properties;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Mp4
{
    [MetadataEncoderExport(".m4a")]
    public class ItunesMetadataEncoder : IMetadataEncoder
    {
        static readonly MetadataEncoderInfo _encoderInfo = new ItunesMetadataEncoderInfo();

        [NotNull]
        public MetadataEncoderInfo EncoderInfo => _encoderInfo;

        public void WriteMetadata(
            [NotNull] Stream stream, 
            [NotNull] MetadataDictionary metadata,
            [NotNull] SettingsDictionary settings)
        {
            var originalMp4 = new Mp4(stream);
            AtomInfo[] topAtoms = originalMp4.GetChildAtomInfo();

            // Create a temporary stream to hold the new atom structure:
            using (var tempStream = new MemoryStream())
            {
                var tempMp4 = new Mp4(tempStream);

                // Copy the ftyp and moov atoms to the temporary stream:
                originalMp4.CopyAtom(topAtoms.Single(atom => atom.FourCC == "ftyp"), tempStream);
                originalMp4.CopyAtom(topAtoms.Single(atom => atom.FourCC == "moov"), tempStream);

                // Move to the start of the list atom:
                originalMp4.DescendToAtom("moov", "udta", "meta", "ilst");
                tempMp4.DescendToAtom("moov", "udta", "meta", "ilst");

                // Remove any copied ilst atoms, then generate new ones:
                tempStream.SetLength(tempStream.Position);
                byte[] ilstData = GenerateIlst(originalMp4, metadata, settings);
                tempStream.Write(ilstData, 0, ilstData.Length);

                // Update the ilst and parent atom sizes:
                tempMp4.UpdateAtomSizes((uint)tempStream.Length - tempMp4.CurrentAtom.End);

                // Update the creation times if they're being set explicitly:
                if (!string.IsNullOrEmpty(settings["CreationTime"]))
                    if (DateTime.TryParse(settings["CreationTime"], out DateTime creationTime))
                        UpdateCreationTimes(creationTime, tempMp4);
                    else
                        throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture,
                            Resources.MetadataEncoderBadCreationTime, settings["CreationTime"]));

                // Update the stco atom to reflect the new location of mdat:
                var mdatOffset = (int)(tempStream.Length - topAtoms.Single(atom => atom.FourCC == "mdat").Start);
                tempMp4.UpdateStco(mdatOffset);

                // Copy the mdat atom to the temporary stream, after the moov atom:
                tempStream.Seek(0, SeekOrigin.End);
                originalMp4.CopyAtom(topAtoms.Single(atom => atom.FourCC == "mdat"), tempStream);

                // Overwrite the original stream with the new, optimized one:
                stream.SetLength(tempStream.Length);
                stream.Position = 0;
                tempStream.Position = 0;
                tempStream.CopyTo(stream);
            }
        }

        [NotNull]
        static byte[] GenerateIlst(
            [NotNull] Mp4 originalMp4, 
            [NotNull] MetadataDictionary metadata,
            [NotNull] SettingsDictionary settings)
        {
            using (var resultStream = new MemoryStream())
            {
                var adaptedMetadata = new MetadataToAtomAdapter(metadata, settings);

                // "Reverse DNS" atoms may need to be preserved:
                foreach (ReverseDnsAtom reverseDnsAtom in 
                    from listAtom in originalMp4.GetChildAtomInfo()
                    where listAtom.FourCC == "----"
                    select new ReverseDnsAtom(originalMp4.ReadAtom(listAtom)))
                {
                    switch (reverseDnsAtom.Name)
                    {
                        // Always preserve the iTunSMPB (gapless playback) atom:
                        case "iTunSMPB":
                            resultStream.Write(reverseDnsAtom.GetBytes(), 0, reverseDnsAtom.GetBytes().Length);
                            break;

                        // Preserve the existing iTunNORM atom if a new one isn't provided, and AddSoundCheck isn't explicitly False:
                        case "iTunNORM":
                            if (!adaptedMetadata.IncludesSoundCheck &&
                                string.Compare(settings["AddSoundCheck"], bool.FalseString,
                                    StringComparison.OrdinalIgnoreCase) != 0)
                                resultStream.Write(reverseDnsAtom.GetBytes(), 0, reverseDnsAtom.GetBytes().Length);
                            break;
                    }
                }

                byte[] atomData = adaptedMetadata.GetBytes();
                resultStream.Write(atomData, 0, atomData.Length);

                return resultStream.ToArray();
            }
        }

        static void UpdateCreationTimes(DateTime creationTime, [NotNull] Mp4 mp4)
        {
            mp4.UpdateMvhd(creationTime, creationTime);
            mp4.UpdateTkhd(creationTime, creationTime);
            mp4.UpdateMdhd(creationTime, creationTime);
        }
    }
}
