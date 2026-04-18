namespace BlogEngine;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

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
        PrepareOutputDirectory(outputDir);

        foreach (var filePath in GetFilesToProcess(inputDir))
        {
            var fileOutputDir = GetOutputDirectory(inputDir, outputDir, filePath);
            this.fileSystem.Directory.CreateDirectory(fileOutputDir);
            ProcessFile(posts, filePath, fileOutputDir);
            logger.LogInformation("processed {filePath}", filePath);
        }
    }

    private IEnumerable<string> GetFilesToProcess(string inputDir) =>
        EnumerateFiles(inputDir)
            .Where(filePath => IsRootFile(inputDir, filePath) || !IsRootOnlyFile(filePath));

    private void PrepareOutputDirectory(string outputDir)
    {
        this.fileSystem.Directory.CreateDirectory(outputDir);

        foreach (var filePath in this.fileSystem.Directory.GetFiles(outputDir)
                     .Where(filePath => !IsPreservedOutputFile(filePath)))
        {
            this.fileSystem.File.Delete(filePath);
        }

        foreach (var directoryPath in this.fileSystem.Directory.GetDirectories(outputDir)
                     .Where(directoryPath => !IsPreservedOutputDirectory(directoryPath)))
        {
            this.fileSystem.Directory.Delete(directoryPath, recursive: true);
        }
    }

    private IEnumerable<string> EnumerateFiles(string directoryPath)
    {
        foreach (var filePath in this.fileSystem.Directory.GetFiles(directoryPath))
        {
            yield return filePath;
        }

        foreach (var subdirectoryPath in this.fileSystem.Directory.GetDirectories(directoryPath)
                     .Where(subdirectoryPath => !IsIgnoredDirectory(subdirectoryPath)))
        {
            foreach (var filePath in EnumerateFiles(subdirectoryPath))
            {
                yield return filePath;
            }
        }
    }

    private string GetOutputDirectory(string inputDir, string outputDir, string filePath)
    {
        var fileDirectory = this.fileSystem.Path.GetDirectoryName(filePath) ?? inputDir;
        var relativeDirectory = this.fileSystem.Path.GetRelativePath(inputDir, fileDirectory);

        return relativeDirectory == "."
            ? outputDir
            : this.fileSystem.Path.Combine(outputDir, relativeDirectory);
    }

    private bool IsRootFile(string inputDir, string filePath) =>
        string.Equals(
            this.fileSystem.Path.GetDirectoryName(filePath),
            inputDir,
            StringComparison.OrdinalIgnoreCase);

    private bool IsRootOnlyFile(string filePath)
    {
        var extension = this.fileSystem.Path.GetExtension(filePath);
        var fileName = this.fileSystem.Path.GetFileName(filePath);

        return string.Equals(extension, ".md", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, "rss.json", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsIgnoredDirectory(string directoryPath) =>
        string.Equals(
            this.fileSystem.Path.GetFileName(directoryPath),
            ".git",
            StringComparison.OrdinalIgnoreCase);

    private bool IsPreservedOutputDirectory(string directoryPath) =>
        string.Equals(
            this.fileSystem.Path.GetFileName(directoryPath),
            ".git",
            StringComparison.OrdinalIgnoreCase);

    private bool IsPreservedOutputFile(string filePath)
    {
        var fileName = this.fileSystem.Path.GetFileName(filePath);

        return string.Equals(fileName, ".gitignore", StringComparison.OrdinalIgnoreCase);
    }

    private void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir) =>
        this.fileProcessor.ProcessFile(posts, filePath, outputDir);

    private readonly IFileSystem fileSystem = fileSystem;
    private readonly IFileProcessor fileProcessor = fileProcessor;
    private readonly ILogger logger = logger;
}