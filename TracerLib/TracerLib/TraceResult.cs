using System;
using System.Collections.Generic;
using System.Xml.Serialization;
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
        public List<ThreadResult> threads = new List<ThreadResult>();
        public void Add(ThreadResult threadResult)
        {
            threads.Add(threadResult);
        }
    }
    [Serializable]
    public class MethodResult
    {
        public MethodResult()
        {

        }
        [XmlAttribute(AttributeName = "name")]
        [JsonProperty(PropertyName = "name")]
        public string methodName;
        [XmlAttribute(AttributeName = "time")]
        [JsonProperty(PropertyName = "time")]
        public TimeSpan executionTime;
        [XmlAttribute(AttributeName = "class")]
        [JsonProperty(PropertyName = "class")]
        public string className;
        [XmlElement(ElementName = "method")]
        [JsonProperty(PropertyName = "methods")]
        public List<MethodResult> children = new List<MethodResult>();
        private MethodResult parent;
        [XmlIgnore]
        [JsonIgnore]
        public DateTime start;
        private TimeSpan ts;
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
        public TimeSpan fulltime;
        [JsonProperty(PropertyName = "methods")]
        [XmlElement(ElementName = "method")]
        public List<MethodResult> methods = new List<MethodResult>();
        public void CalculateTime()
        {
            foreach (MethodResult mr in methods)
            {

            }
        }
    }
}
