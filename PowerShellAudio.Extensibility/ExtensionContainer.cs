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
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;

namespace PowerShellAudio
{
    class ExtensionContainer<T>
    {
        static readonly Lazy<ExtensionContainer<T>> _lazyInstance = new Lazy<ExtensionContainer<T>>(() => new ExtensionContainer<T>());

        internal static ExtensionContainer<T> Instance
        {
            get
            {
                Contract.Ensures(Contract.Result<ExtensionContainer<T>>() != null);

                return _lazyInstance.Value;
            }
        }

        [ImportMany]
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        internal IEnumerable<ExportFactory<T, IDictionary<string, object>>> Factories { get; private set; }

        ExtensionContainer()
        {
            Contract.Ensures(Factories != null);

            Initialize();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The CompositionContainer can't be disposed unless the ExtensionProvider is, and it's a singleton.")]
        void Initialize()
        {
            Contract.Ensures(Factories != null);

            string mainDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            using (var catalog = new AggregateCatalog())
            {
                // Add the root directory as well, so extension references can be found:
                catalog.Catalogs.Add(new DirectoryCatalog(mainDir));

                // Add a catalog for each subdirectory under Extensions:
                foreach (DirectoryInfo directory in new DirectoryInfo(Path.Combine(mainDir, "Extensions")).GetDirectories())
                    catalog.Catalogs.Add(new DirectoryCatalog(directory.FullName));

                // Compose the parts:
                new CompositionContainer(catalog, CompositionOptions.IsThreadSafe | CompositionOptions.DisableSilentRejection).ComposeParts(this);
            }
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(Factories != null);
        }
    }
}
