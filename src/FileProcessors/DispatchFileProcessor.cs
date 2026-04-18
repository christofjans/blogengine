using System.Collections.Generic;
using System.IO;

public class DispatchFileProcessor : IFileProcessor
{
    public DispatchFileProcessor(
        NoopFileProcessor noopFileProcessor,
        CopyFileProcessor copyFileProcessor,
        NoPostFileProcessor markdownFileProcessor,
        PostFileProcessor postFileProcessor,
        RssXmlProcessor rssXmlProcessor
    )
    {
        this.noopFileProcessor = noopFileProcessor;
        this.copyFileProcessor = copyFileProcessor;
        this.postFileProcessor = postFileProcessor;
        this.noPostFileProcessor = markdownFileProcessor;
        this.rssXmlProcessor = rssXmlProcessor;
    }

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
            (_,             _,      true,   _    )  => postFileProcessor,
            ("README.md",   _,      false,  _    )  => copyFileProcessor,
            (_,             _,      false,  ".md")  => noPostFileProcessor,
            ("rss.json",    _,      _,      _    )  => rssXmlProcessor,
            (_,             true,   _,      _    )  => noopFileProcessor,
            _                                       => copyFileProcessor
        };

    private IFileProcessor postFileProcessor;
    private IFileProcessor noopFileProcessor;
    private IFileProcessor copyFileProcessor;
    private IFileProcessor noPostFileProcessor;
    private IFileProcessor rssXmlProcessor;
}