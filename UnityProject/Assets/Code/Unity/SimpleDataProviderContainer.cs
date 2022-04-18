using CTProject.Examples;
using CTProject.Infrastructure;
using UnityEngine;

namespace CTProject.Unity
{
    public class SimpleDataProviderContainer : MonoBehaviour, IDataProviderContainer
    {
        #region properties

        public string DataProviderName => "Simple Data Provider";

        #endregion properties

        #region fields

        // set from Unity
        [SerializeField]
        private DependencyProvider DependencyProvider;

        private SimpleDataProvider dataProvider;

        #endregion fields

        #region IDataProviderContainer

        public IDataProvider GetDataProvider()
        {
            if (dataProvider == null)
                PrepareDataProvider();

            return dataProvider;
        }

        #endregion IDataProviderContainer

        #region private methods

        private void PrepareDataProvider()
        {
            dataProvider = new SimpleDataProvider();

            dataProvider.LoadDependencies(DependencyProvider);
            dataProvider.Initialize();
        }

        #endregion private methods
    }
}
