namespace Acoustics.Shared
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// The xml serialise tool.
    /// </summary>
    public static class XmlSerialiseTool
    {
        /// <summary>
        /// Creates an object from an XML string.
        /// </summary>
        /// <param name="xml">
        /// Xml representing object.
        /// </param>
        /// <param name="objType">
        /// The obj type.
        /// </param>
        /// <returns>
        /// Created object.
        /// </returns>
        public static object FromXml(string xml, Type objType)
        {
            object obj;

            var xmlSerializer = new XmlSerializer(objType);

            using (var stringReader = new StringReader(xml))
            using (var xmlTextReader = new XmlTextReader(stringReader))
            {
                obj = xmlSerializer.Deserialize(xmlTextReader);
            }

            return obj;
        }

        /// <summary>
        /// Convert a utf-8 string to byte array.
        /// </summary>
        /// <param name="pXmlString">
        /// String to convert.
        /// </param>
        /// <returns>
        /// Byte array.
        /// </returns>
        public static byte[] StringToUtf8ByteArray(string pXmlString)
        {
            var encoding = new UTF8Encoding();
            byte[] byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }

        /// <summary>
        /// Serializes the <paramref name="obj"/> to an XML string.
        /// </summary>
        /// <param name="obj">
        /// Object to serialise to XML.
        /// </param>
        /// <param name="objType">
        /// Type of <paramref name="obj"/>.
        /// </param>
        /// <returns>
        /// Xml representing <paramref name="obj"/>.
        /// </returns>
        /// <remarks>
        /// http://www.c-sharpcorner.com/UploadFile/chauhan_sonu57/SerializingObjects07202006065806AM/SerializingObjects.aspx.
        /// </remarks>
        public static string ToXml(object obj, Type objType)
        {
            string xml = string.Empty;

            var xmlSerializer = new XmlSerializer(objType);

            var writerSettings = new XmlWriterSettings();
            writerSettings.OmitXmlDeclaration = true;

            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            using (var stringWriter = new StringWriter())
            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, writerSettings))
            {
                xmlSerializer.Serialize(xmlWriter, obj, ns);

                xml = stringWriter.ToString();
                xml = xml.Substring(xml.IndexOf(Convert.ToChar(60))); // <
                xml = xml.Substring(0, xml.LastIndexOf(Convert.ToChar(62)) + 1); // >
            }

            return xml;
        }

        /// <summary>
        /// Convert a byte array to utf-8 string.
        /// </summary>
        /// <param name="characters">
        /// The characters.
        /// </param>
        /// <returns>
        /// The utf-8  string.
        /// </returns>
        public static string Utf8ByteArrayToString(byte[] characters)
        {
            var encoding = new UTF8Encoding();
            string constructedString = encoding.GetString(characters);
            return constructedString;
        }
    }
}