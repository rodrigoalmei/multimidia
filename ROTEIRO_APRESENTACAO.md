# Roteiro de apresentação

**Disciplina:** Sistemas Multimídia — UEPB  
**Professor:** André Almeida  
**Data:** 13/11/2026  
**Tema:** compressão sem perdas com RLE, Shannon-Fano e Huffman

## Preparação antes de apresentar

1. Instalar o SDK .NET 8 e abrir o terminal na pasta do projeto.
2. Executar `dotnet build MultimediaCompression.sln -c Release`.
3. Executar `dotnet run --project Tests/MultimediaCompression.Tests.csproj -c Release`.
4. Deixar preparado `dotnet run --project MultimediaCompression.csproj`.
5. Aumentar a fonte do terminal e confirmar que acentos e emoji aparecem corretamente.
6. Usar `AAAAABBBCCDAA` no RLE e `BANANA` nos algoritmos por frequência.

Tempo sugerido total: 12 a 15 minutos.

---

## Arthur — introdução e RLE

### 1. Introdução (1 minuto)

Fala sugerida:

> Nosso projeto implementa três técnicas de compressão sem perdas. Sem perdas significa que, depois de descomprimir, recuperamos exatamente a mensagem original. Implementamos tudo manualmente em C#, sem biblioteca de compressão, e validamos o percurso de ida e volta com testes automatizados.

Pontos importantes:

- Compressão reduz ou reorganiza a representação; nem toda entrada fica menor.
- O ganho depende do padrão dos dados.
- O programa trabalha com símbolos Unicode completos.

### 2. Explicação do RLE (2 minutos)

Fala sugerida:

> O RLE procura repetições consecutivas. Ele guarda o símbolo e quantas vezes ele aparece seguido. Não usa a frequência total da mensagem; interessa apenas a sequência local.

Escrever ou mostrar:

```text
AAAAABBBCCDAA
AAAAA | BBB | CC | D | AA
A5B3C2D1A2
```

Explicar o laço:

1. Guardar o símbolo atual.
2. Avançar enquanto o próximo for igual.
3. Registrar símbolo e contagem.
4. Continuar na próxima sequência.

Complexidade: uma passagem, `O(n)` no tempo.

### 3. Demonstração do RLE (1 minuto)

1. No menu, escolher `1`.
2. Digitar `AAAAABBBCCDAA`.
3. Apontar as sequências e o resultado `A5B3C2D1A2`.
4. Apontar a mensagem descomprimida e `SIM`.
5. Se houver tempo, repetir com `11122!!` e mostrar `RLE1|31,3;32,2;21,2;`.

Explicar o formato alternativo:

> Se escrevêssemos números diretamente, símbolo e contagem poderiam se confundir. O formato escapado grava o valor Unicode hexadecimal e a contagem separados por pontuação. Assim, a representação é inequívoca.

### Possíveis perguntas para Arthur

**RLE sempre comprime?**  
Não. Em `ABCDEFG`, a saída clássica seria `A1B1C1D1E1F1G1`, maior que a entrada. Ele é indicado para muitas repetições consecutivas.

**Por que quatro letras A separadas em partes diferentes não são somadas?**  
Porque RLE mede corridas consecutivas, não frequência global.

**Como a descompressão funciona?**  
Ela lê cada par símbolo/contagem e repete o símbolo a quantidade indicada.

**Por que existe `RLE1|`?**  
É um marcador de versão/formato. Permite ao decodificador distinguir a escrita escapada da escrita didática.

**Qual é o melhor tipo de dado para RLE?**  
Imagens simples, máscaras, pixels repetidos ou textos com longas sequências iguais.

Transição:

> O RLE explora repetições vizinhas. Agora Rodrigo mostrará um método que usa a frequência global dos símbolos.

---

## Rodrigo — Shannon-Fano

### 1. Ideia central (2 minutos)

Fala sugerida:

> Shannon-Fano conta a frequência de todos os símbolos e os ordena do mais frequente para o menos frequente. Depois divide a lista recursivamente em dois grupos com somas tão próximas quanto possível. O primeiro grupo recebe zero e o segundo recebe um.

