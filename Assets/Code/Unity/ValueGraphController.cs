using CTProject.Infrastructure;
using CTProject.Unity.Graph;
using UnityEngine;

namespace CTProject.Unity
{
    [RequireComponent(typeof(GraphPresenter))]
    public class ValueGraphController : MonoBehaviour, IDataConsumer

    {
        #region fields

        private GraphPresenter graphPresenter;

        #endregion fields

        #region Unity calls

        private void Awake()
        {
            graphPresenter = GetComponent<GraphPresenter>();
        }

        #endregion Unity calls

        #region IDataConsumer

        public void DataStreamEnded()
        {
        }

        public void DataStreamStarted(long tickCountOnStreamStart)
        {
            graphPresenter.Clear();
        }

        public void OnSettingsChange(IDataProvider source)
        {
        }

        public void ReceiveData(ulong index, float[] data)
        {
            graphPresenter.InsertData((int)index, data);
        }

        public void ResetIndex()
        {
            graphPresenter.Clear();
        }

        #endregion IDataConsumer
    }
}
