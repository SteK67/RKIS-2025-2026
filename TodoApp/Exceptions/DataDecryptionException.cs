using System;

namespace TodoApp.Exceptions
{
    public class DataDecryptionException : Exception
    {
        public DataDecryptionException(string message, Exception? innerException = null) : base(message, innerException) { }
    }
}
