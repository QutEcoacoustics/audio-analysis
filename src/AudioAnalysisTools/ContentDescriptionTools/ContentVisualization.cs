// <copyright file="ContentVisualization.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using TowseyLibrary;

    public static class ContentVisualization
    {
        public static Image DrawLdfcSpectrogramWithContentScoreTracks(Image ldfcSpectrogram, List<Plot> contentScores)
        {
            int trackHeight = 30;
            //int width = ldfcSpectrogram.Width;
            //var imageList = new List<Image>
            //{
            //    ldfcSpectrogram,
            //};

            var image = new Image_MultiTrack(ldfcSpectrogram);
            if (contentScores != null)
            {
                foreach (var plot in contentScores)
                {
                    var track = new ImageTrack(TrackType.scoreArrayNamed, plot.data)
                    {
                        Name = plot.title,
                        ScoreMin = 0.0, // plot.data.Min(),
                        ScoreMax = 1.0, // plot.data.Max(),
                        Height = trackHeight,
                        topOffset = 0,
                        ScoreThreshold = plot.threshold,
                    };

                    image.AddTrack(ImageTrack.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
                }
            }

            return image.GetImage();
        }

        public static void DrawNormalisedIndexMatrices(DirectoryInfo dir, string baseName, Dictionary<string, double[,]> dictionary)
        {
            var list = new List<Image>();
            foreach (string key in ContentDescription.IndexNames)
            {
                var bmp = ImageTools.DrawReversedMatrixWithoutNormalisation(dictionary[key]);

                // need to rotate spectrogram to get correct orientation.
                bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);

                // draw grid lines and add axis scales
                var xAxisPixelDuration = TimeSpan.FromSeconds(60);
                var fullDuration = TimeSpan.FromTicks(xAxisPixelDuration.Ticks * bmp.Width);
                var freqScale = new FrequencyScale(11025, 512, 1000);
                SpectrogramTools.DrawGridLinesOnImage((Bitmap)bmp, TimeSpan.Zero, fullDuration, xAxisPixelDuration, freqScale);
                const int trackHeight = 20;
                var recordingStartDate = default(DateTimeOffset);
                var timeBmp = ImageTrack.DrawTimeTrack(fullDuration, recordingStartDate, bmp.Width, trackHeight);
                var array = new Image[2];
                array[0] = bmp;
                array[1] = timeBmp;
                var image = ImageTools.CombineImagesVertically(array);

                // add a header to the spectrogram
                var header = new Bitmap(image.Width, 20);
                Graphics g = Graphics.FromImage(header);
                g.Clear(Color.LightGray);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawString(key, new Font("Tahoma", 9), Brushes.Black, 4, 4);
                list.Add(ImageTools.CombineImagesVertically(new List<Image>(new[] { header, image })));
            }

            // save the image - the directory for the path must exist
            var path = Path.Combine(dir.FullName, baseName + "__Towsey.Acoustic.GreyScaleImages.png");
            var indexImage = ImageTools.CombineImagesInLine(list);
            indexImage?.Save(path);
        }
    }
}
