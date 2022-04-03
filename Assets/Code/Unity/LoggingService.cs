using CTProject.Infrastructure;
using System;
using UnityEngine;

namespace CTProject.Unity
{
    public class LoggingService : MonoBehaviour, ILoggingService
    {
        public void Log(LogLevel level, string message)
        {
            Log(level, (object)message);
        }

        public void Log(LogLevel level, object message)
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

        public void Log(Exception message)
        {
            Debug.LogException(message);
        }
    }
}
