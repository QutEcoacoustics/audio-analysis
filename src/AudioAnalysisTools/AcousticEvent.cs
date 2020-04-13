// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AcousticEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the AcousticEvent type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Acoustics.Shared.Contracts;
    using Acoustics.Shared.Csv;
    using Acoustics.Shared.ImageSharp;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Events.Interfaces;
    using AudioAnalysisTools.StandardSpectrograms;
    using CsvHelper.Configuration;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;

    public class AcousticEvent : EventBase, ISpectralEvent, ITemporalEvent
    {
        public static readonly Color DefaultBorderColor = Color.Crimson;

        public static readonly Color DefaultScoreColor = Color.Lime;

        public sealed class AcousticEventClassMap : ClassMap<AcousticEvent>
        {
            public static readonly string[] IgnoredProperties =
                {
                    nameof(TimeStart), nameof(TimeEnd),
                    nameof(Bandwidth), nameof(IsMelscale), nameof(FrameOffset),
                    nameof(FramesPerSecond),
                    nameof(ScoreNormalised), nameof(Score_MaxPossible),
                    nameof(Score_MaxInEvent), nameof(Score_TimeOfMaxInEvent),
                    nameof(Score2Name), nameof(Score2), nameof(Periodicity), nameof(DominantFreq),
                    nameof(Tag), nameof(Intensity), nameof(Quality), nameof(HitColour),
                };

            public AcousticEventClassMap()
            {
                this.AutoMap(Csv.DefaultConfiguration);

                foreach (var ignoredProperty in IgnoredProperties)
                {
                    this.MemberMaps.Single(pm => pm.Data.Member.Name == ignoredProperty).Ignore();
                }

                this.Map(m => m.EventStartSeconds, useExistingMap: true).Index(0);
                this.Map(m => m.EventEndSeconds, useExistingMap: true).Index(2);
                this.Map(m => m.EventDurationSeconds, useExistingMap: true).Index(3);
                this.Map(m => m.LowFrequencyHertz, useExistingMap: true).Index(4);

                this.Map(m => m.HighFrequencyHertz, useExistingMap: true).Index(5);

                this.ReferenceMaps.Clear();
                this.References<Oblong.OblongClassMap>(m => m.Oblong);

                // Make sure HitElements is always in the last column!
                this.Map(m => m.HitElements, useExistingMap: true).Index(1000);
            }
        }

        /// <summary>
        /// Gets the time offset from start of current segment to start of event in seconds.
        ///  Proxied to EventBase.EventStartSeconds.
        /// </summary>
        /// <remarks>
        /// <para>
        /// NOTE: <see cref="TimeStart"/> is relative to the start of a segment. This notion is obsolete!
        /// Events must always be stored relative to start of the recording.
        /// </para>
        /// Note: converted to private setter so we can control how this is set. Recommend using <see cref="SetEventPositionRelative"/>
        /// after event instantiation to modify bounds.
        /// </remarks>
        [Obsolete("Bounds relative to the segment are inconsistent with our rules for always measuring from the start of the recording.")]
        public double TimeStart { get; private set; }

        /// <summary>
        /// Gets the time offset (in seconds) from start of current segment to end of the event.
        /// This field is NOT in EventBase. EventBase only requires TimeStart because it is designed to also accomodate points.
        /// </summary>
        /// <remarks>
        /// <para>
        /// NOTE: <see cref="TimeStart"/> is relative to the start of a segment. This notion is obsolete!
        /// Events must always be stored relative to start of the recording.
        /// </para>
        /// Note: converted to private setter so we can control how this is set.
        /// Recommend using <see cref="SetEventPositionRelative"/> after event instantiation to modify bounds.
        /// </remarks>
        [Obsolete("Bounds relative to the segment are inconsistent with our rules for always measuring from the start of the recording.")]
        public double TimeEnd { get; private set; }

        /// <summary>
        /// Gets the end time of an event relative to the recording start.
        /// </summary>
        public double EventEndSeconds => this.TimeEnd + this.SegmentStartSeconds;

        /// <summary>
        /// Gets the start time of an event relative to the recording start.
        /// </summary>
        public override double EventStartSeconds => this.TimeStart + this.SegmentStartSeconds;

        /// <summary>
        /// Gets or sets units = Hertz.
        /// Proxied to EventBase.MinHz.
        /// </summary>
        public double LowFrequencyHertz { get; set; }

        /// <summary>Gets or sets units = Hertz.</summary>
        public double HighFrequencyHertz { get; set; }

        /// <summary>
        /// Gets the bandwidth of an acoustic event.
        /// </summary>
        public double Bandwidth => this.HighFrequencyHertz - this.LowFrequencyHertz + 1;

        /// <summary>
        /// Gets or sets a horizontal or vertical spectral track.
        /// </summary>
        public SpectralTrack_TO_BE_REMOVED TheTrack { get; set; }

        public bool IsMelscale { get; set; }

        /// <summary>
        /// Gets or sets the bounds of an event with respect to the segment start
        /// BUT in terms of the frame count (from segment start) and frequency bin (from zero Hertz).
        /// This is no longer the preferred way to operate with acoustic event bounds.
        /// Better to use real units (seconds and Hertz) and provide the acoustic event with scale information.
        /// </summary>
        public Oblong Oblong { get; set; }

        /// <summary> Gets or sets required for conversions to & from MEL scale AND for drawing event on spectrum.</summary>
        public int FreqBinCount { get; set; }

        /// <summary>
        /// Gets required for freq-binID conversions.
        /// </summary>
        public double FreqBinWidth { get; private set; }

        /// <summary> Gets frame duration in seconds.</summary>
        public double FrameDuration { get; private set; }

        /// <summary> Gets or sets time between frame starts in seconds. Inverse of FramesPerSecond.</summary>
        public double FrameOffset { get; set; }

        /// <summary> Gets or sets number of frame starts per second. Inverse of the frame offset.</summary>
        public double FramesPerSecond { get; set; }

        //PROPERTIES OF THE EVENTS i.e. Name, SCORE ETC
        public string SpeciesName { get; set; }

        public string Name { get; set; }

        /// <summary> Gets or sets score normalised in range [0,1]. NOTE: Max is set = to five times user supplied threshold.</summary>
        public double ScoreNormalised { get; set; }

        /// <summary> Gets max Possible Score: set = to 5x user supplied threshold.
        /// An arbitrary value used for score normalisation - it displays well in plot.
        /// </summary>
        public double Score_MaxPossible { get; set; }

        public double Score_MaxInEvent { get; set; }

        public double Score_TimeOfMaxInEvent { get; set; }

        public string Score2Name { get; set; }

        /// <summary> Gets or sets second score if required.
        /// </summary>
        [Obsolete("We should use another type of Event class to represent this concept")]
        public double Score2 { get; set; }

        ///// <summary>
        ///// Gets or sets a list of points that can be used to identifies features in spectrogram relative to the Event.
        ///// i.e. Points can be outside of events and can have negative values.
        ///// Point location is relative to the top left corner of the event.
        ///// </summary>
        //public List<Point> Points { get; set; }

        /// <summary>
        /// Gets or sets the periodicity of acoustic energy in an event.
        /// Use for events which have an oscillating acoustic energy - e.g. for frog calls.
        /// </summary>
        public double Periodicity { get; set; }

        public double DominantFreq { get; set; } // the dominant freq in the event - used for frog calls

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a value that can be used to filter or tag some members of a list of acoustic events.
        /// Was used for constructing data sets.
        /// </summary>
        public bool Tag { get; set; }

        /// <summary>Gets or sets assigned value when reading in a list of user identified events. Indicates a user assigned assessment of event intensity.</summary>
        public int Intensity { get; set; }

        /// <summary>Gets or sets assigned value when reading in a list of user identified events. Indicates a user assigned assessment of event quality.</summary>
        public int Quality { get; set; }

        public Color BorderColour { get; set; }

        public Color ScoreColour { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcousticEvent"/> class.
        /// Sets some default colors for drawing an event on a spectrogram.
        /// THis is the first of three constructors.
        /// </summary>
        public AcousticEvent()
        {
            this.BorderColour = DefaultBorderColor;
            this.ScoreColour = DefaultScoreColor;
            this.HitColour = this.BorderColour.WithAlpha(0.5f);
            this.IsMelscale = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcousticEvent"/> class.
        /// This constructor requires the minimum information to establish the temporal and frequency bounds of an acoustic event.
        /// </summary>
        /// <param name="segmentStartOffset">The start of the current segment relative to start of recording.</param>
        /// <param name="eventStartSegmentRelative">event start with respect to start of segment.</param>
        /// <param name="eventDuration">event end with respect to start of segment.</param>
        /// <param name="minFreq">Lower frequency bound of event.</param>
        /// <param name="maxFreq">Upper frequency bound of event.</param>
        public AcousticEvent(TimeSpan segmentStartOffset, double eventStartSegmentRelative, double eventDuration, double minFreq, double maxFreq)
            : this()
        {
            var eventEndSegmentRelative = eventStartSegmentRelative + eventDuration;
            this.SetEventPositionRelative(segmentStartOffset, eventStartSegmentRelative, eventEndSegmentRelative);
            this.LowFrequencyHertz = minFreq;
            this.HighFrequencyHertz = maxFreq;

            // have no info to convert time/Hz values to coordinates
            this.Oblong = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcousticEvent"/> class.
        /// This constructor currently works ONLY for linear Hertz scale events.
        /// It requires the event bounds to provided (using Oblong) in terms of time frame and frequency bin counts.
        /// Scale information must also be provided to convert bounds into real values (seconds, Hertz).
        /// </summary>
        /// <param name="o">An oblong initialized with bin and frame numbers marking location of the event.</param>
        /// <param name="nyquistFrequency">to set the freq scale.</param>
        /// <param name="binCount">Number of freq bins.</param>
        /// <param name="frameDuration">tseconds duration of a frame - to set the time scale.</param>
        /// <param name="frameStep">seconds between frame starts i.e. frame step; i.e. inverse of frames per second. Sets the time scale for an event.</param>
        /// <param name="frameCount">to set the time scale.</param>
        public AcousticEvent(TimeSpan segmentStartOffset, Oblong o, int nyquistFrequency, int binCount, double frameDuration, double frameStep, int frameCount)
            : this()
        {
            this.Oblong = o;
            this.FreqBinWidth = nyquistFrequency / (double)binCount;
            this.FrameDuration = frameDuration;
            this.FrameOffset = frameStep;
            this.FreqBinCount = binCount;
            this.FrameCount = frameCount;

            double startTime = o.RowTop * this.FrameOffset;
            double end = (o.RowBottom + 1) * this.FrameOffset;

            this.SetEventPositionRelative(segmentStartOffset, startTime, end);

            this.LowFrequencyHertz = (int)Math.Round(o.ColumnLeft * this.FreqBinWidth);
            this.HighFrequencyHertz = (int)Math.Round(o.ColumnRight * this.FreqBinWidth);
            this.HitElements = o.HitElements;
        }

        public int FrameCount { get; set; }

        public ISet<Point> HitElements { get; set; }

        public Color? HitColour { get; set; }

        /// DIMENSIONS OF THE EVENT
        /// <summary>Gets the event duration in seconds.</summary>
        public double EventDurationSeconds => this.TimeEnd - this.TimeStart;

        /// <summary>
        /// Gets or sets which profile (combination of settings in a config file) produced this event.
        /// </summary>
        public string Profile { get; set; }

        double ITemporalEvent.EventEndSeconds => this.EventEndSeconds;

        double ITemporalEvent.EventDurationSeconds => this.EventDurationSeconds;

        double IInstantEvent.EventStartSeconds => this.EventStartSeconds;

        double ISpectralBand.LowFrequencyHertz => this.LowFrequencyHertz;

        double ISpectralBand.HighFrequencyHertz => this.HighFrequencyHertz;

        double ISpectralBand.BandWidthHertz => this.Bandwidth;

        //public void DoMelScale(bool doMelscale, int freqBinCount)
        //{
        //    this.IsMelscale = doMelscale;
        //    this.FreqBinCount = freqBinCount;
        //}

        /// <summary>
        /// Set the start and end times of an event with respect to the segment start time
        /// AND also calls method to set event start time with respect the recording/file start.
        /// </summary>
        public void SetEventPositionRelative(
            TimeSpan segmentStartOffset,
            double eventStartSegmentRelative,
            double eventEndSegmentRelative)
        {
            this.TimeStart = eventStartSegmentRelative;
            this.TimeEnd = eventEndSegmentRelative;

            this.SetEventStartRelative(segmentStartOffset, eventStartSegmentRelative);
        }

        /// <summary>
        /// THe only call to this method is from a no-longer used recogniser.
        /// Could be deleted.
        /// It sets the time and frequency scales for an event given the sr, and window size.
        /// </summary>
        public void SetTimeAndFreqScales(int samplingRate, int windowSize, int windowOffset)
        {
            //set the frame duration and offset in seconds
            this.FrameDuration = windowSize / (double)samplingRate;
            this.FrameOffset = windowOffset / (double)samplingRate;
            this.FramesPerSecond = 1 / this.FrameOffset;

            //set the Freq Scale. Required for freq-binID conversions
            this.FreqBinWidth = samplingRate / (double)windowSize;

            //required for conversions to & from MEL scale
            this.FreqBinCount = windowSize / 2;

            if (this.Oblong == null)
            {
                this.Oblong = ConvertEvent2Oblong(this);
            }
        }

        /// <summary>
        /// This method assumes that there is no frame overlap i.e. frame duration = frame offset.
        /// </summary>
        /// <param name="framesPerSec">frames per second assuming no overlap.</param>
        /// <param name="freqBinWidth">Number of hertz per freq bin.</param>
        public void SetTimeAndFreqScales(double framesPerSec, double freqBinWidth)
        {
            double frameOffset = 1 / framesPerSec;      //frame offset in seconds
            this.SetTimeAndFreqScales(frameOffset, frameOffset, freqBinWidth);
        }

        public void SetTimeAndFreqScales(double frameOffset, double frameDuration, double freqBinWidth)
        {
            this.FramesPerSecond = 1 / frameOffset;   //inverse of the frame offset
            this.FrameDuration = frameDuration;       //frame duration in seconds
            this.FrameOffset = frameOffset;           //frame duration in seconds
            this.FreqBinWidth = freqBinWidth;         //required for freq-binID conversions

            if (this.Oblong == null)
            {
                this.Oblong = ConvertEvent2Oblong(this);
            }
        }

        /// <summary>
        /// Converts the Hertz (frequency) bounds of an event to the frequency bin number.
        /// The frequency bin is an index into the columns of the spectrogram data matrix.
        /// Since the spectrogram data matrix is oriented with the origin at top left,
        /// the low frequency bin will have a lower column index than the high freq bin.
        /// </summary>
        /// <param name="doMelscale">mel scale.</param>
        /// <param name="minFreq">lower freq bound.</param>
        /// <param name="maxFreq">upper freq bound.</param>
        /// <param name="nyquist">Nyquist freq in Herz.</param>
        /// <param name="binWidth">frequency scale.</param>
        /// <param name="leftCol">return bin index for lower freq bound.</param>
        /// <param name="rightCol">return bin index for upper freq bound.</param>
        public static void ConvertHertzToFrequencyBin(bool doMelscale, int minFreq, int maxFreq, int nyquist, double binWidth, out int leftCol, out int rightCol)
        {
            if (doMelscale)
            {
                int binCount = (int)(nyquist / binWidth) + 1;
                double maxMel = MFCCStuff.Mel(nyquist);
                int melRange = (int)(maxMel - 0 + 1);
                double binsPerMel = binCount / (double)melRange;
                leftCol = (int)Math.Round(MFCCStuff.Mel(minFreq) * binsPerMel);
                rightCol = (int)Math.Round(MFCCStuff.Mel(maxFreq) * binsPerMel);
            }
            else
            {
                leftCol = (int)Math.Round(minFreq / binWidth);
                rightCol = (int)Math.Round(maxFreq / binWidth);
            }
        }

        /// <summary>
        /// Calculates the matrix/image indices of the acoustic event, when given the time/freq scales.
        /// This method called only by previous method:- Acousticevent.SetTimeAndFreqScales().
        /// Translate time/freq dimensions to coordinates in a matrix.
        /// columns of matrix are the freq bins. Origin is top left - as per matrix in the sonogram class.
        /// </summary>
        public static Oblong ConvertEvent2Oblong(AcousticEvent ae)
        {
            // Translate time dimension (seconds) to frames to matrix rows.
            var topRow = (int)Math.Round(ae.TimeStart / ae.FrameOffset);
            var bottomRow = (int)Math.Round((ae.TimeStart + ae.EventDurationSeconds) / ae.FrameOffset);

            //Translate freq dimension = freq bins = matrix columns.
            ConvertHertzToFrequencyBin(ae.IsMelscale, (int)ae.LowFrequencyHertz, (int)ae.HighFrequencyHertz, ae.FreqBinCount, ae.FreqBinWidth, out var leftCol, out var rightCol);

            return new Oblong(topRow, leftCol, bottomRow, rightCol);
        }

        /// <summary>
        /// Should check that Oblong is not null before calling this method.
        /// </summary>
        public Rectangle GetEventAsRectangle() => new Rectangle(this.Oblong.ColumnLeft, this.Oblong.RowTop, this.Oblong.ColWidth, this.Oblong.RowWidth);

        /// <summary>
        /// Sets the passed score and also a value normalised between a min and a max.
        /// </summary>
        public void SetScores(double score, double min, double max)
        {
            this.Score = score;
            this.ScoreNormalised = (score - min) / (max - min);
            if (this.ScoreNormalised > 1.0)
            {
                this.ScoreNormalised = 1.0;
            }

            if (this.ScoreNormalised < 0.0)
            {
                this.ScoreNormalised = 0.0;
            }
        }

        /// <summary>
        /// Draws an event on the image. Allows for custom specification of variables.
        /// Drawing the event requires a time scale and a frequency scale. Hence the additional arguments.
        /// </summary>
        public void DrawEvent<T>(Image<T> imageToReturn, double framesPerSecond, double freqBinWidth, int sonogramHeight)
            where T : unmanaged, IPixel<T>
        {
            Contract.Requires(this.BorderColour != null);
            Contract.Requires(this.HitElements == null || (this.HitElements != null && this.HitColour != null));
            var borderPen = new Pen(this.BorderColour, 1);
            var scorePen = new Pen(this.ScoreColour, 1);

            if (this.TheTrack != null)
            {
                // currently this call assumes that the Track[frame, bin[ elements correspond to the pixels of the passed spectrogram.
                // That is, there is no rescaling of the time and frequency axes.
                this.TheTrack.DrawTrack(imageToReturn, framesPerSecond, freqBinWidth);
                return;
            }

            // calculate top and bottom freq bins
            int minFreqBin = (int)Math.Floor(this.LowFrequencyHertz / freqBinWidth);
            int maxFreqBin = (int)Math.Ceiling(this.HighFrequencyHertz / freqBinWidth);
            int y1 = sonogramHeight - maxFreqBin - 1;
            int y2 = sonogramHeight - minFreqBin - 1;

            // calculate start and end time frames
            int t1 = 0;
            int t2 = 0;
            double duration = this.TimeEnd - this.TimeStart;
            if (duration >= 0.0 && framesPerSecond >= 0.0)
            {
                // -1 because want to draw red line in frame prior to the event start and not cover the event.
                t1 = (int)Math.Round(this.TimeStart * framesPerSecond) - 1;

                t2 = (int)Math.Round(this.TimeEnd * framesPerSecond);
            }
            else if (this.Oblong != null)
            {
                // temporal start and end of event in oblong coordinates
                t1 = this.Oblong.RowTop;
                t2 = this.Oblong.RowBottom;
            }

            imageToReturn.Mutate(g => g.NoAA().DrawRectangle(borderPen, t1, y1, t2, y2));

            //draw on the elements from the hit matrix
            if (this.HitElements != null)
            {
                foreach (var hitElement in this.HitElements)
                {
                    imageToReturn[hitElement.X, sonogramHeight - hitElement.Y] = this.HitColour.Value.ToPixel<T>();
                }
            }

            //draw the score bar to indicate relative score
            var eventHeight = y2 - y1 + 1;
            int scoreHt = (int)Math.Round(eventHeight * this.ScoreNormalised);
            imageToReturn.Mutate(g =>
            {
                g.NoAA().DrawLine(scorePen, t1, y2 - scoreHt, t1, y2 + 1);
                g.DrawTextSafe(this.Name, Drawing.Tahoma6, Color.Black, new PointF(t1, y1 - 4));
            });
        }

        //#################################################################################################################
        //FOLLOWING METHODS DEAL WITH THE OVERLAP OF EVENTS

        /// <summary>
        /// Determines if two events overlap in frequency.
        /// </summary>
        /// <param name="event1">event one.</param>
        /// <param name="event2">event two.</param>
        /// <returns>true if events overlap.</returns>
        public static bool EventsOverlapInFrequency(AcousticEvent event1, AcousticEvent event2)
        {
            //check if event 1 freq band overlaps event 2 freq band
            if (event1.HighFrequencyHertz >= event2.LowFrequencyHertz && event1.HighFrequencyHertz <= event2.HighFrequencyHertz)
            {
                return true;
            }

            // check if event 1 freq band overlaps event 2 freq band
            if (event1.LowFrequencyHertz >= event2.LowFrequencyHertz && event1.LowFrequencyHertz <= event2.HighFrequencyHertz)
            {
                return true;
            }

            //check if event 2 freq band overlaps event 1 freq band
            if (event2.HighFrequencyHertz >= event1.LowFrequencyHertz && event2.HighFrequencyHertz <= event1.HighFrequencyHertz)
            {
                return true;
            }

            // check if event 2 freq band overlaps event 1 freq band
            if (event2.LowFrequencyHertz >= event1.LowFrequencyHertz && event2.LowFrequencyHertz <= event1.HighFrequencyHertz)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if two events overlap in time.
        /// </summary>
        /// <param name="event1">event one.</param>
        /// <param name="event2">event two.</param>
        /// <returns>true if events overlap.</returns>
        public static bool EventsOverlapInTime(AcousticEvent event1, AcousticEvent event2)
        {
            //check if event 1 starts within event 2
            if (event1.EventStartSeconds >= event2.EventStartSeconds && event1.EventStartSeconds <= event2.EventEndSeconds)
            {
                return true;
            }

            // check if event 1 ends within event 2
            if (event1.EventEndSeconds >= event2.EventStartSeconds && event1.EventEndSeconds <= event2.EventEndSeconds)
            {
                return true;
            }

            // now check possibility that event2 is inside event1.
            //check if event 2 starts within event 1
            if (event2.EventStartSeconds >= event1.EventStartSeconds && event2.EventStartSeconds <= event1.EventEndSeconds)
            {
                return true;
            }

            // check if event 2 ends within event 1
            if (event2.EventEndSeconds >= event1.EventStartSeconds && event2.EventEndSeconds <= event1.EventEndSeconds)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Combines overlapping events in the passed List of events and returns a reduced list.
        /// </summary>
        public static List<AcousticEvent> CombineOverlappingEvents(List<AcousticEvent> events, TimeSpan segmentStartOffset)
        {
            if (events.Count < 2)
            {
                return events;
            }

            for (int i = events.Count - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (EventsOverlapInTime(events[i], events[j]) && EventsOverlapInFrequency(events[i], events[j]))
                    {
                        events[j] = AcousticEvent.MergeTwoEvents(events[i], events[j], segmentStartOffset);
                        events.RemoveAt(i);
                        break;
                    }
                }
            }

            return events;
        }

        /// <summary>
        /// Combines events that have similar bottom and top frequency bounds and whose start times are within the passed time range.
        /// </summary>
        public static List<AcousticEvent> CombineSimilarProximalEvents(List<AcousticEvent> events, TimeSpan startDifference, int hertzDifference)
        {
            if (events.Count < 2)
            {
                return events;
            }

            for (int i = events.Count - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    bool eventStartsAreProximal = Math.Abs(events[i].EventStartSeconds - events[j].EventStartSeconds) < startDifference.TotalSeconds;
                    bool eventAreInSimilarFreqBand = Math.Abs(events[i].LowFrequencyHertz - events[j].LowFrequencyHertz) < hertzDifference && Math.Abs(events[i].HighFrequencyHertz - events[j].HighFrequencyHertz) < hertzDifference;
                    if (eventStartsAreProximal && eventAreInSimilarFreqBand)
                    {
                        var segmentStartOffset = TimeSpan.FromSeconds(events[i].SegmentStartSeconds);
                        events[j] = AcousticEvent.MergeTwoEvents(events[i], events[j], segmentStartOffset);
                        events.RemoveAt(i);
                        break;
                    }
                }
            }

            return events;
        }

        /// <summary>
        /// Combines events that are possible stacked harmonics, that is, they are coincident (have similar start and end times)
        /// AND stacked (their maxima are within the passed frequency gap).
        /// </summary>
        public static List<AcousticEvent> CombinePotentialStackedTracks(List<AcousticEvent> events, TimeSpan timeDifference, int hertzDifference)
        {
            if (events.Count < 2)
            {
                return events;
            }

            for (int i = events.Count - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    bool eventsStartTogether = Math.Abs(events[i].EventStartSeconds - events[j].EventStartSeconds) < timeDifference.TotalSeconds;
                    bool eventsEndTogether = Math.Abs(events[i].EventEndSeconds - events[j].EventEndSeconds) < timeDifference.TotalSeconds;
                    bool eventsAreCoincident = eventsStartTogether && eventsEndTogether;
                    bool eventsAreStacked = Math.Abs(events[i].HighFrequencyHertz - events[j].LowFrequencyHertz) < hertzDifference || Math.Abs(events[j].HighFrequencyHertz - events[i].LowFrequencyHertz) < hertzDifference;
                    if (eventsAreCoincident && eventsAreStacked)
                    {
                        var segmentStartOffset = TimeSpan.FromSeconds(events[i].SegmentStartSeconds);
                        events[j] = AcousticEvent.MergeTwoEvents(events[i], events[j], segmentStartOffset);
                        events.RemoveAt(i);
                        break;
                    }
                }
            }

            return events;
        }

        public static AcousticEvent MergeTwoEvents(AcousticEvent e1, AcousticEvent e2, TimeSpan segmentStartOffset)
        {
            //segmentStartOffset = TimeSpan.Zero;
            var minTime = Math.Min(e1.TimeStart, e2.TimeStart);
            var maxTime = Math.Max(e1.TimeEnd, e2.TimeEnd);
            e1.SetEventPositionRelative(segmentStartOffset, minTime, maxTime);
            e1.LowFrequencyHertz = Math.Min(e1.LowFrequencyHertz, e2.LowFrequencyHertz);
            e1.HighFrequencyHertz = Math.Max(e1.HighFrequencyHertz, e2.HighFrequencyHertz);
            e1.Score = Math.Max(e1.Score, e2.Score);
            e1.ScoreNormalised = Math.Max(e1.ScoreNormalised, e2.ScoreNormalised);
            e1.ResultStartSeconds = e1.EventStartSeconds;
            return e1;
        }

        /// <summary>
        /// Returns the first event in the passed list which overlaps with this one IN THE SAME RECORDING.
        /// If no event overlaps return null.
        /// </summary>
        public AcousticEvent OverlapsEventInList(List<AcousticEvent> events)
        {
            foreach (AcousticEvent ae in events)
            {
                if (this.FileName.Equals(ae.FileName) && EventsOverlapInTime(this, ae))
                {
                    return ae;
                }
            }

            return null;
        }

        /*
        /// <summary>
        /// This method not currently called but is POTENTIALLY USEFUL.
        /// Returns the fractional overlap of two events.
        /// Translate time/freq dimensions to coordinates in a matrix.
        /// Freq dimension = bins   = matrix columns. Origin is top left - as per matrix in the sonogram class.
        /// Time dimension = frames = matrix rows.
        /// </summary>
        public static double EventFractionalOverlap(AcousticEvent event1, AcousticEvent event2)
        {
            int timeOverlap = Oblong.RowOverlap(event1.Oblong, event2.Oblong);
            if (timeOverlap == 0)
            {
                return 0.0;
            }

            int hzOverlap = Oblong.ColumnOverlap(event1.Oblong, event2.Oblong);
            if (hzOverlap == 0)
            {
                return 0.0;
            }

            int overlapArea = timeOverlap * hzOverlap;
            double fractionalOverlap1 = overlapArea / (double)event1.Oblong.Area();
            double fractionalOverlap2 = overlapArea / (double)event2.Oblong.Area();

            if (fractionalOverlap1 > fractionalOverlap2)
            {
                return fractionalOverlap1;
            }
            else
            {
                return fractionalOverlap2;
            }
        }
        */

        //#################################################################################################################
        //METHODS FOR SEGMENTATION OF A FREQ BAND BASED ON ACOUSTIC ENERGY

        /// <summary>
        /// Segments or not depending value of boolean doSegmentation.
        /// </summary>
        /// <param name="sonogram">s.</param>
        /// <param name="doSegmentation">segment? yes/no.</param>
        /// <param name="segmentStartOffset">time offset.</param>
        /// <param name="minHz">lower limit of bandwidth.</param>
        /// <param name="maxHz">upper limit of bandwidth.</param>
        /// <param name="smoothWindow">window for smoothing the acoustic intensity array.</param>
        /// <param name="thresholdSD">segmentation threshold - standard deviations above 0 dB.</param>
        /// <param name="minDuration">minimum duration of an event.</param>
        /// <param name="maxDuration">maximum duration of an event.</param>
        public static Tuple<List<AcousticEvent>, double, double, double, double[]> GetSegmentationEvents(
            SpectrogramStandard sonogram,
            bool doSegmentation,
            TimeSpan segmentStartOffset,
            int minHz,
            int maxHz,
            double smoothWindow,
            double thresholdSD,
            double minDuration,
            double maxDuration)
        {
            if (!doSegmentation)
            {
                //by-pass segmentation and make entire recording just one event.
                double oneSD = 0.0;
                double dBThreshold = 0.0;
                double[] intensity = null;
                List<AcousticEvent> segmentEvents = new List<AcousticEvent>();
                var ae = new AcousticEvent(segmentStartOffset, 0.0, sonogram.Duration.TotalSeconds, minHz, maxHz);
                ae.SetTimeAndFreqScales(sonogram.FramesPerSecond, sonogram.FBinWidth);
                segmentEvents.Add(ae);
                return Tuple.Create(segmentEvents, 0.0, oneSD, dBThreshold, intensity);
            }

            var tuple = GetSegmentationEvents(sonogram, segmentStartOffset, minHz, maxHz, smoothWindow, thresholdSD, minDuration, maxDuration);
            return tuple;
        }

        /// <summary>
        /// Segments the acoustic energy in the passed frequency band and returns as list of acoustic events.
        /// Noise reduction is done first.
        /// </summary>
        /// <param name="sonogram">the full spectrogram.</param>
        /// <param name="segmentStartOffset">Start of current segment relative to the recording start.</param>
        /// <param name="minHz">Bottom of the required frequency band.</param>
        /// <param name="maxHz">Top of the required frequency band.</param>
        /// <param name="smoothWindow">To smooth the amplitude array.</param>
        /// <param name="thresholdSD">Determines the threshold for an acoustic event.</param>
        /// <param name="minDuration">Minimum duration of an acceptable acoustic event.</param>
        /// <param name="maxDuration">Maximum duration of an acceptable acoustic event.</param>
        /// <returns>a list of acoustic events.</returns>
        public static Tuple<List<AcousticEvent>, double, double, double, double[]> GetSegmentationEvents(
            SpectrogramStandard sonogram,
            TimeSpan segmentStartOffset,
            int minHz,
            int maxHz,
            double smoothWindow,
            double thresholdSD,
            double minDuration,
            double maxDuration)
        {
            int nyquist = sonogram.SampleRate / 2;
            var tuple = SNR.SubbandIntensity_NoiseReduced(sonogram.Data, minHz, maxHz, nyquist, smoothWindow, sonogram.FramesPerSecond);
            double[] intensity = tuple.Item1; //noise reduced intensity array
            double baselineDb = tuple.Item2;      //baseline dB in the original scale
            double oneSD = tuple.Item3;  //1 SD in dB around the baseline
            double dBThreshold = thresholdSD * oneSD;

            // get list of acoustic events
            var segmentEvents = ConvertIntensityArray2Events(
                intensity,
                segmentStartOffset,
                minHz,
                maxHz,
                sonogram.FramesPerSecond,
                sonogram.FBinWidth,
                dBThreshold,
                minDuration,
                maxDuration);
            foreach (AcousticEvent ev in segmentEvents)
            {
                ev.FileName = sonogram.Configuration.SourceFName;
            }

            return Tuple.Create(segmentEvents, baselineDb, oneSD, dBThreshold, intensity);
        }

        //##############################################################################################################################################
        // THE NEXT FOUR METHODS ARE NOT CURRENTLY CALLED.
        // THEY WERE USED FOR COLLECTING EVENTS INTO DATA SETS for Machine Learning purposes. (Kiwi publications)
        // MAY BE USEFUL IN FUTURE

        /// <summary>
        /// returns all the events in a list that occur in the recording with passed file name.
        /// </summary>
        public static List<AcousticEvent> GetEventsInFile(List<AcousticEvent> eventList, string fileName)
        {
            var events = new List<AcousticEvent>();
            foreach (AcousticEvent ae in eventList)
            {
                if (ae.FileName.Equals(fileName))
                {
                    events.Add(ae);
                }
            }

            return events;
        }

        public static List<AcousticEvent> GetTaggedEventsInFile(List<AcousticEvent> labeledEvents, string filename)
        {
            var events = new List<AcousticEvent>();
            foreach (AcousticEvent ae in events)
            {
                if (ae.FileName.Equals(filename))
                {
                    events.Add(ae);
                }
            }

            return events;
        }

        /// <summary>
        /// Given two lists of AcousticEvents, one being labelled events and the other being predicted events,
        /// this method calculates the accuracy of the predictions in terms of tp, fp, fn etc. The events may come from any number of
        /// recordings or files.
        /// </summary>
        public static void CalculateAccuracy(List<AcousticEvent> results, List<AcousticEvent> labels, out int tp, out int fp, out int fn, out double precision, out double recall, out double accuracy, out string resultsText)
        {
            //init  values
            tp = 0;
            fp = 0;

            //header
            string space = " ";
            int count = 0;
            List<string> resultsSourceFiles = new List<string>();
            string header = string.Format("\nScore Category:    #{0,12}name{0,3}start{0,6}end{0,2}score1{0,2}score2{0,5}duration{0,6}source file", space);
            LoggedConsole.WriteLine(header);
            string line = null;
            var sb = new StringBuilder(header + "\n");
            string previousSourceFile = "  ";

            foreach (AcousticEvent ae in results)
            {
                count++;
                double end = ae.TimeStart + ae.EventDurationSeconds; //calculate end time of the result event
                var labelledEvents = GetEventsInFile(labels, ae.FileName); //get all & only those labelled events in same file as result ae
                resultsSourceFiles.Add(ae.FileName);   //keep list of source files that the detected events come from
                AcousticEvent overlapLabelEvent = ae.OverlapsEventInList(labelledEvents); //get overlapped labelled event
                if (overlapLabelEvent == null)
                {
                    fp++;
                    line =
                        $"False POSITIVE: {count,4} {ae.Name,15} {ae.TimeStart,6:f1} ...{end,6:f1} {ae.Score,7:f1} {ae.Score2,7:f1}\t{ae.EventDurationSeconds,10:f2}";
                }
                else
                {
                    tp++;
                    overlapLabelEvent.Tag = true; //tag because later need to determine fn
                    line =
                        $"True  POSITIVE: {count,4} {ae.Name,15} {ae.TimeStart,6:f1} ...{end,6:f1} {ae.Score,7:f1} {ae.Score2,7:f1}\t{ae.EventDurationSeconds,10:f2}";
                }

                if (previousSourceFile != ae.FileName)
                {
                    LoggedConsole.WriteLine(line + "\t" + ae.FileName);
                    sb.Append(line + "\t" + ae.FileName + "\n");
                    previousSourceFile = ae.FileName;
                }
                else
                {
                    LoggedConsole.WriteLine(line + "\t  ||   ||   ||   ||   ||   ||");
                    sb.Append(line + "\t  ||   ||   ||   ||   ||   ||\n");
                }
            }//end of looking for true and false positives

            //Now calculate the FALSE NEGATIVES. These are the labelled events not tagged in previous search.
            LoggedConsole.WriteLine();
            sb.Append("\n");
            fn = 0;
            count = 0;
            previousSourceFile = " "; //this is just a device to achieve a formatting hwich is easier to interpret
            foreach (AcousticEvent ae in labels)
            {
                count++;
                string hitFile = string.Empty;

                //check if this FN event is in a file that score tp of fp hit.
                if (resultsSourceFiles.Contains(ae.FileName))
                {
                    hitFile = "**";
                }

                if (ae.Tag == false)
                {
                    fn++;
                    line = string.Format(
                        "False NEGATIVE: {0,4} {5,15} {1,6:f1} ...{2,6:f1}    intensity={3}     quality={4}",
                        count,
                        ae.TimeStart,
                        ae.TimeEnd,
                        ae.Intensity,
                        ae.Quality,
                        ae.Name);
                    if (previousSourceFile != ae.FileName)
                    {
                        LoggedConsole.WriteLine(line + "\t" + ae.FileName + " " + hitFile);
                        sb.Append(line + "\t" + ae.FileName + " " + hitFile + "\n");
                        previousSourceFile = ae.FileName;
                    }
                    else
                    {
                        LoggedConsole.WriteLine(line + "\t  ||   ||   ||   ||   ||   ||");
                        sb.Append(line + "\t  ||   ||   ||   ||   ||   ||\n");
                    }
                }
            }

            if (fn == 0)
            {
                line = "NO FALSE NEGATIVES.";
            }
            else
            {
                line = "** This FN event occured in a recording which also scored a tp or fp hit.";
            }

            LoggedConsole.WriteLine(line);
            sb.Append(line + "\n");

            if (tp + fp == 0)
            {
                precision = 0.0;
            }
            else
            {
                precision = tp / (double)(tp + fp);
            }

            if (tp + fn == 0)
            {
                recall = 0.0;
            }
            else
            {
                recall = tp / (double)(tp + fn);
            }

            accuracy = (precision + recall) / 2;

            resultsText = sb.ToString();
        }

        /// <summary>
        /// Given two lists of AcousticEvents, one being labelled events and the other being predicted events,
        /// this method calculates the accuracy of the predictions in terms of tp, fp, fn etc.
        /// This method is similar to the one above except that it is assumed that all the events, both labelled and predicted
        /// come from the same recording.
        /// </summary>
        public static void CalculateAccuracyOnOneRecording(
            List<AcousticEvent> results,
            List<AcousticEvent> labels,
            out int tp,
            out int fp,
            out int fn,
            out double precision,
            out double recall,
            out double accuracy,
            out string resultsText)
        {
            //init  values
            tp = 0;
            fp = 0;
            fn = 0;

            //header
            string space = " ";
            int count = 0;
            List<string> resultsSourceFiles = new List<string>();
            string header = string.Format("PREDICTED EVENTS:  #{0,12}name{0,3}start{0,6}end{0,2}score1{0,2}score2{0,5}duration{0,6}source file", space);

            //LoggedConsole.WriteLine(header);
            string line = null;
            var sb = new StringBuilder(header + "\n");

            foreach (AcousticEvent ae in results)
            {
                count++;
                double end = ae.TimeStart + ae.EventDurationSeconds; //calculate end time of the result event
                var labelledEvents = GetEventsInFile(labels, ae.FileName); //get all & only those labelled events in same file as result ae
                resultsSourceFiles.Add(ae.FileName);   //keep list of source files that the detected events come from
                AcousticEvent overlapLabelEvent = ae.OverlapsEventInList(labelledEvents); //get overlapped labelled event
                if (overlapLabelEvent == null)
                {
                    fp++;
                    line =
                        $"False POSITIVE: {count,4} {ae.Name,15} {ae.TimeStart,6:f1} ...{end,6:f1} {ae.Score,7:f1} {ae.Score2,7:f1}\t{ae.EventDurationSeconds,10:f2}";
                }
                else
                {
                    tp++;
                    overlapLabelEvent.Tag = true; //tag because later need to determine fn
                    line =
                        $"True  POSITIVE: {count,4} {ae.Name,15} {ae.TimeStart,6:f1} ...{end,6:f1} {ae.Score,7:f1} {ae.Score2,7:f1}\t{ae.EventDurationSeconds,10:f2}";
                }

                sb.Append(line + "\t" + ae.FileName + "\n");
            }//end of looking for true and false positives

            //Now calculate the FALSE NEGATIVES. These are the labelled events not tagged in previous search.
            //LoggedConsole.WriteLine();
            sb.Append("\n");
            count = 0;
            foreach (AcousticEvent ae in labels)
            {
                count++;
                if (ae.Tag == false)
                {
                    fn++;
                    line = string.Format(
                        "False NEGATIVE: {0,4} {5,15} {1,6:f1} ...{2,6:f1}    intensity={3}     quality={4}",
                        count,
                        ae.TimeStart,
                        ae.TimeEnd,
                        ae.Intensity,
                        ae.Quality,
                        ae.Name);
                    sb.Append(line + "\t" + ae.FileName + "\n");
                }
            }

            if (tp + fp == 0)
            {
                precision = 0.0;
            }
            else
            {
                precision = tp / (double)(tp + fp);
            }

            if (tp + fn == 0)
            {
                recall = 0.0;
            }
            else
            {
                recall = tp / (double)(tp + fn);
            }

            accuracy = (precision + recall) / 2;

            resultsText = sb.ToString();
        }

        //##############################################################################################################################################
        //  THE NEXT THREE METHODS CONVERT AN ARRAY OF SCORE VALUES (USUALLY INTENSITY VALUES IN A SUB-BAND) TO ACOUSTIC EVENTS.

        public static List<AcousticEvent> ConvertIntensityArray2Events(
            double[] values,
            TimeSpan segmentStartOffset,
            int minHz,
            int maxHz,
            double framesPerSec,
            double freqBinWidth,
            double scoreThreshold,
            double minDuration,
            double maxDuration)
        {
            int count = values.Length;
            var events = new List<AcousticEvent>();
            bool isHit = false;
            double frameOffset = 1 / framesPerSec; //frame offset in fractions of second
            double startTime = 0.0;
            int startFrame = 0;

            //pass over all frames
            for (int i = 0; i < count; i++)
            {
                //start of an event
                if (isHit == false && values[i] > scoreThreshold)
                {
                    isHit = true;
                    startTime = i * frameOffset;
                    startFrame = i;
                }
                else //check for the end of an event
                    if (isHit && values[i] <= scoreThreshold)
                {
                    //this is end of an event, so initialise it
                    isHit = false;
                    double endTime = i * frameOffset;
                    double duration = endTime - startTime;

                    //if (duration < minDuration) continue; //skip events with duration shorter than threshold
                    if (duration < minDuration || duration > maxDuration)
                    {
                        continue; //skip events with duration shorter than threshold
                    }

                    AcousticEvent ev = new AcousticEvent(segmentStartOffset, startTime, duration, minHz, maxHz)
                    {
                        Name = "Acoustic Segment", //default name
                    };
                    ev.SetTimeAndFreqScales(framesPerSec, freqBinWidth);

                    //obtain average intensity score.
                    double av = 0.0;
                    for (int n = startFrame; n <= i; n++)
                    {
                        av += values[n];
                    }

                    ev.Score = av / (i - startFrame + 1);
                    events.Add(ev);
                }
            }

            return events;
        }

        /// <summary>
        /// Given a time series of acoustic amplitude (typically in decibels), finds events that match the passed constraints.
        /// </summary>
        /// <param name="values">an array of amplitude values, typically decibel values.</param>
        /// <param name="segmentStartOffset">not sure what this is about!.</param>
        /// <param name="minHz">minimum freq of event.</param>
        /// <param name="maxHz">maximum freq of event.</param>
        /// <param name="thresholdValue">event threshold in same units as the value array.</param>
        /// <param name="minDuration">minimum duration of an event.</param>
        /// <param name="maxDuration">maximum duration of an event.</param>
        /// <param name="framesPerSec">the time scale - required for drawing events.</param>
        /// <param name="freqBinWidth">the frequency scale - required for drawing events.</param>
        /// <returns>an array of class AcousticEvent.</returns>
        public static List<AcousticEvent> GetEventsAroundMaxima(
            double[] values,
            TimeSpan segmentStartOffset,
            int minHz,
            int maxHz,
            double thresholdValue,
            TimeSpan minDuration,
            TimeSpan maxDuration,
            double framesPerSec,
            double freqBinWidth)
        {
            int count = values.Length;
            var events = new List<AcousticEvent>();
            double frameOffset = 1 / framesPerSec; //frame offset in fractions of second

            // convert min an max times durations to frames
            int minFrames = (int)Math.Floor(minDuration.TotalSeconds * framesPerSec);
            int maxFrames = (int)Math.Ceiling(maxDuration.TotalSeconds * framesPerSec);

            // convert min an max Hertz durations to freq bins
            int minBin = (int)Math.Round(minHz / freqBinWidth);
            int maxBin = (int)Math.Round(maxHz / freqBinWidth);
            int binCount = maxBin - minBin + 1;

            // tried smoothing but not advisable since event onset can be very sudden
            //values = DataTools.filterMovingAverageOdd(values, 3);
            int startFrame = 0;
            int endFrame = 0;

            // for all frames
            for (int i = 1; i < count - minFrames; i++)
            {
                // skip if value is below threshold
                if (values[i] < thresholdValue)
                {
                    continue;
                }

                // skip if value is not maximum
                if (values[i] < values[i - 1] || values[i] < values[i + 1])
                {
                    continue;
                }

                int maxFrame = i;

                // find start frame of current event
                while (values[i] > thresholdValue)
                {
                    if (i <= 0)
                    {
                        break;
                    }

                    i--;
                }

                startFrame = i + 1;

                // find end frame of current event
                i = maxFrame;
                while (values[i] > thresholdValue)
                {
                    i++;
                }

                endFrame = i;

                int frameCount = endFrame - startFrame + 1;
                if (frameCount >= minFrames && frameCount <= maxFrames)
                {
                    double startTime = startFrame * frameOffset; // time in seconds
                    double eventDuration = frameCount * frameOffset; // time in seconds
                    AcousticEvent ev = new AcousticEvent(segmentStartOffset, startTime, eventDuration, minHz, maxHz)
                    {
                        Name = "Event", //default name
                        FrameCount = frameCount,
                        FreqBinCount = binCount,
                        Oblong = new Oblong(startFrame, minBin, endFrame, maxBin),
                    };

                    ev.SetTimeAndFreqScales(framesPerSec, freqBinWidth);

                    //obtain average intensity score. Note-first frame is not actually in the event.
                    var subArray = DataTools.Subarray(values, startFrame + 1, frameCount);
                    ev.Score = subArray.Average();
                    events.Add(ev);
                }

                i++;
            }

            return events;
        }

        /// <summary>
        /// A general method to convert an array of score values to a list of AcousticEvents.
        /// NOTE: The score array is assumed to be temporal i.e. each element of the array is derived from a time frame.
        /// The method uses the passed scoreThreshold in order to calculate a normalised score.
        /// Max possible score := threshold * 5.
        /// normalised score := score / maxPossibleScore.
        /// Some analysis techniques (e.g. Oscillation Detection) have their own methods for extracting events from score arrays.
        /// </summary>
        /// <param name="scores">the array of scores.</param>
        /// <param name="minHz">lower freq bound of the acoustic event.</param>
        /// <param name="maxHz">upper freq bound of the acoustic event.</param>
        /// <param name="framesPerSec">the time scale required by AcousticEvent class.</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class.</param>
        /// <param name="scoreThreshold">threshold.</param>
        /// <param name="minDuration">duration of event must exceed this to count as an event.</param>
        /// <param name="maxDuration">duration of event must be less than this to count as an event.</param>
        /// <param name="segmentStart">offset.</param>
        /// <returns>a list of acoustic events.</returns>
        public static List<AcousticEvent> ConvertScoreArray2Events(
            double[] scores,
            int minHz,
            int maxHz,
            double framesPerSec,
            double freqBinWidth,
            double scoreThreshold,
            double minDuration,
            double maxDuration,
            TimeSpan segmentStart)
        {
            int count = scores.Length;
            var events = new List<AcousticEvent>();
            double maxPossibleScore = 5 * scoreThreshold; // used to calculate a normalised score between 0 - 1.0
            bool isHit = false;
            double frameOffset = 1 / framesPerSec;
            double startTimeInSegment = 0.0; // units = seconds
            int startFrame = 0;

            // pass over all frames
            for (int i = 0; i < count; i++)
            {
                if (isHit == false && scores[i] >= scoreThreshold)
                {
                    //start of an event
                    isHit = true;
                    startTimeInSegment = i * frameOffset;
                    startFrame = i;
                }
                else // check for the end of an event
                if (isHit && scores[i] <= scoreThreshold)
                {
                    // this is end of an event, so initialise it
                    isHit = false;
                    double endTime = i * frameOffset;
                    double duration = endTime - startTimeInSegment;

                    // if (duration < minDuration) continue; //skip events with duration shorter than threshold
                    if (duration < minDuration || duration > maxDuration)
                    {
                        //skip events with duration shorter than threshold
                        continue;
                    }

                    // obtain an average score for the duration of the potential event.
                    double av = 0.0;
                    for (int n = startFrame; n <= i; n++)
                    {
                        av += scores[n];
                    }

                    av /= i - startFrame + 1;

                    // Initialize the event.
                    AcousticEvent ev = new AcousticEvent(segmentStart, startTimeInSegment, duration, minHz, maxHz);
                    ev.SetTimeAndFreqScales(framesPerSec, freqBinWidth);
                    ev.Score = av;

                    // normalised to the user supplied threshold
                    ev.ScoreNormalised = ev.Score / maxPossibleScore;
                    if (ev.ScoreNormalised > 1.0)
                    {
                        ev.ScoreNormalised = 1.0;
                    }

                    ev.Score_MaxPossible = maxPossibleScore;

                    //find max score and its time
                    double max = -double.MaxValue;
                    for (int n = startFrame; n <= i; n++)
                    {
                        if (scores[n] > max)
                        {
                            max = scores[n];
                            ev.Score_MaxInEvent = scores[n];
                            ev.Score_TimeOfMaxInEvent = n * frameOffset;
                        }
                    }

                    events.Add(ev);
                }
            }

            return events;
        }

        /// <summary>
        /// FOR POSSIBLE DELETION!
        /// THis method called only once from a frog recogniser class that is no longer used> LitoriaCaerulea:RecognizerBase.
        /// THis method is potentially useful but can be deleted.
        /// Attempts to reconstruct an array of scores from a list of acoustic events.
        /// The events are required to have the passed name (a filter).
        /// The events are assumed to contain sufficient info about frame rate in order to populate the array.
        /// </summary>
        public static double[] ExtractScoreArrayFromEvents(List<AcousticEvent> events, int arraySize, string nameOfTargetEvent)
        {
            double[] scores = new double[arraySize];
            if (events == null || events.Count == 0)
            {
                return scores;
            }

            double windowOffset = events[0].FrameOffset;
            double frameRate = 1 / windowOffset; //frames per second

            foreach (AcousticEvent ae in events)
            {
                if (!ae.Name.Equals(nameOfTargetEvent))
                {
                    //skip irrelevant events
                    continue;
                }

                int startFrame = (int)(ae.TimeStart * frameRate);
                int endFrame = (int)((ae.TimeStart + ae.EventDurationSeconds) * frameRate);

                for (int s = startFrame; s <= endFrame; s++)
                {
                    scores[s] = ae.Score_MaxInEvent;
                }
            }

            return scores;
        }

        //##############################################################################################################################################
        // METHODS to CLUSTER acoustic events

        /// <summary>
        /// Although not currently used, this method and following methods could be useful in future for clustering of events.
        /// </summary>
        public static List<List<AcousticEvent>> ClusterEvents(AcousticEvent[] events)
        {
            LoggedConsole.WriteLine("# CLUSTERING EVENTS");
            var clusters = new List<List<AcousticEvent>>();

            var firstCluster = new List<AcousticEvent> { events[0] };
            clusters.Add(firstCluster);

            for (int e = 1; e < events.Length; e++)
            {
                const double tolerance = 0.01;

                double[] scoreArray = new double[clusters.Count];
                for (int c = 0; c < clusters.Count; c++)
                {
                    double distance = DistanceFromCluster(events[e], clusters[c]);
                    scoreArray[c] = distance;
                }

                int minId = DataTools.GetMinIndex(scoreArray);

                if (scoreArray[minId] < tolerance)
                {
                    clusters[minId].Add(events[e]);
                }
                else
                {
                    // if get to here, we have no match and therefore create a new cluster.
                    var newCluster = new List<AcousticEvent> { events[e] };
                    clusters.Add(newCluster);
                }
            }

            return clusters;
        }

        public static double DistanceFromCluster(AcousticEvent ae, List<AcousticEvent> cluster)
        {
            // take first event as the centroid
            var centroid = cluster[0];

            // now compare the time duration of the event with the cluster
            double distance = centroid.EventDurationSeconds - ae.EventDurationSeconds;
            if (Math.Abs(distance) > 0.75)
            {
                return 1.0;
            }

            double topFreqDifference = centroid.LowFrequencyHertz - ae.LowFrequencyHertz;
            if (Math.Abs(topFreqDifference) > 300)
            {
                return 1.0;
            }

            double bottomFreqDifference = centroid.HighFrequencyHertz - ae.HighFrequencyHertz;
            if (Math.Abs(bottomFreqDifference) > 300)
            {
                return 1.0;
            }

            return 0.0;
        }

        public static void AssignClusterIds(List<List<AcousticEvent>> clusters)
        {
            LoggedConsole.WriteLine("# ASSIGN CLUSTER IDs");

            for (int c = 0; c < clusters.Count; c++)
            {
                for (int e = 0; e < clusters[c].Count; e++)
                {
                    clusters[c][e].Quality = c;
                }
            }
        }
    }
}