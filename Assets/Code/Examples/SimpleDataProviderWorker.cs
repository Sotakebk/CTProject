using CTProject.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace CTProject.Examples
{
    public class SimpleDataProviderWorker
    {
        private enum Message : int
        {
            Default = 0,
            Stop
        }

        #region fields

        public bool IsWorking { get; private set; }

        #endregion fields

        #region Per worker data

        private readonly uint samplingRate;
        private readonly uint bufferSize;
        private readonly Func<int, uint, float> dataSourceLogic;
        private readonly IDataConsumer consumer;
        private readonly SimpleDataProvider owner;
        private readonly ConcurrentQueue<Message> messages;

        #endregion Per worker data

        #region per run data

        private Stopwatch stopwatch;
        private volatile int index;
        private Thread WorkerThread;
        private object _lock;

        #endregion per run data

        #region constructor

        public SimpleDataProviderWorker(SimpleDataProvider owner, uint samplingRate, uint bufferSize, Func<int, uint, float> dataSourceLogic, IDataConsumer consumer)
        {
            this.samplingRate = samplingRate;
            this.bufferSize = bufferSize;
            this.dataSourceLogic = dataSourceLogic;
            this.consumer = consumer;
            this.owner = owner;

            messages = new ConcurrentQueue<Message>();
            index = 0;
            _lock = new object();
        }

        #endregion constructor

        #region public methods

        public void Start()
        {
            lock (_lock)
            {
                if (WorkerThread != null)
                    return;

                WorkerThread = new Thread(new ThreadStart(Task));
                WorkerThread.Start();
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (!WorkerThread.IsAlive)
                    return;

                messages.Enqueue(Message.Stop);
                WorkerThread.Join();
            }
        }

        #endregion public methods

        #region worker logic

        private void Task()
        {
            IsWorking = true;
            stopwatch = new Stopwatch();
            stopwatch.Start();
            var timestamp = Stopwatch.GetTimestamp();
            owner.OnWorkerStart();
            consumer.DataStreamStarted(timestamp);
            while (index < 32000000)
            {
                while (!messages.IsEmpty)
                {
                    if (!messages.TryDequeue(out Message result))
                        continue;
                    if (result == Message.Default)
                        continue;
                    if (result == Message.Stop)
                    {
                        Finish();
                        return;
                    }
                }

                if (ShouldGenerateDataNow())
                    GenerateData();
                else
                    WaitForNextBlock();
            }
            Finish();
        }

        private void Finish()
        {
            IsWorking = false;
            consumer.DataStreamEnded();
            owner.OnWorkerStop();
        }

        private bool ShouldGenerateDataNow()
        {
            return stopwatch.Elapsed.TotalSeconds > ((index + bufferSize) / (double)samplingRate);
        }

        private void GenerateData()
        {
            float[] arr = new float[bufferSize];

            for (int x = 0; x < bufferSize; x++)
            {
                arr[x] = dataSourceLogic(index + x, samplingRate);
            }

            consumer?.ReceiveData((ulong)index, arr);

            index += (int)bufferSize;
        }

        private void WaitForNextBlock()
        {
            var currentTime = stopwatch.Elapsed.TotalSeconds;
            var timeForNextBuffer = (index + bufferSize) / (double)samplingRate;
            var quarterBuffer = bufferSize / (samplingRate * 4.0); // 1/4 of buffer
            var adjustedForNextBuffer = timeForNextBuffer - currentTime + (bufferSize / (samplingRate * 10.0)); // leftover time + 1/10 of buffer
            var saneValue = 0.5;

            var timeToWait = Math.Min(
                Math.Min(timeForNextBuffer, quarterBuffer),
                Math.Min(adjustedForNextBuffer, saneValue)
                );

            Thread.Sleep((int)(timeToWait * 1000.0));
        }

        #endregion worker logic
    }
}
