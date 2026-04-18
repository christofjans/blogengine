namespace BlogEngine;

using System.Collections.Generic;
using System.Diagnostics;

public interface IProcessRunner
{
    ProcessResult Run(string fileName, IReadOnlyList<string> arguments);
}

public readonly record struct ProcessResult(int ExitCode, string StandardOutput, string StandardError);

public class ProcessRunner : IProcessRunner
{
    public ProcessResult Run(string fileName, IReadOnlyList<string> arguments)
    {
        using Process process = new();
        process.StartInfo = new ProcessStartInfo(fileName)
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();

        string standardOutput = process.StandardOutput.ReadToEnd();
        string standardError = process.StandardError.ReadToEnd();

        process.WaitForExit();

        return new ProcessResult(process.ExitCode, standardOutput, standardError);
    }
}