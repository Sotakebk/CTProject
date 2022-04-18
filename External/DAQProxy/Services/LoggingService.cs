using CTProject.Infrastructure;
using System;

namespace DAQProxy.Services
{
    public class LoggingService : ILoggingService
    {
        public void Log(LogLevel level, string message) => Console.WriteLine($"{level} {message}");

        public void Log(LogLevel level, object message) => Console.WriteLine($"{level} {message}");

        public void Log(Exception exception) => Console.WriteLine($"EXCEPTION!\n {exception}");
    }
}
