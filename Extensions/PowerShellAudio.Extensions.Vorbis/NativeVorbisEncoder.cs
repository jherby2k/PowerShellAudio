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

using PowerShellAudio.Extensions.Vorbis.Properties;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace PowerShellAudio.Extensions.Vorbis
{
    sealed class NativeVorbisEncoder : IDisposable
    {
        [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = "Reference to a structure, not a handle.")]
        readonly IntPtr _info;
        [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = "Reference to a structure, not a handle.")]
        IntPtr _dspState;
        [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = "Reference to a structure, not a handle.")]
        IntPtr _block;

        internal NativeVorbisEncoder()
        {
            _info = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VorbisInfo)));
            SafeNativeMethods.VorbisInfoInitialize(_info);
        }

        internal void Initialize(int channels, int sampleRate, float baseQuality)
        {
            Result result = SafeNativeMethods.VorbisEncodeInitializeVbr(_info, channels, sampleRate, baseQuality);
            if (result != Result.Ok)
                throw new IOException(string.Format(CultureInfo.CurrentCulture,
                    Resources.NativeVorbisEncoderInitializationError, result));

            CompleteInitialization();
        }

        internal void Initialize(int channels, int sampleRate, int minimumBitRate, int nominalBitRate, int maximumBitRate)
        {
            Result result = SafeNativeMethods.VorbisEncodeInitialize(_info, channels, sampleRate, minimumBitRate,
                nominalBitRate, maximumBitRate);
            if (result != Result.Ok)
                throw new IOException(string.Format(CultureInfo.CurrentCulture,
                    Resources.NativeVorbisEncoderInitializationError, result));

            CompleteInitialization();
        }

        internal void SetupManaged(int channels, int sampleRate, int minimumBitRate, int nominalBitRate, int maximumBitRate)
        {
            Result result = SafeNativeMethods.VorbisEncodeSetupManaged(_info, channels, sampleRate, minimumBitRate,
                nominalBitRate, maximumBitRate);
            if (result != Result.Ok)
                throw new IOException(string.Format(CultureInfo.CurrentCulture,
                    Resources.NativeVorbisEncoderSetupError, result));
        }

        internal void Control(int request, IntPtr argument)
        {
            Result result = SafeNativeMethods.VorbisEncodeControl(_info, request, argument);
            if (result != Result.Ok)
                throw new IOException(string.Format(CultureInfo.CurrentCulture, 
                    Resources.NativeVorbisEncoderControlError, result));
        }

        internal void SetupInitialize()
        {
            Result result = SafeNativeMethods.VorbisEncodeSetupInitialize(_info);
            if (result != Result.Ok)
                throw new IOException(string.Format(CultureInfo.CurrentCulture, 
                    Resources.NativeVorbisEncoderInitializationError, result));

            CompleteInitialization();
        }

        internal void HeaderOut(ref VorbisComment comment, out OggPacket first, out OggPacket second, out OggPacket third)
        {
            Result result = SafeNativeMethods.VorbisAnalysisHeaderOut(_dspState, ref comment,
                out first, out second, out third);
            if (result != Result.Ok)
                throw new IOException(string.Format(CultureInfo.CurrentCulture, 
                    Resources.NativeVorbisEncoderHeaderOutError, result));
        }

        internal IntPtr GetBuffer(int samples)
        {
            return SafeNativeMethods.VorbisAnalysisGetBuffer(_dspState, samples);
        }

        internal void Wrote(int samples)
        {
            Result result = SafeNativeMethods.VorbisAnalysisWrote(_dspState, samples);
            if (result != Result.Ok)
                throw new IOException(string.Format(CultureInfo.CurrentCulture, 
                    Resources.NativeVorbisEncoderWroteError, result));
        }

        internal bool BlockOut()
        {
            Result result = SafeNativeMethods.VorbisAnalysisBlockOut(_dspState, _block);
            switch (result)
            {
                case Result.Ok:
                    return false;
                case Result.OkMoreAvailable:
                    return true;
                default:
                    throw new IOException(string.Format(CultureInfo.CurrentCulture, 
                        Resources.NativeVorbisEncoderBlockOutError, result));
            }
        }

        internal void Analysis(IntPtr packet)
        {
            Result result = SafeNativeMethods.VorbisAnalysis(_block, packet);
            if (result != Result.Ok)
                throw new IOException(string.Format(CultureInfo.CurrentCulture, 
                    Resources.NativeVorbisEncoderAnalysisError, result));
        }

        internal void AddBlock()
        {
            Result result = SafeNativeMethods.VorbisBitrateAddBlock(_block);
            if (result != Result.Ok)
                throw new IOException(string.Format(CultureInfo.CurrentCulture,
                    Resources.NativeVorbisEncoderAddBlockError, result));
        }

        internal bool FlushPacket(out OggPacket packet)
        {
            Result result = SafeNativeMethods.VorbisBitrateFlushPacket(_dspState, out packet);
            switch (result)
            {
                case Result.Ok:
                    return false;
                case Result.OkMoreAvailable:
                    return true;
                default:
                    throw new IOException(string.Format(CultureInfo.CurrentCulture, 
                        Resources.NativeVorbisEncoderFlushPacketError, result));
            }
        }

        public void Dispose()
        {
            if (_block != IntPtr.Zero)
                SafeNativeMethods.VorbisBlockClear(_block);
            Marshal.FreeHGlobal(_block);
            if (_dspState != IntPtr.Zero)
                SafeNativeMethods.VorbisDspClear(_dspState);
            Marshal.FreeHGlobal(_dspState);
            SafeNativeMethods.VorbisInfoClear(_info);
            Marshal.FreeHGlobal(_info);

            GC.SuppressFinalize(this);
        }

        ~NativeVorbisEncoder()
        {
            Dispose();
        }

        void CompleteInitialization()
        {
            _dspState = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VorbisDspState)));
            if (SafeNativeMethods.VorbisAnalysisInitialize(_dspState, _info) != Result.Ok)
                throw new IOException(Resources.NativeVorbisEncoderAnalysisInitialize);

            _block = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VorbisBlock)));
            if (SafeNativeMethods.VorbisBlockInitialize(_dspState, _block) != Result.Ok)
                throw new IOException(Resources.NativeVorbisEncoderBlockInitialize);
        }
    }
}
