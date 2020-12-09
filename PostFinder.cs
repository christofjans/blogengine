using System;
using System.Collections.Generic;
using System.IO;
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
            var firstLine = this.fileSystem.File.ReadLines(filePath).FirstOrDefault() ?? "";
            var miniPost = TryGetPost(firstLine);
            if (miniPost!=null) 
            {
                yield return new Post
                {
                    FilePath = filePath,
                    Title = miniPost.title,
                    Date = miniPost.date
                };
            }
        }
    }

    private static MiniPost? TryGetPost(string firstLine)
    {
        try
        {
            return JsonSerializer.Deserialize<MiniPost>(firstLine);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private IFileSystem fileSystem;
}

class MiniPost
{
    public string title {get;set;} = "";
    public DateTime date {get;set;}
}