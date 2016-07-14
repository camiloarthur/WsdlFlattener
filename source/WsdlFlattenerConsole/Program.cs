using System;
using System.IO;
using System.Security;
using WsdlFlattener;

namespace WsdlFlattenerConsole
{
    class Program
    {
        public static SecureString GetPassword()
        {
            var pwd = new SecureString();
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.RemoveAt(pwd.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    pwd.AppendChar(i.KeyChar);
                    Console.Write("*");
                }
            }
            return pwd;
        }

        static void Main(string[] args)
        {
            var flat = new Flattener();
            System.Net.NetworkCredential mexNetworkCredentials = null;
            string url;
            var filePath = string.Empty;
            var tempPath = Path.GetTempFileName();
            var user = string.Empty;
            SecureString password = new SecureString();
            var domain = "AMERICAS";

            if (args.Length >= 3)
            {
                url = args[0];
                filePath = args[1];
                user = args[2];

                if (args.Length == 4 && !string.IsNullOrEmpty(args[3]))
                    domain = args[3];
                
                Console.Write("Enter your password: ");
                password = GetPassword();
                Console.WriteLine();
                Console.WriteLine("Processing WSDL...");
            }
            else if (args.Length == 2)
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

            if(!string.IsNullOrEmpty(user)&&!string.IsNullOrEmpty(domain))
                mexNetworkCredentials = new System.Net.NetworkCredential(user, new System.Net.NetworkCredential(string.Empty, password).Password, domain);

            var xml = flat.GetFlattenedWsdl(url, tempPath, mexNetworkCredentials);

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
            Console.ReadKey();
        }

    }
}
