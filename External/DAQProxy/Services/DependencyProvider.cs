using CTProject.Infrastructure;
using System.Linq;

namespace DAQProxy.Services
{
    public class DependencyProvider : IDependencyProvider
    {
        private static readonly object[] dependencies = new object[]
        {
            new LoggingService()
        };

        public static DependencyProvider Instance { get; private set; }

        static DependencyProvider()
        {
            Instance = new DependencyProvider();
        }

        public T GetDependency<T>()
        {
            return dependencies.OfType<T>().FirstOrDefault();
        }
    }
}
