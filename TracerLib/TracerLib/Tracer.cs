using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace TracerLib
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Text;
    using System.Diagnostics;
    using System.Threading;

        public class Tracer : ITracer
        {
            private ConcurrentDictionary<int, MethodResult> dict = new ConcurrentDictionary<int, MethodResult>();
            public TraceResult GetTraceResult()
            {
                TraceResult tr = new TraceResult();
                tr.threads = new List<ThreadResult>();
                foreach (int index in dict.Keys)
                {
                    ThreadResult thrres = new ThreadResult();
                    thrres.threadId = index;
                    thrres.methods = dict[index].children;
                    thrres.CalculateTime();
                    tr.Add(thrres);
                }
                return tr;
            }

            public void StartTrace()
            {
                if (!dict.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                {
                    dict.TryAdd(Thread.CurrentThread.ManagedThreadId, new MethodResult());
                }
                dict[Thread.CurrentThread.ManagedThreadId] = dict[Thread.CurrentThread.ManagedThreadId].Add(new MethodResult());
            }

            public void StopTrace()
            {
                int currentId = Thread.CurrentThread.ManagedThreadId;
                StackTrace sr = new StackTrace();
                System.Reflection.MethodBase mb = sr.GetFrame(1).GetMethod();
                dict[currentId].className = mb.ReflectedType.Name;
                dict[currentId].methodName = mb.Name;
                dict[currentId].SetExecutionTime();
                dict[currentId] = dict[currentId].CheckOut();
            }
        }
    }
