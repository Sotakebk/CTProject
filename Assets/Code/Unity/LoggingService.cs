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

        #region fields

        private ConcurrentQueue<Action> actionQueue;

        #endregion fields

        #region Unity calls

        private void Start()
        {
            actionQueue = new ConcurrentQueue<Action>();
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

        private void LogInternal(LogLevel level, object message, string stackTrace)
        {
            switch (level)
            {
                case LogLevel.Info:
                    Debug.Log(message);
                    return;

                case LogLevel.Warning:
                    Debug.LogWarning(message);
                    return;

                case LogLevel.Error:
                    Debug.LogError(message);
                    return;
            }
            LogEvent?.Invoke(level, message, stackTrace);
        }

        private void LogInternal(Exception exception)
        {
            Debug.LogException(exception);
            ExceptionEvent?.Invoke(exception);
        }

        #endregion private methods

        #region ILoggingService

        public void Log(LogLevel level, string message)
        {
            actionQueue.Enqueue(() => LogInternal(level, message, Environment.StackTrace));
        }

        public void Log(LogLevel level, object message)
        {
            actionQueue.Enqueue(() => LogInternal(level, message, Environment.StackTrace));
        }

        public void Log(Exception exception)
        {
            actionQueue.Enqueue(() => LogInternal(exception));
        }

        #endregion ILoggingService
    }
}
