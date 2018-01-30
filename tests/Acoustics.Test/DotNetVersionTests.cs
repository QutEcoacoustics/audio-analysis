// <copyright file="DotNetVersionTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test
{
    using System;
    using System.IO;
    using System.Text;
    using Acoustics.Shared.Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DotNetVersionTests
    {
        [TestMethod]
        [Ignore]
        public void HasSupportForLongPaths()
        {
            var random = TestHelpers.Random.GetRandom();
            var longPath = " \\\\?\\C:\\";
            while (longPath.Length < 1024)
            {
                longPath += random.NextGuid().ToString();
            }

            // this should fail if not supported
            var info = new FileInfo(longPath);
        }
    }
}
