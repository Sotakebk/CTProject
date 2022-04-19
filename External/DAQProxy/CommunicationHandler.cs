using CTProject.DataAcquisition;
using CTProject.DataAcquisition.Communication;
using CTProject.Infrastructure;
using DAQProxy.Services;

namespace DAQProxy
{
    public class CommunicationHandler : IDependencyConsumer
    {
        private ILoggingService loggingService;
        private DeviceHandler deviceHandler;

        private TCPClient client;

        public CommunicationHandler()
        {
            client = new TCPClient(DefaultAddress.Address, DefaultAddress.Port);

            client.OnDisconnected = OnDisconnected;
            client.OnConnected = OnConnected;
            client.OnMessageReceived = OnMessageReceived;
        }

        public void LoadDependencies(IDependencyProvider dependencyProvider)
        {
            client.LoadDependencies(dependencyProvider);
            loggingService = dependencyProvider.GetDependency<ILoggingService>();
            deviceHandler = dependencyProvider.GetDependency<DeviceHandler>();
        }

        public void Start()
        {
            client.Start();
        }

        public void Stop()
        {
            client.Stop();
            InnerStop();
        }

        private void OnDisconnected()
        {
            InnerStop();
        }

        private void OnConnected()
        {
            deviceHandler.Prepare();
            SendEntireConfiguration();
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
                    OnMessageRequestAvailableBufferSizes();
                    return;

                case MessageTypeDefinition.BufferSizeSet:
                    OnMessageSetBufferSize(message);
                    return;

                case MessageTypeDefinition.ChannelInfo:
                    OnMessageRequestAvailableChannels();
                    return;

                case MessageTypeDefinition.ChannelSet:
                    OnMessageSetChannel(message);
                    return;

                case MessageTypeDefinition.SamplingRateInfo:
                    OnMessageRequestAvailableSamplingRates();
                    return;

                case MessageTypeDefinition.SamplingRateSet:
                    OnMessageSetSamplingRate(message);
                    return;

                default:
                    loggingService?.Log(LogLevel.Warning, $"Unknown message: {message.Type}");
                    return;
            }
        }

        private void SendEntireConfiguration()
        {
            SendAvailableChannels();
            SendAvailableBufferSizes();
            SendAvailableSamplingRates();
            SendSelectedChannel();
            SendSelectedBufferSize();
            SendSelectedSamplingRate();
        }

        private void SendSelectedSamplingRate()
        {
            var msg = new IntMessage(MessageTypeDefinition.SamplingRateSet, deviceHandler.SelectedSamplingRate);
            client.PushMessage(msg);
        }

        private void SendSelectedBufferSize()
        {
            var msg = new IntMessage(MessageTypeDefinition.BufferSizeSet, deviceHandler.SelectedBufferSize);
            client.PushMessage(msg);
        }

        private void SendSelectedChannel()
        {
            var msg = new StringMessage(MessageTypeDefinition.ChannelSet, deviceHandler.SelectedChannel);
            client.PushMessage(msg);
        }

        private void SendAvailableSamplingRates()
        {
            var msg = new IntArrayMessage(MessageTypeDefinition.SamplingRateInfo, deviceHandler.GetPossibleSamplingRates());
            client.PushMessage(msg);
        }

        private void SendAvailableBufferSizes()
        {
            var msg = new IntArrayMessage(MessageTypeDefinition.BufferSizeInfo, deviceHandler.GetPossibleBufferSizes());
            client.PushMessage(msg);
        }

        private void SendAvailableChannels()
        {
            var msg = new StringArrayMessage(MessageTypeDefinition.ChannelInfo, deviceHandler.GetPossibleChannels());
            client.PushMessage(msg);
        }

        public void SendMessageStarted()
        {
            var msg = new EmptyMessage(MessageTypeDefinition.Start);
            client.PushMessage(msg);
        }

        public void SendMessageDataBuffer(float[] buffer, int index)
        {
            var msg = new DataBufferMessage(MessageTypeDefinition.DataPacket, index, buffer);
            client.PushMessage(msg);
        }

        public void SendMessageStopped()
        {
            var msg = new EmptyMessage(MessageTypeDefinition.Stop);
            client.PushMessage(msg);
        }

        private void OnMessageStart()
        {
            if (deviceHandler.IsWorking)
                InnerStop();

            InnerStart();
        }

        private void OnMessageStop()
        {
            InnerStop();
        }

        private void InnerStart()
        {
            deviceHandler.Start();
        }

        private void InnerStop()
        {
            deviceHandler.Stop();
        }

        private void OnMessageRequestAvailableBufferSizes()
        {
            SendAvailableBufferSizes();
        }

        private void OnMessageSetBufferSize(BinaryMessage msg)
        {
            var value = MessageFactory.GetMessageFromBinary<IntMessage>(msg).MessageContent;
            deviceHandler.SelectedBufferSize = value;
            SendSelectedBufferSize();
        }

        private void OnMessageRequestAvailableSamplingRates()
        {
            SendAvailableSamplingRates();
        }

        private void OnMessageSetSamplingRate(BinaryMessage msg)
        {
            var value = MessageFactory.GetMessageFromBinary<IntMessage>(msg).MessageContent;
            deviceHandler.SelectedSamplingRate = value;
            SendSelectedSamplingRate();
        }

        private void OnMessageRequestAvailableChannels()
        {
            SendAvailableChannels();
        }

        private void OnMessageSetChannel(BinaryMessage msg)
        {
            var value = MessageFactory.GetMessageFromBinary<StringMessage>(msg).MessageContent;
            deviceHandler.SelectedChannel = value;
            SendSelectedChannel();
        }
    }
}
