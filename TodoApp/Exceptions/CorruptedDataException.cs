using System;

namespace TodoApp.Exceptions
{
    public class CorruptedDataException : Exception
    {
        public CorruptedDataException(string message, Exception? innerException = null) : base(message, innerException) { }
    }
}
