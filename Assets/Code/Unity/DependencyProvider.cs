using CTProject.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CTProject.Unity
{
    public class DependencyProvider : MonoBehaviour, IDependencyProvider
    {
        #region fields

        // set from Unity
        [SerializeField]
        private List<MonoBehaviour> registerMonoBehaviours;

        private List<object> registeredObjects;

        #endregion fields

        #region Unity calls

        private void Awake()
        {
            registeredObjects = new List<object>();

            foreach (var obj in registerMonoBehaviours)
            {
                Register(obj);
            }
        }

        #endregion Unity calls

        #region private methods

        private void Register<T>(T value)
        {
            if (!registeredObjects.Contains(value))
            {
                registeredObjects.Add(value);
            }
        }

        #endregion private methods

        #region public methods

        public T GetDependency<T>()
        {
            return registeredObjects.OfType<T>().FirstOrDefault();
        }

        #endregion public methods
    }
}
