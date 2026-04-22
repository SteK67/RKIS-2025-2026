using System;

namespace TodoApp.Services
{
    public class SystemClock : IClock
    {
        public DateTime Now => DateTime.Now;
    }
}
