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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace PowerShellAudio.Extensions.Vorbis
{
    [SuppressUnmanagedCodeSecurity]
    static class SafeNativeMethods
    {
        const string _oggLibrary = @"libogg.dll";
        const string _vorbisLibrary = @"libvorbis.dll";

        static SafeNativeMethods()
        {
            // Select an architecture-appropriate libFLAC.dll by prefixing the PATH variable:
            var newPath = new StringBuilder(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location));
            newPath.Append(Environment.Is64BitProcess ? @"\x64" : @"\x86");
            newPath.Append(Path.PathSeparator);
            newPath.Append(Environment.GetEnvironmentVariable("PATH"));

            Environment.SetEnvironmentVariable("PATH", newPath.ToString());
        }

        [DllImport(_oggLibrary, EntryPoint = "ogg_sync_init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int OggSyncInitialize(IntPtr syncState);

        [DllImport(_oggLibrary, EntryPoint = "ogg_sync_buffer", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr OggSyncBuffer(IntPtr syncState, int size);

        [DllImport(_oggLibrary, EntryPoint = "ogg_sync_wrote", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int OggSyncWrote(IntPtr syncState, int bytes);

        [DllImport(_oggLibrary, EntryPoint = "ogg_sync_pageout", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int OggSyncPageOut(IntPtr syncState, out OggPage page);

        [DllImport(_oggLibrary, EntryPoint = "ogg_sync_clear", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void OggSyncClear(IntPtr syncState);

        [DllImport(_oggLibrary, EntryPoint = "ogg_page_serialno", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int OggPageGetSerialNumber(ref OggPage page);

        [DllImport(_oggLibrary, EntryPoint = "ogg_page_eos", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int OggPageEndOfStream(ref OggPage page);

        [DllImport(_oggLibrary, EntryPoint = "ogg_stream_init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int OggStreamInitialize(IntPtr streamState, int serialNumber);

        [DllImport(_oggLibrary, EntryPoint = "ogg_stream_pagein", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int OggStreamPageIn(IntPtr streamState, ref OggPage page);

        [DllImport(_oggLibrary, EntryPoint = "ogg_stream_pageout", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int OggStreamPageOut(IntPtr streamState, out OggPage page);

        [DllImport(_oggLibrary, EntryPoint = "ogg_stream_flush", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int OggStreamFlush(IntPtr streamState, out OggPage page);

        [DllImport(_oggLibrary, EntryPoint = "ogg_stream_packetin", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int OggStreamPacketIn(IntPtr streamState, ref OggPacket packet);

        [DllImport(_oggLibrary, EntryPoint = "ogg_stream_packetout", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int OggStreamPacketOut(IntPtr streamState, out OggPacket packet);

        [DllImport(_oggLibrary, EntryPoint = "ogg_stream_clear", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void OggStreamClear(IntPtr streamState);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_version_string", CallingConvention = CallingConvention.Cdecl)]
        internal static extern string VorbisVersion();

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_info_init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void VorbisInfoInitialize(IntPtr info);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_info_clear", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void VorbisInfoClear(IntPtr info);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_comment_init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void VorbisCommentInitialize(out VorbisComment comment);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_comment_add_tag", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void VorbisCommentAddTag(ref VorbisComment comment, byte[] tag, byte[] contents);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_commentheader_out", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int VorbisCommentHeaderOut(ref VorbisComment comment, out OggPacket packet);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_comment_clear", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void VorbisCommentClear(ref VorbisComment comment);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_block_init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result VorbisBlockInitialize(IntPtr dspState, IntPtr block);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_block_clear", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void VorbisBlockClear(IntPtr block);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_synthesis_headerin", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result VorbisSynthesisHeaderIn(IntPtr info, ref VorbisComment comment, ref OggPacket packet);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_encode_init_vbr", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result VorbisEncodeInitializeVbr(IntPtr info, int channels, int rate, float baseQuality);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_encode_init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result VorbisEncodeInitialize(IntPtr info, int channels, int sampleRate, int maximumBitRate, int nominalBitRate, int minimumBitRate);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_encode_setup_managed", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result VorbisEncodeSetupManaged(IntPtr info, int channels, int sampleRate, int maximumBitRate, int nominalBitRate, int minimumBitRate);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_encode_ctl", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result VorbisEncodeControl(IntPtr info, int request, IntPtr argument);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_encode_setup_init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result VorbisEncodeSetupInitialize(IntPtr info);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_analysis_init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result VorbisAnalysisInitialize(IntPtr dspState, IntPtr info);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_analysis_headerout", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result VorbisAnalysisHeaderOut(IntPtr dspState, ref VorbisComment comment, out OggPacket first, out OggPacket second, out OggPacket third);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_analysis_buffer", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr VorbisAnalysisGetBuffer(IntPtr dspState, int samples);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_analysis_wrote", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result VorbisAnalysisWrote(IntPtr dspState, int samples);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_analysis_blockout", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result VorbisAnalysisBlockOut(IntPtr dspState, IntPtr block);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_analysis", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result VorbisAnalysis(IntPtr block, IntPtr packet);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_bitrate_addblock", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result VorbisBitrateAddBlock(IntPtr block);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_bitrate_flushpacket", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result VorbisBitrateFlushPacket(IntPtr dspState, out OggPacket packet);

        [DllImport(_vorbisLibrary, EntryPoint = "vorbis_dsp_clear", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void VorbisDspClear(IntPtr dspState);
    }
}
