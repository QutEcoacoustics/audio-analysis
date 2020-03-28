// <copyright file="RuntimeIdentifierSpecificDataTestMethod.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Diagnostics;

    using Acoustics.Shared;
    using global::AnalysisPrograms;
    using JetBrains.Annotations;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public enum RidType
    {
        Pseudo,
        Compiled,
        CompiledIfSelfContained,
        Actual,
    }

    public class RuntimeIdentifierSpecificDataTestMethod : DataTestMethodAttribute
    {
        public RuntimeIdentifierSpecificDataTestMethod(
            RidType runtimeIdentifierSource,
            [CanBeNull] string ignoreMessage = null)
        {
            this.RuntimeIdentifierSource = runtimeIdentifierSource;
            this.IgnoreMessage = ignoreMessage;
        }

        public RidType RuntimeIdentifierSource { get; set; }

        public string IgnoreMessage { get; }

        public override TestResult[] Execute(ITestMethod testMethod)
        {
            var rid = testMethod.Arguments[0] as string;
            if (rid.IsNullOrEmpty())
            {
                throw new ArgumentException(
                    $"The first argument of a {nameof(RuntimeIdentifierSpecificDataTestMethod)} must be a string representing an RID - but we got a null or empty string");
            }

            if (this.RuntimeIdentifierSource != RidType.Actual && !AppConfigHelper.WellKnownRuntimeIdentifiers.Contains(rid))
            {
                throw new ArgumentException(
                    $"The first argument of a {nameof(RuntimeIdentifierSpecificDataTestMethod)} must be a string representing an RID - but we got <{rid}> which is not one of our well known RIDs");
            }

            var pseudo = AppConfigHelper.PseudoRuntimeIdentifier;
            var actual = AppConfigHelper.RuntimeIdentifier;
            var compiled = BuildMetadata.CompiledRuntimeIdentifer;

            var compiledEmpty = compiled.IsNullOrEmpty();
            var selfContained = BuildMetadata.CompiledAsSelfContained;

            var actualRid = this.RuntimeIdentifierSource switch
            {
                RidType.Actual => actual,
                RidType.Compiled => compiled,

                // compiled will be empty when the --runtime argument is not supplied
                RidType.CompiledIfSelfContained when selfContained && !compiledEmpty => compiled,
                RidType.CompiledIfSelfContained when !selfContained && !compiledEmpty => compiled,
                RidType.CompiledIfSelfContained when selfContained && compiledEmpty => throw new InvalidOperationException(
                    $"Compiled RID is empty! Must be non-empty for mode {nameof(RidType.CompiledIfSelfContained)}."),
                RidType.CompiledIfSelfContained when !selfContained && compiledEmpty => pseudo,
                RidType.Pseudo => pseudo,
                _ => throw new InvalidOperationException($"RidType {this.RuntimeIdentifierSource} is not supported"),
            };

            string debugMessage = $"RIDs: Pseudo={pseudo}, Actual={actual}, Compiled={compiled}. Using={this.RuntimeIdentifierSource} which is {actualRid}.";

            if (rid != actualRid)
            {
                var message = $"Test not executed. The current RID <{actualRid}> is not <{rid}>. {this.IgnoreMessage}";
                Trace.WriteLine(message);
                return new[]
                {
                    new TestResult
                    {
                        TestContextMessages = debugMessage,
                        Outcome = UnitTestOutcome.Inconclusive,
                        TestFailureException = new AssertInconclusiveException(message),
                    },
                };
            }

            return base.Execute(testMethod);
        }
    }
}
