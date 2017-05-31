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
using JetBrains.Annotations;

namespace PowerShellAudio.Extensions.Flac
{
    class NativeStreamDecoder : IDisposable
    {
        readonly NativeStreamDecoderHandle _handle = SafeNativeMethods.StreamDecoderNew();
        readonly Stream _input;
        readonly SafeNativeMethods.StreamDecoderReadCallback _readCallback;
        readonly SafeNativeMethods.StreamDecoderSeekCallback _seekCallback;
        readonly SafeNativeMethods.StreamDecoderTellCallback _tellCallback;
        readonly SafeNativeMethods.StreamDecoderLengthCallback _lengthCallback;
        readonly SafeNativeMethods.StreamDecoderEofCallback _eofCallback;
        readonly SafeNativeMethods.StreamDecoderWriteCallback _writeCallback;
        readonly SafeNativeMethods.StreamDecoderMetadataCallback _metadataCallback;
        readonly SafeNativeMethods.StreamDecoderErrorCallback _errorCallback;

        internal DecoderErrorStatus? Error { get; private set; }

        internal NativeStreamDecoder([NotNull] Stream input)
        {
            _input = input;

            _readCallback = ReadCallback;
            _seekCallback = SeekCallback;
            _tellCallback = TellCallback;
            _lengthCallback = LengthCallback;
            _eofCallback = EofCallback;
            _writeCallback = WriteCallback;
            _metadataCallback = MetadataCallback;
            _errorCallback = ErrorCallback;
        }

        internal void SetMetadataRespond(MetadataType type)
        {
            SafeNativeMethods.StreamDecoderSetMetadataRespond(_handle, type);
        }

        internal DecoderInitStatus Initialize()
        {
            return SafeNativeMethods.StreamDecoderInitialize(_handle, _readCallback, _seekCallback, _tellCallback,
                _lengthCallback, _eofCallback, _writeCallback, _metadataCallback, _errorCallback, IntPtr.Zero);
        }

        internal bool ProcessMetadata()
        {
            return SafeNativeMethods.StreamDecoderProcessMetadata(_handle);
        }

        internal bool ProcessSingle()
        {
            return SafeNativeMethods.StreamDecoderProcessSingle(_handle);
        }

        internal void Finish()
        {
            SafeNativeMethods.StreamDecoderFinish(_handle);
        }

        [Pure]
        internal DecoderState GetState()
        {
            return SafeNativeMethods.StreamDecoderGetState(_handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _handle.Dispose();
        }

        DecoderReadStatus ReadCallback(IntPtr handle, [NotNull] byte[] buffer, ref int bytes, IntPtr userData)
        {
            try
            {
                bytes = _input.Read(buffer, 0, bytes);
                return bytes == 0 ? DecoderReadStatus.EndOfStream : DecoderReadStatus.Continue;
            }
            catch (IOException)
            {
                return DecoderReadStatus.Abort;
            }
        }

        DecoderSeekStatus SeekCallback(IntPtr handle, ulong absoluteOffset, IntPtr userData)
        {
            try
            {
                _input.Position = (long)absoluteOffset;
                return DecoderSeekStatus.Ok;
            }
            catch (NotSupportedException)
            {
                return DecoderSeekStatus.Unsupported;
            }
            catch (IOException)
            {
                return DecoderSeekStatus.Error;
            }
        }

        DecoderTellStatus TellCallback(IntPtr handle, out ulong absoluteOffset, IntPtr userData)
        {
            try
            {
                absoluteOffset = (ulong)_input.Position;
                return DecoderTellStatus.Ok;
            }
            catch (NotSupportedException)
            {
                absoluteOffset = 0;
                return DecoderTellStatus.Unsupported;
            }
            catch (IOException)
            {
                absoluteOffset = 0;
                return DecoderTellStatus.Error;
            }
        }

        DecoderLengthStatus LengthCallback(IntPtr handle, out ulong streamLength, IntPtr userData)
        {
            try
            {
                streamLength = (ulong)_input.Length;
                return DecoderLengthStatus.Ok;
            }
            catch (NotSupportedException)
            {
                streamLength = 0;
                return DecoderLengthStatus.Unsupported;
            }
        }

        bool EofCallback(IntPtr handle, IntPtr userData)
        {
            return _input.Position == _input.Length;
        }

        protected virtual DecoderWriteStatus WriteCallback(IntPtr handle, ref Frame frame, IntPtr buffer, IntPtr userData)
        {
            return DecoderWriteStatus.Continue;
        }

        protected virtual void MetadataCallback(IntPtr handle, IntPtr metadataBlock, IntPtr userData)
        {
        }

        void ErrorCallback(IntPtr handle, DecoderErrorStatus error, IntPtr userData)
        {
            Error = error;
        }
    }
}
