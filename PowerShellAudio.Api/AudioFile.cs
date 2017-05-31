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
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace PowerShellAudio
{
    /// <summary>
    /// Represents an audio file within the Windows file system.
    /// </summary>
    /// <remarks>
    /// Consumers of the PowerShellAudio library typically create new <see cref="AudioFile"/> objects directly. During
    /// instantiation, the available extensions are polled according to file extension, and then attempt to read the
    /// file in turn. If no supporting extensions are found, the <see cref="AudioFile"/> is not created and an
    /// <see cref="UnsupportedAudioException"/> is thrown.
    /// </remarks>
    [Serializable]
    [PublicAPI]

    public class AudioFile
    {
        /// <summary>
        /// Gets the file information.
        /// </summary>
        /// <value>
        /// The file information.
        /// </value>
        [NotNull]
        public FileInfo FileInfo { get; private set; }

        /// <summary>
        /// Gets the audio information.
        /// </summary>
        /// <value>
        /// The audio information.
        /// </value>
        [NotNull]
        public AudioInfo AudioInfo { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioFile"/> class from an existing <see cref="AudioFile"/>.
        /// </summary>
        /// <param name="audioFile">The audio file to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="audioFile"/> is null.</exception>
        public AudioFile([NotNull] AudioFile audioFile)
        {
            if (audioFile == null) throw new ArgumentNullException(nameof(audioFile));

            FileInfo = audioFile.FileInfo;
            AudioInfo = audioFile.AudioInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioFile"/> class.
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
        public AudioFile([NotNull] FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
            if (string.IsNullOrEmpty(fileInfo.Extension))
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, Resources.AudioFileEmptyExtensionError, fileInfo),
                    nameof(fileInfo));
            if (!fileInfo.Exists)
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, Resources.AudioFileFileDoesNotExistError, fileInfo),
                    nameof(fileInfo));
            if (fileInfo.Length == 0)
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, Resources.AudioFileFileIsEmptyError, fileInfo),
                    nameof(fileInfo));

            FileInfo = fileInfo;
            AudioInfo = LoadAudioInfo(fileInfo);
        }

        /// <summary>
        /// Renames the <see cref="AudioFile"/> in it's current directory.
        /// </summary>
        /// <param name="fileName">The new, unqualified name of the file.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="fileName"/> is null, empty, or contains path information.
        /// </exception>
        public void Rename([NotNull] string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException(Resources.AudioFileRenameFileNameIsEmptyError, nameof(fileName));
            if (fileName.Contains(Path.DirectorySeparatorChar))
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, Resources.AudioFileRenameFileNameContainsPathError, fileName),
                    nameof(fileName));

            string newFileName = Path.Combine(FileInfo.DirectoryName ?? string.Empty, fileName);

            // If no extension was specified, append the current one:
            if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                newFileName += FileInfo.Extension;

            FileInfo.MoveTo(newFileName);
            FileInfo = new FileInfo(newFileName);
        }

        [NotNull]
        static AudioInfo LoadAudioInfo([NotNull] FileInfo fileInfo)
        {
            using (FileStream fileStream = fileInfo.OpenRead())
            {
                // Try each info decoder that supports this file extension:
                foreach (ExportFactory<IAudioInfoDecoder> decoderFactory in
                    ExtensionProvider.GetFactories<IAudioInfoDecoder>("Extension", fileInfo.Extension))
                {
                    try
                    {
                        using (ExportLifetimeContext<IAudioInfoDecoder> lifetimeContext = decoderFactory.CreateExport())
                            return lifetimeContext.Value.ReadAudioInfo(fileStream);
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
