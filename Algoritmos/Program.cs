using System.Text;
using MultimediaCompression.Algorithms;
using MultimediaCompression.Models;
using MultimediaCompression.Services;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

while (true)
{
    PrintMenu();
    Console.Write("Escolha uma opção: ");
    var option = Console.ReadLine()?.Trim();
    Console.WriteLine();

    switch (option)
    {
        case "1":
            ShowRle(ReadMessage());
            break;
        case "2":
            ShowShannonFano(ReadMessage());
            break;
        case "3":
            ShowHuffman(ReadMessage());
            break;
        case "4":
            ShowComparison(ReadMessage());
            break;
        case "5":
            RunExamples();
            break;
        case "0":
            Console.WriteLine("Programa encerrado.");
            return;
        default:
            Console.WriteLine("Opção inválida. Digite um número de 0 a 5.");
            break;
    }

    Console.WriteLine("\nPressione Enter para voltar ao menu...");
    Console.ReadLine();
    Console.Clear();
}

static void PrintMenu()
{
    Console.WriteLine("===============================================");
    Console.WriteLine(" COMPRESSÃO SEM PERDAS — RLE, SHANNON E HUFFMAN");
    Console.WriteLine("===============================================");
    Console.WriteLine("1. Executar RLE");
    Console.WriteLine("2. Executar Shannon-Fano");
    Console.WriteLine("3. Executar Huffman");
    Console.WriteLine("4. Comparar os três algoritmos");
    Console.WriteLine("5. Executar exemplos predefinidos");
    Console.WriteLine("0. Sair");
    Console.WriteLine();
}

static string ReadMessage()
{
    Console.Write("Digite a mensagem (Enter aceita uma string vazia): ");
    return Console.ReadLine() ?? string.Empty;
}

static void ShowRle(string text)
{
    var result = Rle.Compress(text);
    var decoded = Rle.Decode(result.EncodedText);

    PrintTitle("RUN-LENGTH ENCODING (RLE)");
    Console.WriteLine($"Texto original:      {Quote(text)}");
    Console.WriteLine("Sequências encontradas:");
    if (result.Runs.Count == 0)
    {
        Console.WriteLine("  (nenhuma — entrada vazia)");
    }
    else
    {
        foreach (var run in result.Runs)
        {
            Console.WriteLine($"  {TextUtilities.DisplaySymbol(run.Symbol),-14} → {run.Count}");
        }
    }

    Console.WriteLine($"Resultado comprimido: {Quote(result.EncodedText)}");
    Console.WriteLine($"Formato:              {(result.UsesEscapedFormat ? "escapado e inequívoco (RLE1|HEX,CONTAGEM;)" : "didático (símbolo seguido da contagem)")}");
    Console.WriteLine($"Texto descomprimido:  {Quote(decoded)}");
    PrintValidation(decoded == text);
}

static void ShowShannonFano(string text)
{
    var result = ShannonFano.Compress(text);
    var codes = result.Symbols.ToDictionary(item => item.Symbol, item => item.Code);
    var decoded = ShannonFano.Decode(result.EncodedBits, codes);

    PrintTitle("SHANNON-FANO");
    Console.WriteLine($"Texto original: {Quote(text)}");
    PrintSymbolTable(result.Symbols, includeProbability: true);
    Console.WriteLine("\nDivisões realizadas:");
    PrintSteps(result.Divisions);
    Console.WriteLine($"\nFluxo codificado:   {Quote(result.EncodedBits)}");
    Console.WriteLine($"Bits no fluxo:       {result.EncodedBits.Length}");
    Console.WriteLine($"Texto decodificado:  {Quote(decoded)}");
    Console.WriteLine($"Códigos sem prefixo: {YesNo(TextUtilities.IsPrefixFree(codes.Values))}");
    PrintValidation(decoded == text);
}

static void ShowHuffman(string text)
{
    var result = Huffman.Compress(text);
    var decoded = Huffman.Decode(result.EncodedBits, result.Root);

    PrintTitle("HUFFMAN");
    Console.WriteLine($"Texto original: {Quote(text)}");
    PrintSymbolTable(result.Symbols, includeProbability: false);
    Console.WriteLine("\nCombinações de nós (os dois menores a cada etapa):");
    PrintSteps(result.Combinations);
    Console.WriteLine($"\nFluxo codificado:   {Quote(result.EncodedBits)}");
    Console.WriteLine($"Bits no fluxo:       {result.EncodedBits.Length}");
    Console.WriteLine($"Texto decodificado:  {Quote(decoded)}");
    Console.WriteLine($"Códigos sem prefixo: {YesNo(TextUtilities.IsPrefixFree(result.Symbols.Select(item => item.Code)))}");
    PrintValidation(decoded == text);
}

