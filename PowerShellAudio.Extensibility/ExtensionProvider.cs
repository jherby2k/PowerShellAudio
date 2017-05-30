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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using JetBrains.Annotations;

namespace PowerShellAudio
{
    /// <summary>
    /// Provides discovery of extensions.
    /// </summary>
    public static class ExtensionProvider
    {
        /// <summary>
        /// Gets all the available extension export factories of type T.
        /// </summary>
        /// <typeparam name="T">The extension type.</typeparam>
        /// <returns>The factories.</returns>
        [NotNull]
        public static IEnumerable<ExportFactory<T>> GetFactories<T>() where T : class
        {
            return ExtensionContainer<T>.Instance.Factories;
        }

        /// <summary>
        /// Gets the extension export factories with the specified metadata key and value.
        /// </summary>
        /// <typeparam name="T">The extension type.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The factories.</returns>
        [NotNull]
        public static IEnumerable<ExportFactory<T>> GetFactories<T>(string key, string value) where T : class
        {
            return ExtensionContainer<T>.Instance.Factories.Where(factory =>
                string.Compare((string) factory.Metadata[key], value, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}
