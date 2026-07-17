using System.Text.RegularExpressions;

// Shared "Latin characters only" validation for player-entered names (account nicknames, deck
// names) - keeps sorting/search/display simple and avoids rendering issues for scripts the
// game's fonts don't support. Letters, digits, spaces, and a few common name punctuation marks -
// not just [a-zA-Z0-9], since banning spaces/hyphens/apostrophes would reject perfectly normal
// names too.
public static class TextValidation
{
    private static readonly Regex LatinOnly = new Regex(@"^[a-zA-Z0-9 _'\-]*$", RegexOptions.Compiled);

    public static bool IsLatinOnly(string text)
    {
        return text != null && LatinOnly.IsMatch(text);
    }

    public static bool IsLatinChar(char c)
    {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') ||
               c == ' ' || c == '_' || c == '\'' || c == '-';
    }
}
