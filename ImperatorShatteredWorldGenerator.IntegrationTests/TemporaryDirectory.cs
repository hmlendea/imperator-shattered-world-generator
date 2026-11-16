using System;
using System.IO;

namespace ImperatorShatteredWorldGenerator.IntegrationTests
{
    internal sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            DirectoryPath = Path.Combine(
                Path.GetTempPath(),
                $"ImperatorShatteredWorldGenerator-{Guid.NewGuid():N}");

            Directory.CreateDirectory(DirectoryPath);
        }

        public string DirectoryPath { get; }

        public string GetPath(string relativePath)
            => Path.Combine([DirectoryPath, .. Split(relativePath)]);

        public void CreateDirectory(string relativePath)
            => Directory.CreateDirectory(GetPath(relativePath));

        public void WriteFile(string relativePath, string content)
        {
            string filePath = GetPath(relativePath);
            string directoryPath = Path.GetDirectoryName(filePath)!;

            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(filePath, content);
        }

        public string ReadFile(string relativePath) => File.ReadAllText(GetPath(relativePath));

        public bool FileExists(string relativePath) => File.Exists(GetPath(relativePath));

        public bool DirectoryExists(string relativePath) => Directory.Exists(GetPath(relativePath));

        public void Dispose()
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, true);
            }
        }

        private static string[] Split(string relativePath)
            => relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
    }
}
