using HtmlAgilityPack;
using Markdig;
using Vereyon.Web;

namespace Bastilia.Rating.Portal.Common.Telegram;

public static class NewsHtmlRenderer
{
    private static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseAutoLinks()
        .DisableHtml()
        .Build();

    private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

    public static string RenderMarkdownToTelegramHtml(string markdown)
    {
        var html = Markdown.ToHtml(markdown, MarkdownPipeline);

        return Sanitizer.Sanitize(ToTelegramHtmlString(html));
    }

    /// <summary>
    /// Эквивалент ToTelegramHtmlString из JoinRpg.Markdown (MarkDownRendererFacade):
    /// Telegram не понимает блочные теги, а наш санитайзер выбрасывает их содержимое-теги
    /// (p, ul, ol, li), поэтому переносы строк вставляем заранее, пока закрывающие теги ещё на месте.
    /// </summary>
    /// <remarks>
    /// Отличия от joinrpg: там заменяется только <c>&lt;/p&gt;</c> (плюс TrimEnd) и финальная
    /// telegram-санитизация с флэттингом происходит отдельным этапом при отправке; у нас обёртки
    /// списков (ul/ol) убираются здесь же, а пункты (li) разделяются переносом, чтобы после
    /// флэттинга пункты и абзацы не слипались.
    /// </remarks>
    private static string ToTelegramHtmlString(string html)
        => html
            .Replace("</p>", "</p>\n")
            .Replace("</li>", "</li>\n")
            .Replace("<ul>", string.Empty)
            .Replace("</ul>", string.Empty)
            .Replace("<ol>", string.Empty)
            .Replace("</ol>", string.Empty)
            .Trim();

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
