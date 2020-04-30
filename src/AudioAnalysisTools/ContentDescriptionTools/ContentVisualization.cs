// <copyright file="ContentVisualization.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared.ImageSharp;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;
    using Path = System.IO.Path;

    public static class ContentVisualization
    {
        public static Image DrawLdfcSpectrogramWithContentScoreTracks(Image<Rgb24> ldfcSpectrogram, List<Plot> contentScores)
        {
            int plotHeight = 30;
            var imageList = new List<Image<Rgb24>>
            {
                ldfcSpectrogram,
            };

            if (contentScores != null)
            {
                foreach (var plot in contentScores)
                {
                    var image = plot.DrawAnnotatedPlot(plotHeight);
                    imageList.Add(image);
                }
            }

            return ImageTools.CombineImagesVertically(imageList);
        }

        /// <summary>
        /// Can be used for visual checking and debugging purposes.
        /// </summary>
        public static void DrawNormalisedIndexMatrices(DirectoryInfo dir, string baseName, Dictionary<string, double[,]> dictionary)
        {
            var list = new List<Image<Rgb24>>();
            foreach (string key in ContentSignatures.IndexNames)
            {
                var bmp = ImageTools.DrawReversedMatrixWithoutNormalisation(dictionary[key]);

                // need to rotate spectrogram to get correct orientation.
                bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);

                // draw grid lines and add axis scales
                var xAxisPixelDuration = TimeSpan.FromSeconds(60);
                var fullDuration = TimeSpan.FromTicks(xAxisPixelDuration.Ticks * bmp.Width);
                var freqScale = new FrequencyScale(11025, 512, 1000);
                SpectrogramTools.DrawGridLinesOnImage((Image<Rgb24>)bmp, TimeSpan.Zero, fullDuration, xAxisPixelDuration, freqScale);
                const int trackHeight = 20;
                var recordingStartDate = default(DateTimeOffset);
                var timeBmp = ImageTrack.DrawTimeTrack(fullDuration, recordingStartDate, bmp.Width, trackHeight);

                var image = ImageTools.CombineImagesVertically(bmp, timeBmp);

                // add a header to the spectrogram
                var header = Drawing.NewImage(image.Width, 20, Color.LightGray);
                header.Mutate(g =>
                {
                    g.DrawTextSafe(key, Drawing.Tahoma9, Color.Black, new PointF(4, 4));
                    list.Add(ImageTools.CombineImagesVertically(header, image));
                });
            }

            // save the image - the directory for the path must exist
            var path = Path.Combine(dir.FullName, baseName + "__Towsey.Acoustic.GreyScaleImages.png");
            var indexImage = ImageTools.CombineImagesInLine(list);
            indexImage?.Save(path);
        }
    }
}