using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

public interface IDirectoryProcessor
{
    public void Process(Dictionary<string,Post> posts, string inputDir, string outputDir);
}

public class DirectoryProcessor : IDirectoryProcessor
{
    public DirectoryProcessor(IFileSystem fileSystem, IFileProcessor fileProcessor, ILogger<DirectoryProcessor> logger)
    {
        this.fileSystem = fileSystem;
        this.fileProcessor = fileProcessor;
        this.logger = logger;
    }

    public void Process(Dictionary<string, Post> posts, string inputDir, string outputDir)
    {
        foreach (var filePath in this.fileSystem.Directory.GetFiles(inputDir))
        {
            ProcessFile(posts, filePath, outputDir);
            logger.LogInformation("processed {filePath}", filePath);
        }
    }

    private void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir) =>
        this.fileProcessor.ProcessFile(posts, filePath, outputDir);

    private IFileSystem fileSystem;
    private IFileProcessor fileProcessor;
    private ILogger logger;
}