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

        public void Analyse(Assembly assembly)
        {
            GetAssemblyTypes(assembly.GetTypes());
        }

        private IList<string> GetAssemblyNamespaces(TypeInfo[] types)
        {
            var namespaces = types.Select(t => t.Namespace).Distinct();
            return namespaces.ToList();
        }

        private void GetAssemblyTypes(Type[] types)
        {
            foreach(Type type in types)
            {
                string namespaceName = type.Namespace;
                if (namespaceName == null)
                    namespaceName = "Global";
                AssemblyType assemblyType = new AssemblyType { typeName = type.Name };
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
                string signature="";
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
    }
}
