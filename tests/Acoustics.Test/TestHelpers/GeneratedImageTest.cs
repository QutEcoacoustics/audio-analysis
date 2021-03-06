// <copyright file="GeneratedImageTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared.ImageSharp;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using Towel.DataStructures;
    using TowseyLibrary;

    public abstract class GeneratedImageTest<T> : OutputDirectoryTest
        where T : unmanaged, IPixel<T>
    {
        protected GeneratedImageTest()
            : this(WriteTestOutput.OnFailure)
        {
        }

        protected GeneratedImageTest(WriteTestOutput writeImages)
            : this(writeImages, writeImages, null)
        {
        }

        protected GeneratedImageTest(Image<T> baseActual)
            : this(WriteTestOutput.OnFailure, WriteTestOutput.OnFailure, baseActual)
        {
        }

        protected GeneratedImageTest(WriteTestOutput writeImages, WriteTestOutput writeDelta, Image<T> baseActual)
        {
            this.WriteImages = writeImages;
            this.WriteDelta = writeDelta;
            this.ActualImage = baseActual;
        }

        protected WriteTestOutput WriteImages { get; }

        protected WriteTestOutput WriteDelta { get; }

        protected Image<T> ActualImage { get; set; }

        protected Image<T> ExpectedImage { get; set; }

        /// <summary>
        /// Gets or sets extra string tokens to insert into saved imaged file names.
        /// MSTest does not support getting any information about data rows
        /// or dynamic data tests at runtime :-/.
        /// </summary>
        protected string ExtraName { get; set; }

        [TestCleanup]
        public void TestCleanup()
        {
            if (this.ActualImage == null)
            {
                this.TestContext.WriteLine("The actual image is null, so skipping all of GeneratedImageTest cleanup, save, and delta functions");
                return;
            }

            if (this.ShouldWrite(this.WriteImages))
            {
                this.SaveImage("actual", this.ActualImage);
                this.SaveImage("expected", this.ExpectedImage);
            }

            if (this.ShouldWrite(this.WriteDelta))
            {
                if (this.ExpectedImage == null)
                {
                    this.TestContext.WriteLine($"Skipping writing delta image because `Expected` is null - cannot delta between actual and null!");
                    return;
                }

                var delta = this.ExpectedImage.Clone(
                    x =>
                    {
                        var deltaProcessor = new DeltaImageProcessor<T>(this.ActualImage);
                        x.ApplyProcessor(deltaProcessor);
                    });
                this.SaveImage("delta", delta);

                if (this.ShouldWrite(this.WriteImages))
                {
                    // create combined image for easier comparison
                    var combined = ImageTools.CombineImagesInLine(Color.Transparent, this.ExpectedImage, delta, this.ActualImage);
                    this.SaveImage("combined", combined);
                }
            }
        }

        protected void AssertImagesEqual(double tolerance = 0.0)
        {
            Assert.That.ImageMatches(this.ExpectedImage, this.ActualImage, tolerance);
        }

        protected void SaveExtraImage(string token, Image<T> image)
        {
            if (this.ShouldWrite(this.WriteImages))
            {
                this.SaveImage(token, image);
            }
            else
            {
                this.TestContext.WriteLine($"Skipping writing extra image because `WriteImages` is set to `{this.WriteImages}`");
            }
        }

        private void SaveImage(string typeToken, Image<T> image)
        {
            var extra = this.ExtraName.IsNullOrEmpty() ? string.Empty : "_" + this.ExtraName;

            this.SaveImage(image, $"{extra}_{typeToken}");
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