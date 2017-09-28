
using System;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Enferno.StormApiClient
{
    public static class EntityExtension
    {
        public static string GetCacheKey(this Expose.Request request, string thumbprint = null)
        {
            var type = request.GetType();
            var properties = type.GetProperties();

            var buff = new StringBuilder();
            buff.AppendFormat("{0}:", RemoveRequestFromName(type.Name));
            if (thumbprint != null) buff.AppendFormat("{0}:", thumbprint);
            foreach (var property in properties.Where(p => p.Name != "ExtensionData"))
            {
                buff.AppendFormat("{0}:", property.GetValue(request, null));
            }
            return buff.ToString();
        }

        public static object GetResult(this Expose.Response response)
        {
            Type type = response.GetType();
            var property = type.GetProperties().FirstOrDefault(p => p.Name == "Result");
            if (property != null)
            {
                return property.GetValue(response, null);
            }
            return null;
        }

        private static string RemoveRequestFromName(string name)
        {
            return name.Substring(0, name.Length - "request".Length);
        }

        public static string ToStringEx(this Expose.Message.Entity entity)
        {
            var serializer = new DataContractSerializer(entity.GetType());

            var buff = new StringBuilder();
            using(var sw = new StringWriter(buff))
            using (var writer = new XmlTextWriter(sw))
            {
                serializer.WriteObject(writer, entity);
                return buff.ToString();
            }
        }

        public static XElement ToXml(this Expose.Message.Entity entity)
        {
            XDocument doc = XDocument.Parse(entity.ToStringEx());
            return doc.Root;
        }

        public static T FromString<T>(this string xml)
        {
            if (string.IsNullOrEmpty(xml)) return default(T);

            var serializer = new DataContractSerializer(typeof(T));

            using (var sr = new StringReader(xml))
            using (var reader = new XmlTextReader(sr))
            {
                return (T)serializer.ReadObject(reader, false);
            }
        }
    }
}
