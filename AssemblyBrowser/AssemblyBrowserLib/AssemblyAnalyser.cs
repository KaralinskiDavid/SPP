using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

namespace AssemblyBrowserLib
{
    public class AssemblyAnalyser
    {
        private Dictionary<string, IList<AssemblyType>> assebmlyInformation = new Dictionary<string, IList<AssemblyType>>();

        public IList<AssemblyNamespace> Analyse(Assembly assembly)
        {
            IList<AssemblyNamespace> namespaces = new List<AssemblyNamespace>();
            GetAssemblyTypes(assembly.GetTypes());
            foreach(string key in assebmlyInformation.Keys)
            {
                AssemblyNamespace assemblyNamespace = new AssemblyNamespace { assemblyName = key, types = assebmlyInformation[key] };
                namespaces.Add(assemblyNamespace);
            }
            return namespaces;
        }

        private void GetAssemblyTypes(Type[] types)
        {
            foreach(Type type in types)
            {
                string namespaceName = type.Namespace;
                if (namespaceName == null)
                    namespaceName = "Global";
                StringBuilder typeName = new StringBuilder();
                typeName.Append(type.Name);
                if(type.IsGenericType)
                {
                    typeName.Append('<');
                    foreach(Type genericArgument in type.GetGenericArguments())
                    {
                        typeName.Append(genericArgument.Name + ',');
                    }
                    typeName[typeName.Length - 1] = '>';
                }
                AssemblyType assemblyType = new AssemblyType { typeName = typeName.ToString() };
                assemblyType.fields = GetAssemblyFields(type);
                assemblyType.properties = GetAssemblyProperties(type);
                assemblyType.methods = GetAssemblyMethods(type);
                if(assebmlyInformation.ContainsKey(namespaceName))
                {
                    assebmlyInformation[namespaceName].Add(assemblyType);
                }
                else
                {
                    IList<AssemblyType> assemblyTypes = new List<AssemblyType>();
                    assemblyTypes.Add(assemblyType);
                    assebmlyInformation.Add(namespaceName, assemblyTypes);
                }
            }
        }

        private IList<AssemblyMethod> GetAssemblyMethods(Type type)
        {
            IList<AssemblyMethod> methods = new List<AssemblyMethod>();
            foreach(MethodInfo method in type.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Static))
            {
                string signature=GetMethodSignature(method);
                AssemblyMethod assemblyMethod = new AssemblyMethod { methodName = method.Name, methodSignature = signature };
                methods.Add(assemblyMethod);
            }
            return methods;
        }

        private IList<AssemblyProperty> GetAssemblyProperties(Type type)
        {
            IList<AssemblyProperty> properties = new List<AssemblyProperty>();
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static))
            {
                AssemblyProperty assemblyProperty = new AssemblyProperty { propertyname = property.Name, typename = property.PropertyType.Name };
                properties.Add(assemblyProperty);
            }
            return properties;
        }

        private IList<AssemblyField> GetAssemblyFields(Type type)
        {
            IList<AssemblyField> fields = new List<AssemblyField>();
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static))
            {
                AssemblyField assemblyField = new AssemblyField {fieldName=field.Name, typeName=field.FieldType.Name};
                fields.Add(assemblyField);
            }
            return fields;
        }

        private string GetMethodSignature(MethodInfo method)
        {
            StringBuilder signature = new StringBuilder();

            if (method.IsGenericMethod)
            {
                var genericArguments = method.GetGenericArguments();
                signature.Append('<');
                foreach (Type type in genericArguments)
                {
                    signature.Append(type.Name + ',');
                }
                signature[signature.Length - 1] = '>';
            }
            ParameterInfo[] parameters = method.GetParameters();
            //StringBuilder signature = new StringBuilder();
            signature.Append('(');
            foreach(ParameterInfo parameter in parameters)
            {
                signature.Append(parameter.ParameterType.Name+',');
            }
            if (signature[signature.Length - 1] == ',')
                signature[signature.Length - 1] = ')';
            else
                signature.Append(')');
            return signature.ToString();
        }
    }
}
