using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TodoApp.Exceptions;
using TodoApp.Models;

namespace TodoApp.Services
{
    [Obsolete("SQLite repositories are used for active data storage.")]
    public class FileManager : IDataStorage
    {
        private const string ProfilesFileName = "profiles.dat";
        private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("TodoAppKey123456TodoAppKey123456");
        private static readonly byte[] EncryptionIv = Encoding.UTF8.GetBytes("TodoAppInitVect1");
        private readonly string _dataDirectory;

        public FileManager(string dataDirectory)
        {
            _dataDirectory = dataDirectory;
            Directory.CreateDirectory(_dataDirectory);
        }

        public IEnumerable<Profile> LoadProfiles()
        {
            string filePath = Path.Combine(_dataDirectory, ProfilesFileName);
            if (!File.Exists(filePath))
            {
                return new List<Profile>();
            }

            try
            {
                var profiles = new List<Profile>();
                using var reader = CreateReader(filePath);

                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    profiles.Add(ParseProfile(line));
                }

                return profiles;
            }
            catch (CryptographicException ex)
            {
                throw new DataDecryptionException("Ошибка расшифровки профилей.", ex);
            }
            catch (InvalidDataException ex)
            {
                throw new CorruptedDataException("Файл профилей поврежден.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new DataStorageException("Нет доступа к файлу профилей.", ex);
            }
            catch (IOException ex)
            {
                throw new DataStorageException("Ошибка чтения профилей.", ex);
            }
        }

        public void SaveProfiles(IEnumerable<Profile> profiles)
        {
            string filePath = Path.Combine(_dataDirectory, ProfilesFileName);

            try
            {
                using var writer = CreateWriter(filePath);
                foreach (var profile in profiles)
                {
                    writer.WriteLine(string.Join(";",
                        profile.Id,
                        Escape(profile.Login),
                        Escape(profile.Password),
                        Escape(profile.FirstName),
                        Escape(profile.LastName),
                        profile.BirthYear));
                }
            }
            catch (CryptographicException ex)
            {
                throw new DataStorageException("Ошибка шифрования профилей.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new DataStorageException("Нет доступа к файлу профилей.", ex);
            }
            catch (IOException ex)
            {
                throw new DataStorageException("Ошибка сохранения профилей.", ex);
            }
        }

        public IEnumerable<TodoItem> LoadTodos(Guid userId)
        {
            string filePath = GetTodoFilePath(userId);
            if (!File.Exists(filePath))
            {
                return new List<TodoItem>();
            }

            try
            {
                var todos = new List<TodoItem>();
                using var reader = CreateReader(filePath);

                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    todos.Add(ParseTodo(line));
                }

                return todos;
            }
            catch (CryptographicException ex)
            {
                throw new DataDecryptionException("Ошибка расшифровки задач.", ex);
            }
            catch (InvalidDataException ex)
            {
                throw new CorruptedDataException("Файл задач поврежден.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new DataStorageException("Нет доступа к файлу задач.", ex);
            }
            catch (IOException ex)
            {
                throw new DataStorageException("Ошибка чтения задач.", ex);
            }
        }

        public void SaveTodos(Guid userId, IEnumerable<TodoItem> todos)
        {
            string filePath = GetTodoFilePath(userId);

            try
            {
                using var writer = CreateWriter(filePath);
                foreach (var todo in todos)
                {
                    writer.WriteLine(string.Join(";",
                        Escape(todo.Text),
                        todo.Status,
                        todo.LastUpdate.ToString("O", CultureInfo.InvariantCulture)));
                }
            }
            catch (CryptographicException ex)
            {
                throw new DataStorageException("Ошибка шифрования задач.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new DataStorageException("Нет доступа к файлу задач.", ex);
            }
            catch (IOException ex)
            {
                throw new DataStorageException("Ошибка сохранения задач.", ex);
            }
        }

        private StreamWriter CreateWriter(string filePath)
        {
            var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            var bufferedStream = new BufferedStream(fileStream);
            var aes = CreateAes();
            var cryptoStream = new CryptoStream(bufferedStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            return new StreamWriter(cryptoStream, Encoding.UTF8);
        }

        private StreamReader CreateReader(string filePath)
        {
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var bufferedStream = new BufferedStream(fileStream);
            var aes = CreateAes();
            var cryptoStream = new CryptoStream(bufferedStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            return new StreamReader(cryptoStream, Encoding.UTF8);
        }

        private static Aes CreateAes()
        {
            var aes = Aes.Create();
            aes.Key = EncryptionKey;
            aes.IV = EncryptionIv;
            return aes;
        }

        private string GetTodoFilePath(Guid userId)
        {
            return Path.Combine(_dataDirectory, $"todos_{userId}.dat");
        }

        private static Profile ParseProfile(string line)
        {
            var parts = SplitEscaped(line);
            if (parts.Count != 6)
            {
                throw new InvalidDataException("Некорректная запись профиля.");
            }

            if (!Guid.TryParse(parts[0], out var id) || !int.TryParse(parts[5], out var birthYear))
            {
                throw new InvalidDataException("Некорректные данные профиля.");
            }

            return new Profile
            {
                Id = id,
                Login = Unescape(parts[1]),
                Password = Unescape(parts[2]),
                FirstName = Unescape(parts[3]),
                LastName = Unescape(parts[4]),
                BirthYear = birthYear
            };
        }

        private static TodoItem ParseTodo(string line)
        {
            var parts = SplitEscaped(line);
            if (parts.Count != 3)
            {
                throw new InvalidDataException("Некорректная запись задачи.");
            }

            if (!Enum.TryParse(parts[1], true, out TodoStatus status))
            {
                throw new InvalidDataException("Некорректный статус задачи.");
            }

            if (!DateTime.TryParse(parts[2], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var lastUpdate))
            {
                throw new InvalidDataException("Некорректная дата задачи.");
            }

            return new TodoItem(Unescape(parts[0]))
            {
                Status = status,
                LastUpdate = lastUpdate
            };
        }

        private static string Escape(string value)
        {
            return value
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace(";", "\\;", StringComparison.Ordinal)
                .Replace("\n", "\\n", StringComparison.Ordinal)
                .Replace("\r", "\\r", StringComparison.Ordinal);
        }

        private static string Unescape(string value)
        {
            var builder = new StringBuilder();
            bool escaped = false;

            foreach (char symbol in value)
            {
                if (escaped)
                {
                    builder.Append(symbol switch
                    {
                        'n' => '\n',
                        'r' => '\r',
                        ';' => ';',
                        '\\' => '\\',
                        _ => throw new InvalidDataException("Некорректная escape-последовательность.")
                    });
                    escaped = false;
                    continue;
                }

                if (symbol == '\\')
                {
                    escaped = true;
                    continue;
                }

                builder.Append(symbol);
            }

            if (escaped)
            {
                throw new InvalidDataException("Некорректное окончание escape-последовательности.");
            }

            return builder.ToString();
        }

        private static List<string> SplitEscaped(string line)
        {
            var parts = new List<string>();
            var current = new StringBuilder();
            bool escaped = false;

            foreach (char symbol in line)
            {
                if (escaped)
                {
                    current.Append('\\');
                    current.Append(symbol);
                    escaped = false;
                    continue;
                }

                if (symbol == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (symbol == ';')
                {
                    parts.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                current.Append(symbol);
            }

            if (escaped)
            {
                current.Append('\\');
            }

            parts.Add(current.ToString());
            return parts;
        }
    }
}
