using CTProject.Infrastructure;
using System;
using System.Linq;

namespace CTProject.Examples
{
    public class SimpleDataProvider : IDataProvider, IDependencyConsumer
    {
        #region properties

        public DataProviderState State { get; private set; }
        public uint BufferSize { get; private set; }
        public uint SamplingRate { get; private set; }
        public uint SelectedChannel { get; private set; }

        #endregion properties

        #region Dependencies

        private ILoggingService LoggingService;

        #endregion Dependencies

        #region fields

        private Func<int, uint, float>[] sources;
        private IDataConsumer consumer;
        private SimpleDataProviderWorker worker;

        #endregion fields

        #region IDependencyConsumer

        public void LoadDependencies(IDependencyProvider dependencyProvider)
        {
            LoggingService = dependencyProvider.GetDependency<ILoggingService>();
        }

        #endregion IDependencyConsumer

        #region IDataProvider

        public void Initialize()
        {
            if (State != DataProviderState.Uninitialized)
            {
                LoggingService?.Log(new InvalidOperationException());
            }

            sources = new Func<int, uint, float>[]
            {
                Sin,
                Square,
                Saw
            };

            State = DataProviderState.Initialized;
        }

        public ChannelInfo[] GetAvailableChannels()
        {
            if (sources == null)
            {
                LoggingService?.Log(new InvalidOperationException());
                return null;
            }

            var ci = new ChannelInfo[sources.Length];
            for (int x = 0; x < sources.Length; x++)
            {
                ci[x] = new ChannelInfo()
                {
                    Name = sources[x].Method.Name,
                    ID = (uint)x
                };
            }

            return ci;
        }

        public void SetChannel(uint ChannelID)
        {
            bool working = State == DataProviderState.Working;
            if (working)
                Stop();

            SelectedChannel = ChannelID;
            consumer?.OnSettingsChange(this);

            if (working)
                Start();
        }

        public float GetMaxValue() => 1f;

        public float GetMinValue() => -1f;

        public uint[] GetAvailableBufferSizes() => new uint[] { 128, 256, 512, 1024, 2048, 4096 };

        public void SetBufferSize(uint BufferSize)
        {
            if (!GetAvailableBufferSizes().Contains(BufferSize))
            {
                LoggingService?.Log(new ArgumentException(nameof(BufferSize)));
                return;
            }

            bool working = State == DataProviderState.Working;
            if (working)
                Stop();

            this.BufferSize = BufferSize;
            consumer?.OnSettingsChange(this);

            if (working)
                Start();
        }

        public uint[] GetAvailableSamplingRates() => new uint[] { 1024, 2048, 4096, 8192, 16384, 32768 };

        public void SetSamplingRate(uint SamplingRate)
        {
            if (!GetAvailableSamplingRates().Contains(SamplingRate))
            {
                LoggingService?.Log(new ArgumentException(nameof(SamplingRate)));
                return;
            }

            bool working = State == DataProviderState.Working;
            if (working)
                Stop();

            this.SamplingRate = SamplingRate;
            consumer?.OnSettingsChange(this);

            if (working)
                Start();
        }

        public void Reset()
        {
            Stop();
            Start();
        }

        public void Start()
        {
            worker?.Stop();
            consumer?.ResetIndex();
            worker = new SimpleDataProviderWorker(this, SamplingRate, BufferSize, sources[SelectedChannel], consumer);
            worker.Start();
        }

        public void Stop()
        {
            worker.Stop();
        }

        public void Subscribe(IDataConsumer consumer)
        {
            worker?.Stop();
            this.consumer = consumer;
        }

        #endregion IDataProvider

        #region SimpleDataPRoviderWorker callbacks

        internal void OnWorkerStart()
        {
            UpdateProviderState();
        }

        internal void OnWorkerStop()
        {
            UpdateProviderState();
        }

        internal void OnWorkerLog(LogLevel level, object message)
        {
            LoggingService?.Log(level, message);
        }

        internal void OnWorkerException(Exception exception)
        {
            LoggingService?.Log(exception);
        }

        private void UpdateProviderState()
        {
            if (State == DataProviderState.Uninitialized)
                return;

            bool isWorking = worker?.IsWorking ?? false;

            if (isWorking && State != DataProviderState.Working)
                State = DataProviderState.Working;

            if (!isWorking && (State != DataProviderState.Stopped || State != DataProviderState.Error))
                State = DataProviderState.Stopped;
        }

        #endregion SimpleDataPRoviderWorker callbacks

        private float Sin(int x, uint SamplingRate)
        {
            double position = (x * Math.PI) / (SamplingRate * 2);
            return (float)Math.Sin(position);
        }

        private float Square(int x, uint SamplingRate)
        {
            return Math.Sign(Sin(x, SamplingRate));
        }

        private float Saw(int x, uint SamplingRate)
        {
            double position = (x * Math.PI) / (SamplingRate * 2);
            return (float)(position - Math.Ceiling(position));
        }
    }
}
