// <copyright file="SpectralTrack.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AudioAnalysisTools.StandardSpectrograms;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;

    /// <summary>
    /// Currently (07 April 2020), the only need to distinguish track types is when they are drawn on a spectrogram. See method DrawTrack();.
    /// </summary>
    public enum SpectralTrackType
    {
        Whistle,
        HorizontalTrack,
        VerticalTrack,
        Click,
    }

    /// <summary>
    /// This class stores information about the path of spectral tracks in a spectrogram.
    /// It is used by the VerticalTrackParameters class and SpectralPeakTrackParameters class.
    /// It stores a list of each frames, frequency bins and corresonding amplitudes.
    /// The data stored is only the frame and frequency bin Ids. These Ids are relative to the spectrogram.
    /// Where absolute values are required (ie seconds and Hertz, the relevant scale parameter must be passed to the method.
    /// </summary>
    public class SpectralTrack
    {
        // The type of track - the types of track differ in the algorithm used to find them.
        private readonly SpectralTrackType trackType;

        // a list of all the frame Ids occupied by consecutive points over the length of a track.
        private readonly List<int> frameIds;

        // a list of all the frequency bin Ids occupied  by consecutive points over the length of a track.
        private readonly List<int> freqBinIds;

        // a list of the amplitudes of consecutive points over the length of a track.
        private readonly List<double> amplitudeSequence;

        public SpectralTrack(SpectralTrackType trackType, int frame, int bin, double amplitude)
        {
            this.trackType = trackType;
            this.frameIds = new List<int>();
            this.freqBinIds = new List<int>();
            this.amplitudeSequence = new List<double>();
            this.SetPoint(frame, bin, amplitude);
        }

        public void SetPoint(int frame, int bin, double amplitude)
        {
            this.frameIds.Add(frame);
            this.freqBinIds.Add(bin);
            this.amplitudeSequence.Add(amplitude);
        }

        public int PointCount()
        {
            return this.frameIds.Count;
        }

        public int GetStartFrame()
        {
            return this.frameIds.Min();
        }

        public double GetStartTimeSeconds(double frameStepSeconds)
        {
            return this.frameIds.Min() * frameStepSeconds;
        }

        public int GetEndFrame()
        {
            return this.frameIds.Max();
        }

        public double GetEndTimeSeconds(double frameStepSeconds)
        {
            return this.frameIds.Max() * frameStepSeconds;
        }

        public int GetTrackFrameCount()
        {
            return this.frameIds.Max() - this.frameIds.Min() + 1;
        }

        public double GetTrackDurationSeconds(double frameStepSeconds)
        {
            return this.GetTrackFrameCount() * frameStepSeconds;
        }

        public int GetBottomFreqBin()
        {
            return this.freqBinIds.Min();
        }

        public int GetBottomFreqHertz(double hertzPerBin)
        {
            return (int)Math.Round(this.frameIds.Min() * hertzPerBin);
        }

        public int GetTopFreqBin()
        {
            return this.freqBinIds.Max();
        }

        public int GetTopFreqHertz(double hertzPerBin)
        {
            return (int)Math.Round(this.frameIds.Max() * hertzPerBin);
        }

        public int GetTrackFreqBinCount()
        {
            return this.freqBinIds.Max() - this.freqBinIds.Min() + 1;
        }

        public int GetTrackBandWidthHertz(double hertzPerBin)
        {
            return (int)Math.Round(this.GetTrackFreqBinCount() * hertzPerBin);
        }

        /// <summary>
        /// returns the track as a matrix of seconds, Hertz and amplitude values.
        /// </summary>
        /// <param name="frameStepSeconds">the time scale.</param>
        /// <param name="hertzPerBin">The frequqwency scale.</param>
        /// <returns>The track matrix.</returns>
        public double[,] GetTrackAsMatrix(double frameStepSeconds, double hertzPerBin)
        {
            var trackMatrix = new double[this.PointCount(), 3];
            for (int i = 0; i < this.PointCount(); i++)
            {
                trackMatrix[i, 0] = this.frameIds[i] * frameStepSeconds;
                trackMatrix[i, 1] = this.freqBinIds[i] * hertzPerBin;
                trackMatrix[i, 2] = this.amplitudeSequence[i];
            }

            return trackMatrix;
        }

        /// <summary>
        /// Returns an array that has the same number of time frames as the track.
        /// Each element contains the highest frequency (Hertz) for that time frame.
        /// NOTE: For tracks that include extreme frequency modulation (e.g. clicks and vertical tracks),
        ///       this method returns the highest frequency value in each time frame.
        /// </summary>
        /// <param name="hertzPerBin">the frequency scale.</param>
        /// <returns>An array of Hertz values.</returns>
        public int[] GetTrackAsSequenceOfHertzValues(double hertzPerBin)
        {
            int pointCount = this.frameIds.Count;
            var hertzTrack = new int[this.GetTrackFrameCount()];
            for (int i = 0; i < pointCount; i++)
            {
                int frameId = this.frameIds[i];
                int frequency = (int)Math.Round(this.freqBinIds[i] * hertzPerBin);
                if (hertzTrack[frameId] < frequency)
                {
                    hertzTrack[frameId] = frequency;
                }
            }

            return hertzTrack;
        }

        /// <summary>
        /// Returns the maximum amplitude in each time frame.
        /// </summary>
        /// <returns>an array of amplitude values.</returns>
        public double[] GetAmplitudeOverTimeFrames()
        {
            var frameCount = this.GetTrackFrameCount();
            int startFrame = this.GetStartFrame();
            var amplitudeArray = new double[frameCount];

            // add in amplitude values
            for (int i = 0; i < this.amplitudeSequence.Count; i++)
            {
                int elapsedFrames = this.frameIds[i] - startFrame;
                if (amplitudeArray[elapsedFrames] < this.amplitudeSequence[i])
                {
                    amplitudeArray[elapsedFrames] = this.amplitudeSequence[i];
                }
            }

            return amplitudeArray;
        }

        public void DrawTrack<T>(Image<T> imageToReturn, double framesPerSecond, double freqBinWidth)
            where T : unmanaged, IPixel<T>
        {
            switch (this.trackType)
            {
                case SpectralTrackType.Click:
                    this.DrawClick(imageToReturn, framesPerSecond, freqBinWidth);
                    break;
                case SpectralTrackType.VerticalTrack:
                    this.DrawVerticalTrack(imageToReturn);
                    break;
                case SpectralTrackType.HorizontalTrack:
                    this.DrawHorizontalTrack(imageToReturn, framesPerSecond, freqBinWidth);
                    break;
                case SpectralTrackType.Whistle:
                    this.DrawWhistle(imageToReturn, framesPerSecond, freqBinWidth);
                    break;
                default:
                    this.DrawDefaultTrack(imageToReturn, framesPerSecond, freqBinWidth);
                    break;
            }
        }

        public void DrawClick<T>(Image<T> imageToReturn, double framesPerSecond, double freqBinWidth)
            where T : unmanaged, IPixel<T>
        {
            //double startSec = this.timeOffset.TotalSeconds;
            //int frame1 = (int)Math.Round(startSec * sonogramFramesPerSecond);
            for (int i = 1; i < this.GetTrackFrameCount(); i++)
            {
                //double endSec = startSec + (i * secondsPerTrackFrame);
                //    int frame2 = (int)Math.Round(endSec * sonogramFramesPerSecond);

                //    //int freqBin = (int)Math.Round(this.MinFreq / freqBinWidth);
                //    int f1 = this.GetFrequency(i);
                //    int f1Bin = (int)Math.Round(f1 / freqBinWidth);
                //    int y1 = imageHeight - f1Bin - 1;
                //    int f2 = this.GetFrequency(i + 1);
                //    int f2Bin = (int)Math.Round(f2 / freqBinWidth);
                //    int y2 = imageHeight - f2Bin - 1;
                //    g.DrawLine(p1, frame1, y1, frame2, y2);

                //    //startSec = endSec;
                //    frame1 = frame2;
            }

            //.DrawText("text here", Acoustics.Shared.ImageSharp.Drawing.Tahoma8, Color.Black, new PointF(x, y));
        }

        /// <summary>
        /// Draws a vertical track on the supplied image, assumed to be a spectrogram.
        /// Note that the freq bin ids are counted from zero Hertz upwards.
        /// However the spectrogram image is oriented with vertical frequency axis.
        /// Therefore need to subtract binId from image hieght.
        /// This assumes that the supplied image height is same as the number of frequency bins when extracting the track.
        /// </summary>
        /// <typeparam name="T">Type of image pixel.</typeparam>
        /// <param name="imageToReturn">The spectrogram on which tracks are to be drawn.</param>
        public void DrawVerticalTrack<T>(Image<T> imageToReturn)
            where T : unmanaged, IPixel<T>
        {
            Color? trackColor = Color.Red;

            for (int i = 0; i < this.frameIds.Count; i++)
            {
                int frame = this.frameIds[i];
                int binId = this.freqBinIds[i];
                var yValue = imageToReturn.Height - binId;
                var pixel = imageToReturn[frame, binId];

                //var pixelType = pixel.FromRgb24();
                //imageToReturn[frame, bin] = Color.FromRgb(trackColor.Red, pixel.G, pixel.B);

                var color = trackColor.Value.ToPixel<T>();

                //color = new Rgb24(pixel.);
                //imageToReturn[frame, yValue] = color;
            }
        }

        public void DrawHorizontalTrack<T>(Image<T> imageToReturn, double framesPerSecond, double freqBinWidth)
            where T : unmanaged, IPixel<T>
        {
            for (int i = 0; i < this.frameIds.Count; i++)
            {
                int frame = this.frameIds[i];
                int bin = this.freqBinIds[i];
                var pixel = imageToReturn[frame, bin];

                //bmp[frame, bin] = Color.FromRgb((byte)hits[r, c], pixel.G, pixel.B);
                //imageToReturn[frame, bin] = Color.Green;
                var newColor = Color.Red.ToPixel<Rgb24>();

                //double factor = pixel.R / (255 * 1.2);  // 1.2 is a color intensity adjustment
            }
        }

        public void DrawWhistle<T>(Image<T> imageToReturn, double framesPerSecond, double freqBinWidth)
            where T : unmanaged, IPixel<T>
        {
            Pen p1 = new Pen(AcousticEvent.DefaultBorderColor, 2); // default colour
        }

        public void DrawDefaultTrack<T>(Image<T> imageToReturn, double framesPerSecond, double freqBinWidth)
            where T : unmanaged, IPixel<T>
        {
            Pen pen1 = new Pen(AcousticEvent.DefaultBorderColor, 2); // default colour
        }
    }
}
