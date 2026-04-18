namespace BlogEngine;

using System.Collections.Generic;
using System.IO.Abstractions;

using BlogEngine.FileProcessors;

using Microsoft.Extensions.Logging;

public interface IDirectoryProcessor
{
    public void Process(Dictionary<string, Post> posts, string inputDir, string outputDir);
}

public class DirectoryProcessor(IFileSystem fileSystem, IFileProcessor fileProcessor, ILogger<DirectoryProcessor> logger) : IDirectoryProcessor
{
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

    private readonly IFileSystem fileSystem = fileSystem;
    private readonly IFileProcessor fileProcessor = fileProcessor;
    private readonly ILogger logger = logger;
}