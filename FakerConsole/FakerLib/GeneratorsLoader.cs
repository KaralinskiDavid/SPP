using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace FakerLib
{
    public class GeneratorsLoader
    {
        public Dictionary<Type,IDtoGenerator> GetGenerators()
        {
            Dictionary<Type, IDtoGenerator> generators = new Dictionary<Type, IDtoGenerator>();

            string generatorsPath = Path.Combine(Directory.GetCurrentDirectory(), "Generators");
            DirectoryInfo directoryInfo = new DirectoryInfo(generatorsPath);

            if (!directoryInfo.Exists) return generators; 
            var generatorFiles = directoryInfo.GetFiles();
            foreach(FileInfo file in generatorFiles)
            {
                if(file.Extension==".dll")
                {
                    Assembly assembly = Assembly.LoadFrom(file.FullName);

                    foreach(Type type in assembly.GetTypes())
                    {
                        if(type.GetInterface(nameof(IDtoGenerator))!=null)
                        {
                            IDtoGenerator generator = assembly.CreateInstance(type.FullName) as IDtoGenerator;
                            if(Attribute.IsDefined(generator.GetType(),typeof(GeneratorTypeAttribute)))
                            {
                                string attribute = (Attribute.GetCustomAttribute(generator.GetType(), typeof(GeneratorTypeAttribute)) as GeneratorTypeAttribute).GeneratorType;
                                Type generatorType = Type.GetType(attribute);
                                if (generatorType != null && !generators.ContainsKey(generatorType))
                                    generators.Add(generatorType, generator);
                            }
                        }
                    }
                }
            }

            return generators;
        }
    }
}
