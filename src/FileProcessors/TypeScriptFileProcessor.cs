namespace BlogEngine.FileProcessors;

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

public class TypeScriptFileProcessor(IFileSystem fileSystem, ITypeScriptCompiler typeScriptCompiler) : IFileProcessor
{
    public void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir)
    {
        var outputFilePath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(filePath) + ".js");

        typeScriptCompiler.Compile(filePath, outputDir);

        if (!fileSystem.File.Exists(outputFilePath))
        {
            throw new FileNotFoundException($"TypeScript compiler did not emit '{outputFilePath}'.", outputFilePath);
        }
    }
}