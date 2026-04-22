using System;

namespace TodoApp.Services
{
    public interface IClock
    {
        DateTime Now { get; }
    }
}
