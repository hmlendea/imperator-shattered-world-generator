using System;
using System.IO;

namespace ImperatorShatteredWorldGenerator.IntegrationTests
{
    internal sealed class WorkingDirectoryScope : IDisposable
    {
        private readonly string originalDirectoryPath;

        public WorkingDirectoryScope(string directoryPath)
        {
            originalDirectoryPath = Environment.CurrentDirectory;
            Environment.CurrentDirectory = directoryPath;
        }

        public void Dispose() => Environment.CurrentDirectory = originalDirectoryPath;
    }
}
