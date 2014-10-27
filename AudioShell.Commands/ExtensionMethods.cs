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
using System.Collections.Concurrent;
using System.Management.Automation;
using System.Threading;

namespace AudioShell.Commands
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

        internal static int GetPercent(this int part, int whole)
        {
            return (int)Math.Round(part / (double)whole * 100);
        }
    }
}
