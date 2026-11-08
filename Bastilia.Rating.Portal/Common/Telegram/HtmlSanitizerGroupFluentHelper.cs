using Vereyon.Web;

namespace Bastilia.Rating.Portal.Common.Telegram;

/// <summary>
/// Групповые операции над правилами тегов санитайзера.
/// https://github.com/Vereyon/HtmlRuleSanitizer/pull/40
/// </summary>
internal static class HtmlSanitizerGroupFluentHelper
{
    public record HtmlSanitizerTagRules(IReadOnlyCollection<HtmlSanitizerTagRule> RuleList);

    public static HtmlSanitizerTagRules Tags(this HtmlSanitizer sanitizer, params ReadOnlySpan<string> tagNames)
    {
        var list = new List<HtmlSanitizerTagRule>(tagNames.Length);
        foreach (var tagName in tagNames)
        {
            list.Add(sanitizer.Tag(tagName));
        }
        return new HtmlSanitizerTagRules(list);
    }

    public static HtmlSanitizerTagRules Flatten(this HtmlSanitizerTagRules rules)
    {
        foreach (var rule in rules.RuleList)
        {
            rule.Flatten();
        }
        return rules;
    }

    public static HtmlSanitizerTagRules RemoveEmpty(this HtmlSanitizerTagRules rules)
    {
        foreach (var rule in rules.RuleList)
        {
            _ = rule.RemoveEmpty();
        }
        return rules;
    }

    public static HtmlSanitizerTagRules NoAttributes(this HtmlSanitizerTagRules rules, SanitizerOperation operation)
    {
        foreach (var rule in rules.RuleList)
        {
            rule.NoAttributesOperation = operation;
        }
        return rules;
    }
}
