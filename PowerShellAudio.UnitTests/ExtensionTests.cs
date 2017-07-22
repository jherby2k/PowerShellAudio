using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using Xunit;

namespace PowerShellAudio.UnitTests
{
    public class ExtensionTests : IClassFixture<ExtensionFixture>, IClassFixture<TestFilesFixture>
    {
        readonly TestFilesFixture _testFilesFixture;

        public ExtensionTests(TestFilesFixture testFilesFixture)
        {
            _testFilesFixture = testFilesFixture;
        }

        public static IEnumerable<object[]> GetExportDataSet()
        {
            yield return new object[] { 0, "LPCM 8-bit 8000Hz Stereo.wav", "Wave", "", "Comment=Test Sample", "", "81-8E-E6-CB-F1-6F-76-F9-23-D3-36-50-E7-A5-27-08", "81-8E-E6-CB-F1-6F-76-F9-23-D3-36-50-E7-A5-27-08" };
            yield return new object[] { 1, "LPCM 16-bit 44100Hz Stereo.wav", "Wave", "", "Comment=Test Sample", "", "5D-4B-86-9C-D7-2B-E2-08-BC-7B-47-F3-5E-13-BE-9A", "5D-4B-86-9C-D7-2B-E2-08-BC-7B-47-F3-5E-13-BE-9A" };
            yield return new object[] { 2, "LPCM 16-bit 44100Hz Mono.wav", "Wave", "", "Comment=Test Sample", "", "50-9B-83-82-8F-13-94-5E-41-21-E4-C4-89-7A-86-49", "50-9B-83-82-8F-13-94-5E-41-21-E4-C4-89-7A-86-49" };
            yield return new object[] { 3, "LPCM 16-bit 48000Hz Stereo.wav", "Wave", "", "Comment=Test Sample", "", "EF-BC-44-B9-FA-9C-04-44-9D-67-EC-D1-6C-B7-F3-D8", "EF-BC-44-B9-FA-9C-04-44-9D-67-EC-D1-6C-B7-F3-D8" };
            yield return new object[] { 4, "LPCM 24-bit 96000Hz Stereo.wav", "Wave", "", "Comment=Test Sample", "", "D5-5B-D1-98-76-76-A7-D6-C2-A0-4B-F0-9C-10-F6-4F", "D5-5B-D1-98-76-76-A7-D6-C2-A0-4B-F0-9C-10-F6-4F" };
            yield return new object[] { 5, "FLAC 8-bit 8000Hz Stereo.flac", "Wave", "", "Comment=Test Sample", "", "81-8E-E6-CB-F1-6F-76-F9-23-D3-36-50-E7-A5-27-08", "81-8E-E6-CB-F1-6F-76-F9-23-D3-36-50-E7-A5-27-08" };
            yield return new object[] { 6, "FLAC 16-bit 44100Hz Stereo.flac", "Wave", "", "Comment=Test Sample", "", "5D-4B-86-9C-D7-2B-E2-08-BC-7B-47-F3-5E-13-BE-9A", "5D-4B-86-9C-D7-2B-E2-08-BC-7B-47-F3-5E-13-BE-9A" };
            yield return new object[] { 7, "FLAC 16-bit 44100Hz Mono.flac", "Wave", "", "Comment=Test Sample", "", "50-9B-83-82-8F-13-94-5E-41-21-E4-C4-89-7A-86-49", "50-9B-83-82-8F-13-94-5E-41-21-E4-C4-89-7A-86-49" };
            yield return new object[] { 8, "FLAC 16-bit 48000Hz Stereo.flac", "Wave", "", "Comment=Test Sample", "", "EF-BC-44-B9-FA-9C-04-44-9D-67-EC-D1-6C-B7-F3-D8", "EF-BC-44-B9-FA-9C-04-44-9D-67-EC-D1-6C-B7-F3-D8" };
            yield return new object[] { 9, "FLAC 24-bit 96000Hz Stereo.flac", "Wave", "", "Comment=Test Sample", "", "D5-5B-D1-98-76-76-A7-D6-C2-A0-4B-F0-9C-10-F6-4F", "D5-5B-D1-98-76-76-A7-D6-C2-A0-4B-F0-9C-10-F6-4F" };
            yield return new object[] { 10, "ALAC 16-bit 44100Hz Stereo.m4a", "Wave", "", "Comment=Test Sample", "", "5D-4B-86-9C-D7-2B-E2-08-BC-7B-47-F3-5E-13-BE-9A", "5D-4B-86-9C-D7-2B-E2-08-BC-7B-47-F3-5E-13-BE-9A" };
            yield return new object[] { 11, "ALAC 16-bit 44100Hz Mono.m4a", "Wave", "", "Comment=Test Sample", "", "50-9B-83-82-8F-13-94-5E-41-21-E4-C4-89-7A-86-49", "50-9B-83-82-8F-13-94-5E-41-21-E4-C4-89-7A-86-49" };
            yield return new object[] { 12, "ALAC 16-bit 48000Hz Stereo.m4a", "Wave", "", "Comment=Test Sample", "", "EF-BC-44-B9-FA-9C-04-44-9D-67-EC-D1-6C-B7-F3-D8", "EF-BC-44-B9-FA-9C-04-44-9D-67-EC-D1-6C-B7-F3-D8" };
            yield return new object[] { 13, "ALAC 24-bit 96000Hz Stereo.m4a", "Wave", "", "Comment=Test Sample", "", "D5-5B-D1-98-76-76-A7-D6-C2-A0-4B-F0-9C-10-F6-4F", "D5-5B-D1-98-76-76-A7-D6-C2-A0-4B-F0-9C-10-F6-4F" };
            yield return new object[] { 14, "LPCM 8-bit 8000Hz Stereo.wav", "FLAC", "", "Comment=Test Sample", "", "36-CE-D0-12-BE-63-D6-07-A2-DE-8D-7D-9B-28-FB-B7", "36-CE-D0-12-BE-63-D6-07-A2-DE-8D-7D-9B-28-FB-B7" };
            yield return new object[] { 15, "LPCM 16-bit 44100Hz Stereo.wav", "FLAC", "", "Comment=Test Sample", "", "1D-CF-F1-DE-A7-A0-28-A7-BA-FF-8C-3A-B5-A7-24-CC", "1D-CF-F1-DE-A7-A0-28-A7-BA-FF-8C-3A-B5-A7-24-CC" };
            yield return new object[] { 16, "LPCM 16-bit 44100Hz Mono.wav", "FLAC", "", "Comment=Test Sample", "", "EE-2F-5E-6F-43-8E-BD-6E-17-19-70-F0-3B-E5-5E-B0", "EE-2F-5E-6F-43-8E-BD-6E-17-19-70-F0-3B-E5-5E-B0" };
            yield return new object[] { 17, "LPCM 16-bit 48000Hz Stereo.wav", "FLAC", "", "Comment=Test Sample", "", "75-3D-E6-11-1C-44-C0-DC-82-8A-25-9A-17-FB-AA-9A", "75-3D-E6-11-1C-44-C0-DC-82-8A-25-9A-17-FB-AA-9A" };
            yield return new object[] { 18, "LPCM 24-bit 96000Hz Stereo.wav", "FLAC", "", "Comment=Test Sample", "", "B7-6B-50-1A-A0-28-B8-F8-37-AB-99-75-36-DB-E3-A8", "B7-6B-50-1A-A0-28-B8-F8-37-AB-99-75-36-DB-E3-A8" };
            yield return new object[] { 19, "LPCM 16-bit 44100Hz Stereo.wav", "FLAC", "", "", "", "5D-BA-EB-E2-1B-BB-2A-EC-3C-22-DE-2B-5A-D1-46-16", "5D-BA-EB-E2-1B-BB-2A-EC-3C-22-DE-2B-5A-D1-46-16" };
            yield return new object[] { 20, "LPCM 16-bit 44100Hz Stereo.wav", "FLAC", "CompressionLevel=5;SeekPointInterval=10", "Comment=Test Sample", "", "1D-CF-F1-DE-A7-A0-28-A7-BA-FF-8C-3A-B5-A7-24-CC", "1D-CF-F1-DE-A7-A0-28-A7-BA-FF-8C-3A-B5-A7-24-CC" };
            yield return new object[] { 21, "LPCM 16-bit 44100Hz Stereo.wav", "FLAC", "CompressionLevel=0", "Comment=Test Sample", "", "D9-68-2D-A8-3B-2C-14-40-B3-82-F4-B0-72-57-56-0E", "D9-68-2D-A8-3B-2C-14-40-B3-82-F4-B0-72-57-56-0E" };
            yield return new object[] { 22, "LPCM 16-bit 44100Hz Stereo.wav", "FLAC", "SeekPointInterval=0", "Comment=Test Sample", "", "7A-03-0A-E6-70-A9-B8-18-E7-90-F3-7B-A1-0D-F2-90", "7A-03-0A-E6-70-A9-B8-18-E7-90-F3-7B-A1-0D-F2-90" };
            yield return new object[] { 23, "LPCM 16-bit 44100Hz Stereo.wav", "FLAC", "SeekPointInterval=1", "Comment=Test Sample", "", "B2-DB-53-4D-24-97-4D-A6-AD-09-6D-56-97-80-A4-E1", "B2-DB-53-4D-24-97-4D-A6-AD-09-6D-56-97-80-A4-E1" };
            yield return new object[] { 24, "LPCM 16-bit 44100Hz Stereo.wav", "FLAC", "", "Comment=Test Sample", "TestCover.png", "73-84-FD-35-56-B5-49-05-73-D1-71-EA-2F-21-28-56", "73-84-FD-35-56-B5-49-05-73-D1-71-EA-2F-21-28-56" };
            yield return new object[] { 25, "LPCM 16-bit 44100Hz Stereo.wav", "FLAC", "", "Comment=Test Sample", "TestCover.jpg", "7F-A9-5C-3E-CF-09-BD-36-87-3B-E7-9D-4E-10-12-8D", "7F-A9-5C-3E-CF-09-BD-36-87-3B-E7-9D-4E-10-12-8D" };
            yield return new object[] { 26, "LPCM 8-bit 8000Hz Stereo.wav", "Lame MP3", "", "Comment=Test Sample", "", "F7-99-98-46-6C-DC-8E-60-B3-06-B4-AB-68-CE-05-11", "F7-99-98-46-6C-DC-8E-60-B3-06-B4-AB-68-CE-05-11" };
            yield return new object[] { 27, "LPCM 16-bit 44100Hz Stereo.wav", "Lame MP3", "", "Comment=Test Sample", "", "81-D9-6C-E4-77-CE-F4-0C-F4-AC-DB-BE-1F-F1-D8-EC", "81-D9-6C-E4-77-CE-F4-0C-F4-AC-DB-BE-1F-F1-D8-EC" };
            yield return new object[] { 28, "LPCM 16-bit 44100Hz Mono.wav", "Lame MP3", "", "Comment=Test Sample", "", "BD-FE-BD-08-49-95-30-40-8D-3B-60-FE-FC-F7-5F-F9", "BD-FE-BD-08-49-95-30-40-8D-3B-60-FE-FC-F7-5F-F9" };
            yield return new object[] { 29, "LPCM 16-bit 48000Hz Stereo.wav", "Lame MP3", "", "Comment=Test Sample", "", "FB-BC-41-88-5D-62-F7-FF-88-73-31-4D-CB-65-01-2A", "FB-BC-41-88-5D-62-F7-FF-88-73-31-4D-CB-65-01-2A" };
            yield return new object[] { 30, "LPCM 24-bit 96000Hz Stereo.wav", "Lame MP3", "", "Comment=Test Sample", "", "AC-79-5F-D4-41-2A-27-44-81-BA-58-00-2B-7B-3E-62", "1D-A5-63-FB-18-D5-E6-02-32-6C-79-9F-B8-59-B9-B5" };
            yield return new object[] { 31, "LPCM 16-bit 44100Hz Stereo.wav", "Lame MP3", "", "", "", "E7-CE-BD-CC-18-5B-EF-4C-8C-14-E5-FF-9B-AF-B8-86", "E7-CE-BD-CC-18-5B-EF-4C-8C-14-E5-FF-9B-AF-B8-86" };
            yield return new object[] { 32, "LPCM 16-bit 44100Hz Stereo.wav", "Lame MP3", "Quality=3;VBRQuality=2;ApplyGain=False;AddSoundCheck=False;ID3Version=2.3;PaddingSize=0;UsePadding=False", "Comment=Test Sample", "", "81-D9-6C-E4-77-CE-F4-0C-F4-AC-DB-BE-1F-F1-D8-EC", "81-D9-6C-E4-77-CE-F4-0C-F4-AC-DB-BE-1F-F1-D8-EC" };
            yield return new object[] { 33, "LPCM 16-bit 44100Hz Stereo.wav", "Lame MP3", "Quality=9", "Comment=Test Sample", "", "2D-05-F6-84-76-60-C8-AD-E4-FF-20-88-07-B3-39-28", "2D-05-F6-84-76-60-C8-AD-E4-FF-20-88-07-B3-39-28" };
            yield return new object[] { 34, "LPCM 16-bit 44100Hz Stereo.wav", "Lame MP3", "VBRQuality=9.999", "Comment=Test Sample", "", "D5-3F-5F-36-22-1D-EF-70-23-F3-84-EE-2A-E9-9F-97", "F0-E6-84-94-51-A2-9B-4A-0C-54-3B-EF-BC-BD-FC-E4" };
            yield return new object[] { 35, "LPCM 16-bit 44100Hz Stereo.wav", "Lame MP3", "ApplyGain=Track", "Comment=Test Sample;TrackPeak=1.000000;TrackGain=-6.58 dB", "", "8D-56-6F-32-5E-E2-DD-67-2D-A9-46-42-22-B6-81-45", "8D-56-6F-32-5E-E2-DD-67-2D-A9-46-42-22-B6-81-45" };
            yield return new object[] { 36, "LPCM 16-bit 44100Hz Stereo.wav", "Lame MP3", "ApplyGain=Album", "Comment=Test Sample;AlbumPeak=1.000000;AlbumGain=-6.58 dB", "", "41-87-3A-F4-8E-A9-9C-DD-66-28-26-D6-EB-0C-EB-DC", "41-87-3A-F4-8E-A9-9C-DD-66-28-26-D6-EB-0C-EB-DC" };
            yield return new object[] { 37, "LPCM 16-bit 44100Hz Stereo.wav", "Lame MP3", "AddSoundCheck=Track", "Comment=Test Sample;TrackPeak=1.000000;TrackGain=-6.58 dB", "", "FB-6C-B8-07-7A-92-5F-7A-52-46-0D-63-02-D8-60-C9", "FB-6C-B8-07-7A-92-5F-7A-52-46-0D-63-02-D8-60-C9" };
            yield return new object[] { 38, "LPCM 16-bit 44100Hz Stereo.wav", "Lame MP3", "AddSoundCheck=Album", "Comment=Test Sample;AlbumPeak=1.000000;AlbumGain=-6.58 dB", "", "AD-08-7C-13-3F-8E-DD-64-41-F3-D8-F7-42-44-43-11", "AD-08-7C-13-3F-8E-DD-64-41-F3-D8-F7-42-44-43-11" };
            yield return new object[] { 39, "LPCM 16-bit 44100Hz Stereo.wav", "Lame MP3", "ID3Version=2.4", "Comment=Test Sample", "", "DA-90-0D-DD-7D-B6-CB-15-C1-C6-47-01-0A-51-73-D1", "DA-90-0D-DD-7D-B6-CB-15-C1-C6-47-01-0A-51-73-D1" };
            yield return new object[] { 40, "LPCM 16-bit 44100Hz Stereo.wav", "Lame MP3", "PaddingSize=1024", "Comment=Test Sample", "", "6A-17-57-D5-C3-52-73-99-E7-21-CB-A8-37-A2-D3-3C", "6A-17-57-D5-C3-52-73-99-E7-21-CB-A8-37-A2-D3-3C" };
            yield return new object[] { 41, "LPCM 16-bit 44100Hz Stereo.wav", "Lame MP3", "BitRate=8", "Comment=Test Sample", "", "DF-06-52-08-78-19-25-76-FF-FD-6E-D0-C2-4A-DC-CC", "6E-B3-DB-46-03-78-45-78-92-E9-39-37-31-BF-EB-B3" };
            yield return new object[] { 42, "LPCM 16-bit 44100Hz Stereo.wav", "Lame MP3", "BitRate=8;ForceCBR=True", "Comment=Test Sample", "", "5B-FE-76-F8-D9-48-90-65-0E-32-0F-A9-D2-3E-49-BC", "47-49-53-DF-8B-0C-59-25-AE-6F-8D-AA-51-52-47-D4" };
            yield return new object[] { 43, "LPCM 16-bit 44100Hz Stereo.wav", "Lame MP3", "", "Comment=Test Sample", "TestCover.png", "37-32-84-54-D3-77-1B-A0-65-49-16-91-32-86-E3-9D", "37-32-84-54-D3-77-1B-A0-65-49-16-91-32-86-E3-9D" };
            yield return new object[] { 44, "LPCM 16-bit 44100Hz Stereo.wav", "Lame MP3", "", "Comment=Test Sample", "TestCover.jpg", "86-EA-B2-5F-AC-38-2B-46-DE-B4-18-0D-1B-B4-F7-B1", "86-EA-B2-5F-AC-38-2B-46-DE-B4-18-0D-1B-B4-F7-B1" };
            yield return new object[] { 45, "LPCM 8-bit 8000Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM", "Comment=Test Sample", "", "F7-47-EE-71-2A-2E-6D-7A-7F-AB-86-D8-5B-D9-07-C2", "F7-47-EE-71-2A-2E-6D-7A-7F-AB-86-D8-5B-D9-07-C2" };
            yield return new object[] { 46, "LPCM 16-bit 44100Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM", "Comment=Test Sample", "", "7D-7D-92-BC-FA-E2-40-15-3D-FC-5E-35-10-CF-97-5F", "31-3D-FA-2B-27-9A-29-03-A8-DD-EA-7B-CA-83-05-BB" };
            yield return new object[] { 47, "LPCM 16-bit 44100Hz Mono.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM", "Comment=Test Sample", "", "4C-B4-F7-A4-A1-D5-B2-25-71-6C-6F-C0-1C-10-FF-D6", "9F-50-BB-9B-92-A5-BD-C7-14-50-05-70-9D-84-67-46" };
            yield return new object[] { 48, "LPCM 16-bit 48000Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM", "Comment=Test Sample", "", "5D-7C-5C-74-28-6A-A1-DB-84-58-E1-1C-37-B1-C5-1F", "94-C1-65-83-BE-68-BD-8D-9F-29-BD-98-38-1C-16-DF" };
            yield return new object[] { 49, "LPCM 24-bit 96000Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM", "Comment=Test Sample", "", "6C-1D-22-5A-2C-06-A5-9B-D7-44-53-A3-21-58-BC-EB", "11-42-CA-BC-17-B6-3E-E8-EA-14-1B-6B-98-96-F5-A0" };
            yield return new object[] { 50, "LPCM 16-bit 44100Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM", "", "", "5A-E8-F1-F1-59-70-D0-D8-7A-A9-6C-D4-57-10-5D-AC", "F8-4F-9E-97-B5-23-64-90-CA-69-CC-E0-C9-5A-20-CD" };
            yield return new object[] { 51, "LPCM 16-bit 44100Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM;ControlMode=Variable;Quality=High;VBRQuality=9;ApplyGain=False;;AddSoundCheck=False", "Comment=Test Sample", "", "7D-7D-92-BC-FA-E2-40-15-3D-FC-5E-35-10-CF-97-5F", "31-3D-FA-2B-27-9A-29-03-A8-DD-EA-7B-CA-83-05-BB" };
            yield return new object[] { 52, "LPCM 16-bit 44100Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM;Quality=Low", "Comment=Test Sample", "", "8D-FC-48-AE-70-E7-95-EA-9C-80-61-7F-7A-D9-FD-95", "22-54-39-83-B2-92-30-FA-92-C2-BF-C2-A3-D3-A9-8C" };
            yield return new object[] { 53, "LPCM 16-bit 44100Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM;VBRQuality=14", "Comment=Test Sample", "", "A3-9C-C8-95-C9-FA-23-AC-FF-AE-79-E4-43-0F-4C-F4", "53-A8-AD-BD-63-56-B9-8E-D8-96-63-3F-3E-CD-D1-67" };
            yield return new object[] { 54, "LPCM 16-bit 44100Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM;ApplyGain=Track", "Comment=Test Sample;TrackPeak=1.000000;TrackGain=-6.58 dB", "", "0E-FC-A2-23-B7-97-B9-96-3D-A1-2B-C9-6B-51-0A-F1", "7F-05-43-91-7A-49-15-DF-69-95-85-51-3A-96-A4-D0" };
            yield return new object[] { 55, "LPCM 16-bit 44100Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM;ApplyGain=Album", "Comment=Test Sample;AlbumPeak=1.000000;AlbumGain=-6.58 dB", "", "0E-FC-A2-23-B7-97-B9-96-3D-A1-2B-C9-6B-51-0A-F1", "7F-05-43-91-7A-49-15-DF-69-95-85-51-3A-96-A4-D0" };
            yield return new object[] { 56, "LPCM 16-bit 44100Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM;AddSoundCheck=Track", "Comment=Test Sample;TrackPeak=1.000000;TrackGain=-6.58 dB", "", "71-DA-B8-D8-0A-78-C9-4B-7D-84-69-8C-BF-10-5A-74", "B6-FF-36-9F-7C-CD-8A-71-3F-38-81-69-D2-42-EA-16" };
            yield return new object[] { 57, "LPCM 16-bit 44100Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM;AddSoundCheck=Album", "Comment=Test Sample;AlbumPeak=1.000000;AlbumGain=-6.58 dB", "", "71-DA-B8-D8-0A-78-C9-4B-7D-84-69-8C-BF-10-5A-74", "B6-FF-36-9F-7C-CD-8A-71-3F-38-81-69-D2-42-EA-16" };
            yield return new object[] { 58, "LPCM 16-bit 44100Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM;BitRate=64", "Comment=Test Sample", "", "7A-C1-E5-36-8D-A3-0F-3B-55-D0-FC-A9-D5-8E-1A-E4", "2C-54-BE-41-11-F9-8C-CB-CA-BF-4B-B1-41-05-04-E5" };
            yield return new object[] { 59, "LPCM 16-bit 44100Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM;BitRate=64;ControlMode=Constrained", "Comment=Test Sample", "", "7A-C1-E5-36-8D-A3-0F-3B-55-D0-FC-A9-D5-8E-1A-E4", "2C-54-BE-41-11-F9-8C-CB-CA-BF-4B-B1-41-05-04-E5" };
            yield return new object[] { 60, "LPCM 16-bit 44100Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM;BitRate=64;ControlMode=Constant", "Comment=Test Sample", "", "65-F2-E0-8A-65-3D-84-B3-5B-67-73-A5-F5-39-6D-CD", "C4-E5-60-8F-5A-D8-51-E5-41-BB-D3-87-1B-44-27-C6" };
            yield return new object[] { 61, "LPCM 16-bit 44100Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM", "Comment=Test Sample", "TestCover.png", "7F-A5-D1-40-A6-EA-24-09-76-6A-08-53-AE-35-15-37", "73-FF-0A-39-2C-F0-FF-55-91-19-FC-20-DC-7A-4B-CF" };
            yield return new object[] { 62, "LPCM 16-bit 44100Hz Stereo.wav", "Apple AAC", "CreationTime=2014-01-01 12:00:00 AM", "Comment=Test Sample", "TestCover.jpg", "1C-2B-F5-60-BC-DB-95-CF-15-94-D3-9A-9E-1A-8E-7F", "E9-85-39-C3-04-F7-94-15-29-98-09-7C-CF-7F-E2-5F" };
            yield return new object[] { 63, "LPCM 16-bit 44100Hz Stereo.wav", "Apple Lossless", "CreationTime=2014-01-01 12:00:00 AM", "Comment=Test Sample", "", "DD-DB-A3-C3-8B-EA-A4-F9-32-07-9D-8C-86-C0-51-BE", "DD-DB-A3-C3-8B-EA-A4-F9-32-07-9D-8C-86-C0-51-BE" };
            yield return new object[] { 64, "LPCM 16-bit 44100Hz Mono.wav", "Apple Lossless", "CreationTime=2014-01-01 12:00:00 AM", "Comment=Test Sample", "", "81-D1-C9-6C-0E-F7-B0-44-2E-D1-86-11-42-BA-7F-A3", "81-D1-C9-6C-0E-F7-B0-44-2E-D1-86-11-42-BA-7F-A3" };
            yield return new object[] { 65, "LPCM 16-bit 48000Hz Stereo.wav", "Apple Lossless", "CreationTime=2014-01-01 12:00:00 AM", "Comment=Test Sample", "", "6D-12-21-6B-BB-17-01-7B-43-9E-54-E0-77-43-B6-24", "6D-12-21-6B-BB-17-01-7B-43-9E-54-E0-77-43-B6-24" };
            yield return new object[] { 66, "LPCM 24-bit 96000Hz Stereo.wav", "Apple Lossless", "CreationTime=2014-01-01 12:00:00 AM", "Comment=Test Sample", "", "8F-A9-71-E0-19-D3-F1-C3-AE-50-7A-9A-1B-05-30-1C", "8F-A9-71-E0-19-D3-F1-C3-AE-50-7A-9A-1B-05-30-1C" };
            yield return new object[] { 67, "LPCM 16-bit 44100Hz Stereo.wav", "Apple Lossless", "CreationTime=2014-01-01 12:00:00 AM", "", "", "42-17-BA-C7-E0-E1-28-C8-27-1D-C3-DF-37-70-72-FF", "42-17-BA-C7-E0-E1-28-C8-27-1D-C3-DF-37-70-72-FF" };
            yield return new object[] { 68, "LPCM 16-bit 44100Hz Stereo.wav", "Apple Lossless", "CreationTime=2014-01-01 12:00:00 AM;AddSoundCheck=False", "Comment=Test Sample", "", "DD-DB-A3-C3-8B-EA-A4-F9-32-07-9D-8C-86-C0-51-BE", "DD-DB-A3-C3-8B-EA-A4-F9-32-07-9D-8C-86-C0-51-BE" };
            yield return new object[] { 69, "LPCM 16-bit 44100Hz Stereo.wav", "Apple Lossless", "CreationTime=2014-01-01 12:00:00 AM;AddSoundCheck=Track", "Comment=Test Sample;TrackPeak=1.000000;TrackGain=-6.58 dB", "", "4B-19-31-AF-AF-FD-A8-85-43-24-72-E8-56-BB-22-E3", "4B-19-31-AF-AF-FD-A8-85-43-24-72-E8-56-BB-22-E3" };
            yield return new object[] { 70, "LPCM 16-bit 44100Hz Stereo.wav", "Apple Lossless", "CreationTime=2014-01-01 12:00:00 AM;AddSoundCheck=Album", "Comment=Test Sample;AlbumPeak=1.000000;AlbumGain=-6.58 dB", "", "4B-19-31-AF-AF-FD-A8-85-43-24-72-E8-56-BB-22-E3", "4B-19-31-AF-AF-FD-A8-85-43-24-72-E8-56-BB-22-E3" };
            yield return new object[] { 71, "LPCM 16-bit 44100Hz Stereo.wav", "Apple Lossless", "CreationTime=2014-01-01 12:00:00 AM", "Comment=Test Sample", "TestCover.png", "91-59-10-59-52-F4-62-A4-C9-5A-40-B7-F1-52-16-3D", "91-59-10-59-52-F4-62-A4-C9-5A-40-B7-F1-52-16-3D" };
            yield return new object[] { 72, "LPCM 16-bit 44100Hz Stereo.wav", "Apple Lossless", "CreationTime=2014-01-01 12:00:00 AM", "Comment=Test Sample", "TestCover.jpg", "24-ED-FC-DF-40-F8-69-55-F8-9C-A2-B8-A7-BA-52-FB", "24-ED-FC-DF-40-F8-69-55-F8-9C-A2-B8-A7-BA-52-FB" };
            yield return new object[] { 73, "LPCM 8-bit 8000Hz Stereo.wav", "Ogg Vorbis", "SerialNumber=1", "Comment=Test Sample", "", "83-C2-62-96-A3-D3-FE-82-1C-3D-9A-88-B7-54-22-24", "83-C2-62-96-A3-D3-FE-82-1C-3D-9A-88-B7-54-22-24" };
            yield return new object[] { 74, "LPCM 16-bit 44100Hz Stereo.wav", "Ogg Vorbis", "SerialNumber=1", "Comment=Test Sample", "", "04-13-96-50-CE-90-41-D5-58-8C-42-BE-F0-71-24-AF", "04-13-96-50-CE-90-41-D5-58-8C-42-BE-F0-71-24-AF" };
            yield return new object[] { 75, "LPCM 16-bit 44100Hz Mono.wav", "Ogg Vorbis", "SerialNumber=1", "Comment=Test Sample", "", "02-35-F2-8D-71-06-DE-1D-19-CD-B3-77-F9-57-66-E5", "02-35-F2-8D-71-06-DE-1D-19-CD-B3-77-F9-57-66-E5" };
            yield return new object[] { 76, "LPCM 16-bit 48000Hz Stereo.wav", "Ogg Vorbis", "SerialNumber=1", "Comment=Test Sample", "", "D2-A4-D6-D5-34-71-FA-00-CF-D7-1A-3B-0F-34-9E-0D", "D2-A4-D6-D5-34-71-FA-00-CF-D7-1A-3B-0F-34-9E-0D" };
            yield return new object[] { 77, "LPCM 24-bit 96000Hz Stereo.wav", "Ogg Vorbis", "SerialNumber=1", "Comment=Test Sample", "", "22-E7-7C-6C-2D-A7-73-E8-67-30-1C-07-9A-E2-54-05", "22-E7-7C-6C-2D-A7-73-E8-67-30-1C-07-9A-E2-54-05" };
            yield return new object[] { 78, "LPCM 16-bit 44100Hz Stereo.wav", "Ogg Vorbis", "SerialNumber=1", "", "", "C5-F9-6A-75-5F-F3-C1-2B-81-55-28-C6-A3-9D-44-F7", "C5-F9-6A-75-5F-F3-C1-2B-81-55-28-C6-A3-9D-44-F7" };
            yield return new object[] { 79, "LPCM 16-bit 44100Hz Stereo.wav", "Ogg Vorbis", "SerialNumber=1;ControlMode=Variable;VBRQuality=5;ApplyGain=False", "Comment=Test Sample", "", "04-13-96-50-CE-90-41-D5-58-8C-42-BE-F0-71-24-AF", "04-13-96-50-CE-90-41-D5-58-8C-42-BE-F0-71-24-AF" };
            yield return new object[] { 80, "LPCM 16-bit 44100Hz Stereo.wav", "Ogg Vorbis", "SerialNumber=1;VBRQuality=-1", "Comment=Test Sample", "", "AD-C0-18-B7-6A-9D-57-2D-B3-CC-47-0C-5F-72-B3-E7", "AD-C0-18-B7-6A-9D-57-2D-B3-CC-47-0C-5F-72-B3-E7" };
            yield return new object[] { 81, "LPCM 16-bit 44100Hz Stereo.wav", "Ogg Vorbis", "SerialNumber=1;ApplyGain=Track", "Comment=Test Sample;TrackPeak=1.000000;TrackGain=-6.58 dB", "", "AB-7F-11-9B-07-8C-DE-4C-B6-65-BB-5A-D5-C5-26-AF", "AB-7F-11-9B-07-8C-DE-4C-B6-65-BB-5A-D5-C5-26-AF" };
            yield return new object[] { 82, "LPCM 16-bit 44100Hz Stereo.wav", "Ogg Vorbis", "SerialNumber=1;ApplyGain=Album", "Comment=Test Sample;AlbumPeak=1.000000;AlbumGain=-6.58 dB", "", "25-2D-74-6E-DF-FD-CD-72-A7-8E-F0-6B-72-1C-9E-E8", "25-2D-74-6E-DF-FD-CD-72-A7-8E-F0-6B-72-1C-9E-E8" };
            yield return new object[] { 83, "LPCM 16-bit 44100Hz Stereo.wav", "Ogg Vorbis", "SerialNumber=1;BitRate=32", "Comment=Test Sample", "", "2A-FF-CC-FF-D2-9A-78-89-D3-D7-66-2E-1B-0F-5F-BA", "2A-FF-CC-FF-D2-9A-78-89-D3-D7-66-2E-1B-0F-5F-BA" };
            yield return new object[] { 84, "LPCM 16-bit 44100Hz Stereo.wav", "Ogg Vorbis", "SerialNumber=1;BitRate=32;ControlMode=Constant", "Comment=Test Sample", "", "97-05-F5-43-96-E2-B1-22-7D-3F-79-82-5D-9D-A8-75", "97-05-F5-43-96-E2-B1-22-7D-3F-79-82-5D-9D-A8-75" };
            yield return new object[] { 85, "LPCM 16-bit 44100Hz Stereo.wav", "Ogg Vorbis", "SerialNumber=1", "Comment=Test Sample", "TestCover.png", "01-24-21-B5-79-56-6B-85-27-75-58-53-F3-26-DE-1A", "01-24-21-B5-79-56-6B-85-27-75-58-53-F3-26-DE-1A" };
            yield return new object[] { 86, "LPCM 16-bit 44100Hz Stereo.wav", "Ogg Vorbis", "SerialNumber=1", "Comment=Test Sample", "TestCover.jpg", "6D-CF-EE-30-29-BF-1C-94-EE-D0-5A-32-3D-53-F3-62", "6D-CF-EE-30-29-BF-1C-94-EE-D0-5A-32-3D-53-F3-62" };
        }

