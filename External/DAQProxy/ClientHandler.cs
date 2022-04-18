using CTProject.DataAcquisition.Communication;
using CTProject.Infrastructure;
using CTProject.DataAcquisition;
using NationalInstruments.DAQmx;
using System;

namespace DAQProxy
{
    public class ClientHandler : IDependencyConsumer
    {
        private TCPClient client;

        public ClientHandler()
        {
            client = new TCPClient(DefaultAddress.Address, DefaultAddress.Port);

            client.OnDisconnected = OnDisconnected;
            client.OnConnected = OnConnected;
            client.OnMessageReceived = OnMessageReceived;
        }

        public void LoadDependencies(IDependencyProvider dependencyProvider)
        {
            client.LoadDependencies(dependencyProvider);
        }

        public void Start()
        {
            client.Start();
        }

        private void OnDisconnected()
        {
        }

        private void OnConnected()
        {
        }

        private void OnMessageReceived(BinaryMessage message)
        {
        }
    }
}
