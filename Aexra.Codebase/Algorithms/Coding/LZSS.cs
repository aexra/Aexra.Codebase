namespace Aexra.Codebase.Algorithms.Coding;
public static class LZSS
{
    private class Window
    {
        public int Size { get; private set; }
        public List<char> Container { get; private set; }

        public Window(int size)
        {
            Size = size;
            Container = new List<char>();
        }

        public Window Skip(int count)
        {
            for (var _ = 0; _ < count; _++)
            {
                Container.RemoveAt(0);
            }
            return this;
        }
        public Window Move(string chars)
        {
            foreach (var c in chars)
            {
                Move(c);
            }
            return this;
        }
        public Window Move(char? c)
        {
            if (c != null) Container.Add(c.Value);
            if (Container.Count > Size) Container.RemoveAt(0);
            return this;
        }
    }

    // Метод для кодирования строки
    public static (List<(bool coded, int start, int length, char symbol)> data, int ws, int bs) Encode(string input, int ws, int bs, Action<string> log)
    {
        var encodingData = new List<(bool coded, int start, int length, char symbol)>();
        var sourceQueue = new Queue<char>(input);

        var win = new Window(ws);
        var buf = new Window(bs);

        // INITIAL FILL
        while (buf.Container.Count < buf.Size)
        {
            var isNext = sourceQueue.TryDequeue(out var next);
            if (isNext) buf.Move(next);
            else break;
        }

        // MAIN LOOP
        while (buf.Container.Count > 0)
        {
            // Получим символ из буфера, с которым мы будем работать
            var subwin = buf.Container.First().ToString();

            // Поверим, есть ли он в окне
            var start = win.Container.IndexOf(subwin[0]);

            // Такой символ найден
            if (start >= 0)
            {
                // Попытаемся найти следующие символы в исходной строке
                while (win.Container.Count > start + subwin.Length && buf.Container.Count > subwin.Length && win.Container[start + subwin.Length] == buf.Container[subwin.Length])
                {
                    subwin += buf.Container[subwin.Length];
                }

                // Добавим код закодированного символа
                encodingData.Add((true, win.Container.Count == win.Size ? start : win.Size - win.Container.Count + start, subwin.Length, '0'));
            } 
            else
            {
                // Такой символ не найден
                // Добавим код не закодированного символа
                encodingData.Add((false, 0, -1, subwin[0]));
            }

            log?.Invoke($"{string.Join(",", win.Container)} | {string.Join(",", buf.Container)} -> {(encodingData.Last().coded ? $"(1<{encodingData.Last().start},{encodingData.Last().length}>)" : $"(0<{encodingData.Last().symbol}>)")}");
            
            foreach (var c in subwin)
            {
                win.Move(c);

                var isNext = sourceQueue.TryDequeue(out var next);
                if (isNext) buf.Move(next);
                else buf.Skip(1);
            }
        }

        return (encodingData, ws, bs);
    }

    // Метод для декодирования строки
    public static string Decode(List<(bool coded, int start, int length, char symbol)> encodingData, int ws, int bs, Action<string> log)
    {
        var decoded = "";
        var win = new Window(ws);

        foreach (var g in encodingData)
        {
            log?.Invoke($"{g.coded},{g.start},{g.length},{g.symbol}");
        }

        foreach (var g in encodingData)
        {
            if (!g.coded)
            {
                decoded += g.symbol;
                win.Move(g.symbol);

                log?.Invoke(g.symbol.ToString());
            }
            else
            {
                var start = g.start - (win.Size - win.Container.Count);

                var print = "";

                for (var i = start; i < start + g.length; i++)
                {
                    print += win.Container[i];
                }

                decoded += print;
                win.Move(print);

                log?.Invoke($"{g.coded},{g.start},{g.length},{g.symbol} -> {print}");
            }
        }

        return decoded;
    }
}
