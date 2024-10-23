namespace Aexra.Codebase.Algorithms.Coding;
public static class Huffman
{
    // Вспомогательный класс для узла дерева Хаффмана
    private class HuffmanNode
    {
        public char Symbol { get; set; }
        public int Frequency { get; set; }
        public HuffmanNode Left { get; set; }
        public HuffmanNode Right { get; set; }

        public bool IsLeaf => Left == null && Right == null;
    }

    // Метод для кодирования строки
    public static (string encodedString, Dictionary<char, int> frequencyTable) Encode(string input, Action<string> log)
    {
        var frequencyTable = BuildFrequencyTable(input);

        //log?.Invoke(string.Join("\n", frequencyTable.Select(kv => $"{kv.Key} -> {kv.Value}")));

        var root = BuildHuffmanTree(frequencyTable);
        var encodingTable = BuildEncodingTable(root);

        //log?.Invoke(string.Join("\n", encodingTable.Select(kv => $"{kv.Key} -> {kv.Value}")));

        var encodedString = string.Join("", input.Select(c => encodingTable[c]));
        return (encodedString, frequencyTable);
    }

    // Метод для декодирования строки
    public static string Decode(string encodedString, Dictionary<char, int> frequencyTable, Action<string> log)
    {
        var encodingTable = BuildEncodingTable(BuildHuffmanTree(frequencyTable));

        //log?.Invoke(string.Join("\n", frequencyTable.Select(kv => $"{kv.Key} -> {kv.Value}")));

        var decodingTable = encodingTable.ToDictionary(kv => kv.Value, kv => kv.Key);
        var decodedString = "";
        var buffer = "";

        //log?.Invoke(string.Join("\n", encodingTable.Select(kv => $"{kv.Key} -> {kv.Value}")));

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

    // Построение дерева Хаффмана
    private static HuffmanNode BuildHuffmanTree(Dictionary<char, int> frequencyTable)
    {
        var priorityQueue = new SortedSet<HuffmanNode>(Comparer<HuffmanNode>.Create((x, y) =>
        {
            var compare = x.Frequency.CompareTo(y.Frequency);
            return compare == 0 ? x.Symbol.CompareTo(y.Symbol) : compare;
        }));

        foreach (var symbol in frequencyTable)
        {
            priorityQueue.Add(new HuffmanNode { Symbol = symbol.Key, Frequency = symbol.Value });
        }

        while (priorityQueue.Count > 1)
        {
            var left = priorityQueue.First();
            priorityQueue.Remove(left);
            var right = priorityQueue.First();
            priorityQueue.Remove(right);

            var newNode = new HuffmanNode
            {
                Left = left,
                Right = right,
                Frequency = left.Frequency + right.Frequency
            };

            priorityQueue.Add(newNode);
        }

        return priorityQueue.First();
    }

    // Построение таблицы кодирования Хаффмана
    private static Dictionary<char, string> BuildEncodingTable(HuffmanNode root)
    {
        var encodingTable = new Dictionary<char, string>();
        BuildEncodingTableRecursive(root, "", encodingTable);
        return encodingTable;
    }

    // Рекурсивное построение таблицы кодирования
    private static void BuildEncodingTableRecursive(HuffmanNode node, string code, Dictionary<char, string> encodingTable)
    {
        if (node.IsLeaf)
        {
            encodingTable[node.Symbol] = code;
        }
        else
        {
            BuildEncodingTableRecursive(node.Left, code + "0", encodingTable);
            BuildEncodingTableRecursive(node.Right, code + "1", encodingTable);
        }
    }
}