using CTProject.DataAcquisition.Communication;
using DAQProxy.Services;
using System;
using System.Net;
using System.Threading;

namespace PipeWrapperTest
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine($"Running!");
            var ip = CTProject.DataAcquisition.DefaultAddress.Address;
            var port = CTProject.DataAcquisition.DefaultAddress.Port;
            var server = new TCPServer(IPAddress.Loopback, port);
            var client = new TCPClient(IPAddress.Loopback, port);

            server.LoadDependencies(DependencyProvider.Instance);
            client.LoadDependencies(DependencyProvider.Instance);

            server.OnMessageReceived = (msg) => Console.WriteLine($"Server got: {msg.String}");
            client.OnMessageReceived = (msg) => Console.WriteLine($"Client got: {msg.String}");

            server.OnConnected = () =>
            {
                server.PushMessage(new BinaryMessage() { String = $"Hello from the server side!" });
                client.PushMessage(new BinaryMessage() { String = $"Hello from the client side!" });
            };

            var _lock = new object();
            var i = 1;
            var running = true;
            server.OnMessageReceived = (msg) => CloseBoth();
            client.OnMessageReceived = (msg) => CloseBoth();
            void CloseBoth()
            {
                lock (_lock)
                {
                    i--;
                    if (i <= 0)
                    {
                        client.Stop();
                        server.Stop();
                        running = false;
                    }
                }
            }

            Console.WriteLine($"Starting server and client!");
            server.Start();
            client.Start();

            while (running)
                Thread.Sleep(5);

            Console.WriteLine($"Test finished!");
            Console.WriteLine($"Press any key...");
            Console.Read();
        }
    }
}
