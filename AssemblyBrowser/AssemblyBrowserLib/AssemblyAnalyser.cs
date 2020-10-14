using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Runtime.CompilerServices;

namespace AssemblyBrowserLib
{
    public class AssemblyAnalyser
    {
        private Dictionary<string, IList<AssemblyType>> assebmlyInformation = new Dictionary<string, IList<AssemblyType>>();

        public AssemblyAnalysisResult GetAnalysysResult(string path)
        {
            Assembly assembly = LoadAssembly(path);
            return new AssemblyAnalysisResult { AssemblyName=assembly.FullName, namespaces=Analyse(assembly) };
        }

        private List<AssemblyNamespace> Analyse(Assembly assembly)
        {
            List<AssemblyNamespace> namespaces = new List<AssemblyNamespace>();
            GetAssemblyTypes(assembly.GetTypes());
            foreach(string key in assebmlyInformation.Keys)
            {
                AssemblyNamespace assemblyNamespace = new AssemblyNamespace { NamespaceName = key, types = assebmlyInformation[key] };
                namespaces.Add(assemblyNamespace);
            }
            return namespaces;
        }

        private void GetAssemblyTypes(Type[] types)
        {
            foreach(Type type in types)
            {
                if (!IsCompilatorGeneratedType(type))
                {
                    string namespaceName = type.Namespace;
                    if (namespaceName == null)
                        namespaceName = "Global";
                    string typeName;
                    if (type.IsGenericType)
                    {
                        typeName = GetGenericTypeName(type);
                    }
                    else
                    {
                        typeName = type.Name;
                    }
                    AssemblyType assemblyType = new AssemblyType { typeName = typeName };
                    assemblyType.fields = GetAssemblyFields(type);
                    assemblyType.properties = GetAssemblyProperties(type);
                    assemblyType.methods = GetAssemblyMethods(type);
                    if (assebmlyInformation.ContainsKey(namespaceName))
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
        }

        private IList<AssemblyMethod> GetAssemblyMethods(Type type)
        {
            IList<AssemblyMethod> methods = new List<AssemblyMethod>();
            foreach(MethodInfo method in type.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.NonPublic 
                                                            | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                string signature=GetMethodSignature(method);
                AssemblyMethod assemblyMethod = new AssemblyMethod { methodName = method.Name, methodSignature = signature };
                if (IsExtensionMethod(method))
                    AddExtensionMethod(method, assemblyMethod);
                else if(!method.IsSpecialName)
                    methods.Add(assemblyMethod);
            }
            return methods;
        }

        private IList<AssemblyProperty> GetAssemblyProperties(Type type)
        {
            IList<AssemblyProperty> properties = new List<AssemblyProperty>();

            string propertyTypeName;
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (property.PropertyType.IsGenericType)
                {
                    propertyTypeName = GetGenericTypeName(property.PropertyType);
                }
                else
                {
                    propertyTypeName = property.PropertyType.Name;
                }

                AssemblyProperty assemblyProperty = new AssemblyProperty { propertyname = property.Name, typename = propertyTypeName };
                if(!IsCompilatorGeneratedType(property.PropertyType))
                    properties.Add(assemblyProperty);
            }
            return properties;
        }

        private IList<AssemblyField> GetAssemblyFields(Type type)
        {
            IList<AssemblyField> fields = new List<AssemblyField>();

            string fieldTypeName;
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (field.FieldType.IsGenericType)
                {
                    fieldTypeName = GetGenericTypeName(field.FieldType);
                }
                else
                {
                    fieldTypeName = field.FieldType.Name;
                }
                AssemblyField assemblyField = new AssemblyField {fieldName=field.Name, typeName=fieldTypeName};
                if (!IsCompilatorGeneratedType(field.FieldType))
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
                if (parameter.IsIn) 
                    signature.Append("in ");
                else if (parameter.IsOut) 
                    signature.Append("out ");
                else if (parameter.ParameterType.IsByRef)
                    signature.Append("ref ");
                signature.Append(parameter.ParameterType.Name+',');
                if (signature[signature.Length - 2] == '&')
                    signature.Remove(signature.Length - 2, 1);
            }
            if (signature[signature.Length - 1] == ',')
                signature[signature.Length - 1] = ')';
            else
                signature.Append(')');
            return signature.ToString();
        }

        private string GetGenericTypeName(Type type)
        {
            StringBuilder typeName = new StringBuilder();
            typeName.Append(type.Name.Split('`')[0]);
            typeName.Append('<');
            foreach (Type genericArgument in type.GetGenericArguments())
            {
                string genericArgumentTypeName;
                if (genericArgument.IsGenericType)
                    genericArgumentTypeName = GetGenericTypeName(genericArgument);
                else
                    genericArgumentTypeName = genericArgument.Name;
                typeName.Append(genericArgumentTypeName + ',');
            }
            typeName[typeName.Length - 1] = '>';

            return typeName.ToString();
        }

        private Assembly LoadAssembly(string path)
        {
            FileInfo file = new FileInfo(path);
            Assembly assembly = Assembly.LoadFrom(file.FullName);
            return assembly;
        }

        private void AddExtensionMethod(MethodInfo method, AssemblyMethod assemblyMethod)
        {
            Type ExtendedType = method.GetParameters().First().ParameterType;
            string namespaceName = ExtendedType.Namespace;
            assemblyMethod.extensionMethod="Extension Method";
            if (assebmlyInformation.ContainsKey(namespaceName))
            {
                assebmlyInformation[namespaceName].Where(t => t.typeName == ExtendedType.Name).Single().methods.Add(assemblyMethod);
            }
        }

        private bool IsCompilatorGeneratedType(Type type)
        {
            if (Attribute.GetCustomAttribute(type, typeof(CompilerGeneratedAttribute)) != null)
                return true;
            return false;
        }

        private bool IsExtensionMethod(MethodInfo method)
        {
            if (method.IsDefined(typeof(ExtensionAttribute), false) && method.DeclaringType.IsDefined(typeof(ExtensionAttribute), false))
                return true;
            return false;
        }
    }
}
