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

using PowerShellAudio.Extensions.Flac.Properties;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace PowerShellAudio.Extensions.Flac
{
    class NativeVorbisCommentBlock : NativeMetadataBlock
    {
        internal NativeVorbisCommentBlock()
            : base(MetadataType.VorbisComment)
        {
            Contract.Ensures(Handle != null);
            Contract.Ensures(!Handle.IsClosed);
        }

        internal void Append(string key, string value)
        {
            Contract.Requires(!string.IsNullOrEmpty(key));
            Contract.Requires(!string.IsNullOrEmpty(value));
            Contract.Requires(!Handle.IsClosed);

            VorbisCommentEntry comment;
            if (
                !SafeNativeMethods.VorbisCommentGet(out comment, Encoding.ASCII.GetBytes(key),
                    Encoding.UTF8.GetBytes(value)))
                throw new IOException(Resources.NativeVorbisCommentBlockMemoryError);

            if (!SafeNativeMethods.VorbisCommentAppend(Handle, comment, false))
                throw new IOException(Resources.NativeVorbisCommentBlockMemoryError);
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(Handle != null);
            Contract.Invariant(!Handle.IsInvalid);
        }
    }
}
