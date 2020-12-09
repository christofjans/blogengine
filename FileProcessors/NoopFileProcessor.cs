using System.Collections.Generic;

public class NoopFileProcessor : IFileProcessor
{
    public void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir)
    {
    }
}