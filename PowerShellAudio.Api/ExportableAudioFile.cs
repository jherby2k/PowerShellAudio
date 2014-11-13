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

using PowerShellAudio.Properties;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

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
    public class ExportableAudioFile : TaggedAudioFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportableAudioFile"/> class from an existing
        /// <see cref="AudioFile"/>.
        /// </summary>
        /// <param name="audioFile">The audio file to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="audioFile"/> is null.</exception>
        public ExportableAudioFile(AudioFile audioFile)
            : base(audioFile)
        {
            Contract.Requires<ArgumentNullException>(audioFile != null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyzableAudioFile"/> class.
        /// </summary>
        /// <param name="fileInfo">The file information.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="fileInfo"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fileInfo"/> does not have an extension.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="fileInfo"/> is an empty file.</exception>
        /// <exception cref="FileNotFoundException">Thrown if <paramref name="fileInfo"/> does not exist.</exception>
        /// <exception cref="UnsupportedAudioException">
        /// Thrown if no available extensions are able to read the file.
        /// </exception>
        /// <exception cref="IOException">Thrown if an error occurs while reading the file stream.</exception>
        public ExportableAudioFile(FileInfo fileInfo)
            : base(fileInfo)
        {
            Contract.Requires<ArgumentNullException>(fileInfo != null);
            Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(fileInfo.Extension));
            Contract.Requires<FileNotFoundException>(fileInfo.Exists);
            Contract.Requires<ArgumentOutOfRangeException>(fileInfo.Length > 0);
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
        public ExportableAudioFile Export(string encoder, SettingsDictionary settings = null, DirectoryInfo outputDirectory = null, string outputFileName = null, bool replaceExisting = false)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(encoder));
            Contract.Ensures(Contract.Result<ExportableAudioFile>() != null);

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
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "outputDirectory must represent a directory")]
        public ExportableAudioFile Export(string encoder, CancellationToken cancelToken, SettingsDictionary settings = null, DirectoryInfo outputDirectory = null, string outputFileName = null, bool replaceExisting = false)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(encoder));
            Contract.Ensures(Contract.Result<ExportableAudioFile>() != null);

            if (settings == null)
                settings = new SettingsDictionary();

            var encoderFactory = ExtensionProvider.GetFactories<ISampleEncoder>().Where(factory => string.Compare((string)factory.Metadata["Name"], encoder, StringComparison.OrdinalIgnoreCase) == 0).SingleOrDefault();
            if (encoderFactory == null)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExportableAudioFileFactoryError, encoder), "encoder");

            ExportLifetimeContext<ISampleEncoder> encoderLifetime = null;
            FileInfo outputFileInfo = null;
            FileInfo finalOutputFileInfo = null;
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
                        outputFileInfo = new FileInfo(Path.Combine(outputFileInfo.DirectoryName, Path.GetRandomFileName()));

                        if (!replaceExisting)
                            throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.ExportableAudioFileFileExistsError, finalOutputFileInfo.FullName));
                    }

                    outputStream = new FileStream(outputFileInfo.FullName, replaceExisting ? FileMode.Create : FileMode.CreateNew, FileAccess.ReadWrite);

                    DoExport(encoderLifetime.Value, outputStream, settings, cancelToken);
                }
                finally
                {
                    // Dispose the encoder before closing the output stream:
                    if (encoderLifetime != null)
                        encoderLifetime.Dispose();
                    if (outputStream != null)
                        outputStream.Dispose();
                }
            }
            catch (Exception)
            {
                outputFileInfo.Delete();
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

        void DoExport(ISampleEncoder encoder, Stream outputStream, SettingsDictionary settings, CancellationToken cancelToken)
        {
            Contract.Requires(encoder != null);
            Contract.Requires(outputStream != null);
            Contract.Requires(settings != null);

            encoder.Initialize(outputStream, AudioInfo, Metadata, settings);

            using (FileStream inputStream = FileInfo.OpenRead())
            {
                // Try each decoder that supports this file extension:
                foreach (var decoderFactory in ExtensionProvider.GetFactories<ISampleDecoder>().Where(factory => string.Compare((string)factory.Metadata["Extension"], FileInfo.Extension, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    try
                    {
                        using (var decoderLifetime = decoderFactory.CreateExport())
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

        static void ValidateSettings(SettingsDictionary settings, ISampleEncoder encoder)
        {
            Contract.Requires(settings != null);
            Contract.Requires(encoder != null);

            foreach (var unsupportedKey in settings.Keys.Where(setting => !encoder.AvailableSettings.Contains(setting, StringComparer.OrdinalIgnoreCase)))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExportableAudioFileSettingsError, unsupportedKey));
        }

        static FileInfo GetOutputFileInfo(FileInfo inputFileInfo, DirectoryInfo outputDirectory, string outputFileName, ISampleEncoder sampleEncoder)
        {
            Contract.Requires(inputFileInfo != null);
            Contract.Requires(sampleEncoder != null);
            Contract.Ensures(Contract.Result<FileInfo>() != null);

            // Use the input file name if the output name wasn't specified:
            if (string.IsNullOrEmpty(outputFileName))
                outputFileName = Path.GetFileNameWithoutExtension(inputFileInfo.Name);

            // Use the input file's directory if the output directory wasn't specified:
            if (outputDirectory == null)
                outputDirectory = new DirectoryInfo(inputFileInfo.DirectoryName);
            else
                outputDirectory.Create();

            return new FileInfo(Path.Combine(outputDirectory.FullName, outputFileName + sampleEncoder.Extension));
        }
    }
}
