using System;
using System.Collections.Generic;
using System.Text;

namespace TracerLib
{
    public interface ITracer
    {
        void StartTrace();
        void StopTrace();
        TraceResult GetTraceResult();
    }
}
