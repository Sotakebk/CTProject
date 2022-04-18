using System.Net;

namespace CTProject.DataAcquisition
{
    public static class DefaultAddress
    {
        public static IPAddress Address => IPAddress.Loopback;
        public static int Port => 9001;
    }
}
