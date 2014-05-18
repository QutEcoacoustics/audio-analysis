namespace System.Xml.Linq
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using System;

    /// <summary>
    /// Xml Extension methods.
    /// </summary>
    public static class ExtensionsXml
    {
        #region internal class XmlTranslator

        internal class XmlTranslator
        {
            private readonly StringBuilder _xmlTextBuilder;
            private readonly XmlWriter _writer;

            private XmlTranslator()
            {
                _xmlTextBuilder = new StringBuilder();

                _writer = new XmlTextWriter(new StringWriter(_xmlTextBuilder))
                {
                    Formatting = Formatting.Indented,
                    Indentation = 2
                };
            }

            public XmlTranslator(XNode e)
                : this()
            {
                e.WriteTo(_writer);
            }

            public XmlTranslator(XmlNode e)
                : this()
            {
                e.WriteTo(_writer);
            }

            public XElement CreateXElement()
            {
                return XElement.Load(new StringReader(_xmlTextBuilder.ToString()));
            }

            public XDocument CreateXDocument()
            {
                return XDocument.Load(new StringReader(_xmlTextBuilder.ToString()));
            }

            public XmlElement CreateXmlElement()
            {
                return CreateXmlDocument().DocumentElement;
            }

            public XmlDocument CreateXmlDocument()
            {
                var doc = new XmlDocument();
                doc.Load(new XmlTextReader(new StringReader(_xmlTextBuilder.ToString())));
                return doc;
            }
        }

        #endregion

        public static XmlElement ToXmlElement(this XElement xEl)
        {
            return new XmlTranslator(xEl).CreateXmlElement();
        }

        public static XmlDocument ToXmlDocument(this XDocument xDoc)
        {
            return new XmlTranslator(xDoc).CreateXmlDocument();
        }

        public static XElement ToXElement(this XmlElement xmlEl)
        {
            return new XmlTranslator(xmlEl).CreateXElement();
        }

        public static XDocument ToXDocument(this XmlDocument xmlDoc)
        {
            return new XmlTranslator(xmlDoc).CreateXDocument();
        }

        /// <summary>
        /// Get the absolute XPath to a given XElement
        /// (e.g. "/people/person[6]/name[1]/last[1]").
        /// </summary>
        /// <param name="element">
        /// The element to get the index of.
        /// </param>
        /// <remarks>
        /// Extension methods for the .NET 3.5 System.Xml.Linq namespace.
        /// From: http://seattlesoftware.wordpress.com/2009/03/13/get-the-xpath-to-an-xml-element-xelement/
        /// </remarks>
        /// <returns>
        /// The absolute x path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="element" /> is <c>null</c>.</exception>
        public static string AbsoluteXPath(this XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            Func<XElement, string> relativeXPath = e =>
            {
                int index = e.IndexPosition();
                string name = e.Name.LocalName;

                // If the element is the root, no index is required
                return (index == -1) ? "/" + name : string.Format("/{0}[{1}]", name, index.ToString());
            };

            var ancestors = from e in element.Ancestors()
                            select relativeXPath(e);

            return string.Concat(ancestors.Reverse().ToArray()) +
                   relativeXPath(element);
        }

        /// <summary>
        /// Get the index of the given XElement relative to its
        /// siblings with identical names. If the given element is
        /// the root, -1 is returned.
        /// </summary>
        /// <param name="element">
        /// The element to get the index of.
        /// </param>
        /// <remarks>
        /// Extension methods for the .NET 3.5 System.Xml.Linq namespace.
        /// From: http://seattlesoftware.wordpress.com/2009/03/13/get-the-xpath-to-an-xml-element-xelement/
        /// </remarks>
        /// <returns>
        /// The index position.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="element" /> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">element has been removed from its parent.</exception>
        public static int IndexPosition(this XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (element.Parent == null)
            {
                return -1;
            }

            int i = 1; // Indexes for nodes start at 1, not 0

            foreach (var sibling in element.Parent.Elements(element.Name))
            {
                if (sibling == element)
                {
                    return i;
                }

                i++;
            }

            throw new InvalidOperationException("element has been removed from its parent.");
        }

        /// <summary>
        /// Serialize an object into an XML string.
        /// </summary>
        /// <typeparam name="T">Type of object to serialise.</typeparam>
        /// <param name="obj">Object to serialise.</param>
        /// <returns>Object serialised as xml string.</returns>
        public static string SerializeObject<T>(this T obj)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(T));
                    xs.Serialize(xmlTextWriter, obj);

                    string xmlString = ((MemoryStream)xmlTextWriter.BaseStream).ToArray().Utf8ByteArrayToString();
                    return xmlString;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Reconstruct an object from an XML string.
        /// </summary>
        /// <typeparam name="T">
        /// Type of object to deserialise.
        /// </typeparam>
        /// <param name="xml">
        /// serialized xml string.
        /// </param>
        /// <returns>
        /// Deserialised object T.
        /// </returns>
        public static T DeserializeObject<T>(this string xml)
        {
            using (MemoryStream memoryStream = new MemoryStream(xml.StringToUtf8ByteArray()))
            using (new XmlTextWriter(memoryStream, Encoding.UTF8))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                return (T)xs.Deserialize(memoryStream);
            }
        }
    }
}
