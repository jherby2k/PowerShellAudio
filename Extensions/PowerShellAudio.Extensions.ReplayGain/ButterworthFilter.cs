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

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace PowerShellAudio.Extensions.ReplayGain
{
    class ButterworthFilter : EqualLoudnessFilter
    {
        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member", Justification = "Does not waste space")]
        static readonly float[,] _a = {{ 0.986211925f, -1.97242385f, 0.986211925f },
                                       { 0.985001758f, -1.97000352f, 0.985001758f },
                                       { 0.982524008f, -1.96504802f, 0.982524008f },
                                       { 0.981658268f, -1.96331654f, 0.981658268f },
                                       { 0.979389327f, -1.95877865f, 0.979389327f },
                                       { 0.976479817f, -1.95295963f, 0.976479817f },
                                       { 0.975318432f, -1.95063686f, 0.975318432f },
                                       { 0.973165235f, -1.94633047f, 0.973165235f },
                                       { 0.965353268f, -1.93070654f, 0.965353268f },
                                       { 0.964545156f, -1.92909031f, 0.964545156f },
                                       { 0.960091430f, -1.92018286f, 0.960091430f },
                                       { 0.958569166f, -1.91713833f, 0.958569166f },
                                       { 0.945976856f, -1.89195371f, 0.945976856f }};

        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member", Justification = "Does not waste space")]
        static readonly float[,] _b = {{ -1.97223373f, 0.972613969f },
                                       { -1.96977856f, 0.970228476f },
                                       { -1.96474258f, 0.965353450f },
                                       { -1.96298009f, 0.963652984f },
                                       { -1.95835381f, 0.959203500f },
                                       { -1.95240636f, 0.953512909f },
                                       { -1.95002759f, 0.951246137f },
                                       { -1.94561024f, 0.947050704f },
                                       { -1.92950578f, 0.931907293f },
                                       { -1.92783287f, 0.930347752f },
                                       { -1.91858953f, 0.921776188f },
                                       { -1.91542108f, 0.918855583f },
                                       { -1.88903308f, 0.894874345f }};

        internal ButterworthFilter(int sampleRate)
            : base(sampleRate, _a, _b)
        { }
    }
}
