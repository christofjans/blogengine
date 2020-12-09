using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class PostsViewModel
{
    public static Post[] ToViewModel(this Dictionary<string, Post> posts) =>
        posts.Values
            .Select(p=>new Post
            {
                Title = p.Title,
                Date = p.Date,
                FilePath = Path.GetFileName(p.FilePath)
            })
            .OrderByDescending(p=>p.Date)
            .ToArray();
}