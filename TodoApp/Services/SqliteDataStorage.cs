using System;
using System.Collections.Generic;
using System.Linq;
using TodoApp.Models;

namespace TodoApp.Services
{
    public class SqliteDataStorage : IDataStorage
    {
        private readonly ProfileRepository _profileRepository = new ProfileRepository();
        private readonly TodoRepository _todoRepository = new TodoRepository();

        public IEnumerable<Profile> LoadProfiles()
        {
            return _profileRepository.GetAll();
        }

        public void SaveProfiles(IEnumerable<Profile> profiles)
        {
            _profileRepository.ReplaceAll(profiles.ToList());
        }

        public IEnumerable<TodoItem> LoadTodos(Guid userId)
        {
            return _todoRepository.GetAll(userId);
        }

        public void SaveTodos(Guid userId, IEnumerable<TodoItem> todos)
        {
            _todoRepository.ReplaceForProfile(userId, todos.ToList());
        }
    }
}
