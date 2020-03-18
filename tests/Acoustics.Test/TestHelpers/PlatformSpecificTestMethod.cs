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
        /// <param name="platform">
        /// The platform to run the test on.
        /// If the value is prefixed with an `!` (exclaimation mark)
        /// then the provided platform is NOT tested against and all others are
        /// tested.
        /// </param>
        public PlatformSpecificTestMethod(string platform)
            : this(platform, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformSpecificTestMethod"/> class.
        /// </summary>
        /// <param name="platform">
        /// The platform to run the test on.
        /// If the value is prefixed with an `!` (exclaimation mark)
        /// then the provided platform is NOT tested against and all others are
        /// tested.
        /// </param>
        /// <param name="displayName">The friendly name for this test.</param>
        public PlatformSpecificTestMethod(string platform, string displayName)
            : base(displayName)
        {
            this.IgnorePlatform = platform.StartsWith("!");
            this.Platform = platform
                .Substring(this.IgnorePlatform ? 1 : 0)
                .ToUpper() switch
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
        /// Gets a value indicating whether this test method ignores the provided
        /// <see cref="Platform"/> value or whether it runs the test only on that
        /// platform.
        /// </summary>
        /// <value>True if the <see cref="Platform"/> should be ignored, false if
        /// <see cref="Platform"/> should be the only platform tested.</value>
        public bool IgnorePlatform { get; }

        /// <summary>
        /// Gets or sets an optional reason to add into the test result message.
        /// </summary>
        public string Reason { get; set; }

        /// <inheritdoc />
        public override TestResult[] Execute(ITestMethod testMethod)
        {
            var platformMatches = RuntimeInformation.IsOSPlatform(this.Platform);

            //                       | platformMatches==true | platformMatches==false |
            // IgnorePlatform==true  |        Ignore         |        Run             |
            // IgnorePlatform==false |        Run            |        Ignore          |

            // euqlaity operates likes XNOR
            if (platformMatches == this.IgnorePlatform)
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
