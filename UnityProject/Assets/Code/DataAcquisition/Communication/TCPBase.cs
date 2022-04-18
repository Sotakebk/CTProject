using CTProject.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace CTProject.DataAcquisition.Communication
{
    public abstract class TCPBase : IDependencyConsumer
    {
        protected ILoggingService LoggingService;

        public bool IsOpen { get; set; }
        protected abstract bool IsConnected { get; }

        protected abstract NetworkStream NetworkStream { get; }
        protected Thread WorkerThread { get; set; }

        protected readonly object _lock;

        protected DateTime lastMessageDate;

        protected TCPBase()
        {
            toSendQueue = new ConcurrentQueue<BinaryMessage>();

            _lock = new object();
            IsOpen = true;
        }

        public virtual void LoadDependencies(IDependencyProvider dependencyProvider)
        {
            LoggingService = dependencyProvider.GetDependency<ILoggingService>();
        }

        protected ConcurrentQueue<BinaryMessage> toSendQueue;
        public Action<BinaryMessage> OnMessageReceived { get; set; }
        public Action OnConnected { get; set; }
        public Action OnDisconnected { get; set; }

        public virtual void Start()
        {
            lock (_lock)
            {
                if (WorkerThread == null || !WorkerThread.IsAlive)
                {
                    WorkerThread = new Thread(new ThreadStart(DoWork));
                    WorkerThread.Start();
                }
            }
        }

        public virtual void Stop()
        {
            lock (_lock)
            {
                IsOpen = false;
            }
        }

        protected void DoWork()
        {
            LoggingService?.Log(LogLevel.Info, $"TCP worker starting!");
            lastMessageDate = DateTime.Now;
            while (IsOpen)
            {
                try
                {
                    if (!IsConnected)
                        Connect();
                    else
                    {
                        Work();

                        if (lastMessageDate.AddSeconds(15) < DateTime.Now)
                            throw new TimeoutException();
                    }
                }
                catch (Exception ex)
                {
                    LoggingService?.Log(LogLevel.Warning, $"TCP worker resetting, cause: {ex.Message}");
                    Reset();
                    OnDisconnected?.Invoke();
                }

                Thread.Sleep(5);
            }
            WorkerStop();
            OnDisconnected?.Invoke();
            LoggingService?.Log(LogLevel.Info, $"TCP worker stopped.");
        }

        protected abstract void WorkerStop();

        protected virtual void Connect()
        {
            lastMessageDate = DateTime.Now;
            OnConnected?.Invoke();
        }

        protected virtual void Work()
        {
            bool didSomeWork = true;
            while (didSomeWork)
            {
                didSomeWork = false;

                if (!toSendQueue.IsEmpty)
                {
                    if (toSendQueue.TryDequeue(out var message))
                    {
                        SendMessage(message);
                        didSomeWork = true;
                    }
                }
                if (NetworkStream != null && NetworkStream.DataAvailable)
                {
                    var message = ReceiveMessage();
                    if (message.Type >= 0)
                    {
                        OnMessageReceived?.Invoke(message);
                        didSomeWork = true;
                    }
                }

                if (lastMessageDate.AddSeconds(5) < DateTime.Now)
                {
                    PushMessage(BinaryMessage.Empty);
                }

                if (didSomeWork)
                    lastMessageDate = DateTime.Now;
                else
                    return;
            }
        }

        protected virtual void Reset()
        {
            toSendQueue = new ConcurrentQueue<BinaryMessage>();
        }

        public void PushMessage(BinaryMessage message)
        {
            toSendQueue.Enqueue(message);
        }

        protected void SendMessage(BinaryMessage message)
        {
            var data = message.Serialize();
            int length = data.Length;
            NetworkWriteBytes(BitConverter.GetBytes(length));
            NetworkWriteBytes(data);
        }

        protected BinaryMessage ReceiveMessage()
        {
            int length = BitConverter.ToInt32(NetworkReadBytes(sizeof(int)), 0);
            var data = NetworkReadBytes(length);
            return new BinaryMessage(data);
        }

        protected void NetworkWriteBytes(byte[] array, int mspb = -1)
        {
            if (NetworkStream == null)
                return;

            NetworkStream.WriteTimeout = GetTimeout(mspb, array.Length);

            NetworkStream.Write(array, 0, array.Length);
        }

        protected byte[] NetworkReadBytes(int length, int mspb = -1)
        {
            if (NetworkStream == null || length <= 0)
                return null;

            NetworkStream.ReadTimeout = GetTimeout(mspb, length);

            int counter = 0;
            var array = new byte[length];
            DateTime endDateTime = DateTime.Now.AddSeconds(10); // wait max. 10 seconds for a message to complete

            while (counter < length && DateTime.Now < endDateTime)
            {
                if (NetworkStream.DataAvailable)
                    counter += NetworkStream.Read(array, counter, length - counter);

                Thread.Sleep(2);
            }
            if (counter < length)
            {
                throw new TimeoutException();
            }
            return array;
        }

        protected static int GetTimeout(int mspb, int multiplier)
        {
            if (mspb < 1)
                return Timeout.Infinite;
            else
                return mspb * multiplier;
        }
    }
}
