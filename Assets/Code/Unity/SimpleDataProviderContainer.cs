using CTProject.Examples;
using System.Linq;
using UnityEngine;

namespace CTProject.Unity
{
    public class SimpleDataProviderContainer : MonoBehaviour
    {
        public DependencyProvider DependencyProvider { get; set; }

        public SimpleDataProvider SimpleDataProvider { get; private set; }

        private void Start()
        {
            SimpleDataProvider = new SimpleDataProvider();

            SimpleDataProvider.LoadDependencies(DependencyProvider);

            var channel = SimpleDataProvider.GetAvailableChannels().First();
            var bufferSize = SimpleDataProvider.GetAvailableBufferSizes().Last();
            var samplingRate = SimpleDataProvider.GetAvailableSamplingRates().Last();

            SimpleDataProvider.SetChannel(channel.ID);
            SimpleDataProvider.SetBufferSize(bufferSize);
            SimpleDataProvider.SetSamplingRate(samplingRate);
        }
    }
}
