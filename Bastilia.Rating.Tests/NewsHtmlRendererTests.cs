using System.Text.RegularExpressions;
using Bastilia.Rating.Portal.Common.Telegram;

namespace Bastilia.Rating.Tests;

public class NewsHtmlRendererTests
{
    [Fact]
    public void BoldMarkdown_BecomesStrong()
    {
        var result = NewsHtmlRenderer.RenderMarkdownToTelegramHtml("**жирный**");

        result.ShouldContain("<strong>жирный</strong>");
    }

    [Fact]
    public void LinkMarkdown_BecomesAnchorWithHref()
    {
        var result = NewsHtmlRenderer.RenderMarkdownToTelegramHtml("[текст](https://example.com)");

        result.ShouldContain("<a href=\"https://example.com/\" target=\"_blank\" rel=\"nofollow\">текст</a>");
    }

    [Fact]
    public void ScriptTag_IsStripped()
    {
        var result = NewsHtmlRenderer.RenderMarkdownToTelegramHtml("<script>alert(1)</script>привет");

        result.ShouldNotContain("<script");
        result.ShouldNotContain("</script");
        result.ShouldContain("привет");
    }

    [Fact]
    public void JavascriptLink_HrefIsRemoved()
    {
        var result = NewsHtmlRenderer.RenderMarkdownToTelegramHtml("[клик](javascript:alert(1))");

        result.ShouldNotContain("javascript:");
        result.ShouldContain("клик");
    }

    [Fact]
    public void MailtoLink_HrefPassesThrough()
    {
        var result = NewsHtmlRenderer.RenderMarkdownToTelegramHtml("[почта](mailto:test@example.com)");

        result.ShouldContain("<a href=\"mailto:test@example.com\" target=\"_blank\" rel=\"nofollow\">почта</a>");
    }

    [Fact]
    public void TwoParagraphs_SeparatedByNewline()
    {
        var result = NewsHtmlRenderer.RenderMarkdownToTelegramHtml("один\n\nдва");

        Regex.IsMatch(result, "один\n+два").ShouldBeTrue();
    }
}
