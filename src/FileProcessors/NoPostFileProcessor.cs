namespace BlogEngine.FileProcessors;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

public class NoPostFileProcessor(IFileSystem fileSystem, IMarkdownToHtmlConverter markdownToHtmlConverter, ITemplateEngine templateEngine) : IFileProcessor
{
    public void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir)
    {
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var outputFilePath = Path.Combine(outputDir, fileNameWithoutExtension + ".html");

        var lines = fileSystem.File.ReadAllLines(filePath);
        string title = lines.FirstOrDefault(line => line.Trim().StartsWith("# "))?.Substring(2) ?? fileNameWithoutExtension;

        var html = markdownToHtmlConverter.Convert(string.Join(Environment.NewLine, lines));

        var templatePath = Path.Combine(Path.GetDirectoryName(filePath) ?? "", "nopost.template.html");
        var data = new
        {
            html,
            posts = posts.ToViewModel(),
            title
        };
        html = templateEngine.Merge(templatePath, data);

        fileSystem.File.WriteAllText(outputFilePath, html);
    }
}