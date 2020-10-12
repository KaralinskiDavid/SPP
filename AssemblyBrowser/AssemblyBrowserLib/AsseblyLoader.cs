using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;

namespace AssemblyBrowserLib
{
    public class AssemblyLoader : IAssemblyLoader
    {

        public Assembly LoadAssembly(string path)
        {
            //if (System.IO.Path.GetExtension(path) != ".dll" || System.IO.Path.GetExtension(path) != ".exe")
                //throw new Exception("Wrong extension");

            FileInfo file = new FileInfo(path);
            Assembly assembly = Assembly.LoadFrom(file.FullName);
            return assembly;
        }
    }
}
