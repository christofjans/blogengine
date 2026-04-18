namespace BlogEngine.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

using BlogEngine.FileProcessors;

using Xunit;

public class TypeScriptFileProcessorTests
{
    [Fact]
    public void ProcessFile_CompilesTypeScriptToJavaScript()
    {
        using var tempDirectory = new TemporaryDirectory();
        var fileSystem = new FileSystem();
        string inputFilePath = Path.Combine(tempDirectory.Path, "foo.ts");
        string outputDir = Path.Combine(tempDirectory.Path, "output");
        Directory.CreateDirectory(outputDir);
        File.WriteAllText(inputFilePath, "const answer: number = 42;");

        var compiler = new RecordingTypeScriptCompiler((filePath, outputPath) =>
        {
            File.WriteAllText(Path.Combine(outputPath, "foo.js"), "var answer = 42;");
        });

        var processor = new TypeScriptFileProcessor(fileSystem, compiler);

        processor.ProcessFile([], inputFilePath, outputDir);

        Assert.Equal(inputFilePath, compiler.FilePath);
        Assert.Equal(outputDir, compiler.OutputDir);
        Assert.True(File.Exists(Path.Combine(outputDir, "foo.js")));
        Assert.False(File.Exists(Path.Combine(outputDir, "foo.ts")));
    }

    [Fact]
    public void ProcessFile_ThrowsWhenCompilerDoesNotEmitJavaScript()
    {
        using var tempDirectory = new TemporaryDirectory();
        var fileSystem = new FileSystem();
        string inputFilePath = Path.Combine(tempDirectory.Path, "foo.ts");
        string outputDir = Path.Combine(tempDirectory.Path, "output");
        Directory.CreateDirectory(outputDir);
        File.WriteAllText(inputFilePath, "const answer: number = 42;");

        var processor = new TypeScriptFileProcessor(fileSystem, new RecordingTypeScriptCompiler((_, _) => { }));

        Assert.Throws<FileNotFoundException>(() => processor.ProcessFile([], inputFilePath, outputDir));
    }

    [Fact]
    public void ProcessFile_PropagatesCompilerFailures()
    {
        using var tempDirectory = new TemporaryDirectory();
        var fileSystem = new FileSystem();
        string inputFilePath = Path.Combine(tempDirectory.Path, "foo.ts");
        string outputDir = Path.Combine(tempDirectory.Path, "output");
        Directory.CreateDirectory(outputDir);
        File.WriteAllText(inputFilePath, "const answer: number = 42;");

        var processor = new TypeScriptFileProcessor(fileSystem, new ThrowingTypeScriptCompiler());

        Assert.Throws<InvalidOperationException>(() => processor.ProcessFile([], inputFilePath, outputDir));
    }

    private sealed class RecordingTypeScriptCompiler(Action<string, string> onCompile) : ITypeScriptCompiler
    {
        public string FilePath { get; private set; } = "";

        public string OutputDir { get; private set; } = "";

        public void Compile(string filePath, string outputDir)
        {
            this.FilePath = filePath;
            this.OutputDir = outputDir;
            onCompile(filePath, outputDir);
        }
    }

    private sealed class ThrowingTypeScriptCompiler : ITypeScriptCompiler
    {
        public void Compile(string filePath, string outputDir) => throw new InvalidOperationException("compile failed");
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