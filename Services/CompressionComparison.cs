using System.Text;
using MultimediaCompression.Algorithms;
using MultimediaCompression.Models;

namespace MultimediaCompression.Services;

public static class CompressionComparison
{
    public static IReadOnlyList<CompressionComparisonRow> Compare(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var originalBits = Encoding.UTF8.GetByteCount(text) * 8;

        var rle = Rle.Compress(text);
        var rleDecoded = Rle.Decode(rle.EncodedText);
        var rleBits = Encoding.UTF8.GetByteCount(rle.EncodedText) * 8;

        var shannon = ShannonFano.Compress(text);
        var shannonCodes = shannon.Symbols.ToDictionary(item => item.Symbol, item => item.Code);
        var shannonDecoded = ShannonFano.Decode(shannon.EncodedBits, shannonCodes);
        var shannonMetadata = EstimateCodeTableBits(shannon.Symbols);

        var huffman = Huffman.Compress(text);
        var huffmanDecoded = Huffman.Decode(huffman.EncodedBits, huffman.Root);
        var huffmanMetadata = EstimateCodeTableBits(huffman.Symbols);

        return new[]
        {
            CreateRow(
                "RLE",
                rle.EncodedText,
                $"{rle.EncodedText.EnumerateRunes().Count()} símbolos Unicode",
                originalBits,
                rleBits,
                0,
                rleDecoded == text,
                "Representação textual completa em UTF-8; não é um arquivo empacotado."),
            CreateRow(
                "Shannon-Fano",
                shannon.EncodedBits,
                $"{shannon.EncodedBits.Length} bits no fluxo",
                originalBits,
                shannon.EncodedBits.Length,
                shannonMetadata,
                shannonDecoded == text,
                "Estimativa inclui uma tabela símbolo/código; cabeçalhos e empacotamento não estão incluídos."),
            CreateRow(
                "Huffman",
                huffman.EncodedBits,
                $"{huffman.EncodedBits.Length} bits no fluxo",
                originalBits,
                huffman.EncodedBits.Length,
                huffmanMetadata,
                huffmanDecoded == text,
                "Estimativa inclui uma tabela símbolo/código; uma árvore ou tabela deve acompanhar o fluxo.")
        };
    }

    private static CompressionComparisonRow CreateRow(
        string algorithm,
        string representation,
        string units,
        int originalBits,
        int dataBits,
        int metadataBits,
        bool success,
        string observation)
    {
        var total = dataBits + metadataBits;
        double? percentageOfOriginalSize = originalBits == 0
            ? null
            : (double)total / originalBits * 100;
        return new CompressionComparisonRow(
            algorithm,
            representation,
            units,
            originalBits,
            dataBits,
            metadataBits,
            total,
            percentageOfOriginalSize,
            success,
            observation);
    }

    // Estimativa didática: 32 bits para o valor Unicode, 16 para o comprimento
    // e os próprios bits do código. Um formato real também teria cabeçalho/alinhamento.
    private static int EstimateCodeTableBits(IEnumerable<SymbolCode> symbols) =>
        symbols.Sum(item => 32 + 16 + item.Code.Length);
}
