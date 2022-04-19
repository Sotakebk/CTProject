using DAQProxy.Services;
using System;

namespace DAQProxy
{
    public static class Program
    {
        public static void Main()
        {
            var dependencyProvider = new DependencyProvider();

            var loggingService = new LoggingService();
            var actionPump = new ActionPump();
            var deviceHandler = new DeviceHandler();
            var messageHandler = new CommunicationHandler();
            dependencyProvider.RegisterDependency(actionPump);
            dependencyProvider.RegisterDependency(loggingService);
            dependencyProvider.RegisterDependency(deviceHandler);
            dependencyProvider.RegisterDependency(messageHandler);

            deviceHandler.LoadDependencies(dependencyProvider);
            messageHandler.LoadDependencies(dependencyProvider);
            loggingService.LoadDependencies(dependencyProvider);

            deviceHandler.Prepare(); // prepare device stuff

            messageHandler.Start(); // start TCP client and handling

            actionPump.Join(); // do any work

            // if actionPump stopped
            messageHandler.Stop();

            Console.WriteLine("Press any key to quit...");
            Console.Read();
        }
    }
}
