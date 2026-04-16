using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TodoApp.Exceptions;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Commands
{
    public class SearchCommand : ICommand
    {
        private readonly string? _contains;
        private readonly string? _startsWith;
        private readonly string? _endsWith;
        private readonly DateTime? _from;
        private readonly DateTime? _to;
        private readonly TodoStatus? _status;
        private readonly string? _sortBy;
        private readonly bool _desc;
        private readonly int? _top;

        private SearchCommand(
            string? contains,
            string? startsWith,
            string? endsWith,
            DateTime? from,
            DateTime? to,
            TodoStatus? status,
            string? sortBy,
            bool desc,
            int? top)
        {
            _contains = contains;
            _startsWith = startsWith;
            _endsWith = endsWith;
            _from = from;
            _to = to;
            _status = status;
            _sortBy = sortBy;
            _desc = desc;
            _top = top;
        }

        public static bool TryCreate(string rawArgs, out ICommand command)
        {
            command = new HelpCommand();
            var args = SplitArguments(rawArgs);

            string? contains = null;
            string? startsWith = null;
            string? endsWith = null;
            DateTime? from = null;
            DateTime? to = null;
            TodoStatus? status = null;
            string? sortBy = null;
            bool desc = false;
            int? top = null;

            for (int i = 0; i < args.Count; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "--contains":
                        if (!TryReadValue(args, ref i, out contains))
                        {
                            Console.WriteLine("Используйте: search --contains <text>");
                            return false;
                        }
                        break;
                    case "--starts-with":
                        if (!TryReadValue(args, ref i, out startsWith))
                        {
                            Console.WriteLine("Используйте: search --starts-with <text>");
                            return false;
                        }
                        break;
                    case "--ends-with":
                        if (!TryReadValue(args, ref i, out endsWith))
                        {
                            Console.WriteLine("Используйте: search --ends-with <text>");
                            return false;
                        }
                        break;
                    case "--from":
                        if (!TryReadDate(args, ref i, out from))
                        {
                            Console.WriteLine("Некорректная дата. Используйте формат yyyy-MM-dd");
                            return false;
                        }
                        break;
                    case "--to":
                        if (!TryReadDate(args, ref i, out to))
                        {
                            Console.WriteLine("Некорректная дата. Используйте формат yyyy-MM-dd");
                            return false;
                        }
                        break;
                    case "--status":
                        if (!TryReadValue(args, ref i, out var statusValue))
                        {
                            Console.WriteLine("Используйте: search --status <status>");
                            return false;
                        }

                        if (!CommandParser.TryParseStatus(statusValue, out var parsedStatus))
                        {
                            Console.WriteLine("Некорректный статус");
                            return false;
                        }

                        status = parsedStatus;
                        break;
                    case "--sort":
                        if (!TryReadValue(args, ref i, out sortBy))
                        {
                            Console.WriteLine("Используйте: search --sort text|date");
                            return false;
                        }

                        sortBy = sortBy.ToLowerInvariant();
                        if (sortBy != "text" && sortBy != "date")
                        {
                            Console.WriteLine("Сортировка должна быть text или date");
                            return false;
                        }
                        break;
                    case "--desc":
                        desc = true;
                        break;
                    case "--top":
                        if (!TryReadValue(args, ref i, out var topValue) || !int.TryParse(topValue, out var parsedTop) || parsedTop <= 0)
                        {
                            Console.WriteLine("Параметр top должен быть положительным числом");
                            return false;
                        }

                        top = parsedTop;
                        break;
                    default:
                        Console.WriteLine($"Неизвестный параметр: {args[i]}");
                        return false;
                }
            }

            command = new SearchCommand(contains, startsWith, endsWith, from, to, status, sortBy, desc, top);
            return true;
        }

        public static ICommand CreateOrThrow(string rawArgs)
        {
            var args = SplitArguments(rawArgs);

            string? contains = null;
            string? startsWith = null;
            string? endsWith = null;
            DateTime? from = null;
            DateTime? to = null;
            TodoStatus? status = null;
            string? sortBy = null;
            bool desc = false;
            int? top = null;

            for (int i = 0; i < args.Count; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "--contains":
                        if (!TryReadValue(args, ref i, out contains))
                            throw new InvalidArgumentException("Используйте: search --contains <text>");
                        break;
                    case "--starts-with":
                        if (!TryReadValue(args, ref i, out startsWith))
                            throw new InvalidArgumentException("Используйте: search --starts-with <text>");
                        break;
                    case "--ends-with":
                        if (!TryReadValue(args, ref i, out endsWith))
                            throw new InvalidArgumentException("Используйте: search --ends-with <text>");
                        break;
                    case "--from":
                        if (!TryReadDate(args, ref i, out from))
                            throw new InvalidArgumentException("Некорректная дата. Используйте формат yyyy-MM-dd");
                        break;
                    case "--to":
                        if (!TryReadDate(args, ref i, out to))
                            throw new InvalidArgumentException("Некорректная дата. Используйте формат yyyy-MM-dd");
                        break;
                    case "--status":
                        if (!TryReadValue(args, ref i, out var statusValue))
                            throw new InvalidArgumentException("Используйте: search --status <status>");
                        if (!CommandParser.TryParseStatus(statusValue, out var parsedStatus))
                            throw new InvalidArgumentException("Некорректный статус");
                        status = parsedStatus;
                        break;
                    case "--sort":
                        if (!TryReadValue(args, ref i, out sortBy))
                            throw new InvalidArgumentException("Используйте: search --sort text|date");
                        sortBy = sortBy.ToLowerInvariant();
                        if (sortBy != "text" && sortBy != "date")
                            throw new InvalidArgumentException("Сортировка должна быть text или date");
                        break;
                    case "--desc":
                        desc = true;
                        break;
                    case "--top":
                        if (!TryReadValue(args, ref i, out var topValue) || !int.TryParse(topValue, out var parsedTop) || parsedTop <= 0)
                            throw new InvalidArgumentException("Параметр top должен быть положительным числом");
                        top = parsedTop;
                        break;
                    default:
                        throw new InvalidCommandException($"Неизвестный параметр: {args[i]}");
                }
            }

            return new SearchCommand(contains, startsWith, endsWith, from, to, status, sortBy, desc, top);
        }

        public void Execute()
        {
            var todos = CommandParser.Todos;
            if (todos == null)
            {
                throw new AuthenticationException("Пользователь не авторизован.");
            }

            if (todos.Count == 0)
            {
                Console.WriteLine("Ничего не найдено");
                return;
            }

            IEnumerable<TodoItem> query = todos.GetAll();

            if (!string.IsNullOrWhiteSpace(_contains))
                query = query.Where(todo => todo.Text.Contains(_contains, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(_startsWith))
                query = query.Where(todo => todo.Text.StartsWith(_startsWith, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(_endsWith))
                query = query.Where(todo => todo.Text.EndsWith(_endsWith, StringComparison.OrdinalIgnoreCase));

            if (_from.HasValue)
                query = query.Where(todo => todo.LastUpdate.Date >= _from.Value.Date);

            if (_to.HasValue)
                query = query.Where(todo => todo.LastUpdate.Date <= _to.Value.Date);

            if (_status.HasValue)
                query = query.Where(todo => todo.Status == _status.Value);

            query = ApplySorting(query);

            if (_top.HasValue)
                query = query.Take(_top.Value);

            var results = query.ToList();
            if (results.Count == 0)
            {
                Console.WriteLine("Ничего не найдено");
                return;
            }

            Console.WriteLine(BuildTable(results));
        }

        private IEnumerable<TodoItem> ApplySorting(IEnumerable<TodoItem> query)
        {
            if (_sortBy == "date")
            {
                return _desc
                    ? query.OrderByDescending(todo => todo.LastUpdate).ThenByDescending(todo => todo.Text)
                    : query.OrderBy(todo => todo.LastUpdate).ThenBy(todo => todo.Text);
            }

            if (_sortBy == "text")
            {
                return _desc
                    ? query.OrderByDescending(todo => todo.Text).ThenByDescending(todo => todo.LastUpdate)
                    : query.OrderBy(todo => todo.Text).ThenBy(todo => todo.LastUpdate);
            }

            return query.OrderBy(todo => todo.Text).ThenBy(todo => todo.LastUpdate);
        }

        private static bool TryReadValue(IReadOnlyList<string> args, ref int index, out string value)
        {
            value = string.Empty;
            if (index + 1 >= args.Count)
                return false;

            index++;
            value = args[index];
            return true;
        }

        private static bool TryReadDate(IReadOnlyList<string> args, ref int index, out DateTime? date)
        {
            date = null;
            if (!TryReadValue(args, ref index, out var value))
                return false;

            if (!DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                return false;

            date = parsedDate;
            return true;
        }

        private static List<string> SplitArguments(string input)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(input))
                return result;

            var current = string.Empty;
            bool inQuotes = false;

            foreach (char c in input)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (!string.IsNullOrEmpty(current))
                    {
                        result.Add(current);
                        current = string.Empty;
                    }

                    continue;
                }

                current += c;
            }

            if (!string.IsNullOrEmpty(current))
                result.Add(current);

            return result;
        }

        private static string BuildTable(IReadOnlyList<TodoItem> items)
        {
            var table = new StringBuilder();
            int indexWidth = 5;
            int textWidth = 35;
            int statusWidth = 15;
            int dateWidth = 20;

            string BuildLine(char left, char mid, char right, char fill)
            {
                var sb = new StringBuilder();
                sb.Append(left);
                sb.Append(new string(fill, indexWidth + 2));
                sb.Append(mid);
                sb.Append(new string(fill, textWidth + 2));
                sb.Append(mid);
                sb.Append(new string(fill, statusWidth + 2));
                sb.Append(mid);
                sb.Append(new string(fill, dateWidth + 2));
                sb.Append(right);
                return sb.ToString();
            }

            table.AppendLine(BuildLine('╔', '╦', '╗', '═'));
            table.AppendLine($"║ {"INDEX".PadRight(indexWidth)} ║ {"TEXT".PadRight(textWidth)} ║ {"STATUS".PadRight(statusWidth)} ║ {"LastUpdate".PadRight(dateWidth)} ║");
            table.AppendLine(BuildLine('╠', '╬', '╣', '═'));

            for (int index = 0; index < items.Count; index++)
            {
                var item = items[index];
                table.AppendLine($"║ {index.ToString().PadRight(indexWidth)} ║ {item.GetShortInfo().PadRight(textWidth)} ║ {item.Status.ToString().PadRight(statusWidth)} ║ {item.LastUpdate.ToString("yyyy-MM-dd HH:mm").PadRight(dateWidth)} ║");

                if (index < items.Count - 1)
                    table.AppendLine(BuildLine('╠', '╬', '╣', '═'));
            }

            table.AppendLine(BuildLine('╚', '╩', '╝', '═'));
            return table.ToString();
        }
    }
}
