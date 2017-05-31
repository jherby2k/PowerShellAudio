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
    /// Represents an audio file that can be exported to other formats.
    /// </summary>
    /// <remarks>
    /// Consumers of the PowerShellAudio library typically create new <see cref="ExportableAudioFile"/> objects directly.
    /// During instantiation, the available extensions are polled according to file extension, and then attempt to read
    /// the file in turn. If no supporting extensions are found, the <see cref="ExportableAudioFile"/> is not created
    /// and an <see cref="UnsupportedAudioException"/> is thrown.
    /// </remarks>
    [Serializable]
    public class ExportableAudioFile : TaggedAudioFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportableAudioFile"/> class from an existing
        /// <see cref="AudioFile"/>.
        /// </summary>
        /// <param name="audioFile">The audio file to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="audioFile"/> is null.</exception>
        public ExportableAudioFile([NotNull] AudioFile audioFile)
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
        public ExportableAudioFile([NotNull] FileInfo fileInfo)
            : base(fileInfo)
        {
        }

        /// <summary>
        /// Exports to a new <see cref="ExportableAudioFile"/> using the specified encoder using the provided settings.
        /// </summary>
        /// <param name="encoder">The name of the encoder.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="outputDirectory">The output directory, or <c>null</c> to use the same directory as the input.</param>
        /// <param name="outputFileName">The output file name, or <c>null</c> to use the same file name as the input.</param>
        /// <param name="replaceExisting">if set to <c>true</c>, replace the file if it already exists.</param>
        /// <returns>A new <see cref="ExportableAudioFile"/>, or null if the file already exists and <paramref name="replaceExisting"/> is false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="encoder"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an encoder with the specified name could not be found.</exception>
        /// <exception cref="UnsupportedAudioException">Thrown if no decoders were able to read this file.</exception>
        /// <exception cref="InvalidSettingException">Thrown if one of the setting values is invalid.</exception>
        [NotNull]
        public ExportableAudioFile Export(
            [NotNull] string encoder,
            [CanBeNull] SettingsDictionary settings = null,
            [CanBeNull] DirectoryInfo outputDirectory = null,
            [CanBeNull] string outputFileName = null, 
            bool replaceExisting = false)
        {
            return Export(encoder, CancellationToken.None, settings, outputDirectory, outputFileName, replaceExisting);
        }

        /// <summary>
        /// Exports to a new <see cref="ExportableAudioFile"/> using the specified encoder using the provided settings.
        /// </summary>
        /// <param name="encoder">The name of the encoder.</param>
        /// <param name="cancelToken">The cancellation token.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="outputDirectory">The output directory, or <c>null</c> to use the same directory as the input.</param>
        /// <param name="outputFileName">The output file name, or <c>null</c> to use the same file name as the input.</param>
        /// <param name="replaceExisting">if set to <c>true</c>, replace the file if it already exists.</param>
        /// <returns>A new <see cref="ExportableAudioFile"/>, or null if the file already exists and <paramref name="replaceExisting"/> is false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="encoder"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an encoder with the specified name could not be found.</exception>
        /// <exception cref="UnsupportedAudioException">Thrown if no decoders were able to read this file.</exception>
        /// <exception cref="InvalidSettingException">Thrown if one of the setting values is invalid.</exception>
        /// <exception cref="OperationCanceledException">Throw if the operation was canceled.</exception>
        [NotNull]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "outputDirectory must represent a directory")]
        public ExportableAudioFile Export(
            [NotNull] string encoder, 
            CancellationToken cancelToken,
            [CanBeNull] SettingsDictionary settings = null,
            [CanBeNull] DirectoryInfo outputDirectory = null,
            [CanBeNull] string outputFileName = null, 
            bool replaceExisting = false)
        {
            if (string.IsNullOrEmpty(encoder))
                throw new ArgumentException(Resources.ExportableAudioFileExportEncoderIsEmptyError, nameof(encoder));

            if (settings == null)
                settings = new SettingsDictionary();

            ExportFactory<ISampleEncoder> encoderFactory =
                ExtensionProvider.GetFactories<ISampleEncoder>("Name", encoder).SingleOrDefault();
            if (encoderFactory == null)
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, Resources.ExportableAudioFileFactoryError, encoder),
                    nameof(encoder));

            ExportLifetimeContext<ISampleEncoder> encoderLifetime = null;
            FileInfo outputFileInfo = null;
            FileInfo finalOutputFileInfo;
            FileStream outputStream = null;

            try
            {
                try
                {
                    encoderLifetime = encoderFactory.CreateExport();

                    ValidateSettings(settings, encoderLifetime.Value);

                    outputFileInfo = finalOutputFileInfo = GetOutputFileInfo(FileInfo, outputDirectory, outputFileName, encoderLifetime.Value);

                    // If the output file already exists, write to a temporary file first:
                    if (outputFileInfo.Exists)
                    {
                        outputFileInfo = new FileInfo(Path.Combine(outputFileInfo.DirectoryName ?? string.Empty, Path.GetRandomFileName()));

                        if (!replaceExisting)
                            throw new IOException(string.Format(CultureInfo.CurrentCulture,
                                Resources.ExportableAudioFileFileExistsError, finalOutputFileInfo.FullName));
                    }

                    outputStream = new FileStream(outputFileInfo.FullName, replaceExisting ? FileMode.Create : FileMode.CreateNew, FileAccess.ReadWrite);

                    DoExport(encoderLifetime.Value, outputStream, settings, cancelToken);
                }
                finally
                {
                    // Dispose the encoder before closing the output stream:
                    encoderLifetime?.Dispose();
                    outputStream?.Dispose();
                }
            }
            catch (Exception)
            {
                outputFileInfo?.Delete();
                throw;
            }

            // If using a temporary file, replace the original:
            if (outputFileInfo != finalOutputFileInfo)
            {
                finalOutputFileInfo.Delete();
                outputFileInfo.MoveTo(finalOutputFileInfo.FullName);
            }

            outputFileInfo.Refresh();
            return new ExportableAudioFile(outputFileInfo);
        }

        void DoExport(
            [NotNull] ISampleEncoder encoder, 
            [NotNull] Stream outputStream, 
            [NotNull] SettingsDictionary settings, 
            CancellationToken cancelToken)
        {
            encoder.Initialize(outputStream, AudioInfo, Metadata, settings);

            using (FileStream inputStream = FileInfo.OpenRead())
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

                            sampleDecoder.Initialize(inputStream);
                            sampleDecoder.ReadWriteParallel(encoder, cancelToken, encoder.ManuallyFreesSamples);

                            return;
                        }
                    }
                    catch (UnsupportedAudioException)
                    {
                        // If a decoder wasn't supported, rewind the stream and try another:
                        inputStream.Position = 0;
                    }
                }

                throw new UnsupportedAudioException(Resources.AudioFileDecodeError);
            }
        }

        static void ValidateSettings([NotNull] SettingsDictionary settings, [NotNull] ISampleEncoder encoder)
        {
            foreach (string unsupportedKey in settings.Keys.Where(setting =>
                !encoder.EncoderInfo.AvailableSettings.Contains(setting, StringComparer.OrdinalIgnoreCase)))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    Resources.ExportableAudioFileSettingsError, unsupportedKey));
        }

        [NotNull]
        static FileInfo GetOutputFileInfo(
            [NotNull] FileInfo inputFileInfo, 
            [CanBeNull] DirectoryInfo outputDirectory,
            [CanBeNull] string outputFileName,
            [NotNull] ISampleEncoder sampleEncoder)
        {
            // Use the input file name if the output name wasn't specified:
            if (string.IsNullOrEmpty(outputFileName))
                outputFileName = Path.GetFileNameWithoutExtension(inputFileInfo.Name);

            // Use the input file's directory if the output directory wasn't specified:
            if (outputDirectory == null)
                outputDirectory = new DirectoryInfo(inputFileInfo.DirectoryName ?? string.Empty);
            else
                outputDirectory.Create();

            return new FileInfo(Path.Combine(outputDirectory.FullName, outputFileName + sampleEncoder.EncoderInfo.FileExtension));
        }
    }
}
