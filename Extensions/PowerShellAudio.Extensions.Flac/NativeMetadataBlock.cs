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
using System.Diagnostics.Contracts;

namespace PowerShellAudio.Extensions.Flac
{
    internal abstract class NativeMetadataBlock : IDisposable
    {
        internal NativeMetadataBlockHandle Handle { get; private set; }

        internal NativeMetadataBlock(MetadataType metadataType)
        {
            Contract.Ensures(Handle != null);
            Contract.Ensures(!Handle.IsClosed);

            Handle = SafeNativeMethods.MetadataBlockNew(metadataType);
        }

        internal void ReleaseHandleOwnership()
        {
            Handle.SuppressDisposal();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Handle.Dispose();
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(Handle != null);
            Contract.Invariant(!Handle.IsInvalid);
        }
    }
}
