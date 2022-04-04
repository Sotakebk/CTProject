namespace CTProject.Infrastructure
{
    public enum DataProviderState
    {
        Uninitialized,
        Initialized,
        Working,
        Stopped,
        Error
    }

    public interface IDataProvider
    {
        DataProviderState State { get; }

        ChannelInfo[] GetAvailableChannels();

        void SetChannel(uint ChannelID);

        uint[] GetAvailableSamplingRates();

        void SetSamplingRate(uint SamplingRate);

        uint[] GetAvailableBufferSizes();

        void SetBufferSize(uint BufferSize);

        float GetMinValue();

        float GetMaxValue();

        void Subscribe(IDataConsumer consumer);

        void Initialize();

        void Start();

        void Stop();

        void Reset();
    }
}
