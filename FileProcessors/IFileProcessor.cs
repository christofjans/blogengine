using System.Collections.Generic;

public interface IFileProcessor
{
    void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir);
}