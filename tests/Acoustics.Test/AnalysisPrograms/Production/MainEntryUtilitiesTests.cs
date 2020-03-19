// <copyright file="MainEntryUtilitiesTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Production
{
    using System;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisPrograms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MainEntryUtilitiesTests
    {
#if DEBUG
        [TestMethod]
        public void TestHangBeforeExitDoesNotThrowException()
        {
            Assertions.DoesNotThrow<NullReferenceException>(() => MainEntry.HangBeforeExit());
        }
#endif
    }
}