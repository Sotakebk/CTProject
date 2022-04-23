using System.Net;

namespace CTProject.DataAcquisition
{
    public static class DefaultAddress
    {
        public static IPAddress Address => IPAddress.Loopback;
        public const int Port = 9001;
    }
}
