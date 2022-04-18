using CTProject.DataAcquisition.Communication;
using CTProject.Infrastructure;
using System;

namespace CTProject.DataAcquisition
{
    public class DeviceDataProvider : IDependencyConsumer, IDataProvider
    {
        #region fields

        private TCPServer server;

        private uint[] cachedBufferSizes;
        private uint[] cachedSamplingRates;
        private IChannelInfo[] cachedChannelInfos;

        private uint selectedBufferSize;
        private uint selectedSamplingRate;
        private IChannelInfo selectedChannelInfo;

        #endregion fields

        #region ctor

        public DeviceDataProvider()
        {
            State = DataProviderState.Uninitialized;
            server = new TCPServer(DefaultAddress.Address, DefaultAddress.Port);
            ResetCache();

            server.OnConnected = OnClientConnected;
            server.OnDisconnected = OnClientDisconnected;
            server.OnMessageReceived = OnMessageReceived;
        }

        #endregion ctor

        #region IDependencyConsumer

        public void LoadDependencies(IDependencyProvider dependencyProvider)
        {
            server.LoadDependencies(dependencyProvider);
        }

        #endregion IDependencyConsumer

        #region IDataProvider

        public void Start()
        {
            // TODO
        }

        public void Stop()
        {
            // TODO
        }

        public void Subscribe(IDataConsumer consumer)
        {
        }

        public DataProviderState State { get; private set; }

        public IChannelInfo SelectedChannel
        {
            get => selectedChannelInfo;
            set
            {
                // TODO
            }
        }

        public uint SelectedSamplingRate
        {
            get => selectedSamplingRate;
            set
            {
                // TODO
            }
        }

        public uint SelectedBufferSize
        {
            get => selectedBufferSize;
            set
            {
                // TODO
            }
        }

        public uint[] GetAvailableBufferSizes() => cachedBufferSizes;

        public IChannelInfo[] GetAvailableChannels() => cachedChannelInfos;

        public uint[] GetAvailableSamplingRates() => cachedSamplingRates;

        public float GetMaxValue() => default; // TODO

        public float GetMinValue() => default; // TODO

        public void Initialize()
        {
            if (State != DataProviderState.Uninitialized)
                return;

            server.Start();

            State = DataProviderState.NotReady;
        }

        #endregion IDataProvider

        private void OnClientConnected()
        {
            State = DataProviderState.Ready;
            ResetCache();
        }

        private void OnClientDisconnected()
        {
            State = DataProviderState.NotReady;
            ResetCache();
        }

        private void OnMessageReceived(BinaryMessage message)
        {
        }

        private void ResetCache()
        {
            cachedBufferSizes = Array.Empty<uint>();
            cachedSamplingRates = Array.Empty<uint>();
            cachedChannelInfos = Array.Empty<IChannelInfo>();

            selectedBufferSize = 0;
            selectedSamplingRate = 0;
            selectedChannelInfo = null;
        }
    }
}
