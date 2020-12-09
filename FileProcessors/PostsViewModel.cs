using System.Collections.Generic;
using System.IO;
using System.Linq;

public record PostViewModel(string Title, string Date, string FileName);

public static class PostsViewModel
{
    public static PostViewModel[] ToViewModel(this Dictionary<string, Post> posts) =>
        posts.Values
            .OrderByDescending(p=>p.Date)
            .Select(p=>new PostViewModel(p.Title, p.Date.ToString("yyyy-MM-dd"), Path.GetFileName(p.FilePath)))
            .ToArray();
}