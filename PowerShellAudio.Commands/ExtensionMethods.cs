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

using Microsoft.PowerShell.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Threading;
using JetBrains.Annotations;

namespace PowerShellAudio.Commands
{
    static class ExtensionMethods
    {
        internal static void ProcessOutput([NotNull] this Cmdlet cmdlet, [NotNull] BlockingCollection<object> outputQueue, CancellationToken cancelToken)
        {
            foreach (object queuedObject in outputQueue.GetConsumingEnumerable(cancelToken))
            {
                if (queuedObject is ErrorRecord queuedError)
                    cmdlet.WriteError(queuedError);
                else
                {
                    if (queuedObject is ProgressRecord queuedProgress)
                        cmdlet.WriteProgress(queuedProgress);
                    else
                        cmdlet.WriteObject(queuedObject);
                }
            }
        }

        internal static IEnumerable<string> GetFileSystemPaths(
            [NotNull] this PSCmdlet cmdlet,
            [CanBeNull] string path,
            [CanBeNull] string literalPath)
        {
            ProviderInfo provider;

            if (!string.IsNullOrEmpty(path))
            {
                Collection<string> providerPaths = cmdlet.GetResolvedProviderPathFromPSPath(path, out provider);

                if (provider.ImplementingType == typeof(FileSystemProvider))
                    return providerPaths;
            }

            if (!string.IsNullOrEmpty(literalPath))
            {

                string providerPath = cmdlet.SessionState.Path.GetUnresolvedProviderPathFromPSPath(literalPath,
                    out provider, out PSDriveInfo drive);
                if (provider.ImplementingType == typeof(FileSystemProvider))
                    return new[] { providerPath };
            }

            return new string[0];
        }

        internal static int GetPercent(this int part, int whole)
        {
            return (int)Math.Round(part / (double)whole * 100);
        }
    }
}
