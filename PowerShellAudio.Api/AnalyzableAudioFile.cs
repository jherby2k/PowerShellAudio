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

using PowerShellAudio.Properties;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace PowerShellAudio
{
    /// <summary>
    /// Represents an audio file that can have it's audio stream analyzed.
    /// </summary>
    /// <remarks>
    /// Consumers of the PowerShellAudio library typically create new <see cref="AnalyzableAudioFile"/> objects directly.
    /// During instantiation, the available extensions are polled according to file extension, and then attempt to read
    /// the file in turn. If no supporting extensions are found, the <see cref="AnalyzableAudioFile"/> is not created
    /// and an <see cref="UnsupportedAudioException"/> is thrown.
    /// </remarks>
    [Serializable]
    public class AnalyzableAudioFile : TaggedAudioFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyzableAudioFile"/> class from an existing
        /// <see cref="AudioFile"/>.
        /// </summary>
        /// <param name="audioFile">The audio file to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="audioFile"/> is null.</exception>
        public AnalyzableAudioFile([NotNull] AudioFile audioFile)
            : base(audioFile)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyzableAudioFile"/> class.
        /// </summary>
        /// <param name="fileInfo">The file information.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="fileInfo"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="fileInfo"/> does not have an extension, the file does not exist, or the file is empty.
        /// </exception>
        /// <exception cref="UnsupportedAudioException">
        /// Thrown if no available extensions are able to read the file.
        /// </exception>
        /// <exception cref="IOException">Thrown if an error occurs while reading the file stream.</exception>
        public AnalyzableAudioFile([NotNull] FileInfo fileInfo)
            : base(fileInfo)
        {
        }

        /// <summary>
        /// Analyzes the <see cref="AnalyzableAudioFile"/> using the specified analyzer. The results are added to the
        /// Metadata property, possibly replacing any existing values. These additions are not persisted to disk until
        /// the SaveMetadata method is called.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="groupToken">The group token.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="groupToken"/> is null, or <paramref name="analyzer"/> is null or empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown if an analyzer with the specified name could not be found.</exception>
        /// <exception cref="UnsupportedAudioException">Thrown if no decoders were able to read this file.</exception>
        public void Analyze([NotNull] string analyzer, [CanBeNull] GroupToken groupToken = null)
        {
            Analyze(analyzer, CancellationToken.None, groupToken);
        }

        /// <summary>
        /// Analyzes the <see cref="AnalyzableAudioFile"/> using the specified analyzer. The results are added to the
        /// Metadata property, possibly replacing any existing values. These additions are not persisted to disk until
        /// the SaveMetadata method is called.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        /// <param name="cancelToken">The cancellation token.</param>
        /// <param name="groupToken">The group token.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="groupToken"/> is null, or <paramref name="analyzer"/> is null or empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown if an analyzer with the specified name could not be found.</exception>
        /// <exception cref="UnsupportedAudioException">Thrown if no decoders were able to read this file.</exception>
        /// <exception cref="OperationCanceledException">Throw if the operation was canceled.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Group token should only be disposed when owned by this method.")]
        public void Analyze([NotNull] string analyzer, CancellationToken cancelToken, [CanBeNull] GroupToken groupToken = null)
        {
            if (string.IsNullOrEmpty(analyzer))
                throw new ArgumentException(Resources.AnalyzableAudioFileAnalyzeAnalyzerIsEmptyError, nameof(analyzer));

            var groupTokenOwned = false;
            try
            {
                if (groupToken == null)
                {
                    groupToken = new GroupToken(1);
                    groupTokenOwned = true;
                }

                ExportFactory<ISampleAnalyzer> analyzerFactory =
                    ExtensionProvider.GetFactories<ISampleAnalyzer>("Name", analyzer).SingleOrDefault();
                if (analyzerFactory == null)
                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture, Resources.AnalyzableAudioFileFactoryError, analyzer),
                        nameof(analyzer));

                using (ExportLifetimeContext<ISampleAnalyzer> analyzerLifetime = analyzerFactory.CreateExport())
                    DoAnalyze(analyzerLifetime.Value, cancelToken, groupToken);
            }
            finally
            {
                if (groupTokenOwned)
                    groupToken.Dispose();
            }
        }

        void DoAnalyze([NotNull] ISampleAnalyzer sampleAnalyzer, CancellationToken cancelToken, [NotNull] GroupToken groupToken)
        {
            sampleAnalyzer.Initialize(AudioInfo, groupToken);

            using (FileStream fileStream = FileInfo.OpenRead())
            {
                // Try each decoder that supports this file extension:
                foreach (ExportFactory<ISampleDecoder> decoderFactory in
                    ExtensionProvider.GetFactories<ISampleDecoder>("Extension", FileInfo.Extension))
                {
                    try
                    {
                        using (ExportLifetimeContext<ISampleDecoder> decoderLifetime = decoderFactory.CreateExport())
                        {
                            ISampleDecoder sampleDecoder = decoderLifetime.Value;

                            sampleDecoder.Initialize(fileStream);
                            sampleDecoder.ReadWriteParallel(sampleAnalyzer, cancelToken,
                                sampleAnalyzer.ManuallyFreesSamples);
                            sampleAnalyzer.GetResult().CopyTo(Metadata);
                            return;
                        }
                    }
                    catch (UnsupportedAudioException)
                    {
                        // If a decoder wasn't supported, rewind the stream and try another:
                        fileStream.Position = 0;
                    }
                }
            }

            throw new UnsupportedAudioException(Resources.AudioFileDecodeError);
        }
    }
}
