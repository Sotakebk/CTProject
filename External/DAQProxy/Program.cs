using NationalInstruments.DAQmx;
using System;

namespace DAQProxy
{
    public static class Program
    {
        public static void Main()
        {
            var s = DaqSystem.Local;
            var deviceList = string.Join(", ", s.Devices);
            Console.WriteLine($"Available devices: {deviceList} ({s.Devices.Length})");

            var client = new ClientHandler();
            client.LoadDependencies(Services.DependencyProvider.Instance);

            client.Start();
        }
    }
}
