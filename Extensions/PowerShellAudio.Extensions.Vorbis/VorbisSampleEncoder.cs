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

using PowerShellAudio.Extensions.Vorbis.Properties;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PowerShellAudio.Extensions.Vorbis
{
    [SampleEncoderExport("Ogg Vorbis")]
    public class VorbisSampleEncoder : ISampleEncoder, IDisposable
    {
        static readonly SampleEncoderInfo _encoderInfo = new VorbisSampleEncoderInfo();

        NativeVorbisEncoder _encoder;
        ExportLifetimeContext<ISampleFilter> _replayGainFilterLifetime;
        NativeOggStream _oggStream;
        byte[] _buffer;
        Stream _output;

        public SampleEncoderInfo EncoderInfo
        {
            get
            {
                Contract.Ensures(Contract.Result<SampleEncoderInfo>() != null);

                return _encoderInfo;
            }
        }

        public bool ManuallyFreesSamples
        {
            get { return false; }
        }

        public void Initialize(Stream stream, AudioInfo audioInfo, MetadataDictionary metadata, SettingsDictionary settings)
        {
            _encoder = new NativeVorbisEncoder();
            _output = stream;

            // Load the external gain filter:
            var sampleFilterFactory = ExtensionProvider.GetFactories<ISampleFilter>("Name", "ReplayGain").SingleOrDefault();
            if (sampleFilterFactory == null)
                throw new ExtensionInitializationException(Resources.SampleEncoderReplayGainFilterError);
            _replayGainFilterLifetime = sampleFilterFactory.CreateExport();
            _replayGainFilterLifetime.Value.Initialize(metadata, settings);

            _oggStream = IntializeOggStream(settings);
            _buffer = new byte[4096];

            if (!string.IsNullOrEmpty(settings["BitRate"]))
                ConfigureEncoderForBitRate(settings, audioInfo, _encoder);
            else
                ConfigureEncoderForQuality(settings, audioInfo, _encoder);

            WriteHeader(metadata, settings, stream);
        }

        public void Submit(SampleCollection samples)
        {
            if (!samples.IsLast)
            {
                // Filter by ReplayGain, depending on settings:
                _replayGainFilterLifetime.Value.Submit(samples);

                // Request an unmanaged buffer, then copy the samples to it:
                IntPtr[] buffers = new IntPtr[samples.Channels];
                Marshal.Copy(_encoder.GetBuffer(samples.SampleCount), buffers, 0, buffers.Length);

                for (int i = 0; i < samples.Channels; i++)
                    Marshal.Copy(samples[i], 0, buffers[i], samples[i].Length);
            }

            _encoder.Wrote(samples.SampleCount);

            while (_encoder.BlockOut())
            {
                _encoder.Analysis(IntPtr.Zero);
                _encoder.AddBlock();

                OggPacket packet;
                while (_encoder.FlushPacket(out packet))
                {
                    _oggStream.PacketIn(ref packet);

                    OggPage page;
                    while (_oggStream.PageOut(out page))
                        WritePage(page, _output);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_encoder != null)
                    _encoder.Dispose();
                if (_replayGainFilterLifetime != null)
                    _replayGainFilterLifetime.Dispose();
                if (_oggStream != null)
                    _oggStream.Dispose();
            }
        }

        void WriteHeader(MetadataDictionary metadata, SettingsDictionary settings, Stream stream)
        {
            Contract.Requires(metadata != null);
            Contract.Requires(settings != null);
            Contract.Requires(stream != null);
            Contract.Requires(stream.CanWrite);

            var vorbisComment = new VorbisComment();
            try
            {
                SafeNativeMethods.VorbisCommentInitialize(out vorbisComment);

                foreach (var item in new MetadataToVorbisCommentAdapter(metadata))
                {
                    // The key and value need to be marshaled as null-terminated UTF-8 strings:
                    var keyBytes = new byte[Encoding.UTF8.GetByteCount(item.Key) + 1];
                    Encoding.UTF8.GetBytes(item.Key, 0, item.Key.Length, keyBytes, 0);

                    var valueBytes = new byte[Encoding.UTF8.GetByteCount(item.Value) + 1];
                    Encoding.UTF8.GetBytes(item.Value, 0, item.Value.Length, valueBytes, 0);

                    SafeNativeMethods.VorbisCommentAddTag(ref vorbisComment, keyBytes, valueBytes);
                }

                OggPacket first;
                OggPacket second;
                OggPacket third;
                _encoder.HeaderOut(ref vorbisComment, out first, out second, out third);

                _oggStream.PacketIn(ref first);
                _oggStream.PacketIn(ref second);
                _oggStream.PacketIn(ref third);
            }
            finally
            {
                SafeNativeMethods.VorbisCommentClear(ref vorbisComment);
            }

            OggPage page;
            while (_oggStream.Flush(out page))
                WritePage(page, stream);
        }

        void WritePage(OggPage page, Stream stream)
        {
            Contract.Requires(stream != null);
            Contract.Requires(stream.CanWrite);

            WritePointer(page.Header, page.HeaderLength, stream);
            WritePointer(page.Body, page.BodyLength, stream);
        }

        void WritePointer(IntPtr location, int length, Stream stream)
        {
            Contract.Requires(location != IntPtr.Zero);
            Contract.Requires(stream != null);
            Contract.Requires(stream.CanWrite);

            int offset = 0;
            while (offset < length)
            {
                int bytesCopied = Math.Min(length - offset, _buffer.Length);
                Marshal.Copy(IntPtr.Add(location, offset), _buffer, 0, bytesCopied);
                stream.Write(_buffer, 0, bytesCopied);
                offset += bytesCopied;
            }
        }

        static NativeOggStream IntializeOggStream(SettingsDictionary settings)
        {
            Contract.Requires(settings != null);

            int serialNumber;
            if (string.IsNullOrEmpty(settings["SerialNumber"]))
                serialNumber = new Random().Next();
            else if (!int.TryParse(settings["SerialNumber"], out serialNumber) || serialNumber < 0)
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderBadSerialNumber, settings["SerialNumber"]));

            return new NativeOggStream(serialNumber);
        }

        static void ConfigureEncoderForBitRate(SettingsDictionary settings, AudioInfo audioInfo, NativeVorbisEncoder encoder)
        {
            Contract.Requires(settings != null);
            Contract.Requires(audioInfo != null);
            Contract.Requires(encoder != null);

            int bitRate;
            if (!int.TryParse(settings["BitRate"], out bitRate) || bitRate < 32 || bitRate > 500)
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderBadBitRate, settings["BitRate"]));

            if (string.IsNullOrEmpty(settings["ControlMode"]) || string.Compare(settings["ControlMode"], "Variable", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Configure the nominal bitrate, but then disable bitrate management:
                encoder.SetupManaged(audioInfo.Channels, audioInfo.SampleRate, -1, bitRate * 1000, -1);
                encoder.Control(0x15, IntPtr.Zero); // OV_ECTL_RATEMANAGE2_SET
                encoder.SetupInitialize();
            }
            else if (string.Compare(settings["ControlMode"], "Average", StringComparison.OrdinalIgnoreCase) == 0)
                encoder.Initialize(audioInfo.Channels, audioInfo.SampleRate, -1, bitRate * 1000, -1);
            else if (string.Compare(settings["ControlMode"], "Constant", StringComparison.OrdinalIgnoreCase) == 0)
                encoder.Initialize(audioInfo.Channels, audioInfo.SampleRate, bitRate, bitRate * 1000, bitRate);
            else
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderBadBitRateControlMode, settings["ControlMode"]));
        }

        static void ConfigureEncoderForQuality(SettingsDictionary settings, AudioInfo audioInfo, NativeVorbisEncoder encoder)
        {
            Contract.Requires(settings != null);
            Contract.Requires(audioInfo != null);
            Contract.Requires(encoder != null);

            if (!string.IsNullOrEmpty(settings["ControlMode"]) && string.Compare(settings["ControlMode"], "Variable", StringComparison.OrdinalIgnoreCase) != 0)
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderBadQualityControlMode, settings["ControlMode"]));

            // Set the quality if specified, otherwise select "5":
            float quality;
            if (string.IsNullOrEmpty(settings["VBRQuality"]))
                quality = 5;
            else if (!float.TryParse(settings["VBRQuality"], out quality) || quality < -1 || quality > 10)
                throw new InvalidSettingException(string.Format(CultureInfo.CurrentCulture, Resources.SampleEncoderBadVbrQuality, settings["VBRQuality"]));

            encoder.Initialize(audioInfo.Channels, audioInfo.SampleRate, quality / 10);
        }
    }
}
