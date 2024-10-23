namespace Aexra.Codebase.Algorithms.Coding;
public static class Huffman
{
    private class Symbol
    {
        public char value;
        public int count;
        public double frequency;
    }

    private class Node
    {
        public Node left;
        public Node right;
        public double frequency;
        public string code = string.Empty;

        public bool IsLeaf => left == null && right == null;
    }

    // Метод для кодирования строки
    public static (string encodedString, Dictionary<char, int> frequencyTable) Encode(string input, Action<string> log)
    {
        var symbols = from s in GetInputSymbols(input) orderby s.frequency descending select s;
        var nsTable = GetInitialNodeTable(symbols);

        var frequencyTable = new Dictionary<char, int>();

        foreach (var symbol in symbols)
        {
            frequencyTable[symbol.value] = symbol.count;
        }

        var firstLayerNodes = nsTable.Select(kv => kv.Key);

        var root = BuildRecursiveHuffmanTree(firstLayerNodes);
        var encodingTable = BuildEncodingTable(root, nsTable);

        log?.Invoke(string.Join("\n", encodingTable.Select(t => $"{t.Key} -> {t.Value}")));

        var encodedString = EncodeString(input, encodingTable);

        return (encodedString, frequencyTable);
    }

    // Метод для декодирования строки
    public static string Decode(string encodedString, int msgSize, Dictionary<char, int> frequencyTable, Action<string> log)
    {
        var symbolsTmp = new List<Symbol>();
        foreach (var symbol in frequencyTable)
        {
            symbolsTmp.Add(new Symbol() { value = symbol.Key, count = symbol.Value, frequency = (double)symbol.Value / msgSize });
        }

        var symbols = from s in symbolsTmp orderby s.frequency descending select s;

        var nsTable = GetInitialNodeTable(symbols);
        var firstLayerNodes = nsTable.Select(kv => kv.Key);
        var root = BuildRecursiveHuffmanTree(firstLayerNodes);
        var encodingTable = BuildEncodingTable(root, nsTable);

        log?.Invoke(string.Join("\n", encodingTable.Select(t => $"{t.Key} -> {t.Value}")));

        var decodingTable = encodingTable.ToDictionary(kv => kv.Value, kv => kv.Key);
        var decodedString = "";
        var buffer = "";

        foreach (var bit in encodedString)
        {
            buffer += bit;
            if (decodingTable.ContainsKey(buffer))
            {
                decodedString += decodingTable[buffer];
                buffer = "";
            }
        }

        return decodedString;
    }

    // Построение частотной таблицы символов
    private static Dictionary<char, int> BuildFrequencyTable(string input)
    {
        var frequencyTable = new Dictionary<char, int>();

        foreach (var c in input)
        {
            if (frequencyTable.ContainsKey(c))
                frequencyTable[c]++;
            else
                frequencyTable[c] = 1;
        }

        return frequencyTable;
    }

    private static IEnumerable<Symbol> GetInputSymbols(string input)
    {
        var symbols = new List<Symbol>();
        var length = input.Length;
        foreach (var c in input)
        {
            var symbol = symbols.Find(s => s.value == c);
            if (symbol == null)
            {
                symbols.Add(new Symbol() { value = c, count = 1 });
            }
            else
            {
                symbol.count++;
            }
        }

        foreach (var symbol in symbols)
        {
            symbol.frequency = (double)symbol.count / length;
        }

        return symbols;
    }
    private static IDictionary<Node, Symbol> GetInitialNodeTable(IEnumerable<Symbol> symbols)
    {
        var nodes = new Dictionary<Node, Symbol>();
        foreach (var symbol in from s in symbols orderby s.frequency descending select s)
        {
            nodes.Add(new Node() { frequency = symbol.frequency }, symbol);
        }
        return nodes;
    }
    private static Node BuildRecursiveHuffmanTree(IEnumerable<Node> prevLayer)
    {
        if (prevLayer.Count() == 1) return prevLayer.First();
        else
        {
            var newLayer = new List<Node>();

            var right = prevLayer.ToList()[^1];
            var left = prevLayer.ToList()[^2];
            var newNode = new Node() { left = left, right = right, frequency = left.frequency + right.frequency };

            foreach (var node in prevLayer)
            {
                if (node != left && node != right)
                {
                    newLayer.Add(node);
                }
            }

            newLayer.Add(newNode);

            return BuildRecursiveHuffmanTree(from node in newLayer orderby node.frequency descending select node);
        }
    }
    private static void BuildRecursiveEncodingNodeLayer(Node root, ref List<Node> layer)
    {
        if (root.IsLeaf)
        {
            layer.Add(root);
            return;
        }
        else
        {
            root.left.code = root.code + "0";
            root.right.code = root.code + "1";
            BuildRecursiveEncodingNodeLayer(root.left, ref layer);
            BuildRecursiveEncodingNodeLayer(root.right, ref layer);
        }
    }
    private static IDictionary<char, string> BuildEncodingTable(Node root, IDictionary<Node, Symbol> nsTable)
    {
        var encodingTable = new Dictionary<char, string>();

        var encodingLayer = new List<Node>();
        BuildRecursiveEncodingNodeLayer(root, ref encodingLayer);

        foreach (var node in encodingLayer)
        {
            encodingTable.Add(nsTable[node].value, node.code);
        }

        return encodingTable;
    }
    private static string EncodeString(string input, IDictionary<char, string> encodingTable)
    {
        var output = string.Empty;

        foreach (var c in input)
        {
            output += encodingTable[c];
        }

        return output;
    }
}