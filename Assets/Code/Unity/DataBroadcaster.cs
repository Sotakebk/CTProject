using CTProject.Infrastructure;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;

namespace CTProject.Unity
{
    public class DataBroadcaster : MonoBehaviour, IDataConsumer
    {
        private class Message
        {
        }

        private class OnSettingsChangeMessage : Message
        {
            public IDataProvider Source;
        }

        private class ReceiveDataMessage : Message
        {
            public ulong Index;
            public float[] Data;
        }

        private class ResetIndexMessage : Message
        {
        }

        // set from Unity
        public MonoBehaviour[] RegisterDataConsumers;

        private IDataConsumer[] dataConsumers;

        private ConcurrentQueue<Message> messageQueue;

        private void Awake()
        {
            dataConsumers = RegisterDataConsumers.OfType<IDataConsumer>().ToArray();
        }

        private void Start()
        {
            messageQueue = new ConcurrentQueue<Message>();
            dataConsumers ??= new IDataConsumer[0];
        }

        private void Update()
        {
            ProcessMessages();
        }

        private void ProcessMessages()
        {
            int i = 5;
            while (i > 0 && messageQueue.Count > 0)
            {
                if (!messageQueue.TryDequeue(out var message))
                    continue;

                if (message is OnSettingsChangeMessage settingsChange)
                    PropagateOnSettingsChangeMessage(settingsChange);
                else if (message is ReceiveDataMessage receiveData)
                    PropagateReceiveDataMessage(receiveData);
                else if (message is ResetIndexMessage resetIndexMessage)
                    PropagateResetIndexMessage(resetIndexMessage);
            }
        }

        private void PropagateOnSettingsChangeMessage(OnSettingsChangeMessage arg)
        {
            foreach (var dc in dataConsumers)
                dc.OnSettingsChange(arg.Source);
        }

        private void PropagateReceiveDataMessage(ReceiveDataMessage arg)
        {
            foreach (var dc in dataConsumers)
                dc.ReceiveData(arg.Index, arg.Data);
        }

        private void PropagateResetIndexMessage(ResetIndexMessage arg)
        {
            foreach (var dc in dataConsumers)
                dc.ResetIndex();
        }

        #region IDataConsumer

        public void OnSettingsChange(IDataProvider source)
        {
            messageQueue.Enqueue(new OnSettingsChangeMessage() { Source = source });
        }

        public void ReceiveData(ulong index, float[] data)
        {
            messageQueue.Enqueue(new ReceiveDataMessage() { Index = index, Data = data });
        }

        public void ResetIndex()
        {
            messageQueue.Enqueue(new ResetIndexMessage());
        }

        #endregion IDataConsumer
    }
}
