using System;
using TodoApp.Exceptions;
using TodoApp.Services;

namespace TodoApp.Commands
{
    public class UndoCommand : ICommand
    {
        public void Execute()
        {
            if (AppInfo.UndoStack.Count == 0)
            {
                Console.WriteLine("Нечего отменять.");
                throw new InvalidArgumentException("Стек undo пуст.");
            }

            var command = AppInfo.UndoStack.Pop();
            command.Unexecute();
            AppInfo.RedoStack.Push(command);
        }
    }
}