static void ShowComparison(string text)
{
    var rows = CompressionComparison.Compare(text);
    PrintTitle("COMPARAÇÃO DOS TRÊS ALGORITMOS");
    Console.WriteLine($"Texto original: {Quote(text)}");
    Console.WriteLine($"Tamanho original: {rows.First().OriginalBits} bits em UTF-8\n");
    Console.WriteLine($"{"Algoritmo",-15} {"Unidade",-24} {"Dados",8} {"Metad.",8} {"Total*",8} {"OK?",5}");
    Console.WriteLine(new string('-', 76));
    foreach (var row in rows)
    {
        Console.WriteLine(
            $"{row.Algorithm,-15} {Truncate(row.RepresentationUnits, 24),-24} " +
            $"{row.EncodedDataBits,8} {row.EstimatedMetadataBits,8} {row.EstimatedTotalBits,8} " +
            $"{YesNo(row.RoundTripSucceeded),5}");
    }

    Console.WriteLine("\nPercentual do tamanho original:");
    foreach (var row in rows)
    {
        Console.WriteLine($"  {row.Algorithm,-15} {FormatPercentage(row.PercentageOfOriginalSize),8}");
    }

    Console.WriteLine("\nRepresentações:");
    foreach (var row in rows)
    {
        Console.WriteLine($"  {row.Algorithm}: {Quote(row.EncodedRepresentation)}");
    }

    Console.WriteLine("\n* Total estimado, não tamanho real de arquivo.");
    Console.WriteLine("  Shannon-Fano e Huffman precisam armazenar também a tabela/árvore para decodificação independente.");
    Console.WriteLine("  A estimativa da tabela usa 32 bits por símbolo Unicode, 16 pelo comprimento e os bits do código.");
    Console.WriteLine("  Não inclui cabeçalho, alinhamento, empacotamento de bits nem formato de arquivo.");
    Console.WriteLine("  Percentual do tamanho original = total estimado / tamanho original × 100.");
    Console.WriteLine("  Ele indica quanto do tamanho original a representação ocupa; não é a economia percentual.");
    Console.WriteLine("  Para mensagens vazias, o percentual é matematicamente indefinido e aparece como N/A.");
}

static void RunExamples()
{
    var examples = new[]
    {
        "AAAAABBBCCDAA",
        "BANANA",
        "UEPB Sistemas Multimídia 2026!",
        "ação e coração 💙"
    };

    PrintTitle("EXEMPLOS PREDEFINIDOS");
    foreach (var example in examples)
    {
        Console.WriteLine($"\n>>> Exemplo: {Quote(example)}");
        ShowComparison(example);
    }
}

static void PrintSymbolTable(IReadOnlyList<SymbolCode> symbols, bool includeProbability)
{
    Console.WriteLine("\nTabela de códigos:");
    if (symbols.Count == 0)
    {
        Console.WriteLine("  (nenhum símbolo — entrada vazia)");
        return;
    }

    if (includeProbability)
    {
        Console.WriteLine($"{"Símbolo",-16} {"Frequência",10} {"Probabilidade",14} {"Código",-12}");
        Console.WriteLine(new string('-', 56));
        foreach (var item in symbols)
        {
            Console.WriteLine($"{TextUtilities.DisplaySymbol(item.Symbol),-16} {item.Frequency,10} {item.Probability,13:P2} {item.Code,-12}");
        }
    }
    else
    {
        Console.WriteLine($"{"Símbolo",-16} {"Frequência",10} {"Código",-12}");
        Console.WriteLine(new string('-', 42));
        foreach (var item in symbols)
        {
            Console.WriteLine($"{TextUtilities.DisplaySymbol(item.Symbol),-16} {item.Frequency,10} {item.Code,-12}");
        }
    }
}

static void PrintSteps(IReadOnlyList<string> steps)
{
    if (steps.Count == 0)
    {
        Console.WriteLine("  (nenhuma etapa — entrada vazia)");
        return;
    }

    for (var index = 0; index < steps.Count; index++)
    {
        Console.WriteLine($"  {index + 1}. {steps[index]}");
    }
}

static void PrintTitle(string title)
{
    Console.WriteLine($"\n--- {title} ---");
}

static void PrintValidation(bool success)
{
    Console.WriteLine($"Mensagem recuperada corretamente: {YesNo(success)}");
}

static string Quote(string value) =>
    $"\"{value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t")}\"";
static string YesNo(bool value) => value ? "SIM" : "NÃO";
static string FormatPercentage(double? value) => value.HasValue ? $"{value.Value:F1}%" : "N/A";
static string Truncate(string value, int length) => value.Length <= length ? value : value[..(length - 1)] + "…";
