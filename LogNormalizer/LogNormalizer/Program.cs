using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

class LogNormalizer
{
    static void Main()
    {

        string inputFile = "input.txt";
        string outputFile = "output.txt";
        string problemFile = "problems.txt";

        var outputLines = new List<string>();
        var problemLines = new List<string>();

        var lines = File.ReadAllLines(inputFile);

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            try
            {
                string[] result = ParseLogLine(trimmed);
                if (result != null)
                {
                    // Формируем строку с табуляцией
                    string formatted = string.Join("\t", result);
                    outputLines.Add(formatted);
                }
                else
                {
                    problemLines.Add(line);
                }
            }
            catch
            {
                problemLines.Add(line);
            }
        }

        File.WriteAllLines(outputFile, outputLines);
        File.WriteAllLines(problemFile, problemLines);

        Console.WriteLine("Обработка завершена.");
        Console.WriteLine($"Успешных записей: {outputLines.Count}");
        Console.WriteLine($"Невалидных записей: {problemLines.Count}");
    }

    static string[] ParseLogLine(string line)
    {
        // Попытка — Формат 1: 10.03.2025 15:14:49.523 INFORMATION ...
        var match1 = Regex.Match(line, @"^(\d{2}\.\d{2}\.\d{4}) (\d{2}:\d{2}:\d{2}\.\d+)\s+(INFORMATION|WARNING|ERROR|DEBUG)\s+(.+)$");
        if (match1.Success)
        {
            string date = ConvertDate(match1.Groups[1].Value); // в формат YYYY-MM-DD
            string time = match1.Groups[2].Value;
            string level = NormalizeLevel(match1.Groups[3].Value);
            string message = match1.Groups[4].Value;

            return new string[] { date, time, level, "DEFAULT", message };
        }

        // Попытка — Формат 2: 2025-03-10 15:14:51.5882| INFO|11|MobileComputer.GetDeviceId| ...
        var match2 = Regex.Match(line, @"^(\d{4}-\d{2}-\d{2}) (\d{2}:\d{2}:\d{2}\.\d+)\|\s*(INFO|WARNING|ERROR|DEBUG|INFORMATION)\|[^|]*\|([^\|]*)\|\s*(.+)$");
        if (match2.Success)
        {
            string date = ConvertDate(match2.Groups[1].Value);
            string time = match2.Groups[2].Value;
            string level = NormalizeLevel(match2.Groups[3].Value);
            string method = match2.Groups[4].Value.Trim();
            string message = match2.Groups[5].Value.Trim();

            return new string[] { date, time, level, method, message };
        }

        // Не соответствует ни одному формату
        return null;
    }

    static string ConvertDate(string dateStr)
    {
        // Поддержка двух форматов: DD.MM.YYYY и YYYY-MM-DD
        DateTime dt;
        if (DateTime.TryParseExact(dateStr, "dd.MM.yyyy", null, DateTimeStyles.None, out dt))
        {
            return dt.ToString("yyyy-MM-dd");
        }
        else if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", null, DateTimeStyles.None, out dt))
        {
            return dt.ToString("yyyy-MM-dd");
        }
        else
        {
            throw new FormatException("Неподдерживаемый формат даты");
        }
    }

    static string NormalizeLevel(string level)
    {
        level = level.ToUpperInvariant();
        return level switch
        {
            "INFORMATION" => "INFO",
            "INFO" => "INFO",
            "WARNING" => "WARN",
            "WARN" => "WARN",
            "ERROR" => "ERROR",
            "DEBUG" => "DEBUG",
            _ => "UNKNOWN"
        };
    }
}
