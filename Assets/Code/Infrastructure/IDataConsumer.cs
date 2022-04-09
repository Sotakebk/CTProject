namespace CTProject.Infrastructure
{
    public interface IDataConsumer
    {
        /// <summary>
        /// Call if any of the standard IDataProvider settings are changed.
        /// </summary>
        /// <param name="source">IDataProvider of which settings have changed.</param>
        void OnSettingsChange(IDataProvider source);

        /// <summary>
        /// Receive an array of floats.
        /// </summary>
        /// <param name="index">Index of underlying iterator at start of the data block.</param>
        /// <param name="data">Not guaranteed to not be modified. Expected of length BufferSize</param>
        void ReceiveData(ulong index, float[] data);

        /// <summary>
        /// Call when a new data stream is opened.
        /// </summary>
        /// <param name="tickCountOnStreamStart">Stopwatch.GetTimestamp() value at start of stream.</param>
        void DataStreamStarted(long tickCountOnStreamStart);

        /// <summary>
        /// Call when a new data stream is closed. No more ReceiveData calls are expected after this.
        /// </summary>
        void DataStreamEnded();
    }
}
