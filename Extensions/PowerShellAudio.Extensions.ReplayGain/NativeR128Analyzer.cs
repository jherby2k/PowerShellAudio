﻿/*
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

using PowerShellAudio.Extensions.ReplayGain.Properties;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PowerShellAudio.Extensions.ReplayGain
{
    class NativeR128Analyzer : IDisposable
    {
        static readonly ConcurrentDictionary<GroupToken, NativeR128GroupState> _globalHandles =
            new ConcurrentDictionary<GroupToken, NativeR128GroupState>();

        readonly uint _channels;
        readonly GroupToken _groupToken;
        readonly NativeStateHandle _handle;
        readonly NativeR128GroupState _groupState;

        internal NativeR128Analyzer(uint channels, uint sampleRate, GroupToken groupToken)
        {
            Contract.Requires(channels > 0);
            Contract.Requires(groupToken != null);
            Contract.Ensures(_channels == channels);
            Contract.Ensures(_groupToken == groupToken);
            Contract.Ensures(_handle != null);
            Contract.Ensures(!_handle.IsInvalid);
            Contract.Ensures(_groupState != null);

            _channels = channels;
            _groupToken = groupToken;
            _handle = SafeNativeMethods.Initialize(channels, sampleRate, Mode.Global | Mode.SamplePeak);
            _groupState = _globalHandles.GetOrAdd(groupToken, new NativeR128GroupState());
            _groupState.Handles.Add(_handle);
        }

        internal void AddFrames(float[] frames)
        {
            Contract.Requires(frames != null);

            Ebur128Error result = SafeNativeMethods.AddFrames(_handle, frames,
                new UIntPtr((uint)frames.Length / _channels));
            if (result != Ebur128Error.Success)
                throw new IOException(string.Format(CultureInfo.CurrentCulture,
                    Resources.NativeAnalyzerAddFramesError, result));
        }

        internal double GetLoudness()
        {
            double loudness;
            Ebur128Error result = SafeNativeMethods.GetLoudness(_handle, out loudness);
            if (result != Ebur128Error.Success)
                throw new IOException(string.Format(CultureInfo.CurrentCulture,
                    Resources.NativeAnalyzerGetLoudnessError, result));
            return loudness;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification = "Passing an array of SafeHandles isn't supported.")]
        internal double GetLoudnessMultiple()
        {
            IntPtr[] handles = _groupState.Handles.ToArray().Select(handle => handle.DangerousGetHandle()).ToArray();

            double loudness;
            Ebur128Error result = SafeNativeMethods.GetLoudnessMultiple(handles, new UIntPtr((uint)handles.Length),
                out loudness);
            if (result != Ebur128Error.Success)
                throw new IOException(string.Format(CultureInfo.CurrentCulture,
                    Resources.NativeAnalyzerGetLoudnessError, result));
            return loudness;
        }

        internal double GetSamplePeak()
        {
            double combinedPeak = 0;

            for (uint channel = 0; channel < _channels; channel++)
            {
                double channelPeak;
                Ebur128Error result = SafeNativeMethods.GetSamplePeak(_handle, channel, out channelPeak);
                if (result != Ebur128Error.Success)
                    throw new IOException(string.Format(CultureInfo.CurrentCulture,
                        Resources.NativeAnalyzerGetLoudnessError, result));
                combinedPeak = Math.Max(combinedPeak, channelPeak);
            }

            return combinedPeak;
        }

        internal double GetSamplePeakMultiple()
        {
            double combinedPeak = 0;

            foreach (NativeStateHandle handle in _groupState.Handles)
                for (uint channel = 0; channel < _channels; channel++)
                {
                    double channelPeak;
                    Ebur128Error result = SafeNativeMethods.GetSamplePeak(handle, channel, out channelPeak);
                    if (result != Ebur128Error.Success)
                        throw new IOException(string.Format(CultureInfo.CurrentCulture, 
                            Resources.NativeAnalyzerGetLoudnessError, result));
                    combinedPeak = Math.Max(combinedPeak, channelPeak);
                }

            return combinedPeak;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            _groupState.MemberDisposed();

            if (_groupToken.Count != _groupState.MembersDisposed)
                return;

            // Dispose all the handles at once:
            NativeStateHandle handle;
            while (_groupState.Handles.TryTake(out handle))
                handle.Dispose();

            // Remove the group from the global list:
            NativeR128GroupState groupState;
            _globalHandles.TryRemove(_groupToken, out groupState);
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(_channels > 0);
            Contract.Invariant(_groupToken != null);
            Contract.Invariant(_handle != null);
            Contract.Invariant(!_handle.IsInvalid);
            Contract.Invariant(_groupState != null);
        }
    }
}
