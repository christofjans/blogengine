using Markdig;

public interface IMarkdownToHtmlConverter
{
    string Convert(string markDown);
}

public class MarkDigConverter : IMarkdownToHtmlConverter
{
    public string Convert(string markDown) => Markdown.ToHtml(markDown);
}