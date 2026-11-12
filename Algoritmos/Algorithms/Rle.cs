using System.Globalization;
using System.Text;
using MultimediaCompression.Models;

namespace MultimediaCompression.Algorithms;

public static class Rle
{
    private const string EscapedPrefix = "RLE1|";

    // Mantém a saída didática A5B3C2D1A2 quando os símbolos são letras.
    // Para números, espaços e caracteres especiais usa um formato inequívoco.
    public static string Encode(string texto) => Compress(texto).EncodedText;

    public static string RleEncode(string texto) => Encode(texto);

    public static RleResult Compress(string texto)
    {
        ArgumentNullException.ThrowIfNull(texto);
        var symbols = texto.EnumerateRunes().ToArray();
        if (symbols.Length == 0)
        {
            return new RleResult(texto, string.Empty, Array.Empty<RleRun>(), false);
        }

        var runs = new List<RleRun>();
        for (var index = 0; index < symbols.Length;)
        {
            var current = symbols[index];
            var count = 1;
            while (index + count < symbols.Length && symbols[index + count] == current)
            {
                count++;
            }

            runs.Add(new RleRun(current, count));
            index += count;
        }

        var useEscapedFormat = runs.Any(run => !Rune.IsLetter(run.Symbol));
        var encoded = useEscapedFormat ? EncodeEscaped(runs) : EncodeDidactic(runs);
        return new RleResult(texto, encoded, runs, useEscapedFormat);
    }

    public static string Decode(string encoded)
    {
        ArgumentNullException.ThrowIfNull(encoded);
        if (encoded.Length == 0)
        {
            return string.Empty;
        }

        return encoded.StartsWith(EscapedPrefix, StringComparison.Ordinal)
            ? DecodeEscaped(encoded)
            : DecodeDidactic(encoded);
    }

    private static string EncodeDidactic(IEnumerable<RleRun> runs)
    {
        var output = new StringBuilder();
        foreach (var run in runs)
        {
            output.Append(run.Symbol.ToString());
            output.Append(run.Count.ToString(CultureInfo.InvariantCulture));
        }

        return output.ToString();
    }

    private static string EncodeEscaped(IEnumerable<RleRun> runs)
    {
        var output = new StringBuilder(EscapedPrefix);
        foreach (var run in runs)
        {
            output.Append(run.Symbol.Value.ToString("X", CultureInfo.InvariantCulture));
            output.Append(',');
            output.Append(run.Count.ToString(CultureInfo.InvariantCulture));
            output.Append(';');
        }

        return output.ToString();
    }

    private static string DecodeDidactic(string encoded)
    {
        var output = new StringBuilder();
        var index = 0;
        while (index < encoded.Length)
        {
            var symbol = Rune.GetRuneAt(encoded, index);
            index += symbol.Utf16SequenceLength;
            var countStart = index;
            while (index < encoded.Length && encoded[index] is >= '0' and <= '9')
            {
                index++;
            }

            if (countStart == index ||
                !int.TryParse(encoded[countStart..index], NumberStyles.None, CultureInfo.InvariantCulture, out var count) ||
                count <= 0)
            {
                throw new FormatException("Representação RLE didática inválida.");
            }

            AppendRepeated(output, symbol, count);
        }

        return output.ToString();
    }

    private static string DecodeEscaped(string encoded)
    {
        var output = new StringBuilder();
        var payload = encoded[EscapedPrefix.Length..];
        if (payload.Length == 0 || payload[^1] != ';')
        {
            throw new FormatException("Representação RLE escapada inválida.");
        }

        var serializedRuns = payload[..^1];
        if (serializedRuns.Length == 0)
        {
            throw new FormatException("Representação RLE escapada inválida.");
        }

        foreach (var token in serializedRuns.Split(';', StringSplitOptions.None))
        {
            var fields = token.Split(',');
            if (fields.Length != 2 ||
                !int.TryParse(fields[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var scalar) ||
                !Rune.TryCreate(scalar, out var symbol) ||
                !int.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out var count) ||
                count <= 0)
            {
                throw new FormatException("Representação RLE escapada inválida.");
            }

            AppendRepeated(output, symbol, count);
        }

        return output.ToString();
    }

    private static void AppendRepeated(StringBuilder output, Rune symbol, int count)
    {
        for (var repetition = 0; repetition < count; repetition++)
        {
            output.Append(symbol.ToString());
        }
    }
}
