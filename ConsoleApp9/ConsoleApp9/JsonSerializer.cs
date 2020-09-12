using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using TracerLib;

namespace ConsoleApp9
{
    class JsonSerializator : Serializator
    {

        public JsonSerializator()
        {
        }

        JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
        };

        public override string Serialize(TraceResult tr)
        {
            string JsonString = JsonConvert.SerializeObject(tr, JsonSettings);
            //using (StreamWriter fStream = new StreamWriter(FilePath))
            //{
            //    fStream.Write(JsonString);
            //}
            return JsonString;
        }
    }
}
