using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyBrowserLib
{
    public class AssemblyAnalysisResult
    {
        public AssemblyAnalysisResult()
        {
            namespaces = new List<AssemblyNamespace>();
        }
        public string AssemblyName { get; set; }
        public List<AssemblyNamespace> namespaces { get; set; }
    }

    public class AssemblyNamespace
    {
        public AssemblyNamespace()
        {
            types = new List<AssemblyType>();
        }
        public string NamespaceName { get; set; }
        public IList<AssemblyType> types { get; set; }
    }

    public class AssemblyType
    {
        public AssemblyType()
        {
            methods = new List<AssemblyMethod>();
            properties = new List<AssemblyProperty>();
            fields = new List<AssemblyField>();
        }

        public string typeName { get; set; }
        public IList<AssemblyMethod> methods { get; set; }
        public IList<AssemblyProperty> properties { get; set; }
        public IList<AssemblyField> fields { get; set; }
    }

    public class AssemblyMethod
    {
        public string extensionMethod = "";
        public string methodName;
        public string methodSignature;
        public string Presentation { get { return extensionMethod + methodName + " " + methodSignature; } }
    }

    public class AssemblyProperty
    {
        public string typename;
        public string propertyname { get; set; }
        public string Presentation { get { return typename+" "+propertyname; } }
    }

    public class AssemblyField
    {
        public string typeName;
        public string fieldName { get; set; }
        public string Presentation { get { return typeName+" "+fieldName; } }
    }
}
