using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

public class CopyFileProcessor : IFileProcessor
{
    public CopyFileProcessor(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
    }

    public void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir)
    {
        var fileName = Path.GetFileName(filePath);
        var outputFilePath = Path.Combine(outputDir, fileName);
        this.fileSystem.File.Copy(filePath, outputFilePath, overwrite: true);
    }

    IFileSystem fileSystem;
}