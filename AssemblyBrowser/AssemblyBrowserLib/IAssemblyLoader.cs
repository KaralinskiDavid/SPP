using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace AssemblyBrowserLib
{
    public interface IAssemblyLoader
    {
        Assembly LoadAssembly(string Path);
    }
}
