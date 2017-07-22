using System.IO;
using JetBrains.Annotations;

namespace PowerShellAudio.UnitTests
{
    public class TestFilesFixture
    {
        internal string WorkingDirectory => Directory.GetCurrentDirectory();

        public TestFilesFixture()
        {
            CopyDirectory(Path.Combine(WorkingDirectory, @"..\..\TestFiles"), "TestFiles");
        }

        static void CopyDirectory([NotNull] string source, [NotNull] string destination)
        {
            // Get the subdirectories for the specified directory
            var sourceDir = new DirectoryInfo(source);

            if (!sourceDir.Exists)
                throw new DirectoryNotFoundException(
                    $"Source directory does not exist or could not be found: {source}");

            Directory.CreateDirectory(destination);

            // Get the files in the directory and copy them to the new location
            foreach (FileInfo file in sourceDir.GetFiles())
                file.CopyTo(Path.Combine(destination, file.Name), true);

            // Recurse through subdirectories
            foreach (DirectoryInfo subdir in sourceDir.GetDirectories())
                CopyDirectory(subdir.FullName, Path.Combine(destination, subdir.Name));
        }
    }
}