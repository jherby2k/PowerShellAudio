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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace PowerShellAudio.Extensions.Flac
{
    [SuppressUnmanagedCodeSecurity]
    static class SafeNativeMethods
    {
        const string _kernelLibrary = "kernel32.dll";
        const string _flacLibrary = "libFLAC.dll";

        static SafeNativeMethods()
        {
            // Select an architecture-appropriate libFLAC.dll by prefixing the PATH variable:
            var newPath = new StringBuilder(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            newPath.Append(Path.DirectorySeparatorChar);
            newPath.Append(Environment.Is64BitProcess ? "x64" : "x86");
            newPath.Append(Path.PathSeparator);
            newPath.Append(Environment.GetEnvironmentVariable("PATH"));

            Environment.SetEnvironmentVariable("PATH", newPath.ToString());
        }

        internal static string GetVersion()
        {
            var requiresFreeing = false;
            IntPtr module = GetModuleHandle(_flacLibrary);

            try
            {
                // If the module isn't already loaded, load it:
                if (module == IntPtr.Zero)
                {
                    module = LoadLibrary(_flacLibrary);
                    requiresFreeing = true;
                }

                return Marshal.PtrToStringAnsi(Marshal.PtrToStructure<IntPtr>(GetProcAddress(module, "FLAC__VENDOR_STRING")));
            }
            finally
            {
                if (requiresFreeing)
                    FreeLibrary(module);
            }
        }

        #region Stream Decoder

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_decoder_new", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NativeStreamDecoderHandle StreamDecoderNew();

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_decoder_set_metadata_respond", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool StreamDecoderSetMetadataRespond(NativeStreamDecoderHandle handle, MetadataType metadataType);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_decoder_init_stream", CallingConvention = CallingConvention.Cdecl)]
        internal static extern DecoderInitStatus StreamDecoderInitialize(NativeStreamDecoderHandle handle, StreamDecoderReadCallback readCallback, StreamDecoderSeekCallback seekCallback, StreamDecoderTellCallback tellCallback, StreamDecoderLengthCallback lengthCallback, StreamDecoderEofCallback eofCallback, StreamDecoderWriteCallback writeCallback, StreamDecoderMetadataCallback metadataCallback, StreamDecoderErrorCallback errorCallback, IntPtr userData);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_decoder_process_until_end_of_metadata", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool StreamDecoderProcessMetadata(NativeStreamDecoderHandle handle);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_decoder_process_single", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool StreamDecoderProcessSingle(NativeStreamDecoderHandle handle);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_decoder_get_state", CallingConvention = CallingConvention.Cdecl)]
        internal static extern DecoderState StreamDecoderGetState(NativeStreamDecoderHandle handle);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_decoder_finish", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool StreamDecoderFinish(NativeStreamDecoderHandle handle);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_decoder_delete", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void StreamDecoderDelete(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate DecoderReadStatus StreamDecoderReadCallback(IntPtr handle, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]byte[] buffer, ref int bytes, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate DecoderSeekStatus StreamDecoderSeekCallback(IntPtr handle, ulong absoluteOffset, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate DecoderTellStatus StreamDecoderTellCallback(IntPtr handle, out ulong absoluteOffset, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate DecoderLengthStatus StreamDecoderLengthCallback(IntPtr handle, out ulong streamLength, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool StreamDecoderEofCallback(IntPtr handle, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate DecoderWriteStatus StreamDecoderWriteCallback(IntPtr handle, ref Frame frame, IntPtr buffer, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void StreamDecoderMetadataCallback(IntPtr handle, IntPtr metadataBlock, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void StreamDecoderErrorCallback(IntPtr handle, DecoderErrorStatus error, IntPtr userData);

        #endregion

        #region Stream Encoder

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_encoder_new", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NativeStreamEncoderHandle StreamEncoderNew();

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_encoder_set_channels", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern void StreamEncoderSetChannels(NativeStreamEncoderHandle handle, uint channels);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_encoder_set_bits_per_sample", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern void StreamEncoderSetBitsPerSample(NativeStreamEncoderHandle handle, uint bitsPerSample);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_encoder_set_sample_rate", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern void StreamEncoderSetSampleRate(NativeStreamEncoderHandle handle, uint sampleRate);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_encoder_set_total_samples_estimate", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern void StreamEncoderSetTotalSamplesEstimate(NativeStreamEncoderHandle handle, ulong totalSamples);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_encoder_set_compression_level", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern void StreamEncoderSetCompressionLevel(NativeStreamEncoderHandle handle, uint compressionLevel);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_encoder_set_metadata", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool StreamEncoderSetMetadata(NativeStreamEncoderHandle handle, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]IntPtr[] metaData, uint blocks);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_encoder_init_stream", CallingConvention = CallingConvention.Cdecl)]
        internal static extern EncoderInitStatus StreamEncoderInitialize(NativeStreamEncoderHandle handle, StreamEncoderWriteCallback writeCallback, StreamEncoderSeekCallback seekCallback, StreamEncoderTellCallback tellCallback, StreamEncoderMetadataCallback metadataCallback, IntPtr userData);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_encoder_get_state", CallingConvention = CallingConvention.Cdecl)]
        internal static extern EncoderState StreamEncoderGetState(NativeStreamEncoderHandle handle);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_encoder_process_interleaved", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool StreamEncoderProcessInterleaved(NativeStreamEncoderHandle handle, int[] buffer, uint samples);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_encoder_finish", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool StreamEncoderFinish(NativeStreamEncoderHandle handle);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__stream_encoder_delete", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool StreamEncoderDelete(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate EncoderWriteStatus StreamEncoderWriteCallback(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]byte[] buffer, int bytes, uint samples, uint currentFrame, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate EncoderSeekStatus StreamEncoderSeekCallback(IntPtr handle, ulong absoluteOffset, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate EncoderTellStatus StreamEncoderTellCallback(IntPtr handle, out ulong absoluteOffset, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void StreamEncoderMetadataCallback(IntPtr handle, IntPtr metaData, IntPtr userData);

        #endregion

        #region Metadata

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_chain_new", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NativeMetadataChainHandle MetadataChainNew();

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_chain_status", CallingConvention = CallingConvention.Cdecl)]
        internal static extern MetadataChainStatus MetadataChainGetStatus(NativeMetadataChainHandle handle);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_chain_read_with_callbacks", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MetadataChainRead(NativeMetadataChainHandle handle, IntPtr ioHandle, IoCallbacks callbacks);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_chain_check_if_tempfile_needed", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MetadataChainCheckIfTempFileNeeded(NativeMetadataChainHandle handle, [MarshalAs(UnmanagedType.Bool)]bool usePadding);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_chain_write_with_callbacks", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MetadataChainWrite(NativeMetadataChainHandle handle, [MarshalAs(UnmanagedType.Bool)]bool usePadding, IntPtr ioHandle, IoCallbacks callbacks);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_chain_write_with_callbacks_and_tempfile", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MetadataChainWriteWithTempFile(NativeMetadataChainHandle handle, [MarshalAs(UnmanagedType.Bool)]bool usePadding, IntPtr ioHandle, IoCallbacks callbacks, IntPtr tempIoHandle, IoCallbacks tempCallbacks);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_chain_delete", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void MetadataChainDelete(IntPtr handle);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_iterator_new", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NativeMetadataIteratorHandle MetadataIteratorNew();

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_iterator_init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void MetadataIteratorInitialize(NativeMetadataIteratorHandle handle, NativeMetadataChainHandle chainHandle);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_iterator_next", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MetadataIteratorNext(NativeMetadataIteratorHandle handle);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_iterator_get_block", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr MetadataIteratorGetBlock(NativeMetadataIteratorHandle handle);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_iterator_insert_block_after", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MetadataIteratorInsertBlockAfter(NativeMetadataIteratorHandle handle, NativeMetadataBlockHandle metadatahandle);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_iterator_delete_block", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MetadataIteratorDeleteBlock(NativeMetadataIteratorHandle handle, [MarshalAs(UnmanagedType.Bool)]bool replaceWithPadding);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_iterator_delete", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void MetadataIteratorDelete(IntPtr handle);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_object_new", CallingConvention = CallingConvention.Cdecl)]
        internal static extern NativeMetadataBlockHandle MetadataBlockNew(MetadataType blockType);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_object_vorbiscomment_entry_from_name_value_pair", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool VorbisCommentGet(out VorbisCommentEntry vorbisComment, byte[] key, byte[] value);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_object_vorbiscomment_append_comment", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool VorbisCommentAppend(NativeMetadataBlockHandle handle, VorbisCommentEntry vorbisComment, [MarshalAs(UnmanagedType.Bool)]bool copy);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_object_seektable_template_append_spaced_points", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SeekTableTemplateAppend(NativeMetadataBlockHandle handle, uint count, ulong totalSamples);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_object_seektable_template_sort", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SeekTableTemplateSort(NativeMetadataBlockHandle handle, [MarshalAs(UnmanagedType.Bool)]bool compact);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_object_picture_set_mime_type", CallingConvention = CallingConvention.Cdecl, BestFitMapping = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PictureSetMimeType(NativeMetadataBlockHandle handle, [MarshalAs(UnmanagedType.LPStr)]string mimeType, [MarshalAs(UnmanagedType.Bool)]bool copy);

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_object_picture_set_data", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PictureSetData(NativeMetadataBlockHandle handle, byte[] data, uint length, [MarshalAs(UnmanagedType.Bool)]bool copy); 

        [DllImport(_flacLibrary, EntryPoint = "FLAC__metadata_object_delete", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void MetadataBlockDelete(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr IOCallbacksReadCallback(IntPtr readBuffer, IntPtr bufferSize, IntPtr numberOfRecords, IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr IOCallbacksWriteCallback(IntPtr writeBuffer, IntPtr bufferSize, IntPtr numberOfRecords, IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int IOCallbacksSeekCallback(IntPtr handle, long offset, SeekOrigin whence);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate long IOCallbacksTellCallback(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int IOCallbacksEofCallback(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int IOCallbacksCloseCallback(IntPtr handle);

        #endregion

        [DllImport(_kernelLibrary, CharSet = CharSet.Unicode)]
        static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport(_kernelLibrary, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport(_kernelLibrary)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeLibrary(IntPtr module);

        [DllImport(_kernelLibrary, ExactSpelling = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
        static extern IntPtr GetProcAddress(IntPtr module, string name);
    }
}
