using System;
using System.Collections.Generic;
using System.Text;

namespace TracerLib
{
    class TraceResult
    {
        private List<TraceResult> children = new List<TraceResult>();
        private TraceResult parent;
        public string className;
        public string methodName;
        public TimeSpan executionTime;
        public TraceResult Add(TraceResult tr)
        {
            tr.parent = this;
            children.Add(tr);
            return children[children.Count - 1];
        }

        public TraceResult CheckOut(TraceResult tr)
        {
            return tr.parent;
        }
    }
}
