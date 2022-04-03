using CTProject.Infrastructure;
using UnityEngine;

namespace CTProject.Unity
{
    public class DataConsumer : MonoBehaviour, IDataConsumer
    {
        public void OnSettingsChange(IDataProvider source) => throw new System.NotImplementedException();

        public void ReceiveData(ulong index, float[] data) => throw new System.NotImplementedException();

        public void ResetIndex() => throw new System.NotImplementedException();
    }
}
