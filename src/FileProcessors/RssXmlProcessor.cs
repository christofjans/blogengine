namespace BlogEngine.FileProcessors;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;

using WilderMinds.RssSyndication;

public class RssXmlProcessor(IFileSystem fileSystem) : IFileProcessor
{
    public void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir)
    {
        string json = fileSystem.File.ReadAllText(filePath);
        var config = JsonSerializer.Deserialize<RssConfig>(json);

        if (config == null) throw new NullReferenceException(nameof(config));

        Feed feed = new()
        {
            Title = config.Title,
            Description = config.Description,
            Link = new Uri(config.Link)
        };

        var rssPosts = posts.ToViewModel();

        foreach (var post in rssPosts)
        {
            string url = string.Format(config.PostUrlTemplate, post.FileName);
            Item item = new()
            {
                Title = post.Title,
                Body = post.Summary,
                Link = new Uri(url),
                Permalink = url,
                PublishDate = DateTime.Parse(post.Date),
                Author = new()
                {
                    Name = config.Author,
                    Email = config.AuthorEmail
                }
            };
            feed.Items.Add(item);
        }

        var rss = feed.Serialize();
        var outputFilePath = Path.Combine(outputDir, "rss.xml");
        fileSystem.File.WriteAllText(outputFilePath, rss);
    }
}

public class RssConfig
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Link { get; set; } = "";
    public string Author { get; set; } = "";
    public string AuthorEmail { get; set; } = "";
    public string PostUrlTemplate { get; set; } = "";
}