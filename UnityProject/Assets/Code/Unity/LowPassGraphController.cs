using CTProject.Infrastructure;
using CTProject.Unity.Graph;
using UnityEngine;

namespace CTProject.Unity
{
    [RequireComponent(typeof(GraphPresenter))]
    public class LowPassGraphController : MonoBehaviour, IDataConsumer
    {
        #region fields

        [SerializeField]
        private GraphicsService graphicsService;

        private GraphPresenter graphPresenter;
        private float lastValue;

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
            lastValue = 0;
        }

        public void OnSettingsChange(IDataProvider source)
        {
        }

        public void ReceiveData(ulong index, float[] data)
        {
            var newBuffer = new float[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                newBuffer[i] = (data[i] + lastValue) * 0.5f;
                lastValue = data[i];
            }

            graphPresenter.InsertData((int)index, newBuffer);
        }

        #endregion IDataConsumer
    }
}
