using System;
using System.Collections.Generic;
using System.Text;

namespace TestGeneratorLibTests
{
    public class FirstTestedClass
    {
        public FirstTestedClass(IAsyncDisposable testDependency, int a, string s, SecondTestedClass st)
        {

        }

        public static int StaticCheck(int b)
        {
            return b;
        }

        public int GetSum(int a, int b)
        {
            return a + b;
        }

        public SecondTestedClass GetSecondClass(double d)
        {
            return new SecondTestedClass();
        }

        public void VoidRet()
        {
            return;
        }

        private int GetMultiple(int a, int b)
        {
            return a * b;
        }
    }

    public class SecondTestedClass
    {
        public string GetHello(string name)
        {
            return "Hello, " + name;
        }
    }

    public static class StaticClassCheck
    {
        public static void GetNothing()
        {
            return;
        }
    }
}
