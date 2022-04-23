using CTProject.Infrastructure;
using DAQProxy.Services;
using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAQProxy
{
    internal class DeviceHandler : IDependencyConsumer
    {
        private ILoggingService loggingService;
        private IActionPump actionPump;
        private CommunicationHandler communicationHandler;
        private int selectedBufferSize = 0;
        private int selectedSamplingRate = 0;
        private string selectedChannel = string.Empty;
        private string[] availableChannels = Array.Empty<string>();
        private int[] samplingRates = Array.Empty<int>();
        public bool IsWorking => !(innerTask?.IsDone ?? true);

        public int SelectedBufferSize
        {
            get => selectedBufferSize;
            set
            {
                Stop();
                selectedBufferSize = value;
            }
        }

        public int SelectedSamplingRate
        {
            get => selectedSamplingRate;
            set
            {
                Stop();
                selectedSamplingRate = value;
            }
        }

        public string SelectedChannel
        {
            get => selectedChannel;
            set
            {
                Stop();
                selectedChannel = value;
            }
        }

        public int MinValue { get; set; }
        public int MaxValue { get; set; }

        private volatile Task innerTask;
        private volatile AnalogSingleChannelReader reader;
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

        public Device[] ListDevices()
        {
            List<Device> deviceList = new List<Device>();
            var s = DaqSystem.Local;
            var deviceNames = s.Devices;
            foreach (var name in deviceNames)
            {
                try
                {
                    deviceList.Add(s.LoadDevice(name));
                }
                catch (DaqException ex)
                {
                    loggingService?.Log(ex);
                }
            }
            return deviceList.ToArray();
        }

        public void ApplyDefaultSettings(Device device)
        {
            availableChannels = device.AIPhysicalChannels;

            var maximumSampling = device.AIMaximumSingleChannelRate;
            var minimumSampling = device.AIMinimumRate;
            samplingRates = GetPowersOf2BetweenInclusive((int)Math.Ceiling(minimumSampling), (int)Math.Floor(maximumSampling));

            var bufferSizes = GetPossibleBufferSizes();
            SelectedBufferSize = bufferSizes[bufferSizes.Length / 2];
            SelectedChannel = GetPossibleChannels().First();
            SelectedSamplingRate = GetPossibleSamplingRates().First();
        }

        public string[] GetPossibleChannels()
        {
            return (string[])availableChannels.Clone();
        }

        public int[] GetPossibleBufferSizes()
        {
            return new int[] { 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };
        }

        public int[] GetPossibleSamplingRates()
        {
            return (int[])samplingRates.Clone();
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
                    communicationHandler.SendStarted();

                    innerTask = new Task();

                    innerTask.AIChannels.CreateVoltageChannel(
                        physicalChannelName: SelectedChannel,
                        nameToAssignChannel: $"Reading{readingID}",
                        terminalConfiguration: AITerminalConfiguration.Rse,
                        minimumValue: MinValue,
                        maximumValue: MaxValue,
                        units: AIVoltageUnits.Volts);

                    innerTask.Timing.ConfigureSampleClock(
                        signalSource: string.Empty,
                        rate: SelectedSamplingRate,
                        activeEdge: SampleClockActiveEdge.Rising,
                        sampleMode: SampleQuantityMode.ContinuousSamples,
                        samplesPerChannel: SelectedBufferSize);

                    innerTask.Control(TaskAction.Verify);

                    reader = new AnalogSingleChannelReader(innerTask.Stream);
                    callback = new AsyncCallback(AnalogInCallback);

                    reader.SynchronizeCallbacks = false;
                    reader.BeginReadMultiSample(
                        numberOfSamples: SelectedBufferSize,
                        callback: callback,
                        state: innerTask);
                }
                catch (DaqException ex)
                {
                    loggingService?.Log(LogLevel.Error, $"Exception thrown when starting data read: {ex}");
                    communicationHandler.SendStopped();
                }
            }
        }

        public void Stop()
        {
            if (innerTask != null)
                loggingService?.Log(LogLevel.Info, "Stopping work!");

            readingID++;
            index = 0;
            innerTask?.Stop();
            innerTask?.Dispose();
            innerTask = null;
            communicationHandler.SendStopped();
        }

        private void AnalogInCallback(IAsyncResult ar)
        {
            try
            {
                if (innerTask != null && innerTask == ar.AsyncState)
                {
                    double[] data = reader.EndReadMultiSample(ar);

                    ProcessData(data);

                    reader.BeginReadMultiSample(
                        numberOfSamples: SelectedBufferSize,
                        callback: callback,
                        state: innerTask);
                }
            }
            catch (DaqException ex)
            {
                loggingService?.Log(LogLevel.Error, $"Exception thrown when reading data: {ex}");
                communicationHandler.SendStopped();
                Stop();
            }
        }

        private void ProcessData(double[] data)
        {
            var i = index;
            var id = readingID;
            actionPump.Do(() => ProcessDataAction(data, id, i));
            index += data.Length;
        }

        private void ProcessDataAction(double[] data, int readingID, int index)
        {
            if (readingID != this.readingID)
                return;

            var floatValues = Array.ConvertAll(data, d => (float)d).ToArray();

            communicationHandler.SendDataBuffer(floatValues, index);
        }

        private int[] GetPowersOf2BetweenInclusive(int A, int B)
        {
            if (A < 0) A = 0;
            var itStart = (int)Math.Ceiling(Math.Log(A, 2));
            var itEnd = (int)Math.Floor(Math.Log(B, 2));

            var list = new List<int>();
            list.Add(A);
            for (int x = itStart; x <= itEnd; x++)
                list.Add((int)Math.Pow(2, x));

            list.Add(B);
            return list.Distinct().OrderBy(i => i).ToArray();
        }
    }
}
