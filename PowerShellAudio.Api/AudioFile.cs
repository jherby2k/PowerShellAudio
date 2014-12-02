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
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

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
    public class AudioFile
    {
        /// <summary>
        /// Gets the file information.
        /// </summary>
        /// <value>
        /// The file information.
        /// </value>
        public FileInfo FileInfo { get; private set; }

        /// <summary>
        /// Gets the audio information.
        /// </summary>
        /// <value>
        /// The audio information.
        /// </value>
        public AudioInfo AudioInfo { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioFile"/> class from an existing <see cref="AudioFile"/>.
        /// </summary>
        /// <param name="audioFile">The audio file to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="audioFile"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="audioFile"/> does not have a file extension.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="audioFile"/> is an empty file.</exception>
        /// <exception cref="FileNotFoundException">Thrown if <paramref name="audioFile"/> does not exist.</exception>
        public AudioFile(AudioFile audioFile)
        {
            Contract.Requires<ArgumentNullException>(audioFile != null);
            Contract.Requires<ArgumentException>(audioFile.FileInfo != null);
            Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(audioFile.FileInfo.Extension));
            Contract.Requires<FileNotFoundException>(audioFile.FileInfo.Exists);
            Contract.Requires<ArgumentOutOfRangeException>(audioFile.FileInfo.Length > 0);
            Contract.Requires<ArgumentException>(audioFile.AudioInfo != null);

            FileInfo = audioFile.FileInfo;
            AudioInfo = audioFile.AudioInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioFile"/> class.
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
        public AudioFile(FileInfo fileInfo)
        {
            Contract.Requires<ArgumentNullException>(fileInfo != null);
            Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(fileInfo.Extension));
            Contract.Requires<FileNotFoundException>(fileInfo.Exists);
            Contract.Requires<ArgumentOutOfRangeException>(fileInfo.Length > 0);
            Contract.Ensures(FileInfo != null);
            Contract.Ensures(FileInfo == fileInfo);
            Contract.Ensures(AudioInfo != null);

            FileInfo = fileInfo;
            LoadAudioInfo();
        }

        /// <summary>
        /// Renames the <see cref="AudioFile"/> in it's current directory.
        /// </summary>
        /// <param name="fileName">The new, unqualified name of the file.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="fileName"/> is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fileName"/> contains path information.</exception>
        public void Rename(string fileName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(fileName));
            Contract.Requires<ArgumentException>(!fileName.Contains(Path.DirectorySeparatorChar));
            Contract.Ensures(!string.IsNullOrEmpty(FileInfo.Extension));
            Contract.Ensures(FileInfo.Exists);

            string newFileName = Path.Combine(FileInfo.DirectoryName, fileName);

            // If no extension was specified, append the current one:
            if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                newFileName += FileInfo.Extension;

            FileInfo.MoveTo(newFileName);

            FileInfo = new FileInfo(newFileName);
        }

        void LoadAudioInfo()
        {
            Contract.Ensures(AudioInfo != null);

            using (FileStream fileStream = FileInfo.OpenRead())
            {
                // Try each info decoder that supports this file extension:
                foreach (var decoderFactory in ExtensionProvider.GetFactories<IAudioInfoDecoder>("Extension", FileInfo.Extension))
                {
                    try
                    {
                        using (ExportLifetimeContext<IAudioInfoDecoder> lifetimeContext = decoderFactory.CreateExport())
                            AudioInfo = lifetimeContext.Value.ReadAudioInfo(fileStream);
                        return;
                    }
                    catch (UnsupportedAudioException)
                    {
                        // If a decoder wasn't supported, rewind the stream and try another:
                        fileStream.Position = 0;
                    }
                }
            }

            throw new UnsupportedAudioException(String.Format(Resources.AudioFileDecodeError, FileInfo.FullName));
        }
    }
}
