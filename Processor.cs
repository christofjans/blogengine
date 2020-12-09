using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

public interface IProcessor
{
    void Process(string inputDir, string outputDir);
}

public class Processor : IProcessor
{
    public Processor(IFileSystem fileSystem, IFileProcessor fileProcessor)
    {
        this.fileSystem = fileSystem;
        this.fileProcessor = fileProcessor;
    }

    public void Process(string inputDir, string outputDir)
    {
        var posts = GetPosts(inputDir).ToDictionary(p=>p.FileName, p=>p);

        foreach (var filePath in this.fileSystem.Directory.GetFiles(inputDir))
        {
            ProcessFile(posts, filePath, outputDir);
        }
    }

    private void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir) =>
        this.fileProcessor.ProcessFile(posts, filePath, outputDir);

    private IEnumerable<Post> GetPosts(string inputDir)
    {
        throw new System.NotImplementedException();
    }

    private IFileSystem fileSystem;
    private IFileProcessor fileProcessor;
}