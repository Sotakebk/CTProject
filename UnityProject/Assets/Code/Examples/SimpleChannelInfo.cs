using CTProject.Infrastructure;

namespace CTProject.Examples
{
    public class SimpleChannelInfo : IChannelInfo
    {
        public string UniqueName { get; private set; }
        public uint ID { get; private set; }

        public SimpleChannelInfo(string name, uint id)
        {
            UniqueName = name;
            ID = id;
        }
    }
}
