// <copyright file="ApiTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AcousticWorkbench
{
    using System;
    using global::AcousticWorkbench;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ApiTests
    {
        [TestMethod]
        public void TestParse()
        {
            var api = Api.Parse("https://www.ecosounds.org/v2");

            Assert.AreEqual("www.ecosounds.org", api.Host);
            Assert.AreEqual("https", api.Protocol);
            Assert.AreEqual("v2", api.Version);
        }

        [TestMethod]
        public void TestParseVersionDefaultsToV1()
        {
            var api = Api.Parse("https://www.ecosounds.org");

            Assert.AreEqual("www.ecosounds.org", api.Host);
            Assert.AreEqual("https", api.Protocol);
            Assert.AreEqual("v1", api.Version);
        }

        [TestMethod]
        public void TestDefault()
        {
            var api = Api.Default;

            Assert.AreEqual("www.ecosounds.org", api.Host);
            Assert.AreEqual("https", api.Protocol);
            Assert.AreEqual("v1", api.Version);
        }

        [TestMethod]
        public void TestToString()
        {
            var api = Api.Default;

            Assert.AreEqual("{Protocol=\"https\", Host=\"www.ecosounds.org\", Version=\"v1\"}", api.ToString());
        }

        [DataTestMethod]
        [DataRow("ftp://www.ecosounds.org/v2", "Only https is supported")]
        [DataRow("http://www.ecosounds.org/v2", "Only https is supported")]
        [DataRow("https://www.ecosounds.org/listen", "Invalid version")]
        [DataRow("https://www.ecosounds.org:8080", "Port specifications are not supported")]
        [DataRow("..test", "Uri could not be parsed (ensure it is absolute and valid)")]
        [DataRow("", "Cannot be null or empty")]
        public void TestParseErrors(string uri, string exceptionMessage)
        {
            Assert.ThrowsException<ArgumentException>(
                () =>
                {
                    Api.Parse(uri);
                },
                exceptionMessage);
        }
    }
}