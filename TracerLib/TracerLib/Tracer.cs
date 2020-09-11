using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace TracerLib
{
    class Tracer : ITracer
    {
        private TraceResult traceResult = new TraceResult();
        private DateTime start;
        private DateTime stop;
        public TraceResult GetTraceResult()
        {
            throw new NotImplementedException();
        }

        public void StartTrace()
        {
            start = DateTime.Now;
            traceResult = traceResult.Add(new TraceResult());
        }

        public void StopTrace()
        {
            stop = DateTime.Now;
            traceResult = traceResult.CheckOut(traceResult);
            StackTrace sr = new StackTrace();
            traceResult.className = sr.GetFrame(1).GetMethod().ReflectedType.Name;
            traceResult.methodName = sr.GetFrame(1).GetMethod().Name;
            traceResult.executionTime = stop - start;
        }
    }
}
