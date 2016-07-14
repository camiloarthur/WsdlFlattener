﻿using System;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using ServiceDescription = System.Web.Services.Description.ServiceDescription;

namespace WsdlFlattener
{
    public class Flattener
    {
        public string GetFlattenedWsdl(string wsdlUrl, string filePathAndName, System.Net.NetworkCredential mexNetworkCredentials)
        {
            var metadata = GetServiceMetadata(wsdlUrl, MetadataExchangeClientMode.HttpGet, mexNetworkCredentials);
            var newWsdl = GetFlattenedServiceDescription(metadata);

            if (String.IsNullOrEmpty(filePathAndName))
                filePathAndName = Path.GetTempFileName();

            var tempPath = filePathAndName;
            newWsdl.Write(tempPath);

            string xml;

            using (var sr = new StreamReader(tempPath))
            {
                xml = sr.ReadToEnd();
            }

            if(File.Exists(tempPath))
                File.Delete(tempPath);

            return xml;
        }

        private MetadataSet GetServiceMetadata(string wsdlUrl, MetadataExchangeClientMode clientMode, System.Net.NetworkCredential mexNetworkCredentials)
        {
            MetadataSet metadata = null;
            Binding mexBinding;

            if (clientMode == MetadataExchangeClientMode.HttpGet){
                mexBinding = new BasicHttpBinding { MaxReceivedMessageSize = 50000000L };
            }
            else
            {
                mexBinding = new WSHttpBinding(SecurityMode.None) { MaxReceivedMessageSize = 50000000L };
            }

            var mexClient = new MetadataExchangeClient(mexBinding)
            {
                ResolveMetadataReferences = true,
                MaximumResolvedReferences = 200,
                HttpCredentials = mexNetworkCredentials
            };

            try
            {
                metadata = mexClient.GetMetadata(new Uri(wsdlUrl), clientMode);
            } catch (Exception ex) {
                Console.WriteLine(String.Format("Error: {0}", ex.Message));
            }

            return metadata;
        }

        public ServiceDescription GetFlattenedServiceDescription(MetadataSet metadataSet)
        {
            var obj = Assembly.GetAssembly(typeof(WsdlExporter)).GetType("System.ServiceModel.Description.WsdlHelper", true)
                .GetMethod("GetSingleWsdl", BindingFlags.Static | BindingFlags.Public)
                .Invoke(null, new object[1] { metadataSet });

            if (obj is ServiceDescription)
                return (ServiceDescription)obj;

            return null;
        }
    }
}
