using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TodoApp.Exceptions;

namespace TodoApp.Commands
{
    public class LoadCommand : ICommand
    {
        private static readonly object _consoleLock = new object();
        private readonly int _downloadsCount;
        private readonly int _downloadSize;

        public LoadCommand(int downloadsCount, int downloadSize)
        {
            _downloadsCount = downloadsCount;
            _downloadSize = downloadSize;
        }

        public void Execute()
        {
            RunAsync().GetAwaiter().GetResult();
        }

        private async Task RunAsync()
        {
            int startRow;
            int messageRow;

            lock (_consoleLock)
            {
                startRow = GetSafeCursorTop();
                for (int i = 0; i < _downloadsCount + 1; i++)
                {
                    Console.WriteLine();
                }
                messageRow = startRow + _downloadsCount;
            }

            var tasks = new List<Task>();

            for (int i = 0; i < _downloadsCount; i++)
            {
                tasks.Add(DownloadAsync(i, startRow + i));
            }

            await Task.WhenAll(tasks);

            lock (_consoleLock)
            {
                SetSafeCursorPosition(0, messageRow);
                ClearCurrentLine();
                WriteSafeLine("Все загрузки завершены.");
                Console.WriteLine();
            }
        }

        private async Task DownloadAsync(int index, int row)
        {
            var random = new Random(Guid.NewGuid().GetHashCode());

            for (int current = 0; current <= _downloadSize; current++)
            {
                int percent = current * 100 / _downloadSize;
                string bar = BuildBar(percent);

                lock (_consoleLock)
                {
                    SetSafeCursorPosition(0, row);
                    ClearCurrentLine();
                    WriteSafeLine($"Загрузка {index + 1}: {bar}");
                }

                await Task.Delay(random.Next(30, 120));
            }
        }

        private static string BuildBar(int percent)
        {
            const int width = 20;
            int filled = percent * width / 100;
            return $"[{new string('#', filled)}{new string('-', width - filled)}] {percent}%";
        }

        public static ICommand Create(string[] args)
        {
            if (args.Length < 2)
                throw new InvalidArgumentException("Используйте: load <количество_скачиваний> <размер_скачиваний>");
            if (!int.TryParse(args[0], out int downloadsCount) || !int.TryParse(args[1], out int downloadSize))
                throw new InvalidArgumentException("Аргументы load должны быть числами.");
            if (downloadsCount <= 0 || downloadSize <= 0)
                throw new InvalidArgumentException("Аргументы load должны быть больше 0.");

            return new LoadCommand(downloadsCount, downloadSize);
        }

        private static int GetSafeCursorTop()
        {
            try
            {
                return Console.CursorTop;
            }
            catch
            {
                return 0;
            }
        }

        private static void SetSafeCursorPosition(int left, int top)
        {
            try
            {
                int safeLeft = Math.Min(Math.Max(left, 0), Math.Max(0, Console.BufferWidth - 1));
                int safeTop = Math.Min(Math.Max(top, 0), Math.Max(0, Console.BufferHeight - 1));
                Console.SetCursorPosition(safeLeft, safeTop);
            }
            catch
            {
                Console.WriteLine();
            }
        }

        private static void WriteSafeLine(string text)
        {
            int width = GetSafeWidth();
            if (text.Length >= width)
            {
                Console.Write(text[..Math.Max(1, width - 1)]);
                return;
            }

            Console.Write(text.PadRight(width - 1));
        }

        private static void ClearCurrentLine()
        {
            int width = GetSafeWidth();
            Console.Write(new string(' ', Math.Max(1, width - 1)));
            try
            {
                Console.CursorLeft = 0;
            }
            catch
            {
            }
        }

        private static int GetSafeWidth()
        {
            try
            {
                return Math.Max(20, Console.BufferWidth);
            }
            catch
            {
                return 120;
            }
        }
    }
}
