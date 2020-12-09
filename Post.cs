using System;

public record Post
{
    public string FileName {get;init;} = "";
    public string Title {get;init;} = "";
    public DateTime Date {get;init;}
}