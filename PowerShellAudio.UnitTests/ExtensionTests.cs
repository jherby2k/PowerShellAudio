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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace PowerShellAudio.UnitTests
{
    [TestClass]
    public class ExtensionTests
    {
        [UsedImplicitly]
        public TestContext TestContext { get; set; }

#if DEBUG
        [DeploymentItem(@"..\..\..\Extensions\bin\Debug", "Extensions")]
#else
        [DeploymentItem(@"..\..\..\Extensions\bin\Release", "Extensions")]
#endif
        [DeploymentItem(@"..\..\TestFiles", "TestFiles")]
        [DeploymentItem(@"..\..\AudioFileExportDataSource.csv")]
        [TestMethod, DataSource("System.Data.Odbc", "Driver={Microsoft Access Text Driver (*.txt, *.csv)};Dbq=|DataDirectory|;Extensions=csv", "AudioFileExportDataSource#csv", DataAccessMethod.Sequential)]
        public void AudioFileExport()
        {
            var index = Convert.ToInt32(TestContext.DataRow["Index"]);
            var fileName = Path.Combine(TestContext.DeploymentDirectory, "TestFiles", Convert.ToString(TestContext.DataRow["FileName"]));
            var encoder = Convert.ToString(TestContext.DataRow["Encoder"]);
            var settings = ConvertToDictionary(Convert.ToString(TestContext.DataRow["Settings"]));
            var metadata = ConvertToDictionary(Convert.ToString(TestContext.DataRow["Metadata"]));
            var coverArt = Convert.ToString(TestContext.DataRow["CoverArt"]);
            var expectedHash = Convert.ToString(TestContext.DataRow[Environment.Is64BitProcess ? "ExpectedHashX64" : "ExpectedHashX86"]);

            if (!string.IsNullOrEmpty(expectedHash))
            {
                var input = new ExportableAudioFile(new FileInfo(fileName));
                metadata.CopyTo(input.Metadata);
                if (!string.IsNullOrEmpty(coverArt))
                    input.Metadata.CoverArt = new CoverArt(new FileInfo(Path.Combine(TestContext.DeploymentDirectory, "TestFiles", coverArt)));
                var result = input.Export(encoder, settings, new DirectoryInfo(TestContext.DeploymentDirectory), "Export Row " + index);

                Assert.AreEqual(expectedHash, CalculateHash(result));
            }
        }

#if DEBUG
        [DeploymentItem(@"..\..\..\Extensions\bin\Debug", "Extensions")]
#else
        [DeploymentItem(@"..\..\..\Extensions\bin\Release", "Extensions")]
#endif
        [DeploymentItem(@"..\..\TestFiles", "TestFiles")]
        [DeploymentItem(@"..\..\AudioFileAnalyzeDataSource.csv")]
        [TestMethod, DataSource("System.Data.Odbc", "Driver={Microsoft Access Text Driver (*.txt, *.csv)};Dbq=|DataDirectory|;Extensions=csv", "AudioFileAnalyzeDataSource#csv", DataAccessMethod.Sequential)]
        public void AudioFileAnalyze()
        {
            var fileName = Path.Combine(TestContext.DeploymentDirectory, "TestFiles", Convert.ToString(TestContext.DataRow["FileName"]));
            var analyzer = Convert.ToString(TestContext.DataRow["Analyzer"]);
            var expectedMetadata = Convert.ToString(TestContext.DataRow["ExpectedMetadata"]);

            var input = new AnalyzableAudioFile(new FileInfo(fileName));
            input.Analyze(analyzer);

            Assert.AreEqual(expectedMetadata, ConvertToString(input.Metadata));
        }

        #if DEBUG
        [DeploymentItem(@"..\..\..\Extensions\bin\Debug", "Extensions")]
#else
        [DeploymentItem(@"..\..\..\Extensions\bin\Release", "Extensions")]
#endif
        [DeploymentItem(@"..\..\TestFiles", "TestFiles")]
        [DeploymentItem(@"..\..\AudioFileSaveMetadataDataSource.csv")]
        [TestMethod, DataSource("System.Data.Odbc", "Driver={Microsoft Access Text Driver (*.txt, *.csv)};Dbq=|DataDirectory|;Extensions=csv", "AudioFileSaveMetadataDataSource#csv", DataAccessMethod.Sequential)]
        public void AudioFileSaveMetadata()
        {
            var index = Convert.ToInt32(TestContext.DataRow["Index"]);
            var fileName = Path.Combine(TestContext.DeploymentDirectory, "TestFiles", Convert.ToString(TestContext.DataRow["FileName"]));
            var settings = ConvertToDictionary(Convert.ToString(TestContext.DataRow["Settings"]));
            var metadata = ConvertToDictionary(Convert.ToString(TestContext.DataRow["Metadata"]));
            var coverArt = Convert.ToString(TestContext.DataRow["CoverArt"]);
            var expectedHash = Convert.ToString(TestContext.DataRow["ExpectedHash"]);

            var input = new TaggedAudioFile(new FileInfo(fileName).CopyTo("Save Metadata Row " + index + Path.GetExtension(fileName)));
            metadata.CopyTo(input.Metadata);
            if (!string.IsNullOrEmpty(coverArt))
                input.Metadata.CoverArt = new CoverArt(new FileInfo(Path.Combine(TestContext.DeploymentDirectory, "TestFiles", coverArt)));
            input.SaveMetadata(settings);

            Assert.AreEqual(expectedHash, CalculateHash(input));
        }

        [Pure, NotNull]
        static SettingsDictionary ConvertToDictionary([NotNull] string settings)
        {
            var result = new SettingsDictionary();

            foreach (string item in settings.Split(';'))
            {
                string[] keyAndValue = item.Split('=');
                if (keyAndValue.Length == 2)
                    result.Add(keyAndValue[0], keyAndValue[1]);
            }

            return result;
        }

        [Pure, NotNull]
        static string ConvertToString([NotNull] SettingsDictionary settings)
        {
            var result = new StringBuilder();

            foreach (var item in settings)
            {
                if (result.Length > 0)
                    result.Append(';');
                result.Append(item.Key);
                result.Append('=');
                result.Append(item.Value);
            }

            return result.ToString();
        }

        [Pure, NotNull]
        static string CalculateHash([NotNull] AudioFile audioFile)
        {
            using (MD5 md5 = MD5.Create())
            using (FileStream fileStream = audioFile.FileInfo.OpenRead())
            {
                byte[] hashBytes = md5.ComputeHash(fileStream);
                return BitConverter.ToString(hashBytes);
            }
        }
    }
}
