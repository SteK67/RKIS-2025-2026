using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TodoApp.Exceptions;
using TodoApp.Models;

namespace TodoApp.Services
{
    public class ApiDataStorage : IDataStorage
    {
        private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("TodoAppKey123456TodoAppKey123456");
        private static readonly byte[] EncryptionIv = Encoding.UTF8.GetBytes("TodoAppInitVect1");
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;

        public ApiDataStorage(string baseAddress)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress, UriKind.Absolute),
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        public bool IsAvailable()
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "profiles");
                using var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public void SaveProfiles(IEnumerable<Profile> profiles)
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(profiles, JsonOptions);
            var encrypted = Encrypt(payload);
            PostBytes("profiles", encrypted);
        }

        public IEnumerable<Profile> LoadProfiles()
        {
            var encrypted = GetBytes("profiles");
            if (encrypted.Length == 0)
            {
                return new List<Profile>();
            }

            var json = Decrypt(encrypted);
            var profiles = JsonSerializer.Deserialize<List<Profile>>(json, JsonOptions);
            return profiles ?? new List<Profile>();
        }

        public void SaveTodos(Guid userId, IEnumerable<TodoItem> todos)
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(ToTodoDtos(todos), JsonOptions);
            var encrypted = Encrypt(payload);
            PostBytes($"todos/{userId}", encrypted);
        }

        public IEnumerable<TodoItem> LoadTodos(Guid userId)
        {
            var encrypted = GetBytes($"todos/{userId}");
            if (encrypted.Length == 0)
            {
                return new List<TodoItem>();
            }

            var json = Decrypt(encrypted);
            var dtos = JsonSerializer.Deserialize<List<TodoItemDto>>(json, JsonOptions) ?? new List<TodoItemDto>();
            return FromTodoDtos(dtos);
        }

        private void PostBytes(string uri, byte[] data)
        {
            try
            {
                using var content = new ByteArrayContent(data);
                using var response = _httpClient.PostAsync(uri, content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new DataStorageException("Ошибка отправки данных на сервер.", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new DataStorageException("Сервер недоступен.", ex);
            }
        }

        private byte[] GetBytes(string uri)
        {
            try
            {
                using var response = _httpClient.GetAsync(uri).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            }
            catch (HttpRequestException ex)
            {
                throw new DataStorageException("Ошибка получения данных с сервера.", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new DataStorageException("Сервер недоступен.", ex);
            }
        }

        private static byte[] Encrypt(byte[] data)
        {
            using var output = new MemoryStream();
            using var aes = CreateAes();
            using var cryptoStream = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();
            return output.ToArray();
        }

        private static byte[] Decrypt(byte[] data)
        {
            try
            {
                using var input = new MemoryStream(data);
                using var output = new MemoryStream();
                using var aes = CreateAes();
                using var cryptoStream = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
                cryptoStream.CopyTo(output);
                return output.ToArray();
            }
            catch (CryptographicException ex)
            {
                throw new DataDecryptionException("Ошибка расшифровки данных с сервера.", ex);
            }
            catch (InvalidDataException ex)
            {
                throw new CorruptedDataException("Сервер вернул поврежденные данные.", ex);
            }
        }

        private static Aes CreateAes()
        {
            var aes = Aes.Create();
            aes.Key = EncryptionKey;
            aes.IV = EncryptionIv;
            return aes;
        }

        private static List<TodoItemDto> ToTodoDtos(IEnumerable<TodoItem> todos)
        {
            var result = new List<TodoItemDto>();
            foreach (var todo in todos)
            {
                result.Add(new TodoItemDto
                {
                    Text = todo.Text,
                    Status = todo.Status,
                    LastUpdate = todo.LastUpdate
                });
            }

            return result;
        }

        private static List<TodoItem> FromTodoDtos(IEnumerable<TodoItemDto> dtos)
        {
            var result = new List<TodoItem>();
            foreach (var dto in dtos)
            {
                var item = new TodoItem(dto.Text ?? string.Empty)
                {
                    Status = dto.Status,
                    LastUpdate = dto.LastUpdate
                };
                result.Add(item);
            }

            return result;
        }

        private class TodoItemDto
        {
            public string? Text { get; set; }
            public TodoStatus Status { get; set; }
            public DateTime LastUpdate { get; set; }
        }
    }
}
