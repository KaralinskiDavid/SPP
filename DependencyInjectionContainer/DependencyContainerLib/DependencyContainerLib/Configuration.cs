using System;
using System.Collections.Generic;
using System.Text;

namespace DependencyContainerLib
{
    public class Configuration
    {
        private Dictionary<Type, List<Type>> _dependencies = new Dictionary<Type, List<Type>>();
        private List<Type> _singletons = new List<Type>();

        public Dictionary<Type, List<Type>> Dependencies
        {
            get { return _dependencies; }
        }

        public List<Type> Singletones
        {
            get { return _singletons; }
        }

        public void Register<TDependency, TImplementation>(bool isSingleton=false)
            where TDependency : class
            where TImplementation : TDependency
        {
            if (typeof(TImplementation).IsAbstract || typeof(TImplementation).IsInterface)
                throw new ArgumentException();
            AddDependency(typeof(TDependency), typeof(TImplementation));
            if (isSingleton)
                _singletons.Add(typeof(TImplementation));
        }

        public void Register(Type dependencyType, Type implementationType, bool isSingleton=false)
        {
            if (implementationType.IsAbstract || implementationType.IsInterface)
                throw new ArgumentException();
            AddDependency(dependencyType, implementationType);
            if (isSingleton)
                _singletons.Add(implementationType);
        }

        private void AddDependency(Type TDependency, Type TImplementation)
        {
            if (_dependencies.ContainsKey(TDependency))
                _dependencies[TDependency].Add(TImplementation);
            else
            {
                _dependencies[TDependency] = new List<Type> { TImplementation };
            }
        }

    }
}
