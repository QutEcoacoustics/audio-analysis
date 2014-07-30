using AudioAnalysisTools.Indices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using TowseyLibrary;

namespace AudioAnalysisTools.LongDurationSpectrograms
{


    /// <summary>
    /// This class contains methods to stitch together images and/or indices derived from a sequence of short recordings.
    /// As of August 2014, this class contains only one method to stitch images together.
    /// Other methods could be added to derive the images (prior to stitching) from the original indices stored in csv files.
    /// The following Powershell command was sued by Anthony to analyse a sequence of short recordings:
    /// Y:\Italy_GianniPavan\Sassofratino1day | % {& "C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisPrograms\bin\Release\AnalysisPrograms.exe" audio2csv -so ($_.FullName) -o "Y:\Italy_GianniPavan\output" -c "C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.Parallel.yml" }   
    /// where:
    ///         Y:\Italy_GianniPavan\Sassofratino1day   is the directory containing recordings
    ///         | = a pipe
    ///         % = foreach{}  = perform the operation in curly brackets on each item piped from the directory.
    ///         & "C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisPrograms\bin\Release\AnalysisPrograms.exe"  = runs an executable
    ///         audio2csv = first command line argument which determines the "activity" performed
    ///         -so ($_.FullName)  = the input file
    ///         -o "Y:\Italy_GianniPavan\output" = the output directory
    ///         -c "PATH\Towsey.Acoustic.Parallel.yml" is the config file

    /// 
    /// The following PowerShell command was used by Anthony to stitch together a sequence of spectrogam images without any gap between them.
    /// It requires ImageMagick software to be installed: i.e. C:\Program Files\ImageMagick-6.8.9-Q16\montage.exe
    /// Y:\Italy_GianniPavan\output\Towsey.Acoustic> & "C:\Program Files\ImageMagick-6.8.9-Q16\montage.exe" -mode concatenate -tile x1 *2MAP* "..\..\merge.png"
    /// </summary>
    public static class LDSpectrogramStitching
    {
        /// <summary>
        /// This method stitches together spectrogram images derived from consecutive shorter recordings over a 24 hour period.
        /// Currently set for the recording protocol of Gianna Pavan (10 minutes every 30 minutes).
        /// 
        /// Call this method from Sandpit or where ever!
        /// 
        /// IMPORTANT NOTE: This method does NOT check to see if the images are in temporal order. 
        ///                 A SORT line should be inserted somewhere
        /// </summary>
        public static void StitchPartialSpectrograms()
        {
            //######################################################
            // ********************* set the below parameters
            var inputDirectory = new DirectoryInfo(@"Z:\Italy_GianniPavan\output4\Towsey.Acoustic");
            string opFileStem = "Sassofratino_24hours_v3";
            var outputDirectory = new DirectoryInfo(@"Z:\Italy_GianniPavan\output4\");
            // a filter to select images to be stitched
            string endString = "_000.2MAPS.png";

            // recording protocol
            int minutesBetweenRecordingStarts = 30;
            TimeSpan minOffset = TimeSpan.Zero;
            // X-axis timescale
            int pixelColumnsPerHour = 60;
            int trackHeight = DrawSummaryIndices.DefaultTrackHeight;
            // ********************* set the above parameters
            //######################################################
            
            string[] fileEntries = Directory.GetFiles(inputDirectory.FullName);
            List<Image> images = new List<Image>();
            bool interpolateSpacer = true;
            var imagePair = new Image[2];

            TimeSpan xAxisTicInterval = TimeSpan.FromMinutes(pixelColumnsPerHour); // assume 60 pixels per hour

            // loop through all files in the required directory 
            foreach (string path in fileEntries)
            {
                // filter files.
                if (!path.EndsWith(endString)) continue;
                var image = new Bitmap(path);
                int spacerWidth = minutesBetweenRecordingStarts - image.Width;

                if (interpolateSpacer)
                {
                    var spacer = new Bitmap(spacerWidth, image.Height);


                    imagePair[0] = image;
                    imagePair[1] = spacer;
                    image = (Bitmap)ImageTools.CombineImagesInLine(imagePair);
                }

                images.Add(image);
            }
            Image compositeBmp = ImageTools.CombineImagesInLine(images.ToArray());

            int totalWidth = compositeBmp.Width;
            Bitmap timeBmp = Image_Track.DrawTimeTrack(totalWidth, minOffset, xAxisTicInterval, totalWidth, trackHeight, "hours");

            Graphics gr = Graphics.FromImage(compositeBmp);
            int halfHeight = compositeBmp.Height / 2;

            //add in the title bars
            string title = string.Format("24 hour FALSE-COLOUR SPECTROGRAM      (scale: hours x kHz)      (colour: R-G-B = {0})         (c) QUT.EDU.AU.  ", "BGN-AVG-CVR");
            Bitmap titleBmp = Image_Track.DrawTitleTrack(totalWidth, trackHeight, title);
            int offset = 0;
            gr.DrawImage(titleBmp, 0, offset); //draw in the top time scale
            title = string.Format("24 hour FALSE-COLOUR SPECTROGRAM      (scale: hours x kHz)      (colour: R-G-B = {0})         (c) QUT.EDU.AU.  ", "ACI-ENT-EVN");
            titleBmp = Image_Track.DrawTitleTrack(totalWidth, trackHeight, title);
            offset = halfHeight;
            gr.DrawImage(titleBmp, 0, offset); //draw in the top time scale

            //add in the timescale tracks
            offset = trackHeight;
            gr.DrawImage(timeBmp, 0, offset); //draw in the top time scale
            offset = compositeBmp.Height - trackHeight;
            gr.DrawImage(timeBmp, 0, offset); //draw in the top time scale
            offset = halfHeight - trackHeight;
            gr.DrawImage(timeBmp, 0, offset); //draw in the top time scale
            offset = halfHeight + trackHeight;
            gr.DrawImage(timeBmp, 0, offset); //draw in the top time scale

            compositeBmp.Save(Path.Combine(outputDirectory.FullName, opFileStem + ".png"));
        }

    }
}
