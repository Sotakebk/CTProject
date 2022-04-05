using CTProject.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CTProject.Unity
{
    public class DependencyProvider : MonoBehaviour, IDependencyProvider
    {
        // set from Unity
        [SerializeField]
        private List<MonoBehaviour> registerMonoBehaviours;

        private List<object> registeredObjects;

        private void Awake()
        {
            registeredObjects = new List<object>();

            foreach (var obj in registerMonoBehaviours)
            {
                Register(obj);
            }
        }

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
