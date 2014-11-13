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

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;

namespace PowerShellAudio
{
    /// <summary>
    /// Provides discovery of extensions.
    /// </summary>
    public static class ExtensionProvider
    {
        /// <summary>
        /// Discovers the extension factories for a given type.
        /// </summary>
        /// <typeparam name="T">The type of extension factories to discover.</typeparam>
        /// <returns>The extension factories.</returns>
        public static IEnumerable<ExportFactory<T, IDictionary<string, object>>> GetFactories<T>()
        {
            Contract.Ensures(Contract.Result<IEnumerable<ExportFactory<T, IDictionary<string, object>>>>() != null);

            return ExtensionProviderSingleton<T>.Instance.Factories;
        }
    }
}
