using System;
using CTProject.Infrastructure;

namespace CTProject.Examples
{
    public class SimpleDataProvider : IDataProvider, IDependencyConsumer
    {
        public DataProviderState State { get; private set; }

        public ChannelInfo[] GetAvailableChannels() => throw new NotImplementedException();

        public float GetMaxValue() => throw new NotImplementedException();

        public float GetMinValue() => throw new NotImplementedException();

        public uint[] GetPossibleBufferSizes() => throw new NotImplementedException();

        public void GetPossibleChannels(uint BufferSize) => throw new NotImplementedException();

        public uint[] GetPossibleSamplingRates() => throw new NotImplementedException();

        public void Initialize() => throw new NotImplementedException();

        public void LoadDependencies(IDependencyProvider dependencyProvider) => throw new NotImplementedException();

        public void Reset() => throw new NotImplementedException();

        public void SetChannel(uint ChannelID) => throw new NotImplementedException();

        public void SetSamplingRate(uint SamplesPerSecond) => throw new NotImplementedException();

        public void Start() => throw new NotImplementedException();

        public void Stop() => throw new NotImplementedException();

        public void Subscribe(IDataConsumer consumer) => throw new NotImplementedException();
    }
}
