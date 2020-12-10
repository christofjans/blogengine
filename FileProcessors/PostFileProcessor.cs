using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;

public class PostFileProcessor : IFileProcessor
{
    public PostFileProcessor(IFileSystem fileSystem, IMarkdownToHtmlConverter markdownToHtmlConverter, ITemplateEngine templateEngine)
    {
        this.fileSystem = fileSystem;
        this.markdownToHtmlConverter = markdownToHtmlConverter;
        this.templateEngine = templateEngine;
    }

    public void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir)
    {
        var outputFilePath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(filePath)+".html");
        var post = posts[filePath];

        string text = this.fileSystem.File.ReadAllText(filePath);
        text = string.Join(Environment.NewLine, Regex.Split(text, "\r\n|\r|\n").Skip(1));

        var html = markdownToHtmlConverter.Convert(text);

        var templatePath = Path.Combine(Path.GetDirectoryName(filePath) ?? "", post.Template ?? "post.template.html");
        var data = new {
            html = html,
            posts = posts.ToViewModel(),
            title = post.Title,
            date = post.Date.ToString("yyyy-MM-dd"),
        };
        html = templateEngine.Merge(templatePath, data);

        this.fileSystem.File.WriteAllText(outputFilePath, html);
    }

    private IFileSystem fileSystem;
    private IMarkdownToHtmlConverter markdownToHtmlConverter;
    private ITemplateEngine templateEngine;
}