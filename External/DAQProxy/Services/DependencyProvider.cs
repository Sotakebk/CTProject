using CTProject.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace DAQProxy.Services
{
    public class DependencyProvider : IDependencyProvider
    {
        private List<object> dependencies;

        public DependencyProvider()
        {
            dependencies = new List<object>();
        }

        public T GetDependency<T>()
        {
            return dependencies.OfType<T>().FirstOrDefault();
        }

        public void RegisterDependency(object obj)
        {
            if (!dependencies.Contains(obj))
                dependencies.Add(obj);
        }
    }
}
