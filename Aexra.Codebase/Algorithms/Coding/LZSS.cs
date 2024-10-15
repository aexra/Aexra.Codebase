using System.Text;

namespace Aexra.Codebase.Algorithms.Coding;
public static class LZSSAlgorithm
{
    private const int WindowSize = 2048; // Размер окна поиска
    private const int LookaheadBufferSize = 32; // Размер буфера предварительного просмотра

    // Метод для кодирования строки
    public static (string encodedString, List<(int, int, char)> encodingData) Encode(string input)
    {
        var encodingData = new List<(int, int, char)>();
        StringBuilder encodedString = new StringBuilder();

        int cursor = 0;

        while (cursor < input.Length)
        {
            int matchLength = 0;
            int matchDistance = 0;
            char nextChar = input[cursor];

            // Поиск самой длинной строки, которая совпадает с ранее встречавшейся
            int searchStart = Math.Max(0, cursor - WindowSize);
            for (int searchPos = searchStart; searchPos < cursor; searchPos++)
            {
                int length = 0;
                while (length < LookaheadBufferSize && cursor + length < input.Length &&
                       input[searchPos + length] == input[cursor + length])
                {
                    length++;
                }

                if (length > matchLength)
                {
                    matchLength = length;
                    matchDistance = cursor - searchPos;
                }
            }

            if (matchLength > 0)
            {
                nextChar = input[cursor + matchLength];
            }

            encodingData.Add((matchDistance, matchLength, nextChar));
            encodedString.Append($"({matchDistance},{matchLength},{nextChar})");

            cursor += matchLength + 1;
        }

        return (encodedString.ToString(), encodingData);
    }

    // Метод для декодирования строки
    public static string Decode(List<(int distance, int length, char nextChar)> encodingData)
    {
        StringBuilder decodedString = new StringBuilder();

        foreach (var (distance, length, nextChar) in encodingData)
        {
            if (distance > 0)
            {
                int start = decodedString.Length - distance;
                for (int i = 0; i < length; i++)
                {
                    decodedString.Append(decodedString[start + i]);
                }
            }
            decodedString.Append(nextChar);
        }

        return decodedString.ToString();
    }
}
