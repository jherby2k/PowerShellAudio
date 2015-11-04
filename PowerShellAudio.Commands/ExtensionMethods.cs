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

using Microsoft.PowerShell.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Threading;

namespace PowerShellAudio.Commands
{
    internal static class ExtensionMethods
    {
        internal static void ProcessOutput(this Cmdlet cmdlet, BlockingCollection<object> outputQueue, CancellationToken cancelToken)
        {
            foreach (object queuedObject in outputQueue.GetConsumingEnumerable(cancelToken))
            {
                var queuedError = queuedObject as ErrorRecord;
                if (queuedError != null)
                    cmdlet.WriteError(queuedError);
                else
                {
                    var queuedProgress = queuedObject as ProgressRecord;
                    if (queuedProgress != null)
                        cmdlet.WriteProgress(queuedProgress);
                    else
                        cmdlet.WriteObject(queuedObject);
                }
            }
        }

        internal static IEnumerable<string> GetFileSystemPaths(this PSCmdlet cmdlet, string path, string literalPath)
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
                PSDriveInfo drive;

                string providerPath = cmdlet.SessionState.Path.GetUnresolvedProviderPathFromPSPath(literalPath,
                    out provider, out drive);
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
