using System.Collections.Generic;

public class NoPostFileProcessor : IFileProcessor
{
    public void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir)
    {
        throw new System.NotImplementedException();
    }
}