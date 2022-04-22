using CTProject.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace CTProject.DataAcquisition.Communication
{
    public abstract class TCPBase : IDependencyConsumer
    {
        protected ILoggingService LoggingService;

        public abstract string TCPSideName { get; }

        public bool IsOpen { get; set; }
        public abstract bool IsConnected { get; }

        protected abstract NetworkStream NetworkStream { get; }
        protected Thread WorkerThread { get; set; }

        protected readonly object _lock;

        protected DateTime lastSentMessage;
        protected DateTime lastReceivedMessage;

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
                LoggingService?.Log(LogLevel.Info, $"{TCPSideName} stopping...");
                IsOpen = false;
            }
        }

        public virtual void Abort()
        {
            LoggingService = null;
            WorkerThread?.Abort();
        }

        protected void DoWork()
        {
            LoggingService?.Log(LogLevel.Info, $"{TCPSideName} worker starting!");
            lastSentMessage = DateTime.Now;
            lastReceivedMessage = DateTime.Now;
            while (IsOpen)
            {
                try
                {
                    if (!IsConnected)
                        Connect();
                    else
                    {
                        Work();

                        if (lastReceivedMessage.AddSeconds(15) < DateTime.Now)
                            throw new TimeoutException();
                    }
                }
                catch (Exception ex) when (ex is SocketException || ex is TimeoutException || ex is IOException)
                {
                    LoggingService?.Log(LogLevel.Warning, $"{TCPSideName} resetting, cause: {ex.Message}");
                    Reset(true);
                    OnDisconnected?.Invoke();
                }
                catch (Exception ex)
                {
                    LoggingService?.Log(LogLevel.Error, $"Unexpected exception thrown on TCP thread: {ex.GetType().Name} {ex.Message}");
                    LoggingService?.Log(ex);
                    OnDisconnected?.Invoke();
                }

                Thread.Sleep(20);
            }
            WorkerStop();
            OnDisconnected?.Invoke();
            LoggingService?.Log(LogLevel.Info, $"{TCPSideName} stopped.");
        }

        protected abstract void WorkerStop();

        protected virtual void Connect()
        {
            lastSentMessage = DateTime.Now;
            lastReceivedMessage = DateTime.Now;
            Reset(false);
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
                        lastSentMessage = DateTime.Now;
                        didSomeWork = true;
                    }
                }
                if (NetworkStream != null && NetworkStream.DataAvailable)
                {
                    var message = ReceiveMessage();
                    lastReceivedMessage = DateTime.Now;
                    if (message.Type >= 0)
                    {
                        OnMessageReceived?.Invoke(message);
                        didSomeWork = true;
                    }
                }

                if (lastSentMessage.AddSeconds(5) < DateTime.Now)
                {
                    PushMessage(BinaryMessage.Empty);
                }
            }
        }

        protected virtual void Reset(bool resetTCP = true)
        {
            toSendQueue = new ConcurrentQueue<BinaryMessage>();
        }

        public void PushMessage(BinaryMessage message)
        {
            if (IsConnected)
                toSendQueue.Enqueue(message);
        }

        public void PushMessage(Message message)
        {
            PushMessage(message.Serialize());
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
            var msg = new BinaryMessage(data);
            return msg;
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
