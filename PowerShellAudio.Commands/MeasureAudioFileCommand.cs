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

using PowerShellAudio.Commands.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace PowerShellAudio.Commands
{
    [Cmdlet(VerbsDiagnostic.Measure, "AudioFile"), OutputType(typeof(AnalyzableAudioFile))]
    [PublicAPI]
    public sealed class MeasureAudioFileCommand : Cmdlet, IDisposable
    {
        readonly IList<AnalyzableAudioFile> _audioFiles = new List<AnalyzableAudioFile>();
        readonly CancellationTokenSource _cancelSource = new CancellationTokenSource();

        [Parameter(Mandatory = true, Position = 0)]
        public string Analyzer { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true)]
        public AudioFile AudioFile { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            _audioFiles.Add(new AnalyzableAudioFile(AudioFile));
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Non-terminating Cmdlet exceptions should be written to an ErrorRecord")]
        protected override void EndProcessing()
        {
            if (_audioFiles.Count <= 0) return;
            int completed = 0;

            using (var groupToken = new GroupToken(_audioFiles.Count))
            using (var outputQueue = new BlockingCollection<object>())
            {
                outputQueue.Add(new ProgressRecord(0,
                    string.Format(CultureInfo.CurrentCulture, Resources.MeasureAudioFileCommandActivityMessage,
                        Analyzer),
                    string.Format(CultureInfo.CurrentCulture, Resources.MeasureAudioFileCommandStatusMessage,
                        0, _audioFiles.Count)) {PercentComplete = 0});

                Task.Run(() => Parallel.ForEach(_audioFiles, new ParallelOptions { CancellationToken = _cancelSource.Token }, audioFile =>
                {
                    try
                    {
                        audioFile.Analyze(Analyzer, _cancelSource.Token, groupToken);
                        Interlocked.Increment(ref completed);

                        if (PassThru)
                            outputQueue.Add(audioFile);

                        outputQueue.Add(new ProgressRecord(0,
                            string.Format(CultureInfo.CurrentCulture, Resources.MeasureAudioFileCommandActivityMessage,
                                Analyzer),
                            string.Format(CultureInfo.CurrentCulture, Resources.MeasureAudioFileCommandStatusMessage,
                                completed, _audioFiles.Count))
                        {
                            PercentComplete = completed.GetPercent(_audioFiles.Count)
                        });
                    }
                    catch (Exception e)
                    {
                        Interlocked.Increment(ref completed);
                        outputQueue.Add(new ErrorRecord(e, e.HResult.ToString(CultureInfo.CurrentCulture), ErrorCategory.ReadError, audioFile));
                    }
                })).ContinueWith(task => outputQueue.CompleteAdding());

                // Process output on the main thread:
                this.ProcessOutput(outputQueue, _cancelSource.Token);
            }
        }

        protected override void StopProcessing()
        {
            _cancelSource.Cancel();
        }

        public void Dispose()
        {
            _cancelSource.Dispose();
        }
    }
}
