using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace TracerLib
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Diagnostics;
    using System.Threading;

    namespace ConsoleApp9
    {
        class Tracer : ITracer
        {
            private Dictionary<int, MethodResult> dict = new Dictionary<int, MethodResult>();
            public TraceResult GetTraceResult()
            {
                TraceResult tr = new TraceResult();
                foreach (int index in dict.Keys)
                {
                    ThreadResult thrres = new ThreadResult();
                    thrres.threadId = index;
                    thrres.methods = dict[index].children;
                    thrres.fulltime = dict[index].children[0].CalculateTime();
                    tr.Add(thrres);
                }
                return tr;
            }

            public void StartTrace()
            {
                if (!dict.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                {
                    dict.Add(Thread.CurrentThread.ManagedThreadId, new MethodResult() { start = DateTime.Now });
                }
                dict[Thread.CurrentThread.ManagedThreadId] = dict[Thread.CurrentThread.ManagedThreadId].Add(new MethodResult() { start = DateTime.Now });
            }

            public void StopTrace()
            {
                StackTrace sr = new StackTrace();
                dict[Thread.CurrentThread.ManagedThreadId].className = sr.GetFrame(1).GetMethod().ReflectedType.Name;
                dict[Thread.CurrentThread.ManagedThreadId].methodName = sr.GetFrame(1).GetMethod().Name;
                dict[Thread.CurrentThread.ManagedThreadId].SetExecutionTime(DateTime.Now);
                dict[Thread.CurrentThread.ManagedThreadId] = dict[Thread.CurrentThread.ManagedThreadId].CheckOut(dict[Thread.CurrentThread.ManagedThreadId]);
            }
        }
    }
}
