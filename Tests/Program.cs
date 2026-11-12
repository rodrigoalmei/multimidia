using System.Text;
using MultimediaCompression.Algorithms;
using MultimediaCompression.Services;

Console.OutputEncoding = Encoding.UTF8;

var cases = new Dictionary<string, string>
{
    ["string vazia"] = string.Empty,
    ["um único caractere"] = "X",
    ["caracteres repetidos"] = "AAAAABBBCCDAA",
    ["sem repetição"] = "ABCDEFG",
    ["frase com espaços"] = "sistemas multimídia sem perdas",
    ["maiúsculas e minúsculas"] = "AaAaBBbb",
    ["acentuados"] = "ação, coração, café e você",
    ["números e símbolos"] = "1111!!!@@@## 2026",
    ["frequências iguais"] = "AABBCCDD",
    ["não reduz tamanho"] = "abcdefg",
    ["Unicode fora do BMP"] = "💙💙🚀🎓"
};

var failures = new List<string>();
var checks = 0;

foreach (var testCase in cases)
{
    Run($"RLE — {testCase.Key}", () =>
    {
        var compressed = Rle.Compress(testCase.Value);
        Equal(testCase.Value, Rle.Decode(compressed.EncodedText));
    });

    Run($"Shannon-Fano — {testCase.Key}", () =>
    {
        var compressed = ShannonFano.Compress(testCase.Value);
        var codes = compressed.Symbols.ToDictionary(item => item.Symbol, item => item.Code);
        Equal(testCase.Value, ShannonFano.Decode(compressed.EncodedBits, codes));
        True(TextUtilities.IsPrefixFree(codes.Values), "Os códigos não são livres de prefixo.");
    });

    Run($"Huffman — {testCase.Key}", () =>
    {
        var compressed = Huffman.Compress(testCase.Value);
        Equal(testCase.Value, Huffman.Decode(compressed.EncodedBits, compressed.Root));
        True(
            TextUtilities.IsPrefixFree(compressed.Symbols.Select(item => item.Code)),
            "Os códigos não são livres de prefixo.");
    });
}

Run("RLE mantém o exemplo didático", () =>
    Equal("A5B3C2D1A2", CompressionAlgorithms.Rle("AAAAABBBCCDAA")));

Run("RLE usa formato inequívoco para algarismos", () =>
{
    var result = Rle.Compress("11122!!");
    True(result.UsesEscapedFormat, "O formato escapado não foi usado.");
    True(result.EncodedText.StartsWith("RLE1|", StringComparison.Ordinal), "Prefixo ausente.");
    Equal("11122!!", Rle.Decode(result.EncodedText));
});

Run("RLE preserva contagem com dois dígitos", () =>
{
    var text = new string('A', 12);
    var encoded = Rle.Encode(text);
    Equal("A12", encoded);
    Equal(text, Rle.Decode(encoded));
});

Run("RLE rejeita representações malformadas", () =>
{
    Throws<FormatException>(() => Rle.Decode("A"));
    Throws<FormatException>(() => Rle.Decode("RLE1|41,3"));
    Throws<FormatException>(() => Rle.Decode("RLE1|41,0;"));
    Throws<FormatException>(() => Rle.Decode("RLE1|41,3;;"));
});

Run("Shannon-Fano é determinístico em empates", () =>
{
    var first = ShannonFano.Compress("AABBCCDD");
    var second = ShannonFano.Compress("DDCCBBAA");
    Equal(CodeSignature(first.Symbols), CodeSignature(second.Symbols));
});

Run("Huffman é determinístico em empates", () =>
{
    var first = Huffman.Compress("AABBCCDD");
    var second = Huffman.Compress("DDCCBBAA");
    Equal(CodeSignature(first.Symbols), CodeSignature(second.Symbols));
});

Run("Huffman clássico tem comprimento ponderado de 224 bits", () =>
{
    var text =
        new string('a', 5) +
        new string('b', 9) +
        new string('c', 12) +
        new string('d', 13) +
        new string('e', 16) +
        new string('f', 45);
    var compressed = Huffman.Compress(text);
    var weightedLength = compressed.Symbols.Sum(item => item.Frequency * item.Code.Length);

    Equal(224, weightedLength);
    Equal(224, compressed.EncodedBits.Length);
    Equal(text, Huffman.Decode(compressed.EncodedBits, compressed.Root));
});

Run("Shannon-Fano rejeita fluxos malformados", () =>
{
    var compressed = ShannonFano.Compress("BANANA");
    var codes = compressed.Symbols.ToDictionary(item => item.Symbol, item => item.Code);
    Throws<FormatException>(() => ShannonFano.Decode(compressed.EncodedBits + "2", codes));
    Throws<ArgumentException>(() => ShannonFano.Decode("0", new Dictionary<Rune, string>
    {
        [new Rune('A')] = "0",
        [new Rune('B')] = "01"
    }));
});

Run("Huffman rejeita fluxos malformados", () =>
{
    var compressed = Huffman.Compress("ABCD");
    Throws<FormatException>(() => Huffman.Decode(compressed.EncodedBits + "2", compressed.Root));
    Throws<FormatException>(() => Huffman.Decode(compressed.EncodedBits[..^1], compressed.Root));
});

Run("Comparação valida os três round-trips", () =>
{
    var comparison = CompressionComparison.Compare("UEPB 2026: ação!");
    True(comparison.Count == 3, "A comparação não retornou três algoritmos.");
    True(comparison.All(item => item.RoundTripSucceeded), "Alguma descompressão falhou.");
});

Run("Comparação usa N/A para percentual de mensagem vazia", () =>
{
    var comparison = CompressionComparison.Compare(string.Empty);
    True(
        comparison.All(item => item.PercentageOfOriginalSize is null),
        "O percentual deveria ser indefinido para mensagem vazia.");
});

if (failures.Count == 0)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\nTodos os {checks} testes passaram.");
    Console.ResetColor();
    return 0;
}

Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine($"\n{failures.Count} de {checks} testes falharam:");
foreach (var failure in failures)
{
    Console.WriteLine($"- {failure}");
}
Console.ResetColor();
return 1;

void Run(string name, Action test)
{
    checks++;
    try
    {
        test();
        Console.WriteLine($"[OK] {name}");
    }
    catch (Exception exception)
    {
        failures.Add($"{name}: {exception.Message}");
        Console.WriteLine($"[FALHOU] {name}");
    }
}

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Esperado: '{expected}'. Obtido: '{actual}'.");
    }
}

static void True(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static void Throws<TException>(Action action)
    where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException($"Era esperada uma exceção do tipo {typeof(TException).Name}.");
}

static string CodeSignature(IEnumerable<MultimediaCompression.Models.SymbolCode> symbols) =>
    string.Join("|", symbols.OrderBy(item => item.Symbol.Value).Select(item => $"{item.Symbol.Value}:{item.Code}"));
