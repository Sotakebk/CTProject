using CTProject.Infrastructure;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace CTProject.Unity.Presentation
{
    public class LogPresenter : BasePresenter, IDataConsumer
    {
        #region properties

        public override string ViewName => "Logs";

        private bool LogAllDataSourceCalls { get; set; }
        private bool SaveStackTrace { get; set; }

        #endregion properties

        #region fields

        private const int MaxLogCount = 100;

        // set from Unity
        [SerializeField]
        private LoggingService loggingService;

        [SerializeField]
        private VisualTreeAsset logTemplate;

        #endregion fields

        #region UI elements

        private Toggle showEngineLogsToggle;
        private Toggle logDataSourceCallsToggle;
        private Toggle saveStackTraceToggle;
        private VisualElement logContainer;

        #endregion UI elements

        #region Unity calls

        private void Start()
        {
            loggingService.LogEvent += LoggingService_LogEvent;
            loggingService.ExceptionEvent += LoggingService_ExceptionEvent;
        }

        #endregion Unity calls

        #region Presenter

        public override void PrepareView()
        {
            showEngineLogsToggle = view.Q<Toggle>(name: "showEngineLogsToggle");
            logDataSourceCallsToggle = view.Q<Toggle>(name: "logDataSourceCallsToggle");
            saveStackTraceToggle = view.Q<Toggle>(name: "saveStackTraceToggle");

            logContainer = view.Q(name: "logsContainer");
            Debug.Log(logContainer);

            showEngineLogsToggle.RegisterValueChangedCallback(ShowEngineLogsChangedCallback);
            logDataSourceCallsToggle.RegisterValueChangedCallback(LogDataSourceCallsChangedCallback);
            saveStackTraceToggle.RegisterValueChangedCallback(SaveStackTraceChangedCallback);
        }

        #endregion Presenter

        #region IDataConsumer

        public void OnSettingsChange(IDataProvider source)
        {
            if (LogAllDataSourceCalls)
                AddLogElement(LogLevel.Info, $"DataProvider OnSettingsChange message received from source {source.GetType().Name}", null);
        }

        public void ReceiveData(ulong index, float[] data)
        {
            if (LogAllDataSourceCalls)
                AddLogElement(LogLevel.Info, $"DataProvider ReceiveData message received with index {index} and of length {data.Length}", null);
        }

        public void DataStreamStarted(long tickCountOnStreamStart)
        {
            if (LogAllDataSourceCalls)
                AddLogElement(LogLevel.Info, $"DataProvider DataStreamStarted message received with tickCount {tickCountOnStreamStart}", null);
        }

        public void DataStreamEnded()
        {
            if (LogAllDataSourceCalls)
                AddLogElement(LogLevel.Info, "DataProvider DataStreamEnded message received", null);
        }

        #endregion IDataConsumer

        #region UI commands and callbacks

        private void SaveStackTraceChangedCallback(ChangeEvent<bool> evt)
        {
            SaveStackTrace = evt.newValue;
        }

        private void LogDataSourceCallsChangedCallback(ChangeEvent<bool> evt)
        {
            LogAllDataSourceCalls = evt.newValue;
        }

        private void ShowEngineLogsChangedCallback(ChangeEvent<bool> evt)
        {
            loggingService.SeeEngineLogs = evt.newValue;
        }

        #endregion UI commands and callbacks

        #region event handlers

        private void LoggingService_ExceptionEvent(Exception exception)
        {
            AddLogElement(LogLevel.Error, exception.Message, exception.ToString());
        }

        private void LoggingService_LogEvent(LogLevel logLevel, object message, string stackTrace)
        {
            AddLogElement(logLevel, message.ToString(), stackTrace);
        }

        #endregion event handlers

        #region private methods

        private void AddLogElement(LogLevel level, string message, string stackTrace)
        {
            logContainer.Add(ConstructLogElement(level, message, stackTrace));

            while (logContainer.childCount > MaxLogCount)
                logContainer.RemoveAt(0);
        }

        private VisualElement ConstructLogElement(LogLevel level, string message, string stackTrace)
        {
            var template = logTemplate.CloneTree();
            var typeLabel = template.Q<Label>(name: "typeLabel");
            var messageTextField = template.Q<TextField>(name: "messageTextField");
            var stackTraceFoldout = template.Q<Foldout>(name: "stackTraceFoldout");
            var stackTraceTextField = template.Q<TextField>(name: "stackTraceTextField");

            typeLabel.text = level.ToString();
            typeLabel.style.backgroundColor = LogLevelToColor(level);
            messageTextField.value = message;
            if (SaveStackTrace && !string.IsNullOrEmpty(stackTrace))
            {
                stackTraceFoldout.value = false;
                stackTraceTextField.value = stackTrace;
            }
            else
            {
                stackTraceFoldout.parent.Remove(stackTraceFoldout);
            }

            return template;
        }

        private Color LogLevelToColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Info => Color.white,
                LogLevel.Warning => Color.yellow,
                LogLevel.Error => Color.red,
                _ => Color.white,
            };
        }

        #endregion private methods
    }
}
