using System;
using System.IO;
using System.Linq;
using TodoApp.Commands;
using TodoApp.Exceptions;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp
{
    class Program
    {
        private static IDataStorage _storage = null!;

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Clear();

            _storage = new FileManager("data");
            AppInfo.Storage = _storage;
            AppInfo.TodoListBinder = SubscribeToTodoEvents;

            var profiles = _storage.LoadProfiles().ToList();
            if (profiles.Count == 0)
            {
                var legacyProfiles = LegacyFileManager.LoadAllProfiles();
                if (legacyProfiles.Count > 0)
                {
                    _storage.SaveProfiles(legacyProfiles);
                    foreach (var profile in legacyProfiles)
                    {
                        string legacyTodoPath = LegacyFileManager.GetTodoFilePath(profile.Id);
                        if (File.Exists(legacyTodoPath))
                        {
                            _storage.SaveTodos(profile.Id, LegacyFileManager.LoadTodos(legacyTodoPath).GetAll());
                        }
                    }

                    profiles = _storage.LoadProfiles().ToList();
                }
            }

            AppInfo.Profiles = profiles;

            MainLoop();
        }

        private static bool SelectOrCreateProfile()
        {
            while (true)
            {
                Console.WriteLine("Войти в существующий профиль? [y/n]");
                Console.Write("> ");

                string choice = Console.ReadLine()?.ToLower() ?? "n";

                if (choice == "y")
                {
                    return LoginProfile();
                }

                if (choice == "n")
                {
                    return CreateProfile();
                }

                Console.WriteLine("Пожалуйста, введите 'y' или 'n'");
            }
        }

        private static bool LoginProfile()
        {
            if (AppInfo.Profiles.Count == 0)
            {
                Console.WriteLine("Нет сохранённых профилей. Пожалуйста, создайте новый.");
                return false;
            }

            Console.Write("Логин: ");
            string login = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new InvalidArgumentException("Логин не может быть пустым.");
            }

            Console.Write("Пароль: ");
            string password = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidArgumentException("Пароль не может быть пустым.");
            }

            var profile = AppInfo.Profiles.FirstOrDefault(p => p.Login == login && p.Password == password);
            if (profile == null)
            {
                throw new ProfileNotFoundException("Профиль с такими данными не найден.");
            }

            AppInfo.CurrentProfile = profile;
            AppInfo.SetCurrentTodoList(profile.Id, _storage.LoadTodos(profile.Id));
            AppInfo.ClearUndoRedo();
            return true;
        }

        private static bool CreateProfile()
        {
            Console.Write("Логин: ");
            string login = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new InvalidArgumentException("Логин не может быть пустым.");
            }

            if (AppInfo.Profiles.Any(p => p.Login == login))
            {
                throw new DuplicateLoginException("Этот логин уже занят.");
            }

            Console.Write("Пароль: ");
            string password = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidArgumentException("Пароль не может быть пустым.");
            }

            Console.Write("Имя: ");
            string firstName = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new InvalidArgumentException("Имя не может быть пустым.");
            }

            Console.Write("Фамилия: ");
            string lastName = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new InvalidArgumentException("Фамилия не может быть пустой.");
            }

            Console.Write("Год рождения: ");
            if (!int.TryParse(Console.ReadLine(), out int birthYear))
            {
                throw new InvalidArgumentException("Неверный формат года рождения.");
            }

            if (birthYear < 1900 || birthYear > DateTime.Now.Year)
            {
                throw new InvalidArgumentException("Год рождения вне допустимого диапазона.");
            }

            var profile = new Profile(login, password, firstName, lastName, birthYear);
            AppInfo.Profiles.Add(profile);
            _storage.SaveProfiles(AppInfo.Profiles);

            AppInfo.CurrentProfile = profile;
            AppInfo.SetCurrentTodoList(profile.Id, Array.Empty<TodoItem>());
            _storage.SaveTodos(profile.Id, AppInfo.UserTodos[profile.Id].GetAll());
            AppInfo.ClearUndoRedo();
            return true;
        }

        private static void SubscribeToTodoEvents(Guid profileId, TodoList todoList)
        {
            todoList.OnTodoAdded += _ => SaveCurrentTodos(profileId, todoList);
            todoList.OnTodoDeleted += _ => SaveCurrentTodos(profileId, todoList);
            todoList.OnTodoUpdated += _ => SaveCurrentTodos(profileId, todoList);
            todoList.OnStatusChanged += _ => SaveCurrentTodos(profileId, todoList);
        }

        private static void SaveCurrentTodos(Guid profileId, TodoList todoList)
        {
            _storage.SaveTodos(profileId, todoList.GetAll());
        }

        private static void MainLoop()
        {
            while (true)
            {
                if (AppInfo.CurrentProfile is null)
                {
                    try
                    {
                        if (!SelectOrCreateProfile())
                        {
                            return;
                        }
                    }
                    catch (AuthenticationException ex)
                    {
                        Console.WriteLine($"Ошибка авторизации: {ex.Message}");
                        continue;
                    }
                    catch (InvalidArgumentException ex)
                    {
                        Console.WriteLine($"Ошибка аргумента: {ex.Message}");
                        continue;
                    }
                    catch (ProfileNotFoundException ex)
                    {
                        Console.WriteLine($"Ошибка профиля: {ex.Message}");
                        continue;
                    }
                    catch (DuplicateLoginException ex)
                    {
                        Console.WriteLine($"Ошибка профиля: {ex.Message}");
                        continue;
                    }
                    catch (DataStorageException ex)
                    {
                        Console.WriteLine($"Ошибка хранилища: {ex.Message}");
                        continue;
                    }
                    catch (DataDecryptionException ex)
                    {
                        Console.WriteLine($"Ошибка расшифровки: {ex.Message}");
                        continue;
                    }
                    catch (CorruptedDataException ex)
                    {
                        Console.WriteLine($"Ошибка данных: {ex.Message}");
                        continue;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Неожиданная ошибка.");
                        continue;
                    }

                    Console.WriteLine($"\nДобро пожаловать, {AppInfo.CurrentProfile?.FirstName}!\n");
                }

                Console.Write("> ");
                string input = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("До свидания!");
                    break;
                }

                try
                {
                    ICommand command = CommandParser.Parse(input);
                    command.Execute();

                    if (command is IUndoableCommand undoableCmd)
                    {
                        AppInfo.UndoStack.Push(undoableCmd);
                        AppInfo.RedoStack.Clear();
                    }
                }
                catch (TaskNotFoundException ex)
                {
                    Console.WriteLine($"Ошибка задачи: {ex.Message}");
                }
                catch (AuthenticationException ex)
                {
                    Console.WriteLine($"Ошибка авторизации: {ex.Message}");
                }
                catch (InvalidCommandException ex)
                {
                    Console.WriteLine($"Ошибка команды: {ex.Message}");
                }
                catch (InvalidArgumentException ex)
                {
                    Console.WriteLine($"Ошибка аргумента: {ex.Message}");
                }
                catch (ProfileNotFoundException ex)
                {
                    Console.WriteLine($"Ошибка профиля: {ex.Message}");
                }
                catch (DuplicateLoginException ex)
                {
                    Console.WriteLine($"Ошибка профиля: {ex.Message}");
                }
                catch (DataStorageException ex)
                {
                    Console.WriteLine($"Ошибка хранилища: {ex.Message}");
                }
                catch (DataDecryptionException ex)
                {
                    Console.WriteLine($"Ошибка расшифровки: {ex.Message}");
                }
                catch (CorruptedDataException ex)
                {
                    Console.WriteLine($"Ошибка данных: {ex.Message}");
                }
                catch (Exception)
                {
                    Console.WriteLine("Неожиданная ошибка.");
                }
            }
        }
    }
}
