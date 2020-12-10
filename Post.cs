using System;

public record Post
{
    public string FilePath {get;init;} = "";
    public string Title {get;init;} = "";
    public DateTime Date {get;init;}
    public string? Template {get;init;}
    public bool Rss {get;init;}
}