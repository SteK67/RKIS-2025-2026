using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TodoApp.Commands;
using TodoApp.Exceptions;
using TodoApp.Models;

namespace TodoApp.Services
{
    public static class CommandParser
    {
        private static Dictionary<string, Func<string, ICommand>> _commandHandlers;
        public static TodoList Todos => AppInfo.GetCurrentTodoList();

        static CommandParser()
        {
            InitializeHandlers();
        }

        private static void InitializeHandlers()
        {
            _commandHandlers = new Dictionary<string, Func<string, ICommand>>
            {
                ["help"] = _ => new HelpCommand(),
                ["profile"] = args => ParseProfileCommandSafe(SplitCommand(args)),
                ["add"] = args => ParseAddCommandSafe(SplitCommand(args)),
                ["view"] = args => ParseViewCommandSafe(SplitCommand(args)),
                ["read"] = args => ParseReadCommandSafe(SplitCommand(args)),
                ["status"] = args => ParseStatusCommandSafe(SplitCommand(args)),
                ["update"] = args => ParseUpdateCommandSafe(SplitCommand(args)),
                ["delete"] = args => ParseDeleteCommandSafe(SplitCommand(args)),
                ["search"] = args => ParseSearchCommandSafe(args),
                ["load"] = args => ParseLoadCommandSafe(SplitCommand(args)),
                ["sync"] = args => ParseSyncCommandSafe(SplitCommand(args)),
                ["undo"] = _ => new UndoCommand(),
                ["redo"] = _ => new RedoCommand(),
            };
        }

        public static ICommand Parse(string inputString)
        {
            if (string.IsNullOrWhiteSpace(inputString))
            {
                return new HelpCommand();
            }

            var parts = SplitCommand(inputString);
            if (parts.Length == 0)
                return new HelpCommand();

            string command = parts[0].ToLower();
            string args = inputString.Length > command.Length
                ? inputString.Substring(command.Length).TrimStart()
                : string.Empty;

            if (_commandHandlers.ContainsKey(command))
            {
                try
                {
                    return _commandHandlers[command](args);
                }
                catch
                {
                    Console.WriteLine($"Ошибка при выполнении команды '{command}'");
                    throw;
                }
            }

            Console.WriteLine($"Неизвестная команда: '{command}'. Введите 'help' для справки.");
            throw new InvalidCommandException($"Неизвестная команда: '{command}'.");
        }

        private static ICommand ParseProfileCommand(string[] args)
        {
            bool logout = args.Any(a => a == "-o" || a == "--out");
            return new ProfileCommand(logout);
        }

        private static ICommand ParseAddCommand(string[] args)
        {
            bool isMultiline = args.Any(a => a == "-m" || a == "--multiline");

            if (isMultiline)
            {
                return new AddCommand("", true);
            }

            string text = string.Join(" ", args);
            text = text.Trim('"');

            return new AddCommand(text, false);
        }

        private static ICommand ParseViewCommand(string[] args)
        {
            bool showIndex = args.Any(a => a == "-i" || a == "--index");
            bool showStatus = args.Any(a => a == "-s" || a == "--status");
            bool showDate = args.Any(a => a == "-d" || a == "--update-date");
            bool showAll = args.Any(a => a == "-a" || a == "--all");

            if (showAll)
                return new ViewCommand(true, true, true);

            return new ViewCommand(showIndex, showStatus, showDate);
        }

        private static ICommand ParseReadCommand(string[] args)
        {
            if (args.Length > 0 && int.TryParse(args[0], out int index))
            {
                return new ReadCommand(index);
            }

            Console.WriteLine("Используйте: read <индекс>");
            return new HelpCommand();
        }

        private static ICommand ParseStatusCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Используйте: status <индекс> <статус>");
                return new HelpCommand();
            }

            if (!int.TryParse(args[0], out int index))
            {
                Console.WriteLine("Индекс должен быть числом.");
                return new HelpCommand();
            }

            if (TryParseStatus(args[1], out var status))
            {
                return new StatusCommand(index, status);
            }

