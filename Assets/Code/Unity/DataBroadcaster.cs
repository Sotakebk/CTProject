using CTProject.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;

namespace CTProject.Unity
{
    public class DataBroadcaster : MonoBehaviour, IDataConsumer
    {
        // set from Unity
        public MonoBehaviour[] RegisterDataConsumers;

        private IDataConsumer[] dataConsumers;

        private ConcurrentQueue<Action> actionQueue;

        private void Awake()
        {
            dataConsumers = RegisterDataConsumers.OfType<IDataConsumer>().ToArray();
        }

        private void Start()
        {
            actionQueue = new ConcurrentQueue<Action>();
            dataConsumers ??= Array.Empty<IDataConsumer>();
        }

        private void Update()
        {
            ProcessMessages();
        }

        private void ProcessMessages()
        {
            int i = 5;
            while (i > 0 && actionQueue.Count > 0)
            {
                i++;

                if (!actionQueue.TryDequeue(out var action))
                    continue;

                action();
            }
        }

        private void PropagateOnSettingsChange(IDataProvider source)
        {
            foreach (var dc in dataConsumers)
                dc.OnSettingsChange(source);
        }

        private void PropagateReceiveData(ulong index, float[] data)
        {
            foreach (var dc in dataConsumers)
                dc.ReceiveData(index, data);
        }

        private void PropagateResetIndex()
        {
            foreach (var dc in dataConsumers)
                dc.ResetIndex();
        }

        private void PropagateDataStreamStarted(long tickCountOnStreamStart)
        {
            foreach (var dc in dataConsumers)
                dc.DataStreamStarted(tickCountOnStreamStart);
        }

        private void PropagateDataStreamEnded()
        {
            foreach (var dc in dataConsumers)
                dc.DataStreamEnded();
        }

        #region IDataConsumer

        public void OnSettingsChange(IDataProvider source)
        {
            actionQueue.Enqueue(() => PropagateOnSettingsChange(source));
        }

        public void ReceiveData(ulong index, float[] data)
        {
            actionQueue.Enqueue(() => PropagateReceiveData(index, data));
        }

        public void ResetIndex()
        {
            actionQueue.Enqueue(() => PropagateResetIndex());
        }

        public void DataStreamStarted(long tickCountOnStreamStart)
        {
            actionQueue.Enqueue(() => PropagateDataStreamStarted(tickCountOnStreamStart));
        }

        public void DataStreamEnded()
        {
            actionQueue.Enqueue(() => PropagateDataStreamEnded());
        }

        #endregion IDataConsumer
    }
}
