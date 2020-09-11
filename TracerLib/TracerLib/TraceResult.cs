using System;
using System.Collections.Generic;
using System.Text;

namespace TracerLib
{
    class TraceResult
    {
        List<ThreadResult> threads = new List<ThreadResult>();
        public void Add(ThreadResult threadResult)
        {
            threads.Add(threadResult);
        }
    }
    class MethodResult
    {
        public List<MethodResult> children = new List<MethodResult>();
        private MethodResult parent;
        public string className;
        public string methodName;
        public DateTime start;
        private TimeSpan ts;
        private TimeSpan executionTime;
        public MethodResult Add(MethodResult tr)
        {
            tr.parent = this;
            children.Add(tr);
            return children[children.Count - 1];
        }

        public MethodResult CheckOut(MethodResult tr)
        {
            return tr.parent;
        }

        public void SetExecutionTime(DateTime now)
        {
            executionTime = now - start;
        }

        public TimeSpan CalculateTime()
        {
            foreach (MethodResult mr in this.children)
            {
                ts += mr.executionTime;
                mr.CalculateTime();
            }
            return ts;
        }

    }

    class ThreadResult
    {
        public int threadId;
        public TimeSpan fulltime;
        public List<MethodResult> methods = new List<MethodResult>();
        public void CalculateTime()
        {
            foreach (MethodResult mr in methods)
            {

            }
        }
    }
}
