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
		static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Clear();

            FileManager.EnsureDataDirectory();

            AppInfo.Profiles = FileManager.LoadAllProfiles();

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
                else if (choice == "n")
                {
                    return CreateProfile();
                }
                else
                {
                    Console.WriteLine("Пожалуйста, введите 'y' или 'n'");
                }
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
            string login = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new InvalidArgumentException("Логин не может быть пустым.");
            }

            Console.Write("Пароль: ");
            string password = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidArgumentException("Пароль не может быть пустым.");
            }

            var profile = FileManager.LoadProfile(login, password);

            if (profile != null)
            {
                AppInfo.CurrentProfile = profile;

                string todoPath = FileManager.GetTodoFilePath(profile.Id);
                if (File.Exists(todoPath))
                {
                    AppInfo.UserTodos[profile.Id] = FileManager.LoadTodos(todoPath);
                }
                else
                {
                    AppInfo.UserTodos[profile.Id] = new TodoList();
                    FileManager.SaveTodos(AppInfo.UserTodos[profile.Id], todoPath);
                }

                var todoList = AppInfo.UserTodos[profile.Id];
                SubscribeToTodoEvents(todoList);

				AppInfo.ClearUndoRedo();
                return true;
            }

            Console.WriteLine("Неверный логин или пароль.");
            return LoginProfile();
        }

        private static bool CreateProfile()
        {
            Console.Write("Логин: ");
            string login = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new InvalidArgumentException("Логин не может быть пустым.");
            }

            if (AppInfo.Profiles.Any(p => p.Login == login))
            {
                Console.WriteLine("Этот логин уже занят.");
                throw new DuplicateLoginException("Этот логин уже занят.");
            }

            Console.Write("Пароль: ");
            string password = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidArgumentException("Пароль не может быть пустым.");
            }

            Console.Write("Имя: ");
            string firstName = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new InvalidArgumentException("Имя не может быть пустым.");
            }

            Console.Write("Фамилия: ");
            string lastName = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new InvalidArgumentException("Фамилия не может быть пустой.");
            }

            Console.Write("Год рождения: ");
            if (!int.TryParse(Console.ReadLine(), out int birthYear))
            {
                Console.WriteLine("Неверный формат года.");
                throw new InvalidArgumentException("Неверный формат года рождения.");
            }

            if (birthYear < 1900 || birthYear > DateTime.Now.Year)
            {
                throw new InvalidArgumentException("Год рождения вне допустимого диапазона.");
            }

            var profile = new Profile(login, password, firstName, lastName, birthYear);
            AppInfo.Profiles.Add(profile);
            FileManager.SaveProfile(profile);

            AppInfo.CurrentProfile = profile;
            AppInfo.UserTodos[profile.Id] = new TodoList();

            string todoPath = FileManager.GetTodoFilePath(profile.Id);
            FileManager.SaveTodos(AppInfo.UserTodos[profile.Id], todoPath);

            var todoList = AppInfo.UserTodos[profile.Id];
            SubscribeToTodoEvents(todoList);

			AppInfo.ClearUndoRedo();
            return true;
        }

        private static void SubscribeToTodoEvents(TodoList todoList)
        {
            todoList.OnTodoAdded += FileManager.SaveTodoList;
            todoList.OnTodoDeleted += FileManager.SaveTodoList;
            todoList.OnTodoUpdated += FileManager.SaveTodoList;
            todoList.OnStatusChanged += FileManager.SaveTodoList;
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
                    catch (Exception)
                    {
                        Console.WriteLine("Неожиданная ошибка.");
                        continue;
                    }

					Console.WriteLine($"\nДобро пожаловать, {AppInfo.CurrentProfile?.FirstName}!\n");
				}

				Console.Write("> ");
                string input = Console.ReadLine() ?? "";

                if (input.ToLower() == "exit")
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
                catch (Exception)
                {
                    Console.WriteLine("Неожиданная ошибка.");
                }
            }
        }
    }
}
