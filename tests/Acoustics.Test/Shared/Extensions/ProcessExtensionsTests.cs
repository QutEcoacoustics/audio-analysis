// <copyright file="ProcessExtensionsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared.Extensions
{
#if DEBUG
    using Acoustics.Test.TestHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static System.ProcessExtensions.ParentProcessUtilities;

    [TestClass]
    public class ProcessExtensionsTests
    {
        [PlatformSpecificTestMethod("Windows", Reason = "https://github.com/dotnet/runtime/issues/24423")]
        public void TestGetParentProcessReturnsProcessOnWindows()
        {
            Assert.IsNotNull(GetParentProcess());
        }

        [PlatformSpecificTestMethod("!Windows", Reason = "https://github.com/dotnet/runtime/issues/24423")]
        public void TestGetParentProcessReturnsNullWhenNotWindows()
        {
            Assert.IsNull(GetParentProcess());
        }
    }
#endif
}