        [Theory(DisplayName = "Export"), MemberData(nameof(GetExportDataSet))]
        public void AudioFileExport(int index, string fileName, string encoder, string settings, string metadata,
            string coverArt, string expectedHash32, string expectedHash64)
        {
            var expectedHash = Environment.Is64BitProcess ? expectedHash64 : expectedHash32;

            if (!string.IsNullOrEmpty(expectedHash))
            {
                var input = new ExportableAudioFile(
                    new FileInfo(Path.Combine(_testFilesFixture.WorkingDirectory, "TestFiles", fileName)));
                ConvertToDictionary(metadata).CopyTo(input.Metadata);
                if (!string.IsNullOrEmpty(coverArt))
                    input.Metadata.CoverArt =
                        new CoverArt(new FileInfo(Path.Combine(_testFilesFixture.WorkingDirectory,
                            "TestFiles", coverArt)));
                var result = input.Export(encoder, ConvertToDictionary(settings),
                    new DirectoryInfo(_testFilesFixture.WorkingDirectory),
                    $"Export Row {index:00}", true);

                Assert.Equal(expectedHash, CalculateHash(result));
            }
        }

        public static IEnumerable<object[]> GetAnalyzeDataSet()
        {
            yield return new object[] { 0, "LPCM 8-bit 8000Hz Stereo.wav", "ReplayGain", "TrackPeak=0.976563;TrackGain=-7.60 dB;AlbumPeak=0.976563;AlbumGain=-7.60 dB" };
            yield return new object[] { 1, "LPCM 16-bit 44100Hz Stereo.wav", "ReplayGain", "TrackPeak=1.000000;TrackGain=-6.58 dB;AlbumPeak=1.000000;AlbumGain=-6.58 dB" };
            yield return new object[] { 2, "LPCM 16-bit 44100Hz Mono.wav", "ReplayGain", "TrackPeak=1.000000;TrackGain=-11.05 dB;AlbumPeak=1.000000;AlbumGain=-11.05 dB" };
            yield return new object[] { 3, "LPCM 16-bit 48000Hz Stereo.wav", "ReplayGain", "TrackPeak=0.999970;TrackGain=-6.46 dB;AlbumPeak=0.999970;AlbumGain=-6.46 dB" };
            yield return new object[] { 4, "LPCM 24-bit 96000Hz Stereo.wav", "ReplayGain", "TrackPeak=0.988553;TrackGain=-6.47 dB;AlbumPeak=0.988553;AlbumGain=-6.47 dB" };
            yield return new object[] { 5, "LPCM 8-bit 8000Hz Stereo.wav", "ReplayGain 2.0", "TrackPeak=0.976563;TrackGain=-8.89 dB;AlbumPeak=0.976563;AlbumGain=-8.89 dB" };
            yield return new object[] { 6, "LPCM 16-bit 44100Hz Stereo.wav", "ReplayGain 2.0", "TrackPeak=1.000000;TrackGain=-8.68 dB;AlbumPeak=1.000000;AlbumGain=-8.68 dB" };
            yield return new object[] { 7, "LPCM 16-bit 44100Hz Mono.wav", "ReplayGain 2.0", "TrackPeak=1.000000;TrackGain=-9.79 dB;AlbumPeak=1.000000;AlbumGain=-9.79 dB" };
            yield return new object[] { 8, "LPCM 16-bit 48000Hz Stereo.wav", "ReplayGain 2.0", "TrackPeak=0.999969;TrackGain=-8.67 dB;AlbumPeak=0.999969;AlbumGain=-8.67 dB" };
            yield return new object[] { 9, "LPCM 24-bit 96000Hz Stereo.wav", "ReplayGain 2.0", "TrackPeak=0.988553;TrackGain=-8.64 dB;AlbumPeak=0.988553;AlbumGain=-8.64 dB" };
        }

