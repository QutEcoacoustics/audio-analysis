// <copyright file="EnumExtensionsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared.Extensions
{
    using System;
    using Acoustics.Shared;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EnumExtensionsTests
    {
        [TestMethod]
        public void TestToImageChrome()
        {
            Assert.AreEqual(ImageChrome.With, true.ToImageChrome());
            Assert.AreEqual(ImageChrome.Without, false.ToImageChrome());
        }

        [TestMethod]
        public void TestPrintOptionsContract()
        {
            Assert.ThrowsException<ArgumentException>(
                () => typeof(object).PrintEnumOptions());

            Type type = null;
            Assert.ThrowsException<ArgumentException>(
                () => type.PrintEnumOptions());
        }

        [TestMethod]
        public void TestPrintOptions()
        {
            Assert.AreEqual("ToEven|AwayFromZero|ToZero|ToNegativeInfinity|ToPositiveInfinity", typeof(MidpointRounding).PrintEnumOptions());
        }
    }
}
