using CTProject.Infrastructure;
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace CTProject.Unity
{
    public class LoggingService : MonoBehaviour, ILoggingService
    {
        private ConcurrentQueue<Action> actionQueue;

        private void Start()
        {
            actionQueue = new ConcurrentQueue<Action>();
        }

        private void Update()
        {
            ProcessMessages();
        }

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

        #region ILoggingService

        public void Log(LogLevel level, string message)
        {
            actionQueue.Enqueue(() => LogInternal(level, message));
        }

        public void Log(LogLevel level, object message)
        {
            actionQueue.Enqueue(() => LogInternal(level, message));
        }

        public void Log(Exception exception)
        {
            actionQueue.Enqueue(() => LogInternal(exception));
        }

        #endregion ILoggingService

        private void LogInternal(LogLevel level, object message)
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
        }

        private void LogInternal(Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}
