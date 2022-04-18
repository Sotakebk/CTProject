namespace CTProject.Infrastructure
{
    public enum DataProviderState
    {
        /// <summary>
        /// Data provider was not prepared, call Initialize().
        /// </summary>
        Uninitialized,

        /// <summary>
        /// Data provider prepared, but for some reason not ready to work.
        /// </summary>
        NotReady,

        /// <summary>
        /// Data provider ready to work.
        /// </summary>
        Ready,

        /// <summary>
        /// Data provider busy, already pumping messages.
        /// </summary>
        Working,

        /// <summary>
        /// Data provider in wrong state, recovery impossible.
        /// </summary>
        Error
    }

    public interface IDataProvider
    {
        /// <summary>
        /// The state of this IDataProvider.
        /// </summary>
        DataProviderState State { get; }

        /// <summary>
        /// Get available channels. Value is expected to be prepared and not change after Initialize().
        /// </summary>
        /// <returns></returns>
        IChannelInfo[] GetAvailableChannels();

        IChannelInfo SelectedChannel { get; set; }

        /// <summary>
        /// Get available sampling rates. Value is expected to be prepared and not change after Initialize().
        /// </summary>
        /// <returns></returns>
        uint[] GetAvailableSamplingRates();

        uint SelectedSamplingRate { get; set; }

        /// <summary>
        /// Get available buffer sizes. Value is expected to be prepared and not change after Initialize().
        /// </summary>
        /// <returns></returns>
        uint[] GetAvailableBufferSizes();

        uint SelectedBufferSize { get; set; }

        /// <summary>
        /// Smallest expected value in the buffer sent in ReceiveData calls as a float.
        /// </summary>
        /// <returns></returns>
        float GetMinValue();

        /// <summary>
        /// Greatest expected value in the buffer sent in ReceiveData calls as a float.
        /// </summary>
        /// <returns></returns>
        float GetMaxValue();

        /// <summary>
        /// Set, or change data calls receiver. Can be called with null to clear, after which no more calls are expected.
        /// </summary>
        /// <param name="consumer"></param>
        void Subscribe(IDataConsumer consumer);

        /// <summary>
        /// Initialize the source, device or anything else.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Start the data stream. If stream is already opened, don't start another.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the data stream. Ignore if there is no stream open.
        /// </summary>
        void Stop();
    }
}
