using CTProject.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;

namespace CTProject.Unity
{
    public class DataBroadcaster : MonoBehaviour, IDataConsumer
    {
        #region properties

        public IDataProvider DataProvider
        {
            get
            {
                return _dataProvider;
            }
            set
            {
                _dataProvider?.Stop();
                _dataProvider?.Subscribe(null);
                _dataProvider = value;
                _dataProvider?.Subscribe(this);
            }
        }

        #endregion properties

        #region fields

        // set from Unity
        [SerializeField]
        private MonoBehaviour[] RegisterDataConsumers;

        private IDataConsumer[] dataConsumers;

        private ConcurrentQueue<Action> actionQueue;
        private IDataProvider _dataProvider;

        #endregion fields

        #region Unity calls

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

        #endregion Unity calls

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

        #region private methods

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

        #endregion private methods
    }
}