Usar `BANANA`:

```text
A: 3
N: 2
B: 1
```

Mostrar conceitualmente:

```text
{A, N, B}
  0: {A}       soma 3
  1: {N, B}    soma 3

{N, B}
  0: {N}       soma 2
  1: {B}       soma 1
```

Uma tabela possível é `A=0`, `N=10`, `B=11`. A mensagem vira a concatenação dos códigos.

### 2. Por que é decodificável (1 minuto)

> Os códigos formam um conjunto livre de prefixos: nenhum código completo é o começo de outro. Por isso, lemos os bits até reconhecer um código, emitimos o símbolo e recomeçamos.

Explicar que a tabela precisa acompanhar o fluxo em uma transmissão independente. Somente os bits não informam qual símbolo corresponde a cada código.

### 3. Demonstração (1 a 2 minutos)

1. Escolher `2`.
2. Digitar `BANANA`.
3. Mostrar frequência, probabilidade e código.
4. Ler as divisões exibidas pelo programa.
5. Mostrar fluxo, decodificação, propriedade de prefixo e validação.

### 4. Determinismo

> Quando as frequências empatam, ordenamos pelo valor Unicode. Se dois cortes forem igualmente bons, escolhemos o primeiro. Isso evita mudar o resultado de uma execução para outra.

### Possíveis perguntas para Rodrigo

**Shannon-Fano sempre encontra o melhor código?**  
Não. Ele busca equilibrar grupos de cima para baixo, mas essa decisão local de divisão não garante o menor comprimento médio possível.

**Qual é a diferença entre frequência e probabilidade?**  
Frequência é a contagem. Probabilidade é frequência dividida pelo total de símbolos.

**Por que o símbolo mais frequente costuma ter código menor?**  
Ele aparece em grupos maiores e tende a ser isolado após poucas divisões, recebendo menos bits.

**O que acontece com um texto de um único símbolo?**  
Usamos o código `0` por convenção. Cada ocorrência gera um zero.

**O fluxo `010...` basta para outra máquina decodificar?**  
Não. Ela também precisa conhecer a tabela de códigos, ou dados suficientes para reconstruí-la.

**Qual é o custo principal da implementação?**  
Contar é `O(n)`, ordenar os símbolos distintos custa `O(k log k)` e as divisões percorrem grupos recursivamente.

Transição:

> Shannon-Fano constrói os códigos dividindo do conjunto para os símbolos. Arton mostrará Huffman, que faz o caminho inverso: começa nas folhas e combina os menores pesos.

---

## Arton — Huffman, comparação e conclusão

### 1. Algoritmo de Huffman (2 minutos)

Fala sugerida:

> Huffman cria uma folha para cada símbolo e coloca as folhas em uma fila de prioridade. Em cada passo retira os dois nós de menor frequência, cria um pai com a soma e devolve esse pai à fila. Quando sobra um nó, temos a raiz da árvore.

Com `BANANA`, destacar as combinações mostradas pelo programa:

```text
B(1) + N(2) → 3
A(3) + nó(3) → 6
```

> Depois percorremos a árvore. Esquerda recebe zero e direita recebe um. O caminho da raiz até cada folha é o código do símbolo.

Explicar a natureza gulosa: sempre combinar os dois menores pesos.

### 2. Demonstração de Huffman (1 minuto)

1. Escolher `3`.
2. Digitar `BANANA`.
3. Mostrar a tabela.
4. Ler as combinações de nós.
5. Mostrar fluxo, texto recuperado e códigos sem prefixo.

### 3. Comparação (2 minutos)

1. Escolher `4`.
2. Usar `AAAAABBBCCDAA` ou uma frase curta.
3. Explicar as colunas:
   - original: bits do texto UTF-8;
   - dados: fluxo codificado;
   - metadados: tabela estimada;
   - total: soma estimada;
   - percentual do tamanho original: total estimado/original × 100.

Fala sugerida:

> Não apresentamos o número de bits do fluxo como se fosse um arquivo pronto. Shannon-Fano e Huffman precisam levar uma tabela ou árvore, e um formato real ainda teria cabeçalho e alinhamento. Em textos pequenos, essa informação extra pode eliminar qualquer ganho.

