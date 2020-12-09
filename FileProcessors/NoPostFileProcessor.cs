using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

public class NoPostFileProcessor : IFileProcessor
{
    public NoPostFileProcessor(IFileSystem fileSystem, IMarkdownToHtmlConverter markdownToHtmlConverter)
    {
        this.fileSystem = fileSystem;
        this.markdownToHtmlConverter = markdownToHtmlConverter;
    }

    public void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir)
    {
        var outputFilePath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(filePath)+".html");

        var html = markdownToHtmlConverter.Convert(this.fileSystem.File.ReadAllText(filePath));

        //todo run through templating

        this.fileSystem.File.WriteAllText(outputFilePath, html);
    }

    private IFileSystem fileSystem;
    private IMarkdownToHtmlConverter markdownToHtmlConverter;
}