        [Theory(DisplayName = "Analyze"), MemberData(nameof(GetAnalyzeDataSet))]
        public void AudioFileAnalyze(int index, string fileName, string analyzer, string expectedMetadata)
        {
            var input = new AnalyzableAudioFile(new FileInfo(
                Path.Combine(_testFilesFixture.WorkingDirectory, "TestFiles", fileName)));
            input.Analyze(analyzer);

            Assert.Equal(expectedMetadata, ConvertToString(input.Metadata));
        }

        public static IEnumerable<object[]> GetSaveMetadataDataSet()
        {
            yield return new object[] { 0, "FLAC 16-bit 44100Hz Stereo.flac", "", "", "", "EE-9C-D0-11-02-C4-36-98-A5-6A-A5-89-90-96-DF-8F" };
            yield return new object[] { 1, "FLAC 16-bit 44100Hz Stereo.flac", "", "Comment=Test Sample", "", "0A-35-A5-06-02-E5-FA-C3-4A-2D-DE-AD-09-E5-82-E8" };
            yield return new object[] { 2, "FLAC 16-bit 44100Hz Stereo.flac", "UsePadding=False", "Comment=Test Sample", "", "0A-35-A5-06-02-E5-FA-C3-4A-2D-DE-AD-09-E5-82-E8" };
            yield return new object[] { 3, "MP3 213kbps 44100Hz Stereo.mp3", "", "", "", "E7-CE-BD-CC-18-5B-EF-4C-8C-14-E5-FF-9B-AF-B8-86" };
            yield return new object[] { 4, "MP3 213kbps 44100Hz Stereo.mp3", "", "Comment=Test Sample", "", "81-D9-6C-E4-77-CE-F4-0C-F4-AC-DB-BE-1F-F1-D8-EC" };
            yield return new object[] { 5, "MP3 213kbps 44100Hz Stereo.mp3", "AddSoundCheck=False;ID3Version=2.3;PaddingSize=0;UsePadding=False", "Comment=Test Sample", "", "81-D9-6C-E4-77-CE-F4-0C-F4-AC-DB-BE-1F-F1-D8-EC" };
            yield return new object[] { 6, "MP3 213kbps 44100Hz Stereo.mp3", "ID3Version=2.4", "Comment=Test Sample", "", "DA-90-0D-DD-7D-B6-CB-15-C1-C6-47-01-0A-51-73-D1" };
            yield return new object[] { 7, "MP3 213kbps 44100Hz Stereo.mp3", "PaddingSize=1024", "Comment=Test Sample", "", "6A-17-57-D5-C3-52-73-99-E7-21-CB-A8-37-A2-D3-3C" };
            yield return new object[] { 8, "MP3 213kbps 44100Hz Stereo.mp3", "AddSoundCheck=Track", "Comment=Test Sample;TrackPeak=1.000000;TrackGain=-6.58 dB", "", "FB-6C-B8-07-7A-92-5F-7A-52-46-0D-63-02-D8-60-C9" };
            yield return new object[] { 9, "MP3 213kbps 44100Hz Stereo.mp3", "AddSoundCheck=Album", "Comment=Test Sample;AlbumPeak=1.000000;AlbumGain=-6.58 dB", "", "AD-08-7C-13-3F-8E-DD-64-41-F3-D8-F7-42-44-43-11" };
            yield return new object[] { 10, "MP3 213kbps 44100Hz Stereo.mp3", "", "Comment=Test Sample", "TestCover.png", "37-32-84-54-D3-77-1B-A0-65-49-16-91-32-86-E3-9D" };
            yield return new object[] { 11, "MP3 213kbps 44100Hz Stereo.mp3", "", "Comment=Test Sample", "TestCover.jpg", "86-EA-B2-5F-AC-38-2B-46-DE-B4-18-0D-1B-B4-F7-B1" };
            yield return new object[] { 12, "AAC 153 kbps 44100Hz Stereo.m4a", "", "", "", "DB-2C-A6-C5-7E-B5-27-19-5E-CF-AF-D3-4C-88-33-57" };
            yield return new object[] { 13, "AAC 153 kbps 44100Hz Stereo.m4a", "", "Comment=Test Sample", "", "58-1E-6F-22-02-FB-49-C8-55-68-26-20-9D-42-42-54" };
            yield return new object[] { 14, "AAC 153 kbps 44100Hz Stereo.m4a", "CreationTime=2014-01-01 12:00:00 AM;AddSoundCheck=False", "Comment=Test Sample", "", "58-1E-6F-22-02-FB-49-C8-55-68-26-20-9D-42-42-54" };
            yield return new object[] { 15, "AAC 153 kbps 44100Hz Stereo.m4a", "AddSoundCheck=Track", "Comment=Test Sample;TrackPeak=1.000000;TrackGain=-6.58 dB", "", "ED-3C-02-B6-2C-13-D1-CF-9D-5A-B2-0F-5F-F6-C2-56" };
            yield return new object[] { 16, "AAC 153 kbps 44100Hz Stereo.m4a", "AddSoundCheck=Album", "Comment=Test Sample;AlbumPeak=1.000000;AlbumGain=-6.58 dB", "", "ED-3C-02-B6-2C-13-D1-CF-9D-5A-B2-0F-5F-F6-C2-56" };
            yield return new object[] { 17, "AAC 153 kbps 44100Hz Stereo.m4a", "", "Comment=Test Sample", "TestCover.png", "2D-CF-25-6A-D6-C1-DC-9F-81-AD-9D-73-84-B2-96-E2" };
            yield return new object[] { 18, "AAC 153 kbps 44100Hz Stereo.m4a", "", "Comment=Test Sample", "TestCover.jpg", "88-F6-2A-55-76-B5-9B-3D-95-88-E1-DD-6D-46-35-FF" };
            yield return new object[] { 19, "ALAC 16-bit 44100Hz Stereo.m4a", "", "", "", "42-17-BA-C7-E0-E1-28-C8-27-1D-C3-DF-37-70-72-FF" };
            yield return new object[] { 20, "ALAC 16-bit 44100Hz Stereo.m4a", "", "Comment=Test Sample", "", "DD-DB-A3-C3-8B-EA-A4-F9-32-07-9D-8C-86-C0-51-BE" };
            yield return new object[] { 21, "ALAC 16-bit 44100Hz Stereo.m4a", "CreationTime=2014-01-01 12:00:00 AM;AddSoundCheck=False", "Comment=Test Sample", "", "DD-DB-A3-C3-8B-EA-A4-F9-32-07-9D-8C-86-C0-51-BE" };
            yield return new object[] { 22, "ALAC 16-bit 44100Hz Stereo.m4a", "AddSoundCheck=Track", "Comment=Test Sample;TrackPeak=1.000000;TrackGain=-6.58 dB", "", "4B-19-31-AF-AF-FD-A8-85-43-24-72-E8-56-BB-22-E3" };
            yield return new object[] { 23, "ALAC 16-bit 44100Hz Stereo.m4a", "AddSoundCheck=Album", "Comment=Test Sample;AlbumPeak=1.000000;AlbumGain=-6.58 dB", "", "4B-19-31-AF-AF-FD-A8-85-43-24-72-E8-56-BB-22-E3" };
            yield return new object[] { 24, "ALAC 16-bit 44100Hz Stereo.m4a", "", "Comment=Test Sample", "TestCover.png", "91-59-10-59-52-F4-62-A4-C9-5A-40-B7-F1-52-16-3D" };
            yield return new object[] { 25, "ALAC 16-bit 44100Hz Stereo.m4a", "", "Comment=Test Sample", "TestCover.jpg", "24-ED-FC-DF-40-F8-69-55-F8-9C-A2-B8-A7-BA-52-FB" };
            yield return new object[] { 26, "Vorbis 160kbps 44100Hz Stereo.ogg", "", "", "", "C5-F9-6A-75-5F-F3-C1-2B-81-55-28-C6-A3-9D-44-F7" };
            yield return new object[] { 27, "Vorbis 160kbps 44100Hz Stereo.ogg", "", "Comment=Test Sample", "", "04-13-96-50-CE-90-41-D5-58-8C-42-BE-F0-71-24-AF" };
            yield return new object[] { 28, "Vorbis 160kbps 44100Hz Stereo.ogg", "", "Comment=Test Sample", "TestCover.png", "01-24-21-B5-79-56-6B-85-27-75-58-53-F3-26-DE-1A" };
            yield return new object[] { 29, "Vorbis 160kbps 44100Hz Stereo.ogg", "", "Comment=Test Sample", "TestCover.jpg", "6D-CF-EE-30-29-BF-1C-94-EE-D0-5A-32-3D-53-F3-62" };
        }

