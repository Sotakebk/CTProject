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
            Default,
            Stop
        }

        public bool IsWorking => WorkerThread?.IsAlive ?? false;

        private readonly uint samplingRate;
        private readonly uint bufferSize;
        private readonly Func<int, uint, float> dataSourceLogic;
        private readonly IDataConsumer consumer;
        private readonly SimpleDataProvider owner;

        private Stopwatch stopwatch;
        private ConcurrentQueue<Message> messages;
        private volatile int index;
        private Thread WorkerThread;
        private object _lock;

        public SimpleDataProviderWorker(SimpleDataProvider owner, uint samplingRate, uint bufferSize, Func<int, uint, float> dataSourceLogic, IDataConsumer consumer)
        {
            this.samplingRate = samplingRate;
            this.bufferSize = bufferSize;
            this.dataSourceLogic = dataSourceLogic;
            this.consumer = consumer;
            this.owner = owner;

            messages = new ConcurrentQueue<Message>();
            index = 0;
            WorkerThread = new Thread(new ThreadStart(Task));

            _lock = new object();
        }

        public void Start()
        {
            lock (_lock)
            {
                if (WorkerThread.IsAlive)
                    throw new InvalidOperationException();

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

        private void Task()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            var timestamp = Stopwatch.GetTimestamp();
            owner.OnWorkerStart();
            consumer.DataStreamStarted(timestamp);

            while (true)
            {
                while (!messages.IsEmpty)
                {
                    if (!messages.TryDequeue(out Message result))
                        continue;
                    if (result == Message.Default)
                        continue;
                    if (result == Message.Stop)
                    {
                        owner.OnWorkerStop();
                        consumer.DataStreamEnded();
                        return;
                    }
                }

                if (ShouldGenerateDataNow())
                    GenerateData();

                WaitForNextBlock();
            }
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
            var secondsToWait = bufferSize / (double)(samplingRate * 10); // 1/10 of seconds per buffer
            secondsToWait = Math.Min(secondsToWait, 0.01666666666f);
            secondsToWait = Math.Min(secondsToWait, Math.Abs(stopwatch.Elapsed.TotalSeconds - ((index + bufferSize) / (double)samplingRate)));
            Thread.Sleep((int)(secondsToWait / 1000));
        }
    }
}
