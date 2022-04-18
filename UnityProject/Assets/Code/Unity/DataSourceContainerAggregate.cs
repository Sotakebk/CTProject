using System.Linq;
using UnityEngine;

namespace CTProject.Unity
{
    public class DataSourceContainerAggregate : MonoBehaviour
    {
        #region properties

        public IDataProviderContainer[] DataProviderContainers { get; private set; }

        #endregion properties

        #region fields

        [SerializeField]
        private MonoBehaviour[] registerDataSourceContainers;

        #endregion fields

        #region Unity calls

        private void Awake()
        {
            DataProviderContainers = registerDataSourceContainers.OfType<IDataProviderContainer>().ToArray();
        }

        #endregion Unity calls
    }
}
