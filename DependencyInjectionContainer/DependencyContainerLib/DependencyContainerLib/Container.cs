using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Concurrent;

namespace DependencyContainerLib
{
    public class Container
    {
        private Configuration _config;
        private ConcurrentDictionary<Type, object> _createdObjects = new ConcurrentDictionary<Type, object>();

        public Container(Configuration config)
        {
            _config = config;
        }

        public dynamic Resolve<TDependency>()
        {
            return GetImplementation(typeof(TDependency));
        }

        private dynamic GetImplementation(Type type, bool isImplementation=false)
        {
            if (type.IsGenericType && type.GetInterfaces().Contains(typeof(IEnumerable)) && !_config.Dependencies.ContainsKey(type))
            {
                var res = GetIEnumerableImplementationTypes(type); 
                return res;
            }

            if (_config.Dependencies.ContainsKey(type) || isImplementation)
            {
                Type implementationType;

                if (!isImplementation)
                    implementationType = _config.Dependencies[type].First();
                else
                    implementationType = type;

                if (_config.Singletones.Contains(implementationType))
                {
                    if (_createdObjects.ContainsKey(implementationType))
                        return _createdObjects[implementationType];
                }

                var parameters = GetParametersList(implementationType);
                if (parameters.Count > 0)
                {
                    dynamic created = Activator.CreateInstance(implementationType, parameters.ToArray());
                    if (_config.Singletones.Contains(implementationType))
                        _createdObjects.TryAdd(implementationType, created);
                    return created;
                }
                else
                {
                    dynamic created = Activator.CreateInstance(implementationType);
                    if (_config.Singletones.Contains(implementationType))
                        _createdObjects.TryAdd(implementationType, created);
                    return created;
                }
            }
            else if (isOpenGeneric(type))
            {
                Type implementationType = GetTypeDefinitionForOpenGeneric(type);
                return GetImplementation(implementationType, true);
            }
            else
                return null;
        }

        private bool isOpenGeneric(Type type)
        {
            return (type.IsGenericType && _config.Dependencies.ContainsKey(type.GetGenericTypeDefinition()));
        }


        private Type GetTypeDefinitionForOpenGeneric(Type dependecyType)
        {
            var genericArgument = dependecyType.GetGenericArguments().FirstOrDefault();
            var implementationType = _config.Dependencies[dependecyType.GetGenericTypeDefinition()].FirstOrDefault();
            if (_config.Dependencies.ContainsKey(genericArgument))
            {
                return implementationType.MakeGenericType(_config.Dependencies[genericArgument].FirstOrDefault());
            }
            return implementationType.MakeGenericType(genericArgument);
        }

        private dynamic GetIEnumerableImplementationTypes(Type dependecyTypes)
        {
            List<object> implementationObjects = new List<object>();
            var dependencyType = dependecyTypes.GenericTypeArguments.First();
            if(_config.Dependencies.ContainsKey(dependencyType))
            {
                var implementationTypes = _config.Dependencies[dependencyType];
                foreach(var implementationType in implementationTypes)
                {
                    implementationObjects.Add(GetImplementation(implementationType, true));
                }
                var genericMethod = typeof(Container).GetMethod(nameof(ObjectListToInterfaceList), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(dependencyType);
                return genericMethod.Invoke(this, new object[] { implementationObjects });
                //var res = Enumerable.Cast(implementationObjects);
            }
            else
            {
                return null;
            }
        }

        private IEnumerable<T> ObjectListToInterfaceList<T>(List<object> objects)
        {
            List<T> result = new List<T>();

            foreach(object obj in objects)
            {
                result.Add((T)obj);
            }
            return result.AsEnumerable<T>();
        }

        private List<object> GetParametersList(Type type)
        {
            List<object> parametersList = new List<object>();

            var constructor = type.GetConstructors()?.FirstOrDefault();
            if(constructor!=null)
            {
                var parameters = constructor.GetParameters();
                //if(parameters.Count()==0)
                //    return parametersList.Add(Activator.CreateInstance())
                foreach(var parameter in parameters)
                {
                    Type parameterType = parameter.ParameterType;
                    if (_config.Dependencies.ContainsKey(parameterType))
                        parameterType = _config.Dependencies[parameterType].First();

                    if (_config.Singletones.Contains(parameterType))
                    {
                        if (_createdObjects.ContainsKey(parameterType))
                            parametersList.Add(_createdObjects[parameterType]);
                    }
                    else if(parameterType.IsGenericType && parameterType.GetInterfaces().Contains(typeof(IEnumerable)))
                    {
                        parametersList.Add(GetIEnumerableImplementationTypes(parameterType));
                    }
                    else
                    {
                        var creationParams = GetParametersList(parameterType);
                        if (creationParams.Count > 0)
                        {
                            object created = Activator.CreateInstance(parameterType, creationParams);
                            if (_config.Singletones.Contains(parameterType))
                                _createdObjects.TryAdd(parameterType, created);
                            parametersList.Add(created);
                        }
                        else if(!parameterType.IsAbstract && !parameterType.IsInterface)
                        {
                            object created = Activator.CreateInstance(parameterType);
                            if (_config.Singletones.Contains(parameterType))
                                _createdObjects.TryAdd(parameterType, created);
                            parametersList.Add(created);
                        }
                        else
                        {
                            parametersList.Add(null);
                        }
                    }
                }
            }

            return parametersList;
        }

    }
}
