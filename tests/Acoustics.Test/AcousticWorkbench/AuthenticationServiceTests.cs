// <copyright file="AuthenticationServiceTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AcousticWorkbench
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AuthenticationServiceTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var task = Task.Delay(1.5.Seconds());

            task.Wait(60.Seconds());
        }
    }
}