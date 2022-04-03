namespace CTProject.Infrastructure
{
    public interface IDataConsumer
    {
        void ResetIndex();

        void OnSettingsChange(IDataProvider source);

        void ReceiveData(ulong index, float[] data);
    }
}
