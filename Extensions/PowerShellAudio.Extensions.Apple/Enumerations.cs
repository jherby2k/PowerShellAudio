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

using System;

namespace PowerShellAudio.Extensions.Apple
{
    enum AudioFileStatus
    {
        Ok                              = 0,
        UnspecifiedError                = 0x7768743f, // 'wht?'
        UnsupportedFileTypeError        = 0x7479703f, // 'typ?'
        UnsupportedDataFormatError      = 0x666d743f, // 'fmt?'
        UnsupportedPropertyError        = 0x7074793f, // 'pty?'
        BadPropertySizeError            = 0x2173697a, // '!siz'
        PermissionsError                = 0x70726d3f, // 'prm?'
        NotOptimizedError               = 0x6f70746d, // 'optm'
        InvalidChunkError               = 0x63686b3f, // 'chk?'
        DoesNotAllow64BitDataSizeError  = 0x6f66663f, // 'off?'
        InvalidPacketOffsetError        = 0x70636b3f, // 'pck?'
        InvalidFileError                = 0x6474613f, // 'dta?'
        OperationNotSupportedError      = 0x6f703f3f, // 'op??'
        NotOpenError                    = -38,
        EndOfFileError                  = -39,
        PositionError                   = -40,
        FileNotFoundError               = -43
    }

    enum AudioFilePropertyId
    {
        DataFormat              = 0x64666d74, // 'dfmt'
        MagicCookieData         = 0x6d676963, // 'mgic'
        PacketSizeUpperBound    = 0x706b7562  // 'pkub'
    }

    enum ExtendedAudioFileStatus
    {
        Ok                          = 0,
        InvalidProperty             = -66561,
        InvalidPropertySize         = -66562,
        NonPcmClientFormat          = -66563,
        InvalidChannelMap           = -66564,
        InvalidOperationOrder       = -665665,
        InvalidDataFormat           = -66566,
        MaxPacketSizeUnknown        = -66567,
        InvalidSeek                 = -66568,
        AsyncWriteTooLarge          = -66569,
        AsyncWriteBufferOverflow    = -66570
    }

    enum ExtendedAudioFilePropertyId
    {
        AudioConverter      = 0x61636e76, // 'acnv'
        ClientDataFormat    = 0x63666d74, // 'cfmt'
        ConverterConfig     = 0x61636366 // 'accf'
    }

    enum AudioConverterStatus
    {
        Ok                              = 0,
        FormatNotSupported              = 0x666d743f, // 'fmt?'
        OperationNotSupported           = 0x6f703f3f, // 'op??'
        PropertyNotSupported            = 0x70726f70, // 'prop'
        InvalidInputSize                = 0x696e737a, // 'insz'
        InvalidOutputSize               = 0x6f74737a, // 'otsz'
        UnspecifiedError                = 0x77686174, // 'what'
        BadPropertySizeError            = 0x2173697a, // '!siz'
        RequiresPacketDescriptionsError = 0x21706b64, // '!pkd'
        InputSampleRateOutOfRange       = 0x21697372, // '!isr'
        OutputSampleRateOutOfRange      = 0x216f7372  // '!osr'
    }

    enum AudioFormat : uint
    {
        AacLowComplexity    = 0x61616320, // "aac "
        AppleLossless       = 0x616c6163, // "alac"
        LinearPcm           = 0x6c70636d  // "lpcm"
    }

    [Flags]
    enum AudioFormatFlags : uint
    {
        PcmIsSignedInteger  = 0x4,
        PcmIsPacked         = 0x8,

        Alac16BitSourceData = 1,
        Alac20BitSourceData = 2,
        Alac24BitSourceData = 3,
        Alac32BitSourceData = 4
    }

    enum AudioConverterPropertyId : uint
    {
        CodecQuality                = 0x63647175, // 'cdqu'
        DecompressionMagicCookie    = 0x646d6763, // 'dmgc'
        BitRate                     = 0x62726174, // 'brat'
        BitRateControlMode          = 0x61636266, // 'acbf'
        VbrQuality                  = 0x76627271, // 'vbrq'
    }

    enum BitrateControlMode : uint
    {
        Constant            = 0,
        LongTermAverage     = 1,
        VariableConstrained = 2,
        Variable            = 3
    }

    enum Quality : uint
    {
        Low     = 0x20,
        Medium  = 0x40,
        High    = 0x60,
    }

    enum AudioFileType : uint
    {
        M4A = 0x6d346166 // 'm4af'
    }
}