namespace Aexra.Codebase.Algorithms.Coding;
public static class LZSS
{
    private class Window
    {
        public int Size { get; private set; }
        public string Container { get; private set; }

        public Window(int size)
        {
            Size = size;
            Container = string.Empty;
        }

        public Window Skip(int count)
        {
            for (var _ = 0; _ < count; _++)
            {
                Container = Container[1..];
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
            if (c != null) Container += c.Value;
            if (Container.Length > Size) Container = Container[1..];
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
        while (buf.Container.Length < buf.Size)
        {
            var isNext = sourceQueue.TryDequeue(out var next);
            if (isNext) buf.Move(next);
            else break;
        }

        // MAIN LOOP
        while (buf.Container.Length > 0)
        {
            var (start, size) = FindLongestMatch(win.Container, buf.Container);

            if (start == -1)
            {
                // Подстрока буфера не найдена в окне
                // Добавим код не закодированного символа
                encodingData.Add((false, 0, -1, buf.Container[0]));
            }
            else
            {
                // Подстрока буфера найдена в окне
                // Добавим код закодированного символа
                encodingData.Add((true, win.Container.Length == win.Size ? start : win.Size - win.Container.Length + start, size, '0'));
            }

            log?.Invoke($"{string.Join(",", win.Container)} | {string.Join(",", buf.Container)} -> {(encodingData.Last().coded ? $"(1<{encodingData.Last().start},{encodingData.Last().length}>)" : $"(0<{encodingData.Last().symbol}>)")}");
            
            if (start == -1)
            {
                win.Move(buf.Container[0]);

                var isNext = sourceQueue.TryDequeue(out var next);
                if (isNext) buf.Move(next);
                else buf.Skip(1);

                continue;
            }

            for (var i = 0; i < size; i++)
            {
                win.Move(buf.Container[0]);

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
                var start = g.start - (win.Size - win.Container.Length);

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

    private static (int, int) FindLongestMatch(string win, string buf)
    {
        var maxLength = 0;   // Длина наибольшего вхождения
        var startIndex = -1; // Начальный индекс в win

        for (var i = 0; i <= win.Length - buf.Length; i++)
        {
            var length = 0;

            // Проверяем совпадение символов начиная с позиции i
            for (var j = 0; j < buf.Length && (i + j) < win.Length; j++)
            {
                if (win[i + j] == buf[j])
                {
                    length++;
                }
                else
                {
                    break; // Прекращаем, если символы не совпадают
                }
            }

            // Если нашли более длинное совпадение
            if (length > maxLength)
            {
                maxLength = length;
                startIndex = i;
            }
        }

        return (startIndex, maxLength);
    }
}
