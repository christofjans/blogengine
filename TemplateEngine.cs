using System.Collections.Generic;
using System.IO.Abstractions;
using HandlebarsDotNet;

public interface ITemplateEngine
{
    string Merge(string templatePath, object data);
}

public class HandlebarsTemplateEngine : ITemplateEngine
{
    public HandlebarsTemplateEngine(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
        this.templates = new();
    }

    public string Merge(string templatePath, object data)
    {
        if (!this.templates.TryGetValue(templatePath, out var template))
        {
            templates[templatePath] = template = Handlebars.Compile(this.fileSystem.File.ReadAllText(templatePath));
        }

        return template(data);
    }

    private IFileSystem fileSystem;
    private Dictionary<string, HandlebarsTemplate<object,object>> templates;
}