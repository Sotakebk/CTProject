using CTProject.Infrastructure;
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace CTProject.Unity
{
    public class LoggingService : MonoBehaviour, ILoggingService
    {
        #region events

        public delegate void LogEventHandler(LogLevel logLevel, object message, string stackTrace);

        public delegate void ExceptionEventHandler(Exception exception);

        public event ExceptionEventHandler ExceptionEvent;

        public event LogEventHandler LogEvent;

        #endregion events

        #region properties

        public bool SeeEngineLogs { get; set; }

        #endregion properties

        #region fields

        private const string Tag = "[From LoggingService]";

        private ConcurrentQueue<Action> actionQueue;

        #endregion fields

        #region Unity calls

        private void Start()
        {
            actionQueue = new ConcurrentQueue<Action>();
            Application.logMessageReceived += Application_logMessageReceived;
        }

        private void Update()
        {
            ProcessMessages();
        }

        #endregion Unity calls

        #region private methods

        private void ProcessMessages()
        {
            int i = 5;
            while (i > 0 && actionQueue.Count > 0)
            {
                i++;

                if (!actionQueue.TryDequeue(out var action))
                    continue;

                action();
            }
        }

        private void LogInternal(LogLevel level, object message, string stackTrace, bool fromUnity = false)
        {
            LogEvent?.Invoke(level, message, stackTrace);

            if (fromUnity)
                return;

            switch (level)
            {
                case LogLevel.Info:
                    Debug.Log($"{Tag} {message}");
                    break;

                case LogLevel.Warning:
                    Debug.LogWarning($"{Tag} {message}");
                    break;

                case LogLevel.Error:
                    Debug.LogError($"{Tag} {message}");
                    break;
            }
        }

        private void LogInternal(Exception exception)
        {
            Debug.LogException(exception);
            ExceptionEvent?.Invoke(exception);
        }

        private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (SeeEngineLogs && !condition.StartsWith(Tag))
                LogInternal(LogTypeToLogLevel(type), condition, stackTrace, fromUnity: true);
        }

        private LogLevel LogTypeToLogLevel(LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    return LogLevel.Error;

                case LogType.Assert:
                case LogType.Warning:
                    return LogLevel.Warning;

                case LogType.Log:
                    return LogLevel.Info;

                case LogType.Exception:
                    break;

                default:
                    return LogLevel.Info;
            }
            return LogLevel.Info;
        }

        #endregion private methods

        #region ILoggingService

        public void Log(LogLevel level, string message)
        {
            var stack = Environment.StackTrace;
            actionQueue.Enqueue(() => LogInternal(level, message, stack));
        }

        public void Log(LogLevel level, object message)
        {
            var stack = Environment.StackTrace;
            actionQueue.Enqueue(() => LogInternal(level, message, stack));
        }

        public void Log(Exception exception)
        {
            actionQueue.Enqueue(() => LogInternal(exception));
        }

        #endregion ILoggingService
    }
}
