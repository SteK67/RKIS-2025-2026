using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Services
{
    public class TodoRepository
    {
        public List<TodoItem> GetAll()
        {
            using var context = new AppDbContext();
            return context.Todos
                .AsNoTracking()
                .OrderBy(todo => todo.Id)
                .ToList();
        }

        public List<TodoItem> GetAll(Guid profileId)
        {
            using var context = new AppDbContext();
            return context.Todos
                .AsNoTracking()
                .Where(todo => todo.ProfileId == profileId)
                .OrderBy(todo => todo.Id)
                .ToList();
        }

        public void Add(TodoItem item)
        {
            using var context = new AppDbContext();
            context.Todos.Add(item);
            context.SaveChanges();
        }

        public void Update(TodoItem item)
        {
            using var context = new AppDbContext();
            context.Todos.Update(item);
            context.SaveChanges();
        }

        public void Delete(int id)
        {
            using var context = new AppDbContext();
            var item = context.Todos.FirstOrDefault(todo => todo.Id == id);
            if (item == null)
            {
                return;
            }

            context.Todos.Remove(item);
            context.SaveChanges();
        }

        public void SetStatus(int id, TodoStatus status)
        {
            using var context = new AppDbContext();
            var item = context.Todos.FirstOrDefault(todo => todo.Id == id);
            if (item == null)
            {
                return;
            }

            item.Status = status;
            item.LastUpdate = DateTime.Now;
            context.SaveChanges();
        }

        public void ReplaceForProfile(Guid profileId, IEnumerable<TodoItem> todos)
        {
            using var context = new AppDbContext();
            var existing = context.Todos.Where(todo => todo.ProfileId == profileId).ToList();
            if (existing.Count > 0)
            {
                context.Todos.RemoveRange(existing);
            }

            var prepared = todos.Select(todo => new TodoItem
            {
                Id = todo.Id,
                Text = todo.Text,
                Status = todo.Status,
                LastUpdate = todo.LastUpdate,
                ProfileId = profileId
            }).ToList();

            if (prepared.Count > 0)
            {
                context.Todos.AddRange(prepared);
            }

            context.SaveChanges();
        }
    }
}
