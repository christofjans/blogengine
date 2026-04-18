namespace BlogEngine.FileProcessors;

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

public class CopyFileProcessor(IFileSystem fileSystem) : IFileProcessor
{
    public void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir)
    {
        var fileName = Path.GetFileName(filePath);
        var outputFilePath = Path.Combine(outputDir, fileName);
        fileSystem.File.Copy(filePath, outputFilePath, overwrite: true);
    }
}