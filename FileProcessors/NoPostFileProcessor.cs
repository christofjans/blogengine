using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

public class NoPostFileProcessor : IFileProcessor
{
    public NoPostFileProcessor(IFileSystem fileSystem, IMarkdownToHtmlConverter markdownToHtmlConverter, ITemplateEngine templateEngine)
    {
        this.fileSystem = fileSystem;
        this.markdownToHtmlConverter = markdownToHtmlConverter;
        this.templateEngine = templateEngine;
    }

    public void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir)
    {
        var outputFilePath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(filePath)+".html");

        var html = markdownToHtmlConverter.Convert(this.fileSystem.File.ReadAllText(filePath));

        var templatePath = Path.Combine(Path.GetDirectoryName(filePath) ?? "", "nopost.template.html");
        var data = new {
            html = html
        };
        html = templateEngine.Merge(templatePath, data);

        this.fileSystem.File.WriteAllText(outputFilePath, html);
    }

    private IFileSystem fileSystem;
    private IMarkdownToHtmlConverter markdownToHtmlConverter;
    private ITemplateEngine templateEngine;
}