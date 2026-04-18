using System.Linq;

public interface IProcessor
{
    void Process(string inputDir, string outputDir);
}

public class Processor : IProcessor
{
    public Processor(IPostFinder postFinder, IDirectoryProcessor directoryProcessor)
    {
        this.postFinder = postFinder;
        this.directoryProcessor = directoryProcessor;
    }

    public void Process(string inputDir, string outputDir)
    {
        var posts = this.postFinder.FindPosts(inputDir).ToDictionary(p => p.FilePath, p => p);
        this.directoryProcessor.Process(posts, inputDir, outputDir);
    }

    private IPostFinder postFinder;
    private IDirectoryProcessor directoryProcessor;
}