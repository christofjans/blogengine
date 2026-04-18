namespace BlogEngine.Tests;

using System;
using System.Collections.Generic;
using System.IO;

using Xunit;

public class TypeScriptCompilerTests
{
    [Fact]
    public void Compile_UsesExpectedTscArguments()
    {
        var processRunner = new RecordingProcessRunner(new ProcessResult(0, "", ""));
        var compiler = new TypeScriptCompiler(processRunner);
        string expectedFileName = OperatingSystem.IsWindows() ? "tsc.cmd" : "tsc";

        compiler.Compile(@"C:\input\foo.ts", @"C:\output");

        Assert.Equal(expectedFileName, processRunner.FileName);
        Assert.Equal(
            [
                @"C:\input\foo.ts",
                "--module",
                "es2020",
                "--target",
                "es2017",
                "--outDir",
                @"C:\output",
                "--pretty",
                "false"
            ],
            processRunner.Arguments);
    }

    [Fact]
    public void Compile_ThrowsWhenTscFails()
    {
        var processRunner = new RecordingProcessRunner(new ProcessResult(1, "", "bad compile"));
        var compiler = new TypeScriptCompiler(processRunner);

        var ex = Assert.Throws<InvalidOperationException>(() => compiler.Compile(@"C:\input\foo.ts", @"C:\output"));

        Assert.Contains("foo.ts", ex.Message);
        Assert.Contains("bad compile", ex.Message);
    }

    [Fact]
    public void Compile_EmitsBrowserModuleSyntaxForModuleInput()
    {
        using var tempDirectory = new TemporaryDirectory();
        string inputFilePath = Path.Combine(tempDirectory.Path, "clock.ts");
        string outputDir = Path.Combine(tempDirectory.Path, "output");
        Directory.CreateDirectory(outputDir);
        File.WriteAllText(
            inputFilePath,
            """
            export function runclock(canvasId: string) {
                return canvasId;
            }
            """);

        var compiler = new TypeScriptCompiler(new ProcessRunner());

        compiler.Compile(inputFilePath, outputDir);

        string outputFilePath = Path.Combine(outputDir, "clock.js");
        Assert.True(File.Exists(outputFilePath));

        string compiledJavaScript = File.ReadAllText(outputFilePath);
        Assert.Contains("export function runclock", compiledJavaScript);
        Assert.DoesNotContain("exports.runclock", compiledJavaScript);
    }

    private sealed class RecordingProcessRunner(ProcessResult result) : IProcessRunner
    {
        public string FileName { get; private set; } = "";

        public IReadOnlyList<string> Arguments { get; private set; } = [];

        public ProcessResult Run(string fileName, IReadOnlyList<string> arguments)
        {
            this.FileName = fileName;
            this.Arguments = [.. arguments];
            return result;
        }
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