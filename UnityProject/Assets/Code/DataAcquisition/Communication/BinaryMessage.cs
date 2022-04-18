using System;
using System.Text;

namespace CTProject.DataAcquisition.Communication
{
    public sealed class BinaryMessage
    {
        private byte[] data;
        public int Type { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays

        public byte[] Data
#pragma warning restore CA1819 // Properties should not return arrays
        {
            get => data ?? Array.Empty<byte>();
            set => data = value ?? Array.Empty<byte>();
        }

        public string String
        {
            get
            {
                return Encoding.UTF8.GetString(Data);
            }
            set
            {
                Data = Encoding.UTF8.GetBytes(value);
            }
        }

        public byte[] Serialize()
        {
            var arr = new byte[sizeof(int) + Data.Length];
            Array.Copy(BitConverter.GetBytes(Type), 0, arr, 0, sizeof(int));
            Array.Copy(Data, 0, arr, sizeof(int), Data.Length);
            return arr;
        }

        public BinaryMessage()
        {
            Data = Array.Empty<byte>();
        }

        public BinaryMessage(byte[] arr) : base()
        {
            Type = BitConverter.ToInt32(arr, 0);
            var length = arr.Length - sizeof(int);
            Data = new byte[length];
            Array.Copy(arr, sizeof(int), Data, 0, length);
        }

        public static BinaryMessage Empty => new BinaryMessage() { Type = -1 };
    }
}
