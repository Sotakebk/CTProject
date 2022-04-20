using CTProject.DataAcquisition.Communication;
using CTProject.Infrastructure;
using System;
using System.Linq;

namespace CTProject.DataAcquisition
{
    public class DeviceDataProvider : IDependencyConsumer, IDataProvider
    {
        private class SimpleChannelInfo : IChannelInfo
        {
            public string UniqueName { get; private set; }

            public SimpleChannelInfo(string uniqueName) => UniqueName = uniqueName;
        }

        #region fields

        private ILoggingService loggingService;

        private TCPServer server;

        private uint[] cachedBufferSizes;
        private uint[] cachedSamplingRates;
        private IChannelInfo[] cachedChannelInfos;

        private uint selectedBufferSize;
        private uint selectedSamplingRate;
        private IChannelInfo selectedChannelInfo;

        private IDataConsumer consumer;

        private bool isRemoteRunning;

        #endregion fields

        #region properties

        public IChannelInfo SelectedChannel
        {
            get => selectedChannelInfo;
            set
            {
                selectedChannelInfo = value;
                Stop();
                SendSelectedChannel();
                consumer?.OnSettingsChange(this);
            }
        }

        public uint SelectedSamplingRate
        {
            get => selectedSamplingRate;
            set
            {
                selectedSamplingRate = value;
                Stop();
                SendSelectedSamplingRate();
                consumer?.OnSettingsChange(this);
            }
        }

        public uint SelectedBufferSize
        {
            get => selectedSamplingRate;
            set
            {
                selectedSamplingRate = value;
                Stop();
                SendSelectedBufferSize();
                consumer?.OnSettingsChange(this);
            }
        }

        public uint[] GetAvailableBufferSizes() => cachedBufferSizes;

        public IChannelInfo[] GetAvailableChannels() => cachedChannelInfos;

        public uint[] GetAvailableSamplingRates() => cachedSamplingRates;

        #endregion properties

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
            loggingService = dependencyProvider.GetDependency<ILoggingService>();
            server.LoadDependencies(dependencyProvider);
        }

        #endregion IDependencyConsumer

        #region IDataProvider

        public void Start()
        {
            isRemoteRunning = true;
            SendStart();
        }

        public void Stop()
        {
            isRemoteRunning = false;
            SendStop();
        }

        public void Subscribe(IDataConsumer consumer)
        {
            this.consumer = consumer;
            Stop();
        }

        public DataProviderState State { get; private set; }

        public float GetMaxValue() => 10f;