        [Theory(DisplayName = "SaveMetadata"), MemberData(nameof(GetSaveMetadataDataSet))]
        public void AudioFileSaveMetadata(int index, string fileName, string settings, string metadata, string coverArt,
            string expectedHash)
        {
            var input = new TaggedAudioFile(new FileInfo(Path.Combine(_testFilesFixture.WorkingDirectory, "TestFiles", fileName)).CopyTo("Save Metadata Row " + index + Path.GetExtension(fileName), true));
            ConvertToDictionary(metadata).CopyTo(input.Metadata);
            if (!string.IsNullOrEmpty(coverArt))
                input.Metadata.CoverArt = new CoverArt(new FileInfo(Path.Combine(_testFilesFixture.WorkingDirectory, "TestFiles", coverArt)));
            input.SaveMetadata(ConvertToDictionary(settings));

            Assert.Equal(expectedHash, CalculateHash(input));
        }

        [Pure, NotNull]
        static SettingsDictionary ConvertToDictionary([NotNull] string settings)
        {
            var result = new SettingsDictionary();

            foreach (string item in settings.Split(';'))
            {
                string[] keyAndValue = item.Split('=');
                if (keyAndValue.Length == 2)
                    result.Add(keyAndValue[0], keyAndValue[1]);
            }

            return result;
        }

        [Pure, NotNull]
        static string ConvertToString([NotNull] SettingsDictionary settings)
        {
            var result = new StringBuilder();

            foreach (var item in settings)
            {
                if (result.Length > 0)
                    result.Append(';');
                result.Append(item.Key);
                result.Append('=');
                result.Append(item.Value);
            }

            return result.ToString();
        }

        [Pure, NotNull]
        static string CalculateHash([NotNull] AudioFile audioFile)
        {
            using (MD5 md5 = MD5.Create())
            using (FileStream fileStream = audioFile.FileInfo.OpenRead())
            {
                byte[] hashBytes = md5.ComputeHash(fileStream);
                return BitConverter.ToString(hashBytes);
            }
        }
    }
}
