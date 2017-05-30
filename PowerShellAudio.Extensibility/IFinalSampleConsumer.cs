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

namespace PowerShellAudio
{
    /// <summary>
    /// Represents any extension that consumes <see cref="SampleCollection"/>s. All <see cref="SampleCollection"/>s
    /// are considered unsuable after they are consumed.
    /// </summary>
    public interface IFinalSampleConsumer : ISampleConsumer
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IFinalSampleConsumer"/> manually frees the samples after
        /// submission.
        /// </summary>
        /// <remarks>
        /// Normally the PowerShellAudio runtime will free each <see cref="SampleCollection"/> after the call to Submit
        /// returns. If your <see cref="IFinalSampleConsumer"/> continues to operate on the samples after returning,
        /// set this property to <c>true</c>. You will be responsible for calling Free on the
        /// <see cref="SampleCollectionFactory"/> instance once the samples are no longer needed.
        /// </remarks>
        /// <value>
        /// <c>true</c> if this <see cref="IFinalSampleConsumer"/> manually frees the samples; otherwise, <c>false</c>.
        /// </value>
        bool ManuallyFreesSamples { get; }
    }
}
