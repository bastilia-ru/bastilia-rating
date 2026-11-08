using HtmlAgilityPack;
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
        var sanitizer = new HtmlSanitizer
        {
            WhiteListMode = true,
        };

        sanitizer.Tags("b", "strong", "i", "em", "u", "ins", "s", "strike", "del").RemoveEmpty();

        sanitizer.Tag("a").SetAttribute("target", "_blank").SetAttribute("rel", "nofollow")
            .CheckAttributeUrl("href")
            .RemoveEmpty()
            .NoAttributes(SanitizerOperation.FlattenTag);

        sanitizer.Tags("code", "pre", "blockquote").RemoveEmpty();

        sanitizer.Tags("p", "div", "span", "ul", "ol", "li").Flatten();

        return sanitizer;
    }
}
