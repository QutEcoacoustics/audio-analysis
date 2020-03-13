// <copyright file="PlatformSpecificTestMethod.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Runs a test on a specific platform.
    /// </summary>
    public class PlatformSpecificTestMethod : TestMethodAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformSpecificTestMethod"/> class.
        /// </summary>
        /// <param name="platform">The platform to run the test on.</param>
        public PlatformSpecificTestMethod(string platform)
            : this(platform, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformSpecificTestMethod"/> class.
        /// </summary>
        /// <param name="platform">The platform to run the test on.</param>
        /// <param name="displayName">The friendly name for this test.</param>
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

        /// <summary>
        /// Gets the platform to run the test on.
        /// </summary>
        public OSPlatform Platform { get; }

        /// <summary>
        /// Gets or sets an optional reason to add into the test result message.
        /// </summary>
        public string Reason { get; set; }

        /// <inheritdoc />
        public override TestResult[] Execute(ITestMethod testMethod)
        {
            if (!RuntimeInformation.IsOSPlatform(this.Platform))
            {
                var message = $"Test not executed. The current platform is not {this.Platform}{this.Reason?.Prepend(" ")}.";
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
