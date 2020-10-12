using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyBrowserLib
{
    public class AssemblyAnalysisResult
    {

    }

    public class AssemblyNamespace
    {
        public AssemblyNamespace()
        {
            types = new List<AssemblyType>();
        }
        public string assemblyName="Global";
        public IList<AssemblyType> types;
    }

    public class AssemblyType
    {
        public AssemblyType()
        {
            methods = new List<AssemblyMethod>();
            properties = new List<AssemblyProperty>();
            fields = new List<AssemblyField>();
        }
        public string typeName;
        public IList<AssemblyMethod> methods;
        public IList<AssemblyProperty> properties;
        public IList<AssemblyField> fields;
    }

    public class AssemblyMethod
    {
        public string methodName;
        public string methodSignature;
    }

    public class AssemblyProperty
    {
        public string typename;
        public string propertyname;
    }

    public class AssemblyField
    {
        public string typeName;
        public string fieldName;
    }
}
