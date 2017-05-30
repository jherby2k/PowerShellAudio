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

namespace PowerShellAudio.Extensions.Flac
{
    enum DecoderInitStatus
    {
        Ok,
        UnsupportedContainer,
        InvalidCallbacks,
        ErrorAllocatingMemory,
        ErrorOpeningFile,
        AlreadyInitialized
    }

    enum DecoderState
    {
        SearchForMetadata,
        ReadMetadata,
        SearchForFrameSync,
        ReadFrame,
        EndOfStream,
        OggError,
        SeekError,
        Aborted,
        MemoryAllocationError,
        Uninitialized
    }

    enum DecoderReadStatus
    {
        Continue,
        EndOfStream,
        Abort
    }

    enum DecoderSeekStatus
    {
        Ok,
        Error,
        Unsupported
    }

    enum DecoderTellStatus
    {
        Ok,
        Error,
        Unsupported
    }

    enum DecoderLengthStatus
    {
        Ok,
        Error,
        Unsupported
    }

    enum DecoderWriteStatus
    {
        Continue,
        Abort
    }

    enum DecoderErrorStatus
    {
        LostSync,
        BadHeader,
        FrameCrcMismatch,
        UnparseableStream
    }

    enum ChannelAssignment
    {
        Independent,
        LeftAndSide,
        RightAndSide,
        MidAndSide
    }

    enum FrameNumberType
    {
        FrameNumber,
        SampleNumber
    }

    enum MetadataType
    {
        StreamInfo,
        Padding,
        Application,
        SeekTable,
        VorbisComment,
        CueSheet,
        Picture,
        Undefined
    }

    enum PictureType : uint
    {
        Other,
        PngIcon,
        OtherIcon,
        CoverFront,
        CoverBack,
        Leaflet,
        Media,
        LeadArtist,
        Artist,
        Conductor,
        Band,
        Composer,
        Lyricist,
        Location,
        DuringRecording,
        DuringPerformance,
        ScreenCapture,
        BrightFish,
        Illustration,
        ArtistLogo,
        PublisherLogo
    }

    enum MetadataChainStatus
    {
        Ok,
        IllegalInput,
        ErrorOpeningFile,
        NotAFlacFile,
        NotWritable,
        BadMetadata,
        ReadError,
        SeekError,
        WriteError,
        RenameError,
        UnlinkError,
        MemoryAllocationError,
        InternalError,
        InvalidCallbacks,
        ReadWriteMismatch,
        WrongWriteCall
    }

    enum EncoderWriteStatus
    {
        Ok,
        FatalError
    }

    enum EncoderSeekStatus
    {
        Ok,
        Error,
        Unsupported
    }

    enum EncoderTellStatus
    {
        Ok,
        Error,
        Unsupported
    }

    enum EncoderInitStatus
    {
        Ok,
        EncoderError,
        UnsupportedContainer,
        InvalidCallbacks,
        InvalidNumberOfChannels,
        InvalidBitsPerSample,
        InvalidSampleRate,
        InvalidBlockSize,
        InvalidMaxLpcOrder,
        InvalidQlpCoefficientPrecision,
        BlockSizeTooSmallForLpcOrder,
        NotStreamable,
        InvalidMetaData
    }

    enum EncoderState
    {
        Ok,
        Uninitialized,
        OggError,
        DecoderError,
        AudioDataMismatch,
        ClientError,
        IOError,
        FramingError,
        MemoryAllocationError
    }
}