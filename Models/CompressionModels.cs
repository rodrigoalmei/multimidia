using System.Text;

namespace MultimediaCompression.Models;

public sealed record RleRun(Rune Symbol, int Count);

public sealed record RleResult(
    string OriginalText,
    string EncodedText,
    IReadOnlyList<RleRun> Runs,
    bool UsesEscapedFormat);

public sealed record SymbolCode(Rune Symbol, int Frequency, double Probability, string Code);

public sealed record ShannonFanoResult(
    string OriginalText,
    string EncodedBits,
    IReadOnlyList<SymbolCode> Symbols,
    IReadOnlyList<string> Divisions);

public sealed record HuffmanResult(
    string OriginalText,
    string EncodedBits,
    IReadOnlyList<SymbolCode> Symbols,
    IReadOnlyList<string> Combinations,
    HuffmanNode? Root);

public sealed class HuffmanNode
{
    public Rune? Symbol { get; init; }
    public int Frequency { get; init; }
    public int MinimumSymbolValue { get; init; }
    public HuffmanNode? Left { get; init; }
    public HuffmanNode? Right { get; init; }
    public bool IsLeaf => Symbol.HasValue;
}

public sealed record CompressionComparisonRow(
    string Algorithm,
    string EncodedRepresentation,
    string RepresentationUnits,
    int OriginalBits,
    int EncodedDataBits,
    int EstimatedMetadataBits,
    int EstimatedTotalBits,
    double? PercentageOfOriginalSize,
    bool RoundTripSucceeded,
    string Observation);
