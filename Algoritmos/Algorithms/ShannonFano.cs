using System.Text;
using MultimediaCompression.Models;
using MultimediaCompression.Services;

namespace MultimediaCompression.Algorithms;

public static class ShannonFano
{
    private sealed class WorkingSymbol
    {
        public required Rune Symbol { get; init; }
        public required int Frequency { get; init; }
        public string Code { get; set; } = string.Empty;
    }

    public static ShannonFanoResult ShannonFanoEncode(string texto) => Compress(texto);

    public static ShannonFanoResult Compress(string texto)
    {
        ArgumentNullException.ThrowIfNull(texto);
        var frequencies = TextUtilities.CountFrequencies(texto);
        var total = frequencies.Values.Sum();
        var ordered = frequencies
            .OrderByDescending(item => item.Value)
            .ThenBy(item => item.Key.Value)
            .Select(item => new WorkingSymbol { Symbol = item.Key, Frequency = item.Value })
            .ToList();
        var divisions = new List<string>();

        if (ordered.Count == 1)
        {
            ordered[0].Code = "0";
            divisions.Add("Há somente um símbolo; por convenção, ele recebe o código 0.");
        }
        else if (ordered.Count > 1)
        {
            AssignCodes(ordered, 0, ordered.Count, divisions);
        }

        var codes = ordered.ToDictionary(item => item.Symbol, item => item.Code);
        var encoded = Encode(texto, codes);
        var symbols = ordered
            .Select(item => new SymbolCode(
                item.Symbol,
                item.Frequency,
                (double)item.Frequency / total,
                item.Code))
            .ToArray();

        return new ShannonFanoResult(texto, encoded, symbols, divisions);
    }

    public static string Encode(string texto, IReadOnlyDictionary<Rune, string> codes)
    {
        ArgumentNullException.ThrowIfNull(texto);
        ArgumentNullException.ThrowIfNull(codes);
        var output = new StringBuilder();
        foreach (var symbol in texto.EnumerateRunes())
        {
            if (!codes.TryGetValue(symbol, out var code))
            {
                throw new ArgumentException("A tabela não contém todos os símbolos do texto.", nameof(codes));
            }

            output.Append(code);
        }

        return output.ToString();
    }

    public static string Decode(string bits, IReadOnlyDictionary<Rune, string> codes)
    {
        ArgumentNullException.ThrowIfNull(bits);
        ArgumentNullException.ThrowIfNull(codes);
        if (bits.Length == 0)
        {
            return string.Empty;
        }

        if (!TextUtilities.IsPrefixFree(codes.Values))
        {
            throw new ArgumentException("A tabela de códigos não é livre de prefixos.", nameof(codes));
        }

        var reverse = codes.ToDictionary(item => item.Value, item => item.Key);
        var current = new StringBuilder();
        var output = new StringBuilder();
        foreach (var bit in bits)
        {
            if (bit is not ('0' or '1'))
            {
                throw new FormatException("O fluxo contém um valor diferente de 0 ou 1.");
            }

            current.Append(bit);
            if (reverse.TryGetValue(current.ToString(), out var symbol))
            {
                output.Append(symbol.ToString());
                current.Clear();
            }
        }

        if (current.Length != 0)
        {
            throw new FormatException("O fluxo terminou no meio de um código Shannon-Fano.");
        }

        return output.ToString();
    }

    private static void AssignCodes(
        List<WorkingSymbol> symbols,
        int start,
        int count,
        List<string> divisions)
    {
        if (count <= 1)
        {
            return;
        }

        var total = 0;
        for (var index = start; index < start + count; index++)
        {
            total += symbols[index].Frequency;
        }

        var leftSum = 0;
        var bestDifference = int.MaxValue;
        var leftCount = 1;
        for (var candidateCount = 1; candidateCount < count; candidateCount++)
        {
            leftSum += symbols[start + candidateCount - 1].Frequency;
            var difference = Math.Abs(total - 2 * leftSum);
            if (difference < bestDifference)
            {
                bestDifference = difference;
                leftCount = candidateCount;
            }
        }

        var firstGroup = symbols.GetRange(start, leftCount);
        var secondGroup = symbols.GetRange(start + leftCount, count - leftCount);
        var firstSum = firstGroup.Sum(item => item.Frequency);
        var secondSum = secondGroup.Sum(item => item.Frequency);
        divisions.Add(
            $"{TextUtilities.DisplayGroup(symbols.GetRange(start, count).Select(item => item.Symbol))} " +
            $"→ 0: {TextUtilities.DisplayGroup(firstGroup.Select(item => item.Symbol))} (soma {firstSum}) | " +
            $"1: {TextUtilities.DisplayGroup(secondGroup.Select(item => item.Symbol))} (soma {secondSum})");

        foreach (var item in firstGroup)
        {
            item.Code += "0";
        }

        foreach (var item in secondGroup)
        {
            item.Code += "1";
        }

        AssignCodes(symbols, start, leftCount, divisions);
        AssignCodes(symbols, start + leftCount, count - leftCount, divisions);
    }
}
