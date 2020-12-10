using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.Logging;

public interface IProcessor
{
    void Process(string inputDir, string outputDir);
}

public class Processor : IProcessor
{
    public Processor(IFileSystem fileSystem, IFileProcessor fileProcessor, IPostFinder postFinder, ILogger<Processor> logger)
    {
        this.fileSystem = fileSystem;
        this.fileProcessor = fileProcessor;
        this.postFinder = postFinder;
        this.logger = logger;
    }

    public void Process(string inputDir, string outputDir)
    {
        var posts = GetPosts(inputDir).ToDictionary(p=>p.FilePath, p=>p);

        foreach (var filePath in this.fileSystem.Directory.GetFiles(inputDir))
        {
            ProcessFile(posts, filePath, outputDir);
            logger.LogInformation("processed {filePath}", filePath);
        }
    }

    private void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir) =>
        this.fileProcessor.ProcessFile(posts, filePath, outputDir);

    private IEnumerable<Post> GetPosts(string inputDir) =>
        this.postFinder.FindPosts(inputDir);

    private IFileSystem fileSystem;
    private IFileProcessor fileProcessor;
    private IPostFinder postFinder;
    private ILogger logger;
}