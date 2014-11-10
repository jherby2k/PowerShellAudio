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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;

namespace PowerShellAudio
{
    /// <summary>
    /// Provides discovery of PowerShellAudio extensions using the Managed Extensibility Framework.
    /// </summary>
    /// <remarks>
    /// This class is implemented as a generic singleton, which means that there is at most one
    /// <see cref="ExtensionProvider{T}"/> instance for each type of extension.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class ExtensionProvider<T>
    {
        static readonly Lazy<ExtensionProvider<T>> _lazyInstance = new Lazy<ExtensionProvider<T>>(() => new ExtensionProvider<T>());

        /// <summary>
        /// Gets the singleton instance for extensions of the specified type.
        /// </summary>
        /// <value>
        /// The singleton instance.
        /// </value>
        public static ExtensionProvider<T> Instance
        {
            get
            {
                Contract.Ensures(Contract.Result<ExtensionProvider<T>>() != null);

                return _lazyInstance.Value;
            }
        }

        /// <summary>
        /// Gets the extension factories.
        /// </summary>
        /// <value>
        /// The extension factories.
        /// </value>
        [ImportMany]
        public IEnumerable<ExportFactory<T, IDictionary<string, object>>> Factories { get; private set; }

        ExtensionProvider()
        {
            Contract.Ensures(Factories != null);

            Initialize();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The CompositionContainer can't be disposed unless the ExtensionProvider is, and it's a singleton.")]
        void Initialize()
        {
            Contract.Ensures(Factories != null);

            var extensionsDir = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "Extensions"));

            // Add a catalog for each subdirectory under Extensions:
            using (var catalog = new AggregateCatalog())
            {
                if (extensionsDir.Exists)
                    foreach (DirectoryInfo directory in extensionsDir.GetDirectories())
                        catalog.Catalogs.Add(new DirectoryCatalog(directory.FullName));

                // Compose the parts:
                var compositionContainer = new CompositionContainer(catalog, true);
                compositionContainer.ComposeParts(this);
            }
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(Factories != null);
        }
    }
}
