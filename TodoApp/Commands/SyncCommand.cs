using System;
using System.Linq;
using TodoApp.Exceptions;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Commands
{
    public class SyncCommand : ICommand
    {
        private const string ServerAddress = "http://localhost:5000/";

        private readonly bool _pull;
        private readonly bool _push;
        private readonly ApiDataStorage _apiStorage;

        public SyncCommand(bool pull, bool push)
        {
            _pull = pull;
            _push = push;
            _apiStorage = new ApiDataStorage(ServerAddress);
        }

        public static ICommand Create(string[] args)
        {
            bool pull = args.Any(arg => arg.Equals("--pull", StringComparison.OrdinalIgnoreCase));
            bool push = args.Any(arg => arg.Equals("--push", StringComparison.OrdinalIgnoreCase));

            foreach (var arg in args)
            {
                if (!arg.Equals("--pull", StringComparison.OrdinalIgnoreCase) &&
                    !arg.Equals("--push", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidCommandException($"Неизвестный флаг: {arg}");
                }
            }

            if (!pull && !push)
            {
                throw new InvalidArgumentException("Используйте: sync --pull или sync --push");
            }

            return new SyncCommand(pull, push);
        }

        public void Execute()
        {
            if (!_apiStorage.IsAvailable())
            {
                Console.WriteLine("Ошибка: сервер недоступен.");
                return;
            }

            if (_push)
            {
                Push();
            }

            if (_pull)
            {
                Pull();
            }
        }

        private void Push()
        {
            if (AppInfo.Storage == null)
            {
                throw new DataStorageException("Локальное хранилище не настроено.");
            }

            _apiStorage.SaveProfiles(AppInfo.Profiles);

            foreach (var profile in AppInfo.Profiles)
            {
                var todos = AppInfo.UserTodos.TryGetValue(profile.Id, out var todoList)
                    ? todoList.GetAll()
                    : AppInfo.Storage.LoadTodos(profile.Id).ToList();

                _apiStorage.SaveTodos(profile.Id, todos);
            }

            Console.WriteLine("Синхронизация на сервер завершена.");
        }

        private void Pull()
        {
            if (AppInfo.Storage == null)
            {
                throw new DataStorageException("Локальное хранилище не настроено.");
            }

            var profiles = _apiStorage.LoadProfiles().ToList();
            AppInfo.Storage.SaveProfiles(profiles);
            AppInfo.Profiles = profiles;

            var currentProfile = AppInfo.CurrentProfile;
            if (currentProfile != null)
            {
                var actualProfile = profiles.FirstOrDefault(profile => profile.Id == currentProfile.Id);
                AppInfo.CurrentProfile = actualProfile;

                if (actualProfile != null)
                {
                    var todos = _apiStorage.LoadTodos(actualProfile.Id).ToList();
                    AppInfo.Storage.SaveTodos(actualProfile.Id, todos);
                    AppInfo.SetCurrentTodoList(actualProfile.Id, todos);
                }
                else
                {
                    AppInfo.UserTodos.Remove(currentProfile.Id);
                    AppInfo.ClearUndoRedo();
                }
            }

            Console.WriteLine("Синхронизация с сервера завершена.");
        }
    }
}
