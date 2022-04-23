namespace CTProject.DataAcquisition
{
    public static class MessageTypeDefinition
    {
        // Start signal
        // in - EmptyMessage
        // out - EmptyMessage
        public const int Start = 1;

        // Stop signal
        // in - EmptyMessage
        // out - EmptyMessage
        public const int Stop = 2;

        // Message for data transfer
        // in - does not happen
        // out - DataBufferMessage
        public const int DataPacket = 3;

        // Message for channel information
        // in - EmptyMessage
        // out - StringArrayMessage
        public const int ChannelInfo = 4;

        // Message for changing channel information
        // in - StringMessage
        // out - StringMessage
        public const int ChannelSet = 5;

        // Message for buffer size information
        // in - EmptyMessage
        // out - IntArrayMessage
        public const int BufferSizeInfo = 6;

        // Message for changing channel information
        // in - IntMessage
        // out - IntMessage
        public const int BufferSizeSet = 7;

        // Message for sampling rate information
        // in - EmptyMessage
        // out - IntArrayMessage
        public const int SamplingRateInfo = 8;

        // Message for changing sampling rate
        // in - IntMessage
        // out - IntMessage
        public const int SamplingRateSet = 9;

        // Message for Min-Max values
        // in - EmptyMessage
        // out - IntArrayMessage
        public const int MinMaxValueInfo = 10;
    }
}