            Console.WriteLine("Неизвестный статус. Доступные: NotStarted, InProgress, Completed, Postponed, Failed");
            return new HelpCommand();
        }

        private static ICommand ParseUpdateCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Используйте: update <индекс> \"новый текст\"");
                return new HelpCommand();
            }

            if (!int.TryParse(args[0], out int index))
            {
                Console.WriteLine("Индекс должен быть числом.");
                return new HelpCommand();
            }

            string newText = string.Join(" ", args.Skip(1)).Trim('"');
            return new UpdateCommand(index, newText);
        }

        private static ICommand ParseDeleteCommand(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int index))
            {
                Console.WriteLine("Используйте: delete <индекс>");
                return new HelpCommand();
            }

            return new DeleteCommand(index);
        }

        private static ICommand ParseSearchCommand(string rawArgs)
        {
            return SearchCommand.TryCreate(rawArgs, out var command)
                ? command
                : new HelpCommand();
        }

        public static bool TryParseStatus(string statusValue, out TodoStatus status)
        {
            string normalized = statusValue
                .Replace("-", "", StringComparison.Ordinal)
                .Replace("_", "", StringComparison.Ordinal)
                .Trim();

            return Enum.TryParse(normalized, ignoreCase: true, out status);
        }

        private static ICommand ParseProfileCommandSafe(string[] args) => ParseProfileCommand(args);

        private static ICommand ParseAddCommandSafe(string[] args)
        {
            bool isMultiline = args.Any(a => a == "-m" || a == "--multiline");
            if (isMultiline)
            {
                return new AddCommand("", true);
            }

            string text = string.Join(" ", args).Trim('"');
            return new AddCommand(text, false);
        }

        private static ICommand ParseViewCommandSafe(string[] args) => ParseViewCommand(args);

        private static ICommand ParseReadCommandSafe(string[] args)
        {
            if (args.Length == 0)
                throw new InvalidArgumentException("Используйте: read <индекс>");
            if (!int.TryParse(args[0], out int index) || index < 0)
                throw new InvalidArgumentException("Индекс должен быть неотрицательным числом.");
            return new ReadCommand(index);
        }

        private static ICommand ParseStatusCommandSafe(string[] args)
        {
            if (args.Length < 2)
                throw new InvalidArgumentException("Используйте: status <индекс> <статус>");
            if (!int.TryParse(args[0], out int index) || index < 0)
                throw new InvalidArgumentException("Индекс должен быть неотрицательным числом.");
            if (TryParseStatus(args[1], out var status))
                return new StatusCommand(index, status);

            throw new InvalidArgumentException("Неизвестный статус. Доступные: NotStarted, InProgress, Completed, Postponed, Failed");
        }

        private static ICommand ParseUpdateCommandSafe(string[] args)
        {
            if (args.Length < 2)
                throw new InvalidArgumentException("Используйте: update <индекс> \"новый текст\"");
            if (!int.TryParse(args[0], out int index) || index < 0)
                throw new InvalidArgumentException("Индекс должен быть неотрицательным числом.");

            string newText = string.Join(" ", args.Skip(1)).Trim('"');
            if (string.IsNullOrWhiteSpace(newText))
                throw new InvalidArgumentException("Новый текст задачи не может быть пустым.");

            return new UpdateCommand(index, newText);
        }

        private static ICommand ParseDeleteCommandSafe(string[] args)
        {
            if (args.Length == 0)
                throw new InvalidArgumentException("Используйте: delete <индекс>");
            if (!int.TryParse(args[0], out int index) || index < 0)
                throw new InvalidArgumentException("Индекс должен быть неотрицательным числом.");
            return new DeleteCommand(index);
        }

        private static ICommand ParseSearchCommandSafe(string rawArgs)
        {
            return SearchCommand.CreateOrThrow(rawArgs);
        }

        private static ICommand ParseLoadCommandSafe(string[] args)
        {
            return LoadCommand.Create(args);
        }

        private static ICommand ParseSyncCommandSafe(string[] args)
        {
            return SyncCommand.Create(args);
        }

        private static string[] SplitCommand(string input)
        {
            var result = new List<string>();
            var regex = new Regex(@"[^\s""]+|""([^""]*)""");
            var matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                if (match.Groups[1].Success)
                {
                    result.Add(match.Groups[1].Value);
                }
                else
                {
                    result.Add(match.Value);
                }
            }

            return result.ToArray();
        }
    }
}
