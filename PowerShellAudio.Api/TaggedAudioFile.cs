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
using System.Globalization;
using System.IO;
using System.Linq;

namespace PowerShellAudio
{
    /// <summary>
    /// Represents an audio file that can contain metadata ("tags") within the Windows file system.
    /// </summary>
    /// <remarks>
    /// Consumers of the PowerShellAudio library typically create new <see cref="TaggedAudioFile"/> objects directly. During
    /// instantiation, the available extensions are polled according to file extension, and then attempt to read the
    /// file in turn. If no supporting extensions are found, the <see cref="TaggedAudioFile"/> is not created and an
    /// <see cref="UnsupportedAudioException"/> is thrown.
    /// </remarks>
    public class TaggedAudioFile : AudioFile
    {
        MetadataDictionary _metadata;

        /// <summary>
        /// Gets the metadata. If metadata has not been loaded yet, <see cref="LoadMetadata()"/> is automatically called.
        /// </summary>
        /// <value>The metadata.</value>
        public MetadataDictionary Metadata
        {
            get
            {
                Contract.Ensures(Contract.Result<MetadataDictionary>() != null);

                // Metadata is loaded on demand:
                if (_metadata == null)
                    LoadMetadata();
                return _metadata;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaggedAudioFile"/> class from an existing
        /// <see cref="AudioFile"/>.
        /// </summary>
        /// <param name="audioFile">The audio file to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="audioFile"/> is null.</exception>
        public TaggedAudioFile(AudioFile audioFile)
            : base(audioFile)
        {
            Contract.Requires<ArgumentNullException>(audioFile != null);

            TaggedAudioFile taggedAudioFile = audioFile as TaggedAudioFile;
            if (taggedAudioFile != null)
                _metadata = taggedAudioFile.Metadata;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaggedAudioFile"/> class.
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
        public TaggedAudioFile(FileInfo fileInfo)
            : base(fileInfo)
        {
            Contract.Requires<ArgumentNullException>(fileInfo != null);
            Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(fileInfo.Extension));
            Contract.Requires<FileNotFoundException>(fileInfo.Exists);
            Contract.Requires<ArgumentOutOfRangeException>(fileInfo.Length > 0);
        }

        /// <summary>
        /// Loads the metadata, using an available <see cref="IMetadataDecoder"/>. If no extensions are able to read
        /// the file, the <see cref="Metadata"/> property will be initialized to an empty
        /// <see cref="MetadataDictionary"/>.
        /// </summary>
        /// <exception cref="IOException">Thrown if an error occurs while reading the file stream.</exception>
        public void LoadMetadata()
        {
            Contract.Ensures(_metadata != null);

            using (FileStream fileStream = FileInfo.OpenRead())
                LoadMetadata(fileStream);
        }

        /// <summary>
        /// Saves the metadata, using an available <see cref="IMetadataEncoder"/>. If no extensions are able to write
        /// to this file extension, an <see cref="UnsupportedAudioException"/>
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <exception cref="UnsupportedAudioException">
        /// No metadata encoders are able to save metadata in the required format.
        /// </exception>
        public void SaveMetadata(SettingsDictionary settings = null)
        {
            if (settings == null)
                settings = new SettingsDictionary();

            using (FileStream fileStream = FileInfo.Open(FileMode.Open, FileAccess.ReadWrite))
            {
                // Ensure the existing metadata has been loaded:
                if (_metadata == null)
                {
                    LoadMetadata(fileStream);
                    fileStream.Position = 0;
                }

                // Try each encoder that supports the file extension:
                foreach (var encoderFactory in ExtensionProvider.GetFactories<IMetadataEncoder>().Where(factory => string.Compare((string)factory.Metadata["Extension"], FileInfo.Extension, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    using (ExportLifetimeContext<IMetadataEncoder> lifetimeContext = encoderFactory.CreateExport())
                    {
                        IMetadataEncoder encoder = lifetimeContext.Value;
                        ValidateSettings(settings, encoder);
                        encoder.WriteMetadata(fileStream, Metadata, settings);

                        return;
                    }
                }
            }

            throw new UnsupportedAudioException(Resources.TaggedAudioFileUnsupportedError);
        }

        static void ValidateSettings(SettingsDictionary settings, IMetadataEncoder encoder)
        {
            Contract.Requires(settings != null);
            Contract.Requires(encoder != null);

            foreach (var unsupportedKey in settings.Keys.Where(setting => !encoder.AvailableSettings.Contains(setting, StringComparer.OrdinalIgnoreCase)))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.TaggedAudioFileSettingsError, unsupportedKey));
        }

        void LoadMetadata(Stream stream)
        {
            Contract.Requires(stream != null);
            Contract.Requires(stream.CanRead);
            Contract.Requires(stream.CanSeek);
            Contract.Ensures(_metadata != null);

            // Try each decoder that supports this file extension:
            foreach (var decoderFactory in ExtensionProvider.GetFactories<IMetadataDecoder>().Where(factory => string.Compare((string)factory.Metadata["Extension"], FileInfo.Extension, StringComparison.OrdinalIgnoreCase) == 0))
            {
                try
                {
                    using (ExportLifetimeContext<IMetadataDecoder> lifetimeContext = decoderFactory.CreateExport())
                        _metadata = lifetimeContext.Value.ReadMetadata(stream);
                    return;
                }
                catch (UnsupportedAudioException)
                {
                    // If a decoder wasn't supported, rewind the stream and try another:
                    stream.Position = 0;
                }
            }

            _metadata = new MetadataDictionary();
        }
    }
}
