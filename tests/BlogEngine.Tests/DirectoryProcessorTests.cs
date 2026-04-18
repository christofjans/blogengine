namespace BlogEngine.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

using BlogEngine.FileProcessors;

using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

public class DirectoryProcessorTests
{
    [Fact]
    public void Process_RecursesIntoSubdirectoriesAndPreservesRelativeOutputDirectories()
    {
        using var tempDirectory = new TemporaryDirectory();
        string inputDir = Path.Combine(tempDirectory.Path, "input");
        string outputDir = Path.Combine(tempDirectory.Path, "output");
        Directory.CreateDirectory(Path.Combine(inputDir, "assets"));
        Directory.CreateDirectory(Path.Combine(inputDir, "scripts"));
        Directory.CreateDirectory(Path.Combine(inputDir, "nested"));
        Directory.CreateDirectory(Path.Combine(inputDir, "feed"));
        Directory.CreateDirectory(outputDir);

        string postFilePath = Path.Combine(inputDir, "post.md");
        string indexFilePath = Path.Combine(inputDir, "index.md");
        string rssFilePath = Path.Combine(inputDir, "rss.json");
        string cssFilePath = Path.Combine(inputDir, "assets", "site.css");
        string tsFilePath = Path.Combine(inputDir, "scripts", "app.ts");
        string nestedMarkdownFilePath = Path.Combine(inputDir, "nested", "about.md");
        string nestedRssFilePath = Path.Combine(inputDir, "feed", "rss.json");

        File.WriteAllText(postFilePath, "post");
        File.WriteAllText(indexFilePath, "index");
        File.WriteAllText(rssFilePath, "{}");
        File.WriteAllText(cssFilePath, "body {}");
        File.WriteAllText(tsFilePath, "const answer: number = 42;");
        File.WriteAllText(nestedMarkdownFilePath, "# ignored");
        File.WriteAllText(nestedRssFilePath, "{}");

        var recordingProcessor = new RecordingFileProcessor();
        var processor = new DirectoryProcessor(new FileSystem(), recordingProcessor, NullLogger<DirectoryProcessor>.Instance);

        processor.Process([], inputDir, outputDir);

        Assert.Contains((postFilePath, outputDir), recordingProcessor.ProcessedFiles);
        Assert.Contains((indexFilePath, outputDir), recordingProcessor.ProcessedFiles);
        Assert.Contains((rssFilePath, outputDir), recordingProcessor.ProcessedFiles);
        Assert.Contains((cssFilePath, Path.Combine(outputDir, "assets")), recordingProcessor.ProcessedFiles);
        Assert.Contains((tsFilePath, Path.Combine(outputDir, "scripts")), recordingProcessor.ProcessedFiles);
        Assert.DoesNotContain((nestedMarkdownFilePath, Path.Combine(outputDir, "nested")), recordingProcessor.ProcessedFiles);
        Assert.DoesNotContain((nestedRssFilePath, Path.Combine(outputDir, "feed")), recordingProcessor.ProcessedFiles);
    }

    [Fact]
    public void Process_IgnoresGitDirectoriesCompletely()
    {
        using var tempDirectory = new TemporaryDirectory();
        string inputDir = Path.Combine(tempDirectory.Path, "input");
        string outputDir = Path.Combine(tempDirectory.Path, "output");
        Directory.CreateDirectory(Path.Combine(inputDir, ".git", "objects"));
        Directory.CreateDirectory(outputDir);

        string normalAssetPath = Path.Combine(inputDir, "site.css");
        string gitAssetPath = Path.Combine(inputDir, ".git", "objects", "packed.txt");

        File.WriteAllText(normalAssetPath, "body {}");
        File.WriteAllText(gitAssetPath, "ignored");

        var recordingProcessor = new RecordingFileProcessor();
        var processor = new DirectoryProcessor(new FileSystem(), recordingProcessor, NullLogger<DirectoryProcessor>.Instance);

        processor.Process([], inputDir, outputDir);

        Assert.Contains((normalAssetPath, outputDir), recordingProcessor.ProcessedFiles);
        Assert.DoesNotContain(recordingProcessor.ProcessedFiles, processed => processed.FilePath == gitAssetPath);
    }

    [Fact]
    public void Process_WritesNestedAssetsIntoMatchingOutputSubdirectories()
    {
        using var tempDirectory = new TemporaryDirectory();
        string inputDir = Path.Combine(tempDirectory.Path, "input");
        string outputDir = Path.Combine(tempDirectory.Path, "output");
        Directory.CreateDirectory(Path.Combine(inputDir, "assets"));
        Directory.CreateDirectory(Path.Combine(inputDir, "scripts"));
        Directory.CreateDirectory(outputDir);

        File.WriteAllText(Path.Combine(inputDir, "index.md"), "# Home");
        File.WriteAllText(Path.Combine(inputDir, "nopost.template.html"), "<html><body>{{{html}}}</body></html>");
        File.WriteAllText(Path.Combine(inputDir, "assets", "site.css"), "body { color: black; }");
        File.WriteAllText(Path.Combine(inputDir, "scripts", "app.ts"), "const total: number = 1;");

        IFileSystem fileSystem = new FileSystem();
        var markdownToHtmlConverter = new MarkDigConverter();
        var templateEngine = new HandlebarsTemplateEngine(fileSystem);
        var dispatchProcessor = new DispatchFileProcessor(
            new NoopFileProcessor(),
            new CopyFileProcessor(fileSystem),
            new PostFileProcessor(fileSystem, markdownToHtmlConverter, templateEngine),
            new RssXmlProcessor(fileSystem),
            new NoPostFileProcessor(fileSystem, markdownToHtmlConverter, templateEngine),
            new TypeScriptFileProcessor(fileSystem, new RecordingTypeScriptCompiler((filePath, targetOutputDir) =>
            {
                File.WriteAllText(
                    Path.Combine(targetOutputDir, Path.GetFileNameWithoutExtension(filePath) + ".js"),
                    "var total = 1;");
            })));

        var processor = new Processor(
            new PostFinder(fileSystem),
            new DirectoryProcessor(fileSystem, dispatchProcessor, NullLogger<DirectoryProcessor>.Instance));

        processor.Process(inputDir, outputDir);

        Assert.True(File.Exists(Path.Combine(outputDir, "index.html")));
        Assert.Equal("body { color: black; }", File.ReadAllText(Path.Combine(outputDir, "assets", "site.css")));
        Assert.Equal("var total = 1;", File.ReadAllText(Path.Combine(outputDir, "scripts", "app.js")));
        Assert.False(File.Exists(Path.Combine(outputDir, "scripts", "app.ts")));
    }

    private sealed class RecordingFileProcessor : IFileProcessor
    {
        public List<(string FilePath, string OutputDir)> ProcessedFiles { get; } = [];

        public void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir) =>
            this.ProcessedFiles.Add((filePath, outputDir));
    }

    private sealed class RecordingTypeScriptCompiler(Action<string, string> onCompile) : ITypeScriptCompiler
    {
        public void Compile(string filePath, string outputDir) => onCompile(filePath, outputDir);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            this.Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"blogengine-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(this.Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(this.Path))
            {
                Directory.Delete(this.Path, recursive: true);
            }
        }
    }
}