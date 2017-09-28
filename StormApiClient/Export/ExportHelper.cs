
using System.Runtime.Serialization;
using Enferno.StormApiClient.Products;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Channels;
using System.Xml;

namespace Enferno.StormApiClient.Export
{
    public class ExportHelper
    {
        public IEnumerable<ProductItem> ListProductItems(Guid applicationKey, string statusSeed, DateTime? lastUpdatedFrom, string pricelistSeed = null, string cultureCode = null)
        {
            using (var client = new ExportProxy.ExportServiceClient())
            {
                var response = client.ListProductItems(CreateRequestMessage("Enferno.Services/ExportService/ListProductItems", applicationKey, statusSeed, lastUpdatedFrom, pricelistSeed, cultureCode));

                foreach (var productItem in ReadData<ProductItem>(response.GetReaderAtBodyContents(), "ArrayOfProductItem", "ProductItem"))
                    yield return productItem;
            }
        }

        public IEnumerable<Product> ListProducts(Guid applicationKey, string statusSeed, DateTime? lastUpdatedFrom = null, string pricelistSeed = null, string cultureCode = null)
        {
            using (var client = new ExportProxy.ExportServiceClient())
            {
                var response = client.ListProducts(CreateRequestMessage("Enferno.Services/ExportService/ListProducts", applicationKey, statusSeed, lastUpdatedFrom, pricelistSeed, cultureCode));

                foreach (var product in ReadData<Product>(response.GetReaderAtBodyContents(), "ArrayOfProduct", "Product"))
                    yield return product;
            }
        }

        private static Message CreateRequestMessage(string action, Guid applicationKey, string statusSeed, DateTime? lastUpdatedFrom, string pricelistSeed, string cultureCode)
        {
            var request = Message.CreateMessage(MessageVersion.Soap11, action);

            const string ns = "Enferno.Services";
            request.Headers.Add(MessageHeader.CreateHeader("applicationKey", ns, applicationKey));
            request.Headers.Add(MessageHeader.CreateHeader("statusSeed", ns, statusSeed));
            request.Headers.Add(MessageHeader.CreateHeader("lastUpdatedFrom", ns, lastUpdatedFrom));
            request.Headers.Add(MessageHeader.CreateHeader("pricelistSeed", ns, pricelistSeed));
            request.Headers.Add(MessageHeader.CreateHeader("cultureCode", ns, cultureCode));

            return request;
        }

        private static IEnumerable<T> ReadData<T>(XmlReader dataReader, string startElement, string localName)
        {
            dataReader.ReadStartElement(startElement);

            while (!dataReader.EOF && dataReader.LocalName == localName)
                yield return XmlSerializeFromString<T>(dataReader.ReadOuterXml());

            dataReader.ReadEndElement();
        }

        public static T XmlSerializeFromString<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return default(T);

            var serializer = new DataContractSerializer(typeof(T));

            using (var sr = new StringReader(xml))
            using (var reader = new XmlTextReader(sr))
            {
                return (T)serializer.ReadObject(reader);
            }
        }
    }
}
