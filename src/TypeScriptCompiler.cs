namespace BlogEngine;

using System;

public interface ITypeScriptCompiler
{
    void Compile(string filePath, string outputDir);
}

public class TypeScriptCompiler(IProcessRunner processRunner) : ITypeScriptCompiler
{
    public void Compile(string filePath, string outputDir)
    {
        string compilerFileName = OperatingSystem.IsWindows() ? "tsc.cmd" : "tsc";
        string[] arguments =
        [
            filePath,
            "--module",
            "es2020",
            "--target",
            "es2017",
            "--outDir",
            outputDir,
            "--pretty",
            "false"
        ];

        var result = processRunner.Run(compilerFileName, arguments);

        if (result.ExitCode == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"TypeScript compilation failed for '{filePath}' with exit code {result.ExitCode}.{BuildErrorDetails(result)}");
    }

    private static string BuildErrorDetails(ProcessResult result)
    {
        if (string.IsNullOrWhiteSpace(result.StandardError) && string.IsNullOrWhiteSpace(result.StandardOutput))
        {
            return string.Empty;
        }

        return $"{Environment.NewLine}{result.StandardError}{result.StandardOutput}".TrimEnd();
    }
}