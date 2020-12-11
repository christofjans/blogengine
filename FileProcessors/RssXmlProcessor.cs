using System.Collections.Generic;
using System.IO;
using WilderMinds.RssSyndication;
using System.Text.Json;
using System.IO.Abstractions;
using System;
using static System.Net.WebUtility;

public class RssXmlProcessor : IFileProcessor
{
    public RssXmlProcessor(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
    }

    public void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir)
    {
        string json = this.fileSystem.File.ReadAllText(filePath);
        var config = JsonSerializer.Deserialize<RssConfig>(json);

        if (config==null) throw new NullReferenceException(nameof(config));

        Feed feed = new()
        {
            Title = config.Title,
            Description = config.Description,
            Link = new Uri(config.Link)
        };

        var rssPosts = posts.ToViewModel();

        foreach (var post in rssPosts)
        {
            string url = string.Format(config.PostUrlTemplate, UrlEncode(post.FileName));
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
        this.fileSystem.File.WriteAllText(outputFilePath, rss);
    }

    private IFileSystem fileSystem;
}

public class RssConfig
{
    public string Title {get;set;} = "";
    public string Description {get;set;} = "";
    public string Link {get;set;} = "";
    public string Author {get;set;} = "";
    public string AuthorEmail {get;set;} = "";
    public string PostUrlTemplate {get;set;} = "";
}