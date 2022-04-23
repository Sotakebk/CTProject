using System;
using System.Text;

namespace CTProject.DataAcquisition.Communication
{
    public sealed class BinaryMessage
    {
        public int Type { get; set; }

        private byte[] data;

        public byte[] Data
        {
            get => data;
            set => data = value ?? Array.Empty<byte>();
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

        public BinaryMessage(byte[] arr)
        {
            Type = BitConverter.ToInt32(arr, 0);
            var length = arr.Length - sizeof(int);
            Data = new byte[length];
            Array.Copy(arr, sizeof(int), Data, 0, length);
        }

        public static BinaryMessage Empty => new BinaryMessage() { Type = -1 };
    }

    public static class MessageFactory
    {
        public static T GetMessageFromBinary<T>(BinaryMessage binary) where T : Message, new()
        {
            try
            {
                return new T().DeserializeFrom<T>(binary);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed deserializing message of type {typeof(T)}, reason: {ex.Message}", ex);
            }
        }
    }

    public abstract class Message
    {
        public int MessageType { get; set; }

        public abstract BinaryMessage Serialize();

        public T DeserializeFrom<T>(BinaryMessage binary) where T : Message
        {
            return (T)DeserializeFrom(binary);
        }

        public virtual Message DeserializeFrom(BinaryMessage binary)
        {
            MessageType = binary.Type;
            return this;
        }
    }

    public sealed class EmptyMessage : Message
    {
        public EmptyMessage()
        {
        }

        public EmptyMessage(int messageType)
        {
            MessageType = messageType;
        }

        public override BinaryMessage Serialize()
        {
            var bm = new BinaryMessage();
            bm.Type = MessageType;
            return bm;
        }

        public override Message DeserializeFrom(BinaryMessage binary)
        {
            base.DeserializeFrom(binary);
            return this;
        }
    }

    public sealed class StringMessage : Message
    {
        public string MessageContent { get; set; }

        public StringMessage()
        {
        }

        public StringMessage(int messageType, string messageContent)
        {
            MessageType = messageType;
            MessageContent = messageContent;
        }

        public override BinaryMessage Serialize()
        {
            var bm = new BinaryMessage();
            bm.Type = MessageType;
            bm.Data = Encoding.UTF8.GetBytes(MessageContent ?? string.Empty);
            return bm;
        }

        public override Message DeserializeFrom(BinaryMessage binary)
        {
            base.DeserializeFrom(binary);
            MessageContent = Encoding.UTF8.GetString(binary.Data);
            return this;
        }
    }

    public sealed class StringArrayMessage : Message
    {
        private const string Separator = ((("😁"))); // I shouldn't do this... too bad!

        public string[] MessageContent { get; set; }

        public StringArrayMessage()
        {
            MessageContent = new string[0];
        }

        public StringArrayMessage(int messageType, string[] messageContent)
        {
            MessageType = messageType;
            MessageContent = messageContent;
        }

        public override BinaryMessage Serialize()
        {
            var bm = new BinaryMessage();
            bm.Type = MessageType;
            var joinedString = string.Join(Separator, MessageContent);
            bm.Data = Encoding.UTF8.GetBytes(joinedString);
            return bm;
        }

        public override Message DeserializeFrom(BinaryMessage binary)
        {
            base.DeserializeFrom(binary);
            var joinedString = Encoding.UTF8.GetString(binary.Data);
            MessageContent = joinedString.Split(new string[] { Separator }, StringSplitOptions.None);
            return this;
        }
    }

    public sealed class IntMessage : Message
    {
        public int MessageContent { get; set; }

        public IntMessage()
        {
        }

        public IntMessage(int messageType, int messageContent)
        {
            MessageType = messageType;
            MessageContent = messageContent;
        }

        public override BinaryMessage Serialize()
        {
            var bm = new BinaryMessage();
            bm.Type = MessageType;
            bm.Data = BitConverter.GetBytes(MessageContent);
            return bm;
        }

        public override Message DeserializeFrom(BinaryMessage binary)
        {
            base.DeserializeFrom(binary);
            MessageContent = BitConverter.ToInt32(binary.Data, 0);
            return this;
        }
    }

    public sealed class DataBufferMessage : Message
    {
        public int MessageContentIndex { get; set; }
        public float[] MessageContentData { get; set; }

        public DataBufferMessage()
        {
            MessageContentData = new float[0];
        }

        public DataBufferMessage(int messageType, int messageContentIndex, float[] messageContentData)
        {
            MessageType = messageType;
            MessageContentIndex = messageContentIndex;
            MessageContentData = messageContentData;
        }

        public override BinaryMessage Serialize()
        {
            var bm = new BinaryMessage();
            var byteCount = sizeof(int) + sizeof(float) * MessageContentData.Length;
            var buffer = new byte[byteCount];
            bm.Type = MessageType;
            Buffer.BlockCopy(BitConverter.GetBytes(MessageContentIndex), 0, buffer, 0, sizeof(int));
            Buffer.BlockCopy(MessageContentData, 0, buffer, sizeof(int), sizeof(float) * MessageContentData.Length);
            bm.Data = buffer;
            return bm;
        }

        public override Message DeserializeFrom(BinaryMessage binary)
        {
            base.DeserializeFrom(binary);
            MessageContentIndex = BitConverter.ToInt32(binary.Data, 0);
            var floatCount = (binary.Data.Length - sizeof(int)) / sizeof(float);
            var buffer = new float[floatCount];
            Buffer.BlockCopy(binary.Data, sizeof(int), buffer, 0, floatCount * sizeof(float));
            MessageContentData = buffer;
            return this;
        }
    }

    public sealed class IntArrayMessage : Message
    {
        public int[] MessageContent { get; set; }

        public IntArrayMessage()
        {
            MessageContent = new int[0];
        }

        public IntArrayMessage(int messageType, int[] messageContent)
        {
            MessageType = messageType;
            MessageContent = messageContent;
        }

        public override BinaryMessage Serialize()
        {
            var bm = new BinaryMessage();
            var buffer = new byte[sizeof(int) * MessageContent.Length];
            bm.Type = MessageType;
            Buffer.BlockCopy(MessageContent, 0, buffer, 0, sizeof(int) * MessageContent.Length);
            bm.Data = buffer;
            return bm;
        }

        public override Message DeserializeFrom(BinaryMessage binary)
        {
            base.DeserializeFrom(binary);
            var intCount = binary.Data.Length / sizeof(int);
            var buffer = new int[intCount];
            Buffer.BlockCopy(binary.Data, 0, buffer, 0, binary.Data.Length);
            MessageContent = buffer;
            return this;
        }
    }
}
