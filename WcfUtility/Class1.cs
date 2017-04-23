using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace WcfUtility
{
    public static class WcfSerializer
    {
        public static XDocument Serialize<T>(T objectToSerialize)
        {
            if (objectToSerialize == null)
            {
                return XDocument.Parse("<Root>Object is null</Root>");
            }

            var type = objectToSerialize.GetType();
            if (type.Name.Contains("AnonymousType"))
            {
                return new XDocument("<Root>AnonymousType</Root>");
            }

            var serializer = new XmlSerializer(objectToSerialize.GetType());
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, objectToSerialize);

                return XDocument.Parse(writer.ToString());
            }
        }
    }
}