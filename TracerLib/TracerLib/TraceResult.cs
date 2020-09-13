using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Diagnostics;
using Newtonsoft.Json;

namespace TracerLib
{
    [Serializable]
    public class TraceResult
    {
        public TraceResult()
        {

        }
        [XmlElement(ElementName = "thread")]
        public List<ThreadResult> threads { get; set; }
        internal void Add(ThreadResult threadResult)
        {
            threads.Add(threadResult);
        } 
    }
    [Serializable]
    public class MethodResult
    {
        Stopwatch timer;
        public MethodResult()
        {
            timer = new Stopwatch();
            timer.Start();
        }
        [XmlAttribute(AttributeName = "name")]
        [JsonProperty(PropertyName = "name")]
        public string methodName;
        [XmlAttribute(AttributeName = "time")]
        [JsonProperty(PropertyName = "time")]
        public long executionTime;
        [XmlAttribute(AttributeName = "class")]
        [JsonProperty(PropertyName = "class")]
        public string className;
        [XmlElement(ElementName = "method")]
        [JsonProperty(PropertyName = "methods")]
        public List<MethodResult> children = new List<MethodResult>();
        private MethodResult parent;
        public MethodResult Add(MethodResult tr)
        {
            tr.parent = this;
            children.Add(tr);
            return children[children.Count - 1];
        }

        public MethodResult CheckOut()
        {
            return parent;
        }

        public void SetExecutionTime()
        {
            timer.Stop();
            executionTime = timer.ElapsedMilliseconds;
        }


    }
    [Serializable]
    public class ThreadResult
    {
        public ThreadResult()
        {

        }
        [XmlAttribute(AttributeName = "id")]
        [JsonProperty(PropertyName = "id")]
        public int threadId;
        [XmlAttribute(AttributeName = "time")]
        [JsonProperty(PropertyName = "time")]
        public long fulltime;
        [JsonProperty(PropertyName = "methods")]
        [XmlElement(ElementName = "method")]
        public List<MethodResult> methods = new List<MethodResult>();
        internal void CalculateTime()
        {
            foreach (MethodResult mr in methods)
            {
                fulltime+=mr.executionTime;
            }
        }
    }
}
