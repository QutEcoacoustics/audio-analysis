// <copyright file="GeneratedImageTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using Acoustics.Shared.ImageSharp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    public abstract class GeneratedImageTest<T> : OutputDirectoryTest
        where T : struct, IPixel<T>
    {
        protected GeneratedImageTest()
            : this(WriteTestOutput.OnFailure)
        {
        }

        protected GeneratedImageTest(WriteTestOutput writeImages)
            : this(writeImages, writeImages)
        {
        }

        protected GeneratedImageTest(WriteTestOutput writeImages, WriteTestOutput writeDelta)
        {
            this.WriteImages = writeImages;
            this.WriteDelta = writeDelta;
        }

        protected WriteTestOutput WriteImages { get; }

        protected WriteTestOutput WriteDelta { get; }

        protected Image<T> Actual { get; set; }

        protected Image<T> Expected { get; set; }

        [TestInitialize]
        public virtual void TestSetup()
        {
            this.Actual = new Image<T>(Configuration.Default, 100, 100, Color.Black.ToPixel<T>());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Assert.IsNotNull(this.Actual, "The actual image cannot be null!");

            if (this.ShouldWrite(this.WriteImages))
            {
                this.SaveImage("actual", this.Actual);
                this.SaveImage("expected", this.Expected);
            }

            if (this.ShouldWrite(this.WriteDelta))
            {
                if (this.Expected == null)
                {
                    this.TestContext.WriteLine($"Skipping writing delta image because `Expected` is null - cannot delta between actual and null!");
                    return;
                }

                var delta = this.Expected.Clone(
                    x =>
                    {
                        var deltaProcessor = new DeltaImageProcessor<T>(this.Actual);
                        x.ApplyProcessor(deltaProcessor);
                    });
                this.SaveImage("delta", delta);
            }
        }

        protected void AssertImagesEqual()
        {
            Assert.That.ImageMatches(this.Expected, this.Actual);
        }

        protected void SaveImage(string token, Image<T> image)
        {
            var path = this.ClassOutputDirectory.CombinePath($"{this.TestContext.TestName}_{token}.png");
            if (image == null)
            {
                this.TestContext.WriteLine($"Skipping writing expected image `{path}` because it is null");
                return;
            }

            image.Save(path);
            this.TestContext.AddResultFile(path);
        }

        private bool ShouldWrite(WriteTestOutput should) =>
            should switch
            {
                WriteTestOutput.Always => true,
                WriteTestOutput.Never => false,
                WriteTestOutput.OnFailure => this.TestContext.CurrentTestOutcome != UnitTestOutcome.Passed,
                _ => throw new InvalidOperationException(),
            };
    }
}
