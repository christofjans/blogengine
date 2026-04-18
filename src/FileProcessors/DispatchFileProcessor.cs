namespace BlogEngine.FileProcessors;

using System.Collections.Generic;
using System.IO;

public class DispatchFileProcessor(
    NoopFileProcessor noopFileProcessor,
    CopyFileProcessor copyFileProcessor,
    PostFileProcessor postFileProcessor,
    RssXmlProcessor rssXmlProcessor,
    NoPostFileProcessor noPostFileProcessor
    ) : IFileProcessor
{
    public void ProcessFile(Dictionary<string, Post> posts, string filePath, string outputDir)
    {
        var fname = Path.GetFileName(filePath);
        bool isTemplate = fname.Contains(".template.");
        bool isPost = posts.ContainsKey(filePath);
        string extension = Path.GetExtension(filePath);

        var fileProcessor = GetFileProcessor(fname, isTemplate, isPost, extension);

        fileProcessor.ProcessFile(posts, filePath, outputDir);
    }

    private IFileProcessor GetFileProcessor(string fname, bool isTemplate, bool isPost, string extension) =>
        (fname, isTemplate, isPost, extension) switch
        {
            (_, _, true, _) => postFileProcessor,
            ("README.md", _, false, _) => copyFileProcessor,
            (_, _, false, ".md") => noPostFileProcessor,
            ("rss.json", _, _, _) => rssXmlProcessor,
            (_, true, _, _) => noopFileProcessor,
            _ => copyFileProcessor
        };
}