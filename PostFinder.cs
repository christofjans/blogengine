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
            var post = TryGetPost(firstLine);
            if (post!=null) 
            {
                post.FilePath = filePath;
                Console.WriteLine(post.FilePath);
                yield return post;
            }
        }
    }

    private static Post? TryGetPost(string firstLine)
    {
        try
        {
            var mp = JsonSerializer.Deserialize<MiniPost>(firstLine);
            if (mp==null) return null;
            return new Post
            {
                Title = mp.title,
                Date = mp.date
            };
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