using System.Text;

namespace MultimediaCompression.Services;

public static class TextUtilities
{
    public static string DisplaySymbol(Rune symbol) => symbol.Value switch
    {
        0x20 => "<espaço>",
        0x09 => "<tab>",
        0x0A => "<nova linha>",
        0x0D => "<retorno>",
        _ when Rune.IsControl(symbol) => $"U+{symbol.Value:X4}",
        _ => symbol.ToString()
    };

    public static string DisplayGroup(IEnumerable<Rune> symbols) =>
        "{" + string.Join(", ", symbols.Select(DisplaySymbol)) + "}";

    public static Dictionary<Rune, int> CountFrequencies(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var frequencies = new Dictionary<Rune, int>();
        foreach (var symbol in text.EnumerateRunes())
        {
            frequencies[symbol] = frequencies.GetValueOrDefault(symbol) + 1;
        }

        return frequencies;
    }

    public static bool IsPrefixFree(IEnumerable<string> codes)
    {
        var ordered = codes.OrderBy(code => code.Length).ThenBy(code => code).ToArray();
        for (var i = 0; i < ordered.Length; i++)
        {
            for (var j = i + 1; j < ordered.Length; j++)
            {
                if (ordered[j].StartsWith(ordered[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
