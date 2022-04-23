using CTProject.DataAcquisition;
using CTProject.Infrastructure;
using UnityEngine;

namespace CTProject.Unity
{
    public class DAQDataProviderContainer : MonoBehaviour, IDataProviderContainer
    {
        #region properties

        public string DataProviderName => "niDAQ Data Provider";

        public short Port
        {
            get => port;
            set
            {
                port = value;
                dataProvider?.ChangePort(port);
            }
        }

        #endregion properties

        #region fields

        // set from Unity
        [SerializeField]
        private DependencyProvider DependencyProvider;

        private DeviceDataProvider dataProvider;

        private short port = DefaultAddress.Port;

        #endregion fields

        #region IDataProviderContainer

        public IDataProvider GetDataProvider()
        {
            if (dataProvider == null)
                PrepareDataProvider();

            return dataProvider;
        }

        #endregion IDataProviderContainer

        #region Unity calls

        private void OnDestroy()
        {
            // called when the play mode is stopped
            dataProvider?.AbortTCPThread();
        }

        #endregion Unity calls

        #region private methods

        private void PrepareDataProvider()
        {
            dataProvider = new DeviceDataProvider(Port);

            dataProvider.LoadDependencies(DependencyProvider);
            dataProvider.Initialize();
        }

        #endregion private methods
    }
}
