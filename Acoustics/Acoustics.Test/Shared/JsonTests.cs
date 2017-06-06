// --------------------------------------------------------------------------------------------------------------------
// <copyright file="YamlTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the YamlTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Acoustics.Test.Shared
{
    using System;
    using Acoustics.Shared;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class JsonTests
    {
        private const string LagacyTimeSpanDataConverterTestInt = @"{""A"":123}";
        private const string LagacyTimeSpanDataConverterTestIntTimeSpan = @"{""A"":""123""}";
        private const string LagacyTimeSpanDataConverterTestIntTimeSpan2 = @"{""A"":""14.06:56:07""}";
        private const string LagacyTimeSpanDataConverterTestDouble = @"{""B"":123.456}";
        private const string LagacyTimeSpanDataConverterTestDoubleTimeSpan = @"{""B"":""12:34:56""}";
        private const string LagacyTimeSpanDataConverterTestDoubleTimeSpan2 = @"{""B"":""14.06:56:07.8900000""}";

        [TestMethod]
        public void TestLegacyConverterInt()
        {
            var data = JsonConvert.DeserializeObject<TestDataClass>(LagacyTimeSpanDataConverterTestInt);
            Assert.AreEqual(123, data.A);
        }

        [TestMethod]
        public void TestLegacyConverterIntTimeSpan()
        {
            var data = JsonConvert.DeserializeObject<TestDataClass>(LagacyTimeSpanDataConverterTestIntTimeSpan);
            Assert.AreEqual(TimeSpan.Parse("123").TotalSeconds, data.A);
        }

        [TestMethod]
        public void TestLegacyConverterIntTimeSpan2()
        {
            var data = JsonConvert.DeserializeObject<TestDataClass>(LagacyTimeSpanDataConverterTestIntTimeSpan2);
            Assert.AreEqual(1234567, data.A);
        }

        [TestMethod]
        public void TestLegacyConverterDouble()
        {
            var data = JsonConvert.DeserializeObject<TestDataClass>(LagacyTimeSpanDataConverterTestDouble);
            Assert.AreEqual(123.456, data.B);
        }

        [TestMethod]
        public void TestLegacyConverterDoubleTimeSpan()
        {
            var data = JsonConvert.DeserializeObject<TestDataClass>(LagacyTimeSpanDataConverterTestDoubleTimeSpan);
            Assert.AreEqual(TimeSpan.Parse("12:34:56").TotalSeconds, data.B);
        }

        [TestMethod]
        public void TestLegacyConverterDoubleTimeSpan2()
        {
            var data = JsonConvert.DeserializeObject<TestDataClass>(LagacyTimeSpanDataConverterTestDoubleTimeSpan2);
            Assert.AreEqual(1234567.89, data.B);
        }

        public class TestDataClass
        {
            [JsonConverter(typeof(Json.LegacyTimeSpanDataConverter))]
            public int A { get; set; }

            [JsonConverter(typeof(Json.LegacyTimeSpanDataConverter))]
            public double B { get; set; }
        }

    }
}