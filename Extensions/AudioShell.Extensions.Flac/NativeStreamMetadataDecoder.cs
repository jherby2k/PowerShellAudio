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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AudioShell.Extensions.Flac
{
    class NativeStreamMetadataDecoder : NativeStreamDecoder
    {
        readonly int _nativePtrSize = Marshal.SizeOf(typeof(IntPtr));

        internal MetadataDictionary Metadata { get; private set; }

        internal NativeStreamMetadataDecoder(Stream input)
            : base(input)
        {
            Contract.Requires(input != null);
            Contract.Requires(input.CanRead);
            Contract.Requires(input.CanSeek);
            Contract.Requires(input.Length > 0);
        }

        protected override void MetadataCallback(IntPtr handle, IntPtr metadata, IntPtr userData)
        {
            if ((MetadataType)Marshal.ReadInt32(metadata) == MetadataType.VorbisComment)
            {
                var vorbisComments = new Dictionary<string, string>();

                int commentCount = Marshal.ReadInt32(metadata, 16 + _nativePtrSize * 2);
                IntPtr commentsPtr = Marshal.ReadIntPtr(metadata, 16 + _nativePtrSize * 3);
                for (int commentIndex = 0; commentIndex < commentCount; commentIndex++)
                {
                    int commentStructSize = _nativePtrSize * 2;
                    int commentLength = Marshal.ReadInt32(commentsPtr, commentIndex * commentStructSize);
                    var commentBytes = new byte[commentLength];
                    IntPtr commentPtr = Marshal.ReadIntPtr(commentsPtr, _nativePtrSize + commentIndex * commentStructSize);
                    Marshal.Copy(commentPtr, commentBytes, 0, commentLength);
                    string[] comment = Encoding.UTF8.GetString(commentBytes).Split('=');

                    Contract.Assert(comment.Length == 2);

                    vorbisComments[comment[0]] = comment[1];
                }

                Metadata = new VorbisCommentToMetadataAdapter(vorbisComments);
            }
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_nativePtrSize == Marshal.SizeOf(typeof(IntPtr)));
        }
    }
}
