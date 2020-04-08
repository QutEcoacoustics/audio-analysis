// <copyright file="RetryTestMethodAttribute.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RetryTestMethodAttribute : TestMethodAttribute
    {
        public RetryTestMethodAttribute(int retryCount)
        {
            if (retryCount < 0)
            {
                throw new ArgumentException($"{nameof(retryCount)} cannot be less than 0", nameof(retryCount));
            }

            this.Retrycount = retryCount;
        }

        public int Retrycount { get; }

        public override TestResult[] Execute(ITestMethod testMethod)
        {
            // always run at least once, and then rety n times if fails
            int retryCount = this.Retrycount + 1;

            var result = new List<TestResult>();

            for (int count = 0; count < retryCount; count++)
            {
                var testResults = base.Execute(testMethod);
                result.AddRange(testResults);

                var failed = testResults.FirstOrDefault((tr) => tr.Outcome == UnitTestOutcome.Failed);
                if (failed != null)
                {
                    var isLast = count + 1 == retryCount;
                    var retryString = isLast ? " Attempting to run again." : " Exhausted retries, remaining failed.";
                    failed.TestContextMessages += $"Test iteration {count + 1} of {retryCount} failed.{retryString}";

                    if (!isLast)
                    {
                        failed.Outcome = UnitTestOutcome.Inconclusive;
                    }

                    continue;
                }

                break;
            }

            return result.ToArray();
        }
    }
}