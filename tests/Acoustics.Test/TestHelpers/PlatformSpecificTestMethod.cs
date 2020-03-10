// <copyright file="PlatformSpecificTestMethod.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class PlatformSpecificTestMethod : TestMethodAttribute
    {
        public PlatformSpecificTestMethod(string platform)
            : this(platform, null)
        {
        }

        public PlatformSpecificTestMethod(string platform, string displayName)
            : base(displayName)
        {
            this.Platform = platform.ToUpper() switch
            {
                "WINDOWS" => OSPlatform.Windows,
                "OSX" => OSPlatform.OSX,
                "LINUX" => OSPlatform.Linux,
                "FREEBSD" => OSPlatform.FreeBSD,
                _ => throw new ArgumentException($"Unknown platform `{platform}`.", nameof(platform)),
            };
        }

        public OSPlatform Platform { get; }

        public override TestResult[] Execute(ITestMethod testMethod)
        {
            if (!RuntimeInformation.IsOSPlatform(this.Platform))
            {
                var message = $"Test not executed. The current platform is not {this.Platform}";
                return new[]
                {
                    new TestResult
                    {
                        Outcome = UnitTestOutcome.Inconclusive,
                        TestFailureException = new AssertInconclusiveException(message),
                    },
                };
            }

            return base.Execute(testMethod);
        }
    }
}
