using MultimediaCompression.Models;

namespace MultimediaCompression.Algorithms;

// Fachada com os nomes solicitados no enunciado.
public static class CompressionAlgorithms
{
    public static string Rle(string texto) => Algorithms.Rle.Encode(texto);

    public static ShannonFanoResult ShannonFano(string texto) => Algorithms.ShannonFano.Compress(texto);

    public static HuffmanResult Huffman(string texto) => Algorithms.Huffman.Compress(texto);
}
