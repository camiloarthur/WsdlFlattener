using System;
using System.IO;
using WsdlFlattener;

namespace WsdlFlattenerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var flat = new Flattener();
            string url;
            var filePath = string.Empty;
            var tempPath = Path.GetTempFileName();

            if (args.Length >= 2)
            {
                url = args[0];
                filePath = args[1];
            }
            else if (args.Length == 1)
            {
                url = args[0];
            }
            else
            {
                Console.WriteLine("Usage: wf [wsdlUrl] [outputFile]");
                return;
            }
            
            var xml = flat.GetFlattenedWsdl(url, tempPath);

            if (String.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine(xml);
            }
            else
            {
                if(File.Exists(filePath))
                    File.Delete(filePath);

                using (var sw = File.CreateText(filePath))
                {
                    sw.Write(xml);
                }

                Console.WriteLine(@"File ""{0}"" successfully created", filePath);
            }
        }
    }
}
