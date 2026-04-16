using System;

namespace TodoApp.Exceptions
{
    public class DataStorageException : Exception
    {
        public DataStorageException(string message, Exception? innerException = null) : base(message, innerException) { }
    }
}
