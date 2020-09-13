using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ConsoleApp9
{
    public class FileWriter : Writer
    {
        private string filepath;
        public FileWriter(string FilePath)
        {
            filepath = FilePath;
        }

        public override void Write(string Serialized)
        {
            using (StreamWriter fStream = new StreamWriter(filepath))
            {
                fStream.Write(Serialized);
            }
        }
    }
}