Esclarecer a métrica:

> O percentual do tamanho original indica quanto a representação estimada ocupa em relação ao original. Ele não é diretamente a economia: se o resultado for 76%, a economia estimada é 24%. Para uma mensagem vazia, essa divisão é indefinida e aparece como N/A.

Resumo comparativo:

- RLE: melhor para repetições consecutivas.
- Shannon-Fano: divisão recursiva das frequências; simples para visualizar, mas não garante ótimo.
- Huffman: combina os menores pesos e é ótimo entre códigos de prefixo por símbolo para a distribuição dada.

### 4. Conclusão (30 segundos)

> Os três métodos recuperaram exatamente as entradas nos testes. O projeto mostra que não existe um algoritmo universalmente melhor: o resultado depende dos dados e também do custo dos metadados. A principal diferença está no padrão explorado por cada técnica.

### Possíveis perguntas para Arton

**Por que Huffman é chamado guloso?**  
Porque toma repetidamente a decisão local de combinar os dois menores pesos. Essa escolha leva ao código de prefixo ótimo para as frequências dadas.

**Por que esquerda é 0 e direita é 1?**  
É uma convenção. Inverter todos os lados ainda produziria um código válido com os mesmos comprimentos.

**Empates podem gerar árvores diferentes?**  
Sim, várias árvores podem ter o mesmo custo. Nosso desempate pelo menor valor Unicode e ordem de criação torna o resultado determinístico.

**Huffman sempre deixa o arquivo menor?**  
Não. O fluxo pode ser menor, mas a árvore/tabela e o cabeçalho têm custo. Para entradas curtas, o total pode crescer.

**Qual a diferença essencial para Shannon-Fano?**  
Shannon-Fano divide o conjunto de cima para baixo. Huffman combina nós de baixo para cima e garante comprimento médio mínimo entre códigos binários de prefixo por símbolo.

**Como a decodificação percorre a árvore?**  
Começa na raiz, vai à esquerda com `0` e à direita com `1`. Ao chegar a uma folha, emite o símbolo e volta à raiz.

**Por que medir o original em UTF-8?**  
Porque caracteres acentuados e emoji não ocupam necessariamente um byte. Contar apenas caracteres daria uma comparação incorreta.

**O percentual do tamanho original é a economia obtida?**  
Não. Ele é `total estimado / original × 100`. Quando fica abaixo de 100%, a economia estimada é `100% - percentual`. Para entrada vazia, o valor é `N/A` porque não é possível dividir por zero.

---

## Perguntas gerais que qualquer integrante pode responder

**O projeto comprime arquivos?**  
Não. Ele demonstra os algoritmos sobre mensagens e fluxos em memória. Criar um formato de arquivo exigiria definir cabeçalho, serialização da tabela e empacotamento de bits.

**Vocês usaram biblioteca pronta?**  
Não. Frequências, divisões, fila de prioridade, árvore, codificação e decodificação foram implementadas no projeto. A fila de prioridade é uma estrutura da biblioteca padrão, não um compressor pronto.

**Como provar que não houve perda?**  
O programa mostra a mensagem recuperada e compara com a original. Os testes automatizados repetem essa verificação para todos os cenários obrigatórios.

**Por que usar `Rune` em vez de `char`?**  
Em C#, `char` é uma unidade UTF-16. Alguns símbolos, como muitos emoji, usam dois `char`. `Rune` mantém o símbolo Unicode completo durante contagem e codificação.

**O que significa código livre de prefixo?**  
Nenhum código de símbolo é prefixo de outro. Isso elimina a necessidade de separadores no fluxo binário.

## Plano B para a demonstração

Se houver problema no terminal:

- mostrar o exemplo RLE deste roteiro;
- explicar as divisões de `BANANA`;
- desenhar três folhas com pesos 1, 2 e 3 para Huffman;
- abrir o `README.md`, que contém comandos, exemplos e diferenças;
- nunca afirmar que uma execução passou se ela não foi realizada naquela máquina.
