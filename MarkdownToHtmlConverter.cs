using Markdig;

public interface IMarkdownToHtmlConverter
{
    string Convert(string markDown);
}

public class MarkDigConverter : IMarkdownToHtmlConverter
{
    public MarkDigConverter()
    {
        this.pipeline = (new MarkdownPipelineBuilder()).UseAdvancedExtensions().Build();
    }

    public string Convert(string markDown) => Markdown.ToHtml(markDown, this.pipeline);

    private MarkdownPipeline pipeline;
}