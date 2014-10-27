/*
 * Copyright © 2014 Jeremy Herbison
 * 
 * This file is part of AudioShell.
 * 
 * AudioShell is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option)
 * any later version.
 * 
 * AudioShell is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for more
 * details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with AudioShell.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

namespace AudioShell.Extensions.Flac
{
    internal enum DecoderInitStatus
    {
        OK,
        UnsupportedContainer,
        InvalidCallbacks,
        ErrorAllocatingMemory,
        ErrorOpeningFile,
        AlreadyInitialized
    };

    internal enum DecoderState
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
    };

    internal enum DecoderReadStatus
    {
        Continue,
        EndOfStream,
        Abort
    };

    internal enum DecoderSeekStatus
    {
        OK,
        Error,
        Unsupported
    };

    internal enum DecoderTellStatus
    {
        OK,
        Error,
        Unsupported
    };

    internal enum DecoderLengthStatus
    {
        OK,
        Error,
        Unsupported
    };

    internal enum DecoderWriteStatus
    {
        Continue,
        Abort
    };

    internal enum DecoderErrorStatus
    {
        LostSync,
        BadHeader,
        FrameCrcMismatch,
        UnparseableStream
    };

    internal enum ChannelAssignment
    {
        Independent,
        LeftAndSide,
        RightAndSide,
        MidAndSide
    };

    internal enum FrameNumberType
    {
        FrameNumber,
        SampleNumber
    };

    internal enum MetadataType
    {
        StreamInfo,
        Padding,
        Application,
        SeekTable,
        VorbisComment,
        CueSheet,
        Picture,
        Undefined
    };

    internal enum MetadataChainStatus
    {
        OK,
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
    };

    internal enum EncoderWriteStatus
    {
        OK,
        FatalError
    };

    internal enum EncoderSeekStatus
    {
        OK,
        Error,
        Unsupported
    };

    internal enum EncoderTellStatus
    {
        OK,
        Error,
        Unsupported
    };

    internal enum EncoderInitStatus
    {
        OK,
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
    };

    internal enum EncoderState
    {
        OK,
        Uninitialized,
        OggError,
        DecoderError,
        AudioDataMismatch,
        ClientError,
        IOError,
        FramingError,
        MemoryAllocationError
    };
}