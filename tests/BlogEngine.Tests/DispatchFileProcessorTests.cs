namespace BlogEngine.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

using BlogEngine.FileProcessors;

using Xunit;

public class DispatchFileProcessorTests
{
    [Fact]
    public void ProcessFile_RoutesTypeScriptFilesToTypeScriptProcessor()
    {
        using var tempDirectory = new TemporaryDirectory();
        var dispatchProcessor = CreateProcessor(new RecordingTypeScriptCompiler((_, outputDir) =>
        {
            File.WriteAllText(Path.Combine(outputDir, "script.js"), "var total = 1;");
        }));
        string inputFilePath = Path.Combine(tempDirectory.Path, "script.ts");
        string outputDir = Path.Combine(tempDirectory.Path, "output");
        Directory.CreateDirectory(outputDir);
        File.WriteAllText(inputFilePath, "const total: number = 1;");

        dispatchProcessor.ProcessFile([], inputFilePath, outputDir);

        Assert.True(File.Exists(Path.Combine(outputDir, "script.js")));
        Assert.False(File.Exists(Path.Combine(outputDir, "script.ts")));
    }

    [Fact]
    public void ProcessFile_LeavesReadmeCopyBehaviorUnchanged()
    {
        using var tempDirectory = new TemporaryDirectory();
        var dispatchProcessor = CreateProcessor(new RecordingTypeScriptCompiler((_, _) => { }));
        string inputFilePath = Path.Combine(tempDirectory.Path, "README.md");
        string outputDir = Path.Combine(tempDirectory.Path, "output");
        Directory.CreateDirectory(outputDir);
        File.WriteAllText(inputFilePath, "readme");

        dispatchProcessor.ProcessFile([], inputFilePath, outputDir);

        Assert.Equal("readme", File.ReadAllText(Path.Combine(outputDir, "README.md")));
    }

    [Fact]
    public void ProcessFile_LeavesNonPostMarkdownBehaviorUnchanged()
    {
        using var tempDirectory = new TemporaryDirectory();
        var dispatchProcessor = CreateProcessor(new RecordingTypeScriptCompiler((_, _) => { }));
        string inputFilePath = Path.Combine(tempDirectory.Path, "about.md");
        string templatePath = Path.Combine(tempDirectory.Path, "nopost.template.html");
        string outputDir = Path.Combine(tempDirectory.Path, "output");
        Directory.CreateDirectory(outputDir);
        File.WriteAllText(inputFilePath, "# About");
        File.WriteAllText(templatePath, "<html><body>{{{html}}}</body></html>");

        dispatchProcessor.ProcessFile([], inputFilePath, outputDir);

        Assert.True(File.Exists(Path.Combine(outputDir, "about.html")));
        Assert.False(File.Exists(Path.Combine(outputDir, "about.md")));
    }

    [Fact]
    public void ProcessFile_LeavesPostMarkdownBehaviorUnchanged()
    {
        using var tempDirectory = new TemporaryDirectory();
        var dispatchProcessor = CreateProcessor(new RecordingTypeScriptCompiler((_, _) => { }));
        string inputFilePath = Path.Combine(tempDirectory.Path, "post.md");
        string templatePath = Path.Combine(tempDirectory.Path, "post.template.html");
        string outputDir = Path.Combine(tempDirectory.Path, "output");
        Directory.CreateDirectory(outputDir);
        File.WriteAllText(inputFilePath, "2025-01-01 My Post\r\nBody");
        File.WriteAllText(templatePath, "<html><body>{{{html}}}</body></html>");
        Dictionary<string, Post> posts = new()
        {
            [inputFilePath] = new Post
            {
                FilePath = inputFilePath,
                Title = "My Post",
                Date = new DateTime(2025, 1, 1)
            }
        };

        dispatchProcessor.ProcessFile(posts, inputFilePath, outputDir);

        Assert.True(File.Exists(Path.Combine(outputDir, "post.html")));
    }

    [Fact]
    public void ProcessFile_LeavesTemplateBehaviorUnchanged()
    {
        using var tempDirectory = new TemporaryDirectory();
        var dispatchProcessor = CreateProcessor(new RecordingTypeScriptCompiler((_, _) => { }));
        string inputFilePath = Path.Combine(tempDirectory.Path, "post.template.html");
        string outputDir = Path.Combine(tempDirectory.Path, "output");
        Directory.CreateDirectory(outputDir);
        File.WriteAllText(inputFilePath, "<html></html>");

        dispatchProcessor.ProcessFile([], inputFilePath, outputDir);

        Assert.Empty(Directory.GetFiles(outputDir));
    }

    [Fact]
    public void ProcessFile_LeavesRssBehaviorUnchanged()
    {
        using var tempDirectory = new TemporaryDirectory();
        var dispatchProcessor = CreateProcessor(new RecordingTypeScriptCompiler((_, _) => { }));
        string inputFilePath = Path.Combine(tempDirectory.Path, "rss.json");
        string outputDir = Path.Combine(tempDirectory.Path, "output");
        Directory.CreateDirectory(outputDir);
        File.WriteAllText(inputFilePath, """
            {
              "Title": "Feed",
              "Description": "desc",
              "Link": "https://example.com",
              "Author": "Chris",
              "AuthorEmail": "chris@example.com",
              "PostUrlTemplate": "https://example.com/{0}.html"
            }
            """);
        Dictionary<string, Post> posts = new()
        {
            [Path.Combine(tempDirectory.Path, "post.md")] = new Post
            {
                FilePath = Path.Combine(tempDirectory.Path, "post.md"),
                Title = "My Post",
                Date = new DateTime(2025, 1, 1),
                Summary = "summary",
                Rss = true
            }
        };

        dispatchProcessor.ProcessFile(posts, inputFilePath, outputDir);

        Assert.True(File.Exists(Path.Combine(outputDir, "rss.xml")));
    }

    [Fact]
    public void ProcessFile_LeavesOrdinaryAssetCopyBehaviorUnchanged()
    {
        using var tempDirectory = new TemporaryDirectory();
        var dispatchProcessor = CreateProcessor(new RecordingTypeScriptCompiler((_, _) => { }));
        string inputFilePath = Path.Combine(tempDirectory.Path, "site.css");
        string outputDir = Path.Combine(tempDirectory.Path, "output");
        Directory.CreateDirectory(outputDir);
        File.WriteAllText(inputFilePath, "body { color: black; }");

        dispatchProcessor.ProcessFile([], inputFilePath, outputDir);

        Assert.Equal("body { color: black; }", File.ReadAllText(Path.Combine(outputDir, "site.css")));
    }

    private static DispatchFileProcessor CreateProcessor(ITypeScriptCompiler typeScriptCompiler)
    {
        IFileSystem fileSystem = new FileSystem();
        var markdownToHtmlConverter = new MarkDigConverter();
        var templateEngine = new HandlebarsTemplateEngine(fileSystem);

        return new DispatchFileProcessor(
            new NoopFileProcessor(),
            new CopyFileProcessor(fileSystem),
            new PostFileProcessor(fileSystem, markdownToHtmlConverter, templateEngine),
            new RssXmlProcessor(fileSystem),
            new NoPostFileProcessor(fileSystem, markdownToHtmlConverter, templateEngine),
            new TypeScriptFileProcessor(fileSystem, typeScriptCompiler));
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