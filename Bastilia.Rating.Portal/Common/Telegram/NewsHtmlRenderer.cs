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
        var sanitizer = new HtmlSanitizer();

        foreach (var tag in new[]
        {
            "b", "strong", "i", "em", "u", "ins", "s", "strike", "del", "code", "pre", "blockquote",
        })
        {
            sanitizer.Tag(tag);
        }

        sanitizer.Tag("a").SanitizeAttributes("href", new HrefSchemeSanitizer());

        foreach (var tag in new[] { "p", "div", "span", "ul", "ol", "li" })
        {
            sanitizer.Tag(tag).Flatten();
        }

        return sanitizer;
    }

    private sealed class HrefSchemeSanitizer : IHtmlAttributeSanitizer
    {
        private static readonly HashSet<string> AllowedSchemes = new(StringComparer.OrdinalIgnoreCase)
        {
            "http", "https", "mailto",
        };

        public SanitizerOperation SanitizeAttribute(HtmlAttribute attribute, HtmlSanitizerTagRule tagRule)
        {
            // Браузеры игнорируют ASCII-пробельные символы внутри схемы (java\tscript:), убираем их до проверки.
            var value = attribute.Value.Trim().Replace("\t", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);

            var colonIndex = value.IndexOf(':');
            if (colonIndex <= 0 || !char.IsAsciiLetter(value[0]))
            {
                return SanitizerOperation.DoNothing; // относительный URL без схемы
            }

            var scheme = value[..colonIndex];
            if (scheme.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ('+' or '-' or '.')))
            {
                return SanitizerOperation.DoNothing; // двоеточие в query/path, не схема
            }

            return AllowedSchemes.Contains(scheme)
                ? SanitizerOperation.DoNothing
                : SanitizerOperation.RemoveAttribute;
        }
    }
}
