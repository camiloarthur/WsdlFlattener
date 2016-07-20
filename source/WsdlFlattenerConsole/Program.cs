using System;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
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
            var domain = string.Empty;
            SecureString password = new SecureString();
            
            if (args.Length >= 4)
            {
                url = args[0];
                filePath = args[1];
                user = args[2];
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
                Console.WriteLine("wf [wsdlUrl] [outputFile] [user] [domain]");
                return;
            }

            if(!string.IsNullOrEmpty(user)&&!string.IsNullOrEmpty(domain))
                mexNetworkCredentials = new System.Net.NetworkCredential(user, new System.Net.NetworkCredential(string.Empty, password).Password, domain);

            var xml = flat.GetFlattenedWsdl(url, tempPath, mexNetworkCredentials);

            //Apply Auto-fix
            if (args.Length >= 4)
            {                
                var extractor = Regex.Match(xml, "targetNamespace=\"(?<value>.*?)\"", RegexOptions.IgnoreCase);
                var dynamicSearch = String.Format("xmlns:(?<SoapHeader>[a-zA-Z0-9]+)=\"{0}\"", extractor.Groups["value"]);
                var newSoapHeader = Regex.Match(xml, dynamicSearch, RegexOptions.IgnoreCase);
                var xmlnsToHead = Regex.Match(xml, ":part.*(?<xmlnsTag>xmlns:[a-zA-Z0-9\"/:=.]+\")", RegexOptions.IgnoreCase);

                xml = new Regex("(?<SoapHeader>[a-zA-Z0-9]+):HeaderSoap").Replace(xml, String.Format("{0}:HeaderSoap", newSoapHeader.Groups["SoapHeader"]));
                xml = new Regex(":part.*(?<xmlnsTag>xmlns:[a-zA-Z0-9\"/:=.]+\")").Replace(xml, String.Empty);
                xml = new Regex(":definitions ").Replace(xml, String.Format(":definitions {0} ", xmlnsToHead.Groups["xmlnsTag"]));
            }            

            if (String.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine(xml.ToString());
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
