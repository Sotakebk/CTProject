using CTProject.Infrastructure;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CTProject.DataAcquisition.Communication
{
    public class TCPClient : TCPBase, IDisposable
    {
        protected override bool IsConnected => Client?.Connected ?? false;

        protected override NetworkStream NetworkStream => Client?.GetStream();

        protected TcpClient Client { get; set; }

        protected IPAddress address;
        protected int port;

        public TCPClient(IPAddress address, int port) : base()
        {
            this.address = address;
            this.port = port;
        }

        protected override void Connect()
        {
            try
            {
                Client?.Dispose();
                Client = new TcpClient();
                LoggingService?.Log(LogLevel.Info, $"Trying to connect to {address}:{port}");
                Client.Connect(address, port);
                base.Connect();
            }
            catch (SocketException ex)
            {
                LoggingService?.Log(LogLevel.Info, ex.Message);
                Thread.Sleep(1000);
            }
        }

        public void Dispose()
        {
            Client?.Dispose();
            Client = null;
        }

        protected override void WorkerStop()
        {
            Client?.Dispose();
        }

        ~TCPClient()
        {
            Dispose();
        }

        protected override void Reset()
        {
            base.Reset();
            Client?.Close();
            Client = null;
        }
    }
}
