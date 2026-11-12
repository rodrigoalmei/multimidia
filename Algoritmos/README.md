# Compressão sem perdas em C#

Projeto acadêmico da disciplina **Sistemas Multimídia**, do curso de Ciência da Computação da Universidade Estadual da Paraíba (UEPB), ministrada pelo professor André Almeida. A aplicação demonstra manualmente três algoritmos de compressão sem perdas: Run-Length Encoding (RLE), Shannon-Fano e Huffman.

Apresentação: **13/11/2026**.

## Integrantes

- Arthur Marques
- Rodrigo Almeida
- Arton

## Objetivo

Receber mensagens textuais, comprimi-las e descomprimi-las sem perder informação. Além do resultado, o programa mostra as frequências, divisões ou combinações que explicam cada algoritmo. Nenhuma biblioteca pronta de compressão é utilizada.

## Tecnologias

- C# 12
- .NET 8
- `System.Text.Rune` para tratar símbolos Unicode completos
- Aplicação e testes de console, sem pacotes externos

## Estrutura

```text
multimidia/
├── Algorithms/
│   ├── CompressionAlgorithms.cs  # Fachada com os métodos pedidos
│   ├── Rle.cs
│   ├── ShannonFano.cs
│   └── Huffman.cs
├── Models/
│   └── CompressionModels.cs
├── Services/
│   ├── CompressionComparison.cs
│   └── TextUtilities.cs
├── Tests/
│   ├── MultimediaCompression.Tests.csproj
│   └── Program.cs
├── Program.cs
├── MultimediaCompression.csproj
├── MultimediaCompression.sln
├── README.md
└── ROTEIRO_APRESENTACAO.md
```

## Pré-requisitos e instalação

Instale o **SDK do .NET 8** (não apenas o Runtime) pela página oficial da Microsoft. O projeto não possui dependências NuGet externas; por isso, não há biblioteca adicional para instalar.

Confirme a instalação:

```powershell
dotnet --version
```

## Como executar

No diretório do projeto:

```powershell
dotnet run --project MultimediaCompression.csproj
```

O menu oferece:

1. Executar RLE
2. Executar Shannon-Fano
3. Executar Huffman
4. Comparar os três algoritmos
5. Executar exemplos predefinidos
0. Sair

Para compilar em modo Release:

```powershell
dotnet build MultimediaCompression.sln -c Release
```

## Como executar os testes

```powershell
dotnet run --project Tests/MultimediaCompression.Tests.csproj -c Release
```

O executor retorna código `0` quando tudo passa e código `1` quando há falhas. Ele cobre string vazia, símbolo único, repetições, contagens RLE com dois dígitos, ausência de repetição, espaços, maiúsculas/minúsculas, acentos, números, símbolos, empates de frequência, casos sem ganho, símbolos Unicode representados por pares substitutos em UTF-16 e fluxos malformados. Também valida o exemplo clássico de Huffman com frequências 5, 9, 12, 13, 16 e 45, cujo comprimento ponderado é 224 bits.

Em todos os casos é verificado que:

```text
Descomprimir(Comprimir(texto)) == texto
```

Também são validados a propriedade de prefixo de Shannon-Fano e Huffman, o desempate determinístico e a saída didática do RLE.

## RLE

O Run-Length Encoding percorre o texto uma vez e reúne símbolos iguais consecutivos. Cada sequência é substituída pelo símbolo e sua contagem.

```text
Entrada: AAAAABBBCCDAA
Saída:   A5B3C2D1A2
```

Essa escrita simples seria ambígua se o próprio texto contivesse números. Por isso, a implementação usa automaticamente o formato inequívoco `RLE1|HEX,CONTAGEM;` quando existe qualquer símbolo que não seja uma letra. O valor Unicode do símbolo é escrito em hexadecimal.

```text
Entrada: 11122!!
Saída:   RLE1|31,3;32,2;21,2;
```

A saída clássica do exemplo continua preservada. A complexidade é `O(n)` no tempo e `O(n)` no resultado. RLE funciona melhor em dados com longas repetições consecutivas e pode aumentar mensagens sem repetições.

## Shannon-Fano

1. Conta a frequência de cada símbolo.
2. Ordena por frequência decrescente.
3. Divide a lista em dois grupos cujas somas ficam o mais próximas possível.
4. Acrescenta `0` aos símbolos do primeiro grupo e `1` aos do segundo.
5. Repete recursivamente até cada grupo possuir um símbolo.

Empates de frequência são resolvidos pelo valor Unicode crescente do símbolo. Se mais de um ponto de divisão tiver a mesma diferença, é usado o primeiro. Assim, a mesma distribuição sempre produz a mesma tabela, independentemente da ordem do texto.

