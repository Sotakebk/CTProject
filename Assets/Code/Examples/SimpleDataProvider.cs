using CTProject.Infrastructure;
using System;
using System.Linq;

namespace CTProject.Examples
{
    public class SimpleDataProvider : IDataProvider, IDependencyConsumer
    {
        #region Dependencies

        private ILoggingService LoggingService;

        #endregion Dependencies

        #region properties

        public DataProviderState State { get; private set; }

        public IChannelInfo SelectedChannel
        {
            get
            {
                return _selectedChannel;
            }
            set
            {
                var simpleChannelInfo = GetAvailableChannels()
                    .FirstOrDefault(c => c.UniqueName == value.UniqueName);

                if (simpleChannelInfo == null)
                    throw new ArgumentException();

                bool working = State == DataProviderState.Working;
                if (working)
                    Stop();

                _selectedChannel = simpleChannelInfo as SimpleChannelInfo;
                consumer?.OnSettingsChange(this);

                if (working)
                    Start();
            }
        }

        public uint SelectedSamplingRate
        {
            get
            {
                return _selectedSamplingRate;
            }
            set
            {
                if (!GetAvailableSamplingRates().Contains(value))
                    throw new ArgumentException();

                bool working = State == DataProviderState.Working;
                if (working)
                    Stop();

                _selectedSamplingRate = value;
                consumer?.OnSettingsChange(this);

                if (working)
                    Start();
            }
        }

        public uint SelectedBufferSize
        {
            get
            {
                return _selectedBufferSize;
            }
            set
            {
                if (!GetAvailableBufferSizes().Contains(value))
                    throw new ArgumentException();

                bool working = State == DataProviderState.Working;
                if (working)
                    Stop();

                _selectedBufferSize = value;
                consumer?.OnSettingsChange(this);

                if (working)
                    Start();
            }
        }

        #endregion properties

        #region fields

        private SimpleChannelInfo _selectedChannel;
        private uint _selectedSamplingRate;
        private uint _selectedBufferSize;

        private Func<int, uint, float>[] sources;
        private IDataConsumer consumer;
        private SimpleDataProviderWorker worker;
        private bool initializing = false;
        private SimpleChannelInfo[] cachedChannelInfos;

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
            if (State != DataProviderState.Uninitialized || initializing)
                throw new InvalidOperationException();

            initializing = true;

            sources = new Func<int, uint, float>[]
            {
                Sin,
                Square,
                Saw
            };

            State = DataProviderState.Initialized;

            SelectedBufferSize = GetAvailableBufferSizes().First();
            SelectedSamplingRate = GetAvailableSamplingRates().First();
            SelectedChannel = GetAvailableChannels().First();
        }

        public IChannelInfo[] GetAvailableChannels()
        {
            if (cachedChannelInfos != null)
                return cachedChannelInfos.Select(c => c as IChannelInfo).ToArray();

            if (State == DataProviderState.Uninitialized)
                throw new InvalidOperationException();

            cachedChannelInfos = sources
                .Select((s, index) => new SimpleChannelInfo(s.Method.Name, (uint)index))
                .ToArray();

            return cachedChannelInfos.Select(c => c as IChannelInfo).ToArray();
        }

        public uint[] GetAvailableBufferSizes() => new uint[] { 128, 256, 512, 1024, 2048, 4096 };

        public uint[] GetAvailableSamplingRates() => new uint[] { 1024, 2048, 4096, 8192, 16384 };

        public float GetMaxValue() => 1f;

        public float GetMinValue() => -1f;

        public void Start()
        {
            worker?.Stop();
            consumer?.ResetIndex();
            worker = new SimpleDataProviderWorker(
                owner: this,
                samplingRate: SelectedSamplingRate,
                bufferSize: SelectedBufferSize,
                dataSourceLogic: sources[((int?)(SelectedChannel as SimpleChannelInfo)?.ID) ?? 0],
                consumer: consumer);
            worker.Start();
        }

        public void Stop()
        {
            if (State == DataProviderState.Working)
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

        #region worker delegate methods for different channels

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

        #endregion worker delegate methods for different channels
    }
}
