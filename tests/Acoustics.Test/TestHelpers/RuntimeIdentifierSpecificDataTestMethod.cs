// <copyright file="RuntimeIdentifierSpecificTestMethod.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Acoustics.Shared;
    using JetBrains.Annotations;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class RuntimeIdentifierSpecificDataTestMethod : DataTestMethodAttribute
    {

        public RuntimeIdentifierSpecificDataTestMethod([CanBeNull] string ignoreMessage = null)
        {
            this.IgnoreMessage = ignoreMessage;
        }

        public string IgnoreMessage { get; }

        public override TestResult[] Execute(ITestMethod testMethod)
        {
            var rid = testMethod.Arguments[0] as string;
            if (rid.IsNullOrEmpty())
            {
                throw new ArgumentException(
                    $"The first argument of a {nameof(RuntimeIdentifierSpecificDataTestMethod)} must be a string representing an RID - but we got a null or empty string");
            }

            if (!AppConfigHelper.WellKnownRuntimeIdentifiers.Contains(rid))
            {
                throw new ArgumentException(
                    $"The first argument of a {nameof(RuntimeIdentifierSpecificDataTestMethod)} must be a string representing an RID - but we got <{rid}> which is not one of our well known RIDs");
            }

            var actualRid = AppConfigHelper.PseudoRuntimeIdentifier;

            if (rid != actualRid)
            {
                var message = $"Test not executed. The current RID <{actualRid}> is not <{rid}>. {this.IgnoreMessage}";
                Trace.WriteLine(message);
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
