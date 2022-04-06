namespace CTProject.Infrastructure
{
    public enum LogLevel : int
    {
        Info,
        Warning,
        Error
    }

    public interface ILoggingService
    {
        void Log(LogLevel level, string message);

        void Log(LogLevel level, object message);

        void Log(System.Exception exception);
    }
}
