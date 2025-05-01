using System;
using System.Text;

class Program
{
    static void Main()
    {
        Console.Write("Введите строку для сжатия: ");
        string input = Console.ReadLine();

        string compressed = Compress(input);

        Console.WriteLine($"Сжатая строка: {compressed}");
    }

    static string Compress(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";

        StringBuilder result = new StringBuilder();
        int count = 1;
        char prev = input[0];

        for (int i = 1; i < input.Length; i++)
        {
            if (input[i] == prev)
            {
                count++;
            }
            else
            {
                result.Append(prev);
                if (count > 1) result.Append(count);
                prev = input[i];
                count = 1;
            }
        }
        result.Append(prev);
        if (count > 1) result.Append(count);

        return result.ToString();
    }
}
