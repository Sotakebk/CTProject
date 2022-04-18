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

            server.OnConnected = () =>
            {
                server.PushMessage(new StringMessage() { MessageContent = $"Hello from the server side!" }.Serialize());
                client.PushMessage(new StringMessage() { MessageContent = $"Hello from the client side!" }.Serialize());
            };

            var _lock = new object();
            var i = 3;
            var running = true;
            server.OnMessageReceived = (msg) => CloseBoth(msg, true);
            client.OnMessageReceived = (msg) => CloseBoth(msg, false);
            void CloseBoth(BinaryMessage msg, bool isServer)
            {
                var stringMessage = MessageFactory.GetMessageFromBinary<StringMessage>(msg);
                var side = isServer ? "Server" : "Client";
                Console.WriteLine($"{side} got: {stringMessage.MessageContent}");

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
