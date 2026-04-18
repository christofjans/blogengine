using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

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
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var outputFilePath = Path.Combine(outputDir, fileNameWithoutExtension+".html");

        var lines = this.fileSystem.File.ReadAllLines(filePath);
        string title = lines.FirstOrDefault(line=>line.Trim().StartsWith("# "))?.Substring(2) ?? fileNameWithoutExtension;

        var html = markdownToHtmlConverter.Convert(string.Join(Environment.NewLine, lines));

        var templatePath = Path.Combine(Path.GetDirectoryName(filePath) ?? "", "nopost.template.html");
        var data = new {
            html = html,
            posts = posts.ToViewModel(),
            title = title
        };
        html = templateEngine.Merge(templatePath, data);

        this.fileSystem.File.WriteAllText(outputFilePath, html);
    }

    private IFileSystem fileSystem;
    private IMarkdownToHtmlConverter markdownToHtmlConverter;
    private ITemplateEngine templateEngine;
}