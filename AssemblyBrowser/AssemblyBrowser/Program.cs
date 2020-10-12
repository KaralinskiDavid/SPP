using System;
using AssemblyBrowserLib;

namespace AssemblyBrowser
{
    class Program
    {
        static void Main(string[] args)
        {
            AssemblyLoader loader = new AssemblyLoader();
            AssemblyAnalyser analyser = new AssemblyAnalyser();
            analyser.Analyse(loader.LoadAssembly(@"./FakerLib.dll"));
            Console.WriteLine("Hello World!");
        }
    }
}
