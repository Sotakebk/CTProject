using CTProject.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CTProject.Unity
{
    public class DependencyProvider : MonoBehaviour, IDependencyProvider
    {
        private List<object> registeredObjects;

        public void Register<T>(T value)
        {
            if (!registeredObjects.Contains(value))
            {
                registeredObjects.Add(value);
            }
        }

        public T GetDependency<T>()
        {
            return registeredObjects.OfType<T>().FirstOrDefault();
        }
    }
}