A tabela gerada é livre de prefixos: nenhum código completo inicia outro código. Isso permite ler o fluxo da esquerda para a direita sem separadores. Para uma entrada com um único símbolo é usado, por convenção, o código `0`.

## Huffman

1. Cria uma folha para cada símbolo e sua frequência.
2. Coloca as folhas em uma fila de prioridade mínima.
3. Retira os dois nós de menor frequência.
4. Cria um pai com a soma e coloca esse pai novamente na fila.
5. Repete até restar a raiz da árvore.
6. Percorre a árvore: esquerda recebe `0` e direita recebe `1`.

Nos empates, a fila compara o menor valor Unicode existente em cada subárvore e, por fim, a ordem de criação do nó. Isso torna a árvore reproduzível. Assim como em Shannon-Fano, o símbolo único recebe `0`.

Huffman é guloso: combina os dois menores pesos a cada etapa. Para as frequências dadas, produz um código de prefixo com comprimento médio mínimo entre códigos binários baseados em símbolos.

## Diferenças principais

| Aspecto | RLE | Shannon-Fano | Huffman |
|---|---|---|---|
| Ideia | Substituir sequências repetidas | Dividir frequências em grupos equilibrados | Combinar repetidamente os dois menores pesos |
| Usa frequência global | Não | Sim | Sim |
| Estrutura | Lista de sequências | Tabela criada por divisões recursivas | Árvore binária criada por fila de prioridade |
| Melhor cenário | Muitas repetições consecutivas | Símbolos com probabilidades desiguais | Símbolos com probabilidades desiguais |
| Código ótimo para as frequências | Não | Não é garantido | Sim, entre códigos de prefixo por símbolo |
| Informação extra | O próprio formato contém símbolo e contagem | Tabela de códigos | Árvore, frequências ou tabela de códigos |

## Comparação de tamanhos

O programa separa explicitamente:

- tamanho original: quantidade de bytes UTF-8 multiplicada por 8;
- bits de dados: bits do fluxo Shannon-Fano/Huffman ou bytes UTF-8 da representação textual RLE;
- metadados estimados: tabela necessária para decodificação independente;
- total estimado: dados + estimativa de metadados.

Para a tabela de códigos, a estimativa didática usa 32 bits para o valor Unicode, 16 bits para o comprimento do código e os próprios bits do código. Ela **não é o tamanho de um arquivo real**: cabeçalho, alinhamento, serialização e empacotamento de bits dependeriam de um formato definido. Em mensagens pequenas, os metadados frequentemente custam mais do que a economia no fluxo.

A métrica **Percentual do tamanho original** é:

```text
total estimado / tamanho original × 100
```

Ela informa quanto do tamanho original a representação estimada ocupa; não representa diretamente a economia percentual. Valor abaixo de 100% indica redução estimada e acima de 100% indica expansão. Quando existe redução, a economia percentual pode ser obtida por `100% - percentual do tamanho original`. Para uma mensagem vazia, a divisão é matematicamente indefinida e o programa exibe `N/A`.

## Exemplo de uso

Escolha a opção `1` e digite `AAAAABBBCCDAA`. O programa exibirá as cinco sequências, `A5B3C2D1A2`, o texto recuperado e a confirmação `SIM`.

Na opção `4`, a mesma mensagem passa pelos três algoritmos. A tela mostra o fluxo de cada um, os bits de dados, a estimativa de metadados, o percentual do tamanho original e a validação da descompressão.

## Decisões de implementação

- O texto é percorrido como `Rune`, não como unidades UTF-16 isoladas. Portanto, símbolos como `💙` permanecem inteiros.
- Entradas vazias geram saídas vazias.
- Entradas com apenas um símbolo usam o código binário `0`.
- A descompressão rejeita fluxos malformados.
- Os algoritmos e os testes não dependem de serviços, banco de dados, API ou interface web.

## Referências

- GeeksforGeeks — [Run Length Encoding and Decoding](https://www.geeksforgeeks.org/dsa/run-length-encoding/)
- GeeksforGeeks — [Shannon-Fano Algorithm for Data Compression](https://www.geeksforgeeks.org/dsa/shannon-fano-algorithm-for-data-compression/)
- GeeksforGeeks — [Huffman Coding Algorithm](https://www.geeksforgeeks.org/dsa/huffman-coding-greedy-algo-3/)
- Microsoft — [Download .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)

As referências foram usadas como base para a lógica algorítmica. O projeto adapta as ideias para entrada textual, Unicode, decodificação, visualização de etapas, desempates determinísticos e testes em C#.
