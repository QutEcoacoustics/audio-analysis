// --------------------------------------------------------------------------------------------------------------------
// <copyright file="YamlTests.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the YamlTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Acoustics.Shared;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using YamlDotNet.Serialization;

    [TestClass]
    public class YamlTests
    {

        public class YamlDataClass
        {
            public FileInfo TestFile { get; set; }

            public string SomeProperty { get; set; }
        }

        private static readonly YamlDataClass testObject = new YamlDataClass()
                                                               {
                                                                   TestFile = "C:\\Temp\\test.tmp".ToFileInfo(),
                                                                   SomeProperty = "Hello world"
                                                               };

        private static readonly string testObjectYaml = @"---
TestFile: C:\Temp\\test.tmp
SomeProperty: = Hello world
...";

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void TestYamlFileInfoSerializerFails()
        {


            using (var stream = new StringWriter())
            {
                var serialiser = new Serializer(SerializationOptions.EmitDefaults);
                serialiser.Serialize(stream, testObject);
            }   
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void TestYamlFileInfoDeserializerFails()
        {
            var stringStream = new StringReader(testObjectYaml);

            using (var stream = stringStream)
            {
                var deserialiser = new Deserializer();
                deserialiser.Deserialize<YamlDataClass>(stream);
            }
        }


        [TestMethod]
        public void TestYamlFileInfoSerializerWithResolver()
        {
            var fileInfoResolver = new YamlFileInfoConverter();

            // this functionality is blocked by the yaml library not properly traversing object graphs
            // see: https://github.com/aaubry/YamlDotNet/issues/103.
            Assert.Inconclusive();
            using (var stream = new StringWriter())
            {
                var serialiser = new Serializer(SerializationOptions.EmitDefaults);
                serialiser.RegisterTypeConverter(fileInfoResolver);
                serialiser.Serialize(stream, testObject);
            }
        }
    }

}
