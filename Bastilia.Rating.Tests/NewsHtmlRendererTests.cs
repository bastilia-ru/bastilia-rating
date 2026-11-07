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

        result.ShouldContain("<a href=\"https://example.com\">текст</a>");
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
    public void TwoParagraphs_SeparatedByNewline()
    {
        var result = NewsHtmlRenderer.RenderMarkdownToTelegramHtml("один\n\nдва");

        Regex.IsMatch(result, "один\n+два").ShouldBeTrue();
    }
}
