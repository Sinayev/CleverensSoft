using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Потокобезопасный статический сервер.
/// Поддерживает параллельное чтение и эксклюзивную запись.
/// </summary>
public static class Server
{
    private static int _count = 0;

    // Используется для безопасной синхронизации чтений и записей
    private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    /// <summary>
    /// Получить текущее значение счётчика.
    /// Несколько потоков могут читать одновременно.
    /// </summary>
    public static int GetCount()
    {
        _lock.EnterReadLock();
        try
        {
            return _count;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Добавить значение к счётчику.
    /// Запись осуществляется эксклюзивно — читатели и другие писатели ждут.
    /// </summary>
    public static void AddToCount(int value)
    {
        _lock.EnterWriteLock();
        try
        {
            _count += value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Тест многопоточности: ===");

        var tasks = new List<Task>();

        // Параллельные читатели
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                Console.WriteLine($"[Reader] Count = {Server.GetCount()}");
            }));
        }

        // Параллельные писатели
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                Server.AddToCount(10);
                Console.WriteLine("[Writer] Added 10");
            }));
        }

        Task.WaitAll(tasks.ToArray());

        Console.WriteLine($"[Final Count] = {Server.GetCount()}");

        Console.WriteLine("\nНажмите Enter для выхода...");
        Console.ReadLine();
    }
}
