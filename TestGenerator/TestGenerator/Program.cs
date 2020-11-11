using System;
using System.Threading.Tasks.Dataflow;
using System.IO;
using TestGeneratorLib;

namespace TestGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            ExecutionDataflowBlockOptions inputOptions = null;
            ExecutionDataflowBlockOptions processingOptions = null;
            ExecutionDataflowBlockOptions outputOptions =  null;
            string inputPath = null;
            string outputPath = null;

            try
            {
                Console.WriteLine("Max degree of input parallelism - ");
                int maxInput = Int32.Parse(Console.ReadLine());
                if (maxInput <= 0)
                    throw new ArgumentException("Parallelism degree must be positive");
                inputOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism=maxInput};
                Console.WriteLine("Max degree of processing parallelism - ");
                int maxProcessing = Int32.Parse(Console.ReadLine());
                if (maxProcessing <= 0)
                    throw new ArgumentException("Parallelism degree must be positive");
                processingOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxProcessing };
                Console.WriteLine("Max degree of output parallelism - ");
                int maxOutput = Int32.Parse(Console.ReadLine());
                if (maxOutput <= 0)
                    throw new ArgumentException("Parallelism degree must be positive");
                outputOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxOutput };
            }
            catch(ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            catch
            {
                Console.WriteLine("Parallelism degree must be a numeric value");
                return;
            }

            try
            {
                Console.WriteLine("InputPath - ");
                inputPath = Console.ReadLine();
                if (!Directory.Exists(inputPath))
                    throw new Exception("No such directory");


                Console.WriteLine("OutputPath - ");
                outputPath = Console.ReadLine();
                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var getFiles = new TransformManyBlock<string, string>(path =>
              {
                  Console.WriteLine("path - {0}", path);
                  return Directory.GetFiles(path);
              }
            );

            var getFile = new TransformBlock<string, string>(async filename =>
             {
                 Console.WriteLine("file - {0}", filename);
                 using(StreamReader reader = new StreamReader(filename))
                 {
                     return await reader.ReadToEndAsync();
                 }
             }, inputOptions
            );

            var generateTest = new TransformManyBlock<string, TestClass>(async classText =>
            {
                return await Generator.GetTestClasses(classText);
            },  processingOptions
            );

            var writeTest = new ActionBlock<TestClass>(async testClass =>
            {
                string fullPath = Path.Combine(outputPath, testClass.Name);
                Console.WriteLine("Test path - {0}", fullPath);
                using(StreamWriter writer = new StreamWriter(fullPath))
                {
                    await writer.WriteAsync(testClass.Code);
                }
            }, outputOptions

            );

            getFiles.LinkTo(getFile);
            getFile.LinkTo(generateTest);
            generateTest.LinkTo(writeTest);

            getFiles.Post(inputPath);
            getFiles.Complete();
            writeTest.Completion.Wait();
            return;
        }

    }

}
