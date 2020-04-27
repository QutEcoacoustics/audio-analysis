// <copyright file="Track.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events.Tracks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Interfaces;
    using SixLabors.ImageSharp.Processing;

    public enum TrackType
    {
        /// <summary>
        /// Sounds like single tone whistle.
        /// Each track point advances one time step.
        /// All points remain in the same frequency bin.
        /// </summary>
        OneBinTrack,

        /// <summary>
        /// Sounds like fluctuating tone/chirp.
        /// Each track point advances one time step.
        /// Points may move up or down two frequency bins.
        /// </summary>
        FowardTrack,

        /// <summary>
        /// Sounds like whip.
        /// Each track point ascends one frequency bin.
        /// Points may move forwards or back one frame step.
        /// </summary>
        UpwardTrack,

        /// <summary>
        /// Sounds like click.
        /// Each track point ascends one frequency bin.
        /// All points remain in the same time frame.
        /// </summary>
        OneFrameTrack,

        /// <summary>
        /// A track containing segments of two or more of the above.
        /// </summary>
        MixedTrack,
    }

    public class Track : ITrack
    {
        private readonly UnitConverters converter;

        private readonly TrackType trackType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Track"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="converter">
        /// A reference to unit conversions this track class should use to
        /// convert spectrogram data to real units.
        /// </param>
        /// <param name="trackType"> The type of track.</param>
        public Track(UnitConverters converter, TrackType trackType)
        {
            this.converter = converter;
            this.trackType = trackType;

            this.Points = new List<ISpectralPoint>();
        }

        /// <inheritdoc cref="Track.Track(UnitConverters, TrackType)"/>
        /// <param name="initialPoints">
        /// A set of initial points to add into the point data collection.
        /// </param>
        public Track(
            UnitConverters converter,
            TrackType trackType,
            params (int Frame, int Bin, double Amplitude)[] initialPoints)
            : this(converter, trackType)
        {
            foreach (var point in initialPoints)
            {
                this.SetPoint(point.Frame, point.Bin, point.Amplitude);
            }
        }

        public int PointCount => this.Points.Count;

        //public double StartTimeSeconds => this.converter.SegmentStartOffset + this.Points.Min(x => x.Seconds.Minimum);
        public double StartTimeSeconds => this.Points.Min(x => x.Seconds.Minimum);

        //public double EndTimeSeconds => this.converter.SegmentStartOffset + this.Points.Max(x => x.Seconds.Maximum);
        public double EndTimeSeconds => this.Points.Max(x => x.Seconds.Maximum);

        // TODO: use a more efficient collection like a quadtree
        // note must maintain insertion order
        public ICollection<ISpectralPoint> Points { get; }

        public double DurationSeconds => this.EndTimeSeconds - this.StartTimeSeconds;

        public TimeSpan SegmentStartOffset => TimeSpan.FromSeconds(this.converter.SegmentStartOffset);

        public double LowFreqHertz => this.Points.Min(x => x.Hertz.Minimum);

        public double HighFreqHertz => this.Points.Max(x => x.Hertz.Maximum);

        public double TrackBandWidthHertz => this.HighFreqHertz - this.LowFreqHertz;

        /// <summary>
        /// Gets the frequency of the first point in the track.
        /// Where there is more than one frequency in this first frame, returns the lowest frequency.
        /// </summary>
        public double StartFrequency
        {
            get
            {
                var startTime = this.StartTimeSeconds;
                return this.Points.Where(x => x.Seconds.Contains(startTime)).Min(y => y.Hertz.Minimum);
            }
        }

        /// <summary>
        /// Adds a new point to track given the fram, freq bin and amplitude.
        /// </summary>
        /// <param name="frame">The frame number.</param>
        /// <param name="bin">The freq bin number.</param>
        /// <param name="amplitude">The amplitude at given point.</param>
        public void SetPoint(int frame, int bin, double amplitude)
        {
            var secondsStart = this.converter.GetStartTimeInSecondsOfFrame(frame);

            var hertzLow = this.converter.GetHertzFromFreqBin(bin);

            var point = new SpectralPoint(
                (secondsStart, secondsStart + this.converter.SecondsPerFrame),
                (hertzLow, hertzLow + this.converter.HertzPerFreqBin),
                amplitude);

            this.Points.Add(point);
        }

        /// <summary>
        /// Does a sanity check on the conversion of frame/bins to real values and back again.
        /// </summary>
        /// <param name="frame">The frame number.</param>
        /// <param name="bin">The freq bin number.</param>
        public string CheckPoint(int frame, int bin)
        {
            var secondsStart = this.converter.GetStartTimeInSecondsOfFrame(frame);
            var hertzLow = this.converter.GetHertzFromFreqBin(bin);
            double amplitude = 1.0; // a filler.

            var point = new SpectralPoint(
                (secondsStart, secondsStart + this.converter.SecondsPerFrame),
                (hertzLow, hertzLow + this.converter.HertzPerFreqBin),
                amplitude);

            var outFrame = this.converter.FrameFromStartTime(point.Seconds.Minimum);
            var outBin = this.converter.GetFreqBinFromHertz(point.Hertz.Minimum);
            var info = new string($"In frame:{frame}, In bin:{bin}, SecondsStart:{point.Seconds.Minimum.ToString("0.000")}, HertzLow:{point.Hertz.Minimum:F3}, Out frame:{outFrame}, Out bin: {outBin}");

            if (frame != outFrame || bin != outBin)
            {
                LoggedConsole.WriteWarnLine("WARNING" + info);
                throw new Exception("WARNING" + info);
            }
            else
            {
                LoggedConsole.WriteLine(info);
            }

            return info;
        }

        /// <summary>
        /// Returns an array that has the same number of time frames as the track.
        /// Each element contains the highest frequency (Hertz) for that time frame.
        /// NOTE: For tracks that include extreme frequency modulation (e.g. clicks and vertical tracks),
        ///       this method returns the highest frequency value in each time frame.
        /// </summary>
        /// <returns>An array of Hertz values.</returns>
        public double[] GetTrackAsSequenceOfHertzValues()
        {
            //TODO
            throw new NotImplementedException("Method not implemented.");
        }

        /// <summary>
        /// Returns an array of Hertz difference values.
        /// The array has length one less than the number of dicrete time frames in the track.
        /// THis array can be used to compare simularity bewteen the shapes of tracks even if absolute frequency values are not similar.
        /// </summary>
        /// <returns>An array of Hertz difference values.</returns>
        public double[] GetTrackFrequencyProfile()
        {
            // get points, group by start bucket, order by grouped key (start bucket) and then provide sequence of windowed pairs
            var sorted = this
                .Points
                .GroupBy(g => g.Seconds)
                .OrderBy(x => x.Key)
                .Windowed(2)
                .Select(pointPair =>
                {
                    var firstPoints = pointPair[0];
                    var secondPoints = pointPair[1];

                    var firstMaxFrequency = firstPoints.Max(y => y.Hertz.Minimum);
                    var secondMaxFrequency = secondPoints.Max(y => y.Hertz.Minimum);

                    var delta = secondMaxFrequency - firstMaxFrequency;
                    return delta;
                })
                .ToArray();
            return sorted;
        }

        /// <summary>
        /// Returns the maximum amplitude in each time frame.
        /// </summary>
        public double[] GetAmplitudeOverTimeFrames()
        {
            // get points, group by start bucket, order by grouped key (start bucket)
            var sorted = this
                .Points
                .GroupBy(g => g.Seconds)
                .OrderBy(x => x.Key)
                .Select(framePoints =>
                {
                    var maxAmplitude = framePoints.Max(y => y.Value);
                    return maxAmplitude;
                })
                .ToArray();
            return sorted;
        }

        /// <summary>
        /// Returns the maximum amplitude in each time frame.
        /// </summary>
        public double GetAverageTrackAmplitude()
        {
            var sum = 0.0;
            foreach (var point in this.Points)
            {
                sum += point.Value;
            }

            var av = sum / this.Points.Count;
            return av;
        }

        /// <summary>
        /// Draws the track on an image given by its processing context.
        /// </summary>
        /// <remarks>
        /// Implementation is fairly simple. It sorts all points by the default IComparable method
        /// which sorts points by time (ascending), frequency (ascending) and finally value.
        /// The sorted collection is then used as a set of points to connect lines to.
        /// </remarks>
        public void Draw(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            switch (this.trackType)
            {
                case TrackType.UpwardTrack:
                    ((IPointData)this).DrawPointsAsFillExperiment(graphics, options);
                    break;
                case TrackType.OneBinTrack:
                    ((IPointData)this).DrawPointsAsFillExperiment(graphics, options);
                    break;
                case TrackType.OneFrameTrack:
                    ((IPointData)this).DrawPointsAsFillExperiment(graphics, options);
                    break;
                case TrackType.FowardTrack:
                    ((IPointData)this).DrawPointsAsFillExperiment(graphics, options);
                    break;
                default:
                    ((IPointData)this).DrawPointsAsPath(graphics, options);
                    break;
            }
        }
    }
}