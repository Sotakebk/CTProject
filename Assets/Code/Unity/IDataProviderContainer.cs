using CTProject.Infrastructure;

namespace CTProject.Unity
{
    public interface IDataProviderContainer
    {
        string DataProviderName { get; }

        IDataProvider GetDataProvider();
    }
}
