using CTProject.Infrastructure;
using System;
using System.Net;
using System.Net.Sockets;

namespace CTProject.DataAcquisition.Communication
{
    public class TCPServer : TCPBase
    {
        protected override bool IsConnected => CurrentClient?.Connected ?? false;

        protected override NetworkStream NetworkStream => CurrentClient?.GetStream();

        protected TcpListener Server { get; set; }
        protected TcpClient CurrentClient { get; set; }

        protected IPAddress address;
        protected int port;

        public TCPServer(IPAddress address, int port) : base()
        {
            this.address = address;
            this.port = port;

            Server = new TcpListener(address, port);
        }

        protected override void Connect()
        {
            try
            {
                DateTime until = DateTime.Now.AddSeconds(10);
                Server.Start();
                LoggingService?.Log(LogLevel.Info, $"Listening on {address}:{port}");
                while (DateTime.Now < until)
                {
                    if (Server.Pending())
                    {
                        CurrentClient?.Dispose();
                        CurrentClient = Server.AcceptTcpClient();
                        LoggingService?.Log(LogLevel.Info, $"Client connected: {CurrentClient.Client.RemoteEndPoint}");
                        base.Connect();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService?.Log(LogLevel.Info, $"Exception when listening for clients on {address}:{port} {ex}");
            }
            finally
            {
                Server.Stop();
            }
        }

        public void Dispose()
        {
            Server.Stop();
            CurrentClient?.Dispose();
            CurrentClient = null;
        }

        protected override void WorkerStop()
        {
            Server.Stop();
            CurrentClient?.Dispose();
        }

        protected override void Reset()
        {
            base.Reset();
            CurrentClient?.Close();
            CurrentClient = null;
        }

        ~TCPServer()
        {
            Dispose();
        }
    }
}
