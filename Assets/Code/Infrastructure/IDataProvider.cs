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

        uint[] GetPossibleSamplingRates();

        void SetSamplingRate(uint SamplesPerSecond);

        uint[] GetPossibleBufferSizes();

        void GetPossibleChannels(uint BufferSize);

        float GetMinValue();

        float GetMaxValue();

        void Subscribe(IDataConsumer consumer);

        void Initialize();

        void Start();

        void Stop();

        void Reset();
    }
}
