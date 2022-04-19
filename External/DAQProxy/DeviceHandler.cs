using CTProject.Infrastructure;
using DAQProxy.Services;
using NationalInstruments;
using NationalInstruments.DAQmx;
using System;
using System.Linq;
using System.Text;

namespace DAQProxy
{
    internal class DeviceHandler : IDependencyConsumer
    {
        private ILoggingService loggingService;
        private IActionPump actionPump;
        private CommunicationHandler communicationHandler;

        public bool IsWorking { get; private set; }
        public int SelectedBufferSize { get; set; }
        public int SelectedSamplingRate { get; set; }
        public string SelectedChannel { get; set; }

        private string[] channels;

        private volatile Task innerTask;
        private volatile AnalogMultiChannelReader reader;
        private volatile AsyncCallback callback;

        private volatile int readingID;
        private volatile int index;

        public DeviceHandler()
        {
        }

        public void LoadDependencies(IDependencyProvider dependencyProvider)
        {
            loggingService = dependencyProvider.GetDependency<ILoggingService>();
            actionPump = dependencyProvider.GetDependency<IActionPump>();
            communicationHandler = dependencyProvider.GetDependency<CommunicationHandler>();
        }

        public void Prepare()
        {
            Stop();
            readingID++;
            index = 0;

            var s = DaqSystem.Local;

            var report = new StringBuilder();
            report.AppendLine($"Devices connected: {string.Join(", ", s.Devices)}");

            channels = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External);
            report.AppendLine($"Analog In channels available: {string.Join(", ", channels)}");

            SelectedBufferSize = GetPossibleBufferSizes().First();
            SelectedSamplingRate = GetPossibleSamplingRates().First();
            SelectedChannel = GetPossibleChannels().First();

            report.AppendLine($"Buffer size: {SelectedBufferSize}, sampling rate: {SelectedSamplingRate}, channel: {SelectedChannel}");

            loggingService.Log(LogLevel.Info, report.ToString());
        }

        public string[] GetPossibleChannels()
        {
            return (string[])channels.Clone();
        }

        public int[] GetPossibleBufferSizes()
        {
            return new int[] { 64, 128, 256, 512, 1024, 2048, 4096, 8192 };
        }

        public int[] GetPossibleSamplingRates()
        {
            return new int[] { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 48000 };
        }

        public void Start()
        {
            readingID++;
            index = 0;

            var report = new StringBuilder();
            report.AppendLine($"Starting work!");
            report.AppendLine($"Buffer size: {SelectedBufferSize}, sampling rate: {SelectedSamplingRate}, channel: {SelectedChannel}");
            loggingService?.Log(LogLevel.Info, report.ToString());

            if (innerTask == null)
            {
                try
                {
                    communicationHandler.SendMessageStarted();

                    innerTask = new Task();

                    innerTask.AIChannels.CreateVoltageChannel(
                        physicalChannelName: SelectedChannel,
                        nameToAssignChannel: string.Empty,
                        terminalConfiguration: (AITerminalConfiguration)(-1),
                        minimumValue: 0,
                        maximumValue: 10,
                        units: AIVoltageUnits.Volts);

                    innerTask.Timing.ConfigureSampleClock(
                        signalSource: string.Empty,
                        rate: SelectedSamplingRate,
                        activeEdge: SampleClockActiveEdge.Rising,
                        sampleMode: SampleQuantityMode.ContinuousSamples,
                        samplesPerChannel: SelectedBufferSize);

                    innerTask.Control(TaskAction.Verify);

                    reader = new AnalogMultiChannelReader(innerTask.Stream);
                    callback = new AsyncCallback(AnalogInCallback);

                    reader.SynchronizeCallbacks = true;
                    reader.BeginReadWaveform(
                        samplesPerChannel: SelectedBufferSize,
                        callback: callback,
                        state: innerTask);
                }
                catch (DaqException ex)
                {
                    loggingService?.Log(LogLevel.Error, $"Exception thrown when starting data read: {ex}");
                    communicationHandler.SendMessageStopped();
                }
            }
        }

        public void Stop()
        {
            if (innerTask != null)
                loggingService?.Log(LogLevel.Info, "Stopping work!");

            readingID++;
            index = 0;
            innerTask?.Dispose();
            innerTask = null;
        }

        private void AnalogInCallback(IAsyncResult ar)
        {
            try
            {
                if (innerTask != null && innerTask == ar.AsyncState)
                {
                    // Read the available data from the channels
                    var data = reader.EndReadWaveform(ar);

                    ProcessData(data);

                    reader.BeginMemoryOptimizedReadWaveform(
                        samplesPerChannel: SelectedBufferSize,
                        callback: callback,
                        state: innerTask,
                        data);
                }
            }
            catch (DaqException ex)
            {
                loggingService?.Log(LogLevel.Error, $"Exception thrown when reading data: {ex}");
                communicationHandler.SendMessageStopped();
                Stop();
            }
        }

        private void ProcessData(AnalogWaveform<double>[] Data)
        {
            for (int x = 0; x < Data.Length; x++)
            {
                var buffer = Data[x].GetScaledData();
                actionPump.Do(() => ProcessDataAction(buffer, readingID, index));
                index += buffer.Length;
            }
        }

        private void ProcessDataAction(double[] Data, int readingID, int index)
        {
            if (readingID != this.readingID)
                return;

            var floatValues = Data.Cast<float>().ToArray();

            communicationHandler.SendMessageDataBuffer(floatValues, index);
        }
    }
}
