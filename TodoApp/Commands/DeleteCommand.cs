using System;
using TodoApp.Exceptions;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Commands
{
	public class DeleteCommand : IUndoableCommand
	{
		private readonly int _index;
		private TodoItem _deletedItem;
		private TodoList _todos;

		public DeleteCommand(int index)
		{
			_index = index;
		}

		public void Execute()
		{
			_todos = AppInfo.RequireCurrentTodoList();


			_deletedItem = _todos[_index];

			if (_deletedItem == null)
			{
				Console.WriteLine($"Ошибка: задача с индексом {_index} не найдена.");
				throw new TaskNotFoundException($"Задача с индексом {_index} не существует.");
			}

			_todos.Delete(_index);
			// Событие OnTodoDeleted будет вызвано автоматически в TodoList.Delete()

			Console.WriteLine($"Задача удалена: {_deletedItem.Text}");
		}

		public void Unexecute()
		{
			_todos = AppInfo.RequireCurrentTodoList();
			if (_deletedItem == null) return;

			_todos.Add(_deletedItem);
			// Событие OnTodoAdded будет вызвано автоматически в TodoList.Add()

			Console.WriteLine("Отменено удаление задачи");
		}
	}
}
