using Markdig;
using Vereyon.Web;

namespace Bastilia.Rating.Portal.Common.Telegram;

public static class NewsHtmlRenderer
{
    private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

    public static string RenderMarkdownToTelegramHtml(string markdown)
    {
        var html = Markdown.ToHtml(markdown, new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseAutoLinks()
            .DisableHtml()
            .Build());

        html = html
            .Replace("</p>", "\n")
            .Replace("</li>", "\n")
            .Replace("<ul>", string.Empty)
            .Replace("</ul>", string.Empty)
            .Replace("<ol>", string.Empty)
            .Replace("</ol>", string.Empty);

        return Sanitizer.Sanitize(html);
    }

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();

        foreach (var tag in new[]
        {
            "b", "strong", "i", "em", "u", "ins", "s", "strike", "del", "code", "pre", "blockquote",
        })
        {
            sanitizer.Tag(tag);
        }

        sanitizer.Tag("a").AllowAttributes("href");

        foreach (var tag in new[] { "p", "div", "span", "ul", "ol", "li" })
        {
            sanitizer.Tag(tag).Flatten();
        }

        return sanitizer;
    }
}
