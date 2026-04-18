using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public record PostViewModel(string Title, string Date, string FileName, string Summary);

public static class PostsViewModel
{
    public static PostViewModel[] ToViewModel(this Dictionary<string, Post> posts)
    {
        var now = DateTime.Now;

        return posts.Values
            .Where(p=>p.Rss)
            .Where(p=>p.Date<now)
            .OrderByDescending(p=>p.Date)
            .Select(p=>new PostViewModel(p.Title, p.Date.ToString("yyyy-MM-dd"), Path.GetFileNameWithoutExtension(p.FilePath)+".html", p.Summary))
            .ToArray();
    }
}