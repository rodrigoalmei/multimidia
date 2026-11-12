using System.Text;
using MultimediaCompression.Models;
using MultimediaCompression.Services;

namespace MultimediaCompression.Algorithms;

public static class Huffman
{
    public static HuffmanResult HuffmanEncode(string texto) => Compress(texto);

    public static HuffmanResult Compress(string texto)
    {
        ArgumentNullException.ThrowIfNull(texto);
        var frequencies = TextUtilities.CountFrequencies(texto);
        if (frequencies.Count == 0)
        {
            return new HuffmanResult(
                texto,
                string.Empty,
                Array.Empty<SymbolCode>(),
                Array.Empty<string>(),
                null);
        }

        var queue = new PriorityQueue<HuffmanNode, (int Frequency, int MinimumSymbol, long Sequence)>();
        long sequence = 0;
        foreach (var item in frequencies.OrderBy(item => item.Key.Value))
        {
            var leaf = new HuffmanNode
            {
                Symbol = item.Key,
                Frequency = item.Value,
                MinimumSymbolValue = item.Key.Value
            };
            queue.Enqueue(leaf, (leaf.Frequency, leaf.MinimumSymbolValue, sequence++));
        }

        var combinations = new List<string>();
        while (queue.Count > 1)
        {
            var left = queue.Dequeue();
            var right = queue.Dequeue();
            var parent = new HuffmanNode
            {
                Frequency = left.Frequency + right.Frequency,
                MinimumSymbolValue = Math.Min(left.MinimumSymbolValue, right.MinimumSymbolValue),
                Left = left,
                Right = right
            };
            combinations.Add(
                $"{DescribeNode(left)} + {DescribeNode(right)} → nó interno ({parent.Frequency})");
            queue.Enqueue(parent, (parent.Frequency, parent.MinimumSymbolValue, sequence++));
        }

        var root = queue.Dequeue();
        var codes = new Dictionary<Rune, string>();
        BuildCodes(root, string.Empty, codes);
        var total = frequencies.Values.Sum();
        var symbols = frequencies
            .OrderByDescending(item => item.Value)
            .ThenBy(item => item.Key.Value)
            .Select(item => new SymbolCode(
                item.Key,
                item.Value,
                (double)item.Value / total,
                codes[item.Key]))
            .ToArray();
        var encoded = Encode(texto, codes);

        return new HuffmanResult(texto, encoded, symbols, combinations, root);
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

    public static string Decode(string bits, HuffmanNode? root)
    {
        ArgumentNullException.ThrowIfNull(bits);
        if (root is null)
        {
            return bits.Length == 0
                ? string.Empty
                : throw new FormatException("Não existe árvore para decodificar um fluxo não vazio.");
        }

        if (root.IsLeaf)
        {
            if (bits.Any(bit => bit != '0'))
            {
                throw new FormatException("O fluxo de símbolo único deve conter somente zeros.");
            }

            return string.Concat(Enumerable.Repeat(root.Symbol!.Value.ToString(), bits.Length));
        }

        var output = new StringBuilder();
        var current = root;
        foreach (var bit in bits)
        {
            current = bit switch
            {
                '0' => current.Left,
                '1' => current.Right,
                _ => throw new FormatException("O fluxo contém um valor diferente de 0 ou 1.")
            } ?? throw new FormatException("O fluxo não corresponde à árvore de Huffman.");

            if (current.IsLeaf)
            {
                output.Append(current.Symbol!.Value.ToString());
                current = root;
            }
        }

        if (!ReferenceEquals(current, root))
        {
            throw new FormatException("O fluxo terminou no meio de um código Huffman.");
        }

        return output.ToString();
    }

    private static void BuildCodes(HuffmanNode node, string prefix, Dictionary<Rune, string> codes)
    {
        if (node.IsLeaf)
        {
            codes[node.Symbol!.Value] = prefix.Length == 0 ? "0" : prefix;
            return;
        }

        if (node.Left is not null)
        {
            BuildCodes(node.Left, prefix + "0", codes);
        }

        if (node.Right is not null)
        {
            BuildCodes(node.Right, prefix + "1", codes);
        }
    }

    private static string DescribeNode(HuffmanNode node) => node.IsLeaf
        ? $"'{TextUtilities.DisplaySymbol(node.Symbol!.Value)}' ({node.Frequency})"
        : $"nó interno ({node.Frequency})";
}
