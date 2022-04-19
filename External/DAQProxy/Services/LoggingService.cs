using CTProject.Infrastructure;
using System;

namespace DAQProxy.Services
{
    public class LoggingService : ILoggingService, IDependencyConsumer
    {
        private IActionPump pump;

        public void Log(LogLevel level, string message)
        {
            pump.Do(() => Console.WriteLine($"{level} {message}"));
        }

        public void Log(LogLevel level, object message)
        {
            pump.Do(() => Console.WriteLine($"{level} {message}"));
        }

        public void Log(Exception exception)
        {
            pump.Do(() => Console.WriteLine($"EXCEPTION! {exception}"));
        }

        public LoggingService()
        {
        }

        public void LoadDependencies(IDependencyProvider dependencyProvider)
        {
            pump = dependencyProvider.GetDependency<IActionPump>();
        }
    }
}
