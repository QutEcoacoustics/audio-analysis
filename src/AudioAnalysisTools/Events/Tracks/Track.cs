// <copyright file="Track.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Acoustics.Shared;
    using AudioAnalysisTools.Events.Drawing;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    public class Track : ITrack
    {
        private readonly UnitConverters converter;

        /// <summary>
        /// Initializes a new instance of the <see cref="Track"/> class.
        /// Constructor.
        /// </summary>
        public Track(UnitConverters converter)
        {
            this.converter = converter;
            this.Points = new HashSet<ISpectralPoint>();
        }

        //public ISet<ISpectralPoint> Points => throw new System.NotImplementedException();

        public ISet<ISpectralPoint> Points { get; }

        /// <summary>
        /// Adds a new point to track given the fram, freq bin and amplitude.
        /// </summary>
        /// <param name="frame">The frame number.</param>
        /// <param name="bin">The freq bin number.</param>
        /// <param name="amplitude">The amplitude at given point.</param>
        public void SetPoint(int frame, int bin, double amplitude)
        {
            var sStart = this.converter.GetSecondsDurationFromFrameCount(frame);
            var seconds = new Interval<double>(sStart, sStart + this.converter.SecondsScale.SecondsPerFrame);
            var hertzLow = this.converter.PixelsToHertz(bin);
            var hertz = new Interval<double>(hertzLow, hertzLow + this.converter.HertzPerFreqBin);
            var point = new SpectralPoint(seconds, hertz, amplitude);
            this.Points.Add(point);
        }

        public int PointCount()
        {
            return this.Points.Count;
        }

        //public int GetStartFrame()
        //{
        //    return this.Points x => x.Seconds.Min();
        //}

        public double GetStartTimeSeconds()
        {
            return this.Points.Min(x => x.Seconds.Minimum);
        }

        //public int GetEndFrame()
        //{
        //    return this.frameIds.Max();
        //}

        public double GetEndTimeSeconds(double frameStepSeconds)
        {
            return this.Points.Max(x => x.Seconds.Maximum);
        }

        //public int GetTrackFrameCount()
        //{
        //    return this.frameIds.Max() - this.frameIds.Min() + 1;
        //}

        //public double GetTrackDurationSeconds(double frameStepSeconds)
        //{
        //    return this.GetTrackFrameCount() * frameStepSeconds;
        //}

        //public int GetBottomFreqBin()
        //{
        //    return this.freqBinIds.Min();
        //}

        public int GetBottomFreqHertz(double hertzPerBin)
        {
            return (int)Math.Round(this.Points.Min(x => x.Hertz.Minimum));
        }

        //public int GetTopFreqBin()
        //{
        //    return this.freqBinIds.Max();
        //}

        public int GetTopFreqHertz(double hertzPerBin)
        {
            return (int)Math.Round(this.Points.Max(x => x.Hertz.Maximum));
        }

        //public int GetTrackFreqBinCount()
        //{
        //    return this.freqBinIds.Max() - this.freqBinIds.Min() + 1;
        //}

        public int GetTrackBandWidthHertz(double hertzPerBin)
        {
            var minHertz = this.Points.Min(x => x.Hertz.Minimum);
            var maxHertz = this.Points.Max(x => x.Hertz.Maximum);
            return (int)Math.Round(maxHertz - minHertz);
        }

        /// <summary>
        /// returns the track as a matrix of seconds, Hertz and amplitude values.
        /// </summary>
        /// <param name="frameStepSeconds">the time scale.</param>
        /// <param name="hertzPerBin">The frequqwency scale.</param>
        /// <returns>The track matrix.</returns>
        //public double[,] GetTrackAsMatrix(double frameStepSeconds, double hertzPerBin)
        //{
        //    var trackMatrix = new double[this.PointCount(), 3];
        //    for (int i = 0; i < this.PointCount(); i++)
        //    {
        //        trackMatrix[i, 0] = this.frameIds[i] * frameStepSeconds;
        //        trackMatrix[i, 1] = this.freqBinIds[i] * hertzPerBin;
        //        trackMatrix[i, 2] = this.amplitudeSequence[i];
        //    }

        //    return trackMatrix;
        //}

        /// <summary>
        /// Returns an array that has the same number of time frames as the track.
        /// Each element contains the highest frequency (Hertz) for that time frame.
        /// NOTE: For tracks that include extreme frequency modulation (e.g. clicks and vertical tracks),
        ///       this method returns the highest frequency value in each time frame.
        /// </summary>
        /// <param name="hertzPerBin">the frequency scale.</param>
        /// <returns>An array of Hertz values.</returns>
        //public int[] GetTrackAsSequenceOfHertzValues(double hertzPerBin)
        //{
        //    int pointCount = this.frameIds.Count;
        //    var hertzTrack = new int[this.GetTrackFrameCount()];
        //    for (int i = 0; i < pointCount; i++)
        //    {
        //        int frameId = this.frameIds[i];
        //        int frequency = (int)Math.Round(this.freqBinIds[i] * hertzPerBin);
        //        if (hertzTrack[frameId] < frequency)
        //        {
        //            hertzTrack[frameId] = frequency;
        //        }
        //    }

        //    return hertzTrack;
        //}

        /// <summary>
        /// Returns the maximum amplitude in each time frame.
        /// </summary>
        /// <returns>an array of amplitude values.</returns>
        //public double[] GetAmplitudeOverTimeFrames()
        //{
        //    var frameCount = this.GetTrackFrameCount();
        //    int startFrame = this.GetStartFrame();
        //    var amplitudeArray = new double[frameCount];

        //    // add in amplitude values
        //    for (int i = 0; i < this.amplitudeSequence.Count; i++)
        //    {
        //        int elapsedFrames = this.frameIds[i] - startFrame;
        //        if (amplitudeArray[elapsedFrames] < this.amplitudeSequence[i])
        //        {
        //            amplitudeArray[elapsedFrames] = this.amplitudeSequence[i];
        //        }
        //    }

        //    return amplitudeArray;
        //}

        public void Draw<T>(IImageProcessingContext graphics, EventRenderingOptions options)
            where T : struct, IPixel<T>
        {
            throw new System.NotImplementedException();
        }
    }
}