using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;

public interface IPostFinder
{
    IEnumerable<Post> FindPosts(string inputDir);
}

public class PostFinder : IPostFinder
{
    public PostFinder(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
    }

    public IEnumerable<Post> FindPosts(string inputDir)
    {
        foreach (var filePath in this.fileSystem.Directory.GetFiles(inputDir, "*.md"))
        {
            var lines = this.fileSystem.File.ReadAllLines(filePath);
            string firstLine = lines.FirstOrDefault() ?? "";
            var header = TryGetPost(firstLine);
            if (header!=null) 
            {
                yield return new Post
                {
                    FilePath = filePath,
                    Title = header.title,
                    Date = header.date,
                    Template = header.template,
                    Rss = !header.norss,
                    Summary = lines.Skip(1).SkipWhile(l=>string.IsNullOrWhiteSpace(l)).FirstOrDefault() ?? header.title
                };
            }
        }
    }

    private static PostHeader? TryGetPost(string firstLine)
    {
        try
        {
            return JsonSerializer.Deserialize<PostHeader>(firstLine);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private IFileSystem fileSystem;
}

class PostHeader
{
    public string title {get;set;} = "";
    public DateTime date {get;set;}
    public string? template {get;set;}
    public bool norss {get;set;}
}