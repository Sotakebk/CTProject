using CTProject.DataAcquisition.Communication;
using CTProject.Infrastructure;
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
            var dependencyProvider = new DependencyProvider();
            var loggingService = new LoggingService();
            var actionPump = new ActionPump();
            dependencyProvider.RegisterDependency(loggingService);
            dependencyProvider.RegisterDependency(actionPump);

            loggingService.LoadDependencies(dependencyProvider);

            var ip = CTProject.DataAcquisition.DefaultAddress.Address;
            var port = CTProject.DataAcquisition.DefaultAddress.Port;
            var server = new TCPServer(IPAddress.Loopback, port);
            var client = new TCPClient(IPAddress.Loopback, port);

            server.LoadDependencies(dependencyProvider);
            client.LoadDependencies(dependencyProvider);

            server.OnConnected = () =>
            {
                actionPump.Do(() =>
                {
                    Thread.Sleep(100);
                    server.PushMessage(new StringMessage() { MessageContent = $"Hello from the server side!" });
                    client.PushMessage(new StringMessage() { MessageContent = $"Hello from the client side!" });
                });
            };

            var _lock = new object();
            var i = 1;
            void CloseBoth(BinaryMessage msg, string name)
            {
                var stringMessage = MessageFactory.GetMessageFromBinary<StringMessage>(msg);
                Console.WriteLine($"{name} got: {stringMessage.MessageContent}");

                lock (_lock)
                {
                    i--;
                    if (i <= 0)
                    {
                        actionPump.Do(() =>
                        {
                            client.Stop();
                            server.Stop();
                            actionPump.Quit();
                        });
                    }
                }
            }

            server.OnMessageReceived = (msg) => CloseBoth(msg, server.TCPSideName);
            client.OnMessageReceived = (msg) => CloseBoth(msg, client.TCPSideName);

            actionPump.Do(() =>
            {
                loggingService.Log(LogLevel.Info, $"Starting server and client!");
                server.Start();
                client.Start();
            });

            actionPump.Join();

            Thread.Sleep(1000);
            actionPump.Quit();
            actionPump.Join();

            Console.WriteLine($"Test finished!");
            Console.WriteLine($"Press enter to quit...");
            Console.ReadLine();
        }
    }
}
