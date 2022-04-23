using CTProject.DataAcquisition;
using DAQProxy.Services;
using System;
using System.Linq;
using System.Net;

namespace DAQProxy
{
    public static class Program
    {
        private static LoggingService loggingService;
        private static ActionPump actionPump;
        private static DeviceHandler deviceHandler;
        private static CommunicationHandler communicationHandler;
        private static DependencyProvider dependencyProvider;

        private static IPAddress address;
        private static int port;

        public static void Main()
        {
            PrepareInstancesAndDependencies();
            InitializeDevices();
            GetNetworkConfiguration();

            communicationHandler.Start();

            actionPump.Join(); // do any work

            // if actionPump stopped
            communicationHandler.Stop();

            Console.WriteLine("Press any key to quit...");
            Console.Read();
        }

        private static void PrepareInstancesAndDependencies()
        {
            dependencyProvider = new DependencyProvider();
            loggingService = new LoggingService();
            actionPump = new ActionPump();
            deviceHandler = new DeviceHandler();
            communicationHandler = new CommunicationHandler();
            dependencyProvider.RegisterDependency(actionPump);
            dependencyProvider.RegisterDependency(loggingService);
            dependencyProvider.RegisterDependency(deviceHandler);
            dependencyProvider.RegisterDependency(communicationHandler);

            deviceHandler.LoadDependencies(dependencyProvider);
            communicationHandler.LoadDependencies(dependencyProvider);
            loggingService.LoadDependencies(dependencyProvider);
        }

        private static void InitializeDevices()
        {
            SelectDevice();

            Console.WriteLine($"Channels: {string.Join(", ", deviceHandler.GetPossibleChannels())}");
            Console.WriteLine($"Sampling rates: {string.Join(", ", deviceHandler.GetPossibleSamplingRates())}");

            var minValue = GetMinValue();
            var maxValue = GetMaxValue(minValue);

            deviceHandler.MinValue = minValue;
            deviceHandler.MaxValue = maxValue;
            Console.WriteLine($"Value range: ({deviceHandler.MinValue}) to ({deviceHandler.MaxValue})");

            Console.WriteLine();
        }

        private static void SelectDevice()
        {
            var devices = deviceHandler.ListDevices();
            Console.WriteLine("Detected devices:");
            foreach (var d in devices)
            {
                Console.WriteLine($"{d.ProductType} (ID: {d.DeviceID})");
                Console.WriteLine($"AI channels: {d.AIPhysicalChannels.Length}");
            }
            if (devices.Length == 0)
                ExitMessage("No connected devices detected :(");

            var supportedDevices = devices.Where(d => IsSupportedDevice(d.ProductType)).ToArray();
            Console.WriteLine("Detected devices:");
            foreach (var d in supportedDevices)
            {
                Console.WriteLine($"{d.ProductType} (ID: {d.DeviceID})");
            }

            if (supportedDevices.Length == 0)
                Console.WriteLine("No supported devices detected, this program is not guaranteed to work!");

            var device = supportedDevices.FirstOrDefault() ?? devices.FirstOrDefault();
            if (devices.Length > 1)
            {
                string input = string.Empty;
                int i = 0;

                do
                {
                    Console.WriteLine($"Please decide which device to use: (0-{devices.Length})");
                    for (int x = 0; x < devices.Length; x++)
                    {
                        Console.WriteLine($"{x} {devices[x].ProductType} (ID: {devices[x].DeviceID})");
                    }
                    input = Console.ReadLine();
                }
                while (!(int.TryParse(input, out i) && i < devices.Length && i >= 0));

                device = devices[i];
            }

            Console.WriteLine($"Using device {device.ProductType} (ID: {device.DeviceID})");

            deviceHandler.ApplyDefaultSettings(device);
        }

        private static int GetMinValue()
        {
            string input;
            int minValue;
            do
            {
                Console.WriteLine("Minimum expected voltage?");
                input = Console.ReadLine();

            } while (!int.TryParse(input, out minValue));
            return minValue;
        }

        private static int GetMaxValue(int minValue)
        {
            string input;
            int maxValue;
            do
            {
                Console.WriteLine("Maximum expected voltage?");
                input = Console.ReadLine();

            } while (!int.TryParse(input, out maxValue) && maxValue >= minValue);
            return maxValue;
        }

        private static void GetNetworkConfiguration()
        {
            Console.WriteLine("Change default ip/address? (y or n/empty)");
            if (!Console.ReadLine().ToLowerInvariant().StartsWith("y"))
                return;

            GetAddress();
            GetPort();

            communicationHandler.ChangeAddress(address, port);
        }

        private static void GetAddress()
        {
             address = DefaultAddress.Address;
            string input;
            do
            {
                address = DefaultAddress.Address;
                Console.WriteLine("Input Unity3D CTProject server IP address (leave blank for loopback)");
                input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    break;
            } while (!IPAddress.TryParse(input, out address));

        }


        private static void GetPort()
        {
            port = DefaultAddress.Port;
            string input;
            do
            {
                port = DefaultAddress.Port;
                Console.WriteLine("Input Unity3D CTProject server port (leave blank for default)");
                input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    break;
            } while (!(int.TryParse(input, out port) && port >= 0 && port < 65535));
        }


        private static void ExitMessage(string msg = null)
        {
            if (msg != null)
                Console.WriteLine(msg);
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }

        private static bool IsSupportedDevice(string productName)
        {
            var arr = new string[] { "USB-6008", "USB-6009" };

            return arr.Contains(productName);
        }
    }
}