        public float GetMinValue() => 0f;

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
            if (isRemoteRunning)
            {
                isRemoteRunning = false;
                consumer?.DataStreamEnded();
            }
            ResetCache();
        }

        private void ResetCache()
        {
            cachedBufferSizes = Array.Empty<uint>();
            cachedSamplingRates = Array.Empty<uint>();
            cachedChannelInfos = Array.Empty<IChannelInfo>();

            selectedBufferSize = 0;
            selectedSamplingRate = 0;
            selectedChannelInfo = null;
            isRemoteRunning = false;
        }

        private void OnMessageReceived(BinaryMessage message)
        {
            switch (message.Type)
            {
                case MessageTypeDefinition.Start:
                    OnMessageStart();
                    return;

                case MessageTypeDefinition.Stop:
                    OnMessageStop();
                    return;

                case MessageTypeDefinition.BufferSizeInfo:
                    OnMessageReceiveBufferSizeInfo(message);
                    return;

                case MessageTypeDefinition.ChannelInfo:
                    OnMessageReceiveSamplingRateInfo(message);
                    return;

                case MessageTypeDefinition.SamplingRateInfo:
                    OnMessageReceiveChannelInfo(message);
                    return;

                case MessageTypeDefinition.BufferSizeSet:
                    OnMessageSetBufferSize(message);
                    return;

                case MessageTypeDefinition.ChannelSet:
                    OnMessageSetChannel(message);
                    return;

                case MessageTypeDefinition.SamplingRateSet:
                    OnMessageSetSamplingRate(message);
                    return;

                case MessageTypeDefinition.DataPacket:
                    OnMessageDataPacket(message);
                    return;

                default:
                    loggingService?.Log(LogLevel.Warning, $"Unknown message: {message.Type}");
                    return;
            }
        }

        private void SendSelectedSamplingRate()
        {
            var msg = new IntMessage(MessageTypeDefinition.SamplingRateSet, (int)selectedSamplingRate);
            server.PushMessage(msg);
        }

        private void SendSelectedBufferSize()
        {
            var msg = new IntMessage(MessageTypeDefinition.BufferSizeSet, (int)selectedBufferSize);
            server.PushMessage(msg);
        }

        private void SendSelectedChannel()
        {
            var msg = new StringMessage(MessageTypeDefinition.ChannelSet, selectedChannelInfo?.UniqueName);
            server.PushMessage(msg);
        }

        private void SendRequestSamplingRates()
        {
            var msg = new EmptyMessage(MessageTypeDefinition.SamplingRateInfo);
            server.PushMessage(msg);
        }

        private void SendRequestBufferSizes()
        {
            var msg = new EmptyMessage(MessageTypeDefinition.BufferSizeInfo);
            server.PushMessage(msg);
        }

        private void SendRequestChannels()
        {
            var msg = new EmptyMessage(MessageTypeDefinition.ChannelInfo);
            server.PushMessage(msg);
        }

        public void SendStart()
        {
            var msg = new EmptyMessage(MessageTypeDefinition.Start);
            server.PushMessage(msg);
        }

        public void SendStop()
        {
            var msg = new EmptyMessage(MessageTypeDefinition.Stop);
            server.PushMessage(msg);
        }

        private void OnMessageStart()
        {
            isRemoteRunning = true;
            consumer?.DataStreamStarted(0);
        }

        private void OnMessageStop()
        {
            isRemoteRunning = false;
            consumer.DataStreamEnded();
        }

        private void OnMessageReceiveBufferSizeInfo(BinaryMessage message)
        {
            var msg = MessageFactory.GetMessageFromBinary<IntArrayMessage>(message);
            cachedBufferSizes = msg.MessageContent.Cast<uint>().ToArray();
        }

        private void OnMessageReceiveSamplingRateInfo(BinaryMessage message)
        {
            var msg = MessageFactory.GetMessageFromBinary<IntArrayMessage>(message);
            cachedSamplingRates = msg.MessageContent.Cast<uint>().ToArray();
        }

        private void OnMessageReceiveChannelInfo(BinaryMessage message)
        {
            var msg = MessageFactory.GetMessageFromBinary<StringArrayMessage>(message);
            cachedChannelInfos = msg.MessageContent.Select(i => new SimpleChannelInfo(i)).ToArray();
            consumer?.OnSettingsChange(this);
        }

        private void OnMessageSetBufferSize(BinaryMessage message)
        {
            var msg = MessageFactory.GetMessageFromBinary<IntMessage>(message);
            selectedBufferSize = (uint)msg.MessageContent;
        }

        private void OnMessageSetSamplingRate(BinaryMessage message)
        {
            var msg = MessageFactory.GetMessageFromBinary<IntMessage>(message);
            selectedSamplingRate = (uint)msg.MessageContent;
            SendSelectedSamplingRate();
        }

        private void OnMessageSetChannel(BinaryMessage message)
        {
            var msg = MessageFactory.GetMessageFromBinary<StringMessage>(message);
            selectedChannelInfo = cachedChannelInfos.FirstOrDefault(c => c.UniqueName == msg.MessageContent);
            consumer?.OnSettingsChange(this);
        }

        private void OnMessageDataPacket(BinaryMessage message)
        {
            var msg = MessageFactory.GetMessageFromBinary<DataBufferMessage>(message);
            consumer?.ReceiveData((ulong)msg.MessageContentIndex, msg.MessageContentData);
        }
    }
}
