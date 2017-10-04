﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using Acoustics.Shared.Contracts;
    using Acoustics.Shared.Csv;
    using AnalysisBase.ResultBases;
    using CsvHelper.Configuration;
    using DSP;
    using StandardSpectrograms;
    using TowseyLibrary;

    public class AcousticEvent : EventBase
    {
        public static readonly Color DefaultBorderColor = Color.FromArgb(255, Color.Crimson);

        public static readonly Color DefaultScoreColor = Color.FromArgb(255, Color.Lime);

        public sealed class AcousticEventClassMap : CsvClassMap<AcousticEvent>
        {
            public static readonly string[] IgnoredProperties =
                {
                    nameof(TimeStart), nameof(TimeEnd), nameof(LowFrequencyHertz), nameof(HighFrequencyHertz),
                    nameof(Bandwidth), nameof(IsMelscale), nameof(FrameOffset),
                    nameof(FramesPerSecond), nameof(Name2), nameof(ScoreComment),
                    nameof(ScoreNormalised), nameof(Score_MaxPossible),
                    nameof(Score_MaxInEvent), nameof(Score_TimeOfMaxInEvent),
                    nameof(Score2Name), nameof(Score2), nameof(Periodicity), nameof(DominantFreq),
                    nameof(Tag), nameof(Intensity), nameof(Quality), nameof(HitColour),
                    nameof(EventDurationSeconds), nameof(EventStartSeconds), nameof(EventEndSeconds),
                };

            public static readonly string[] RemappedProperties =
            {
                nameof(LowFrequencyHertz), nameof(HighFrequencyHertz), nameof(EventDurationSeconds), nameof(EventStartSeconds), nameof(EventEndSeconds),
            };

            public AcousticEventClassMap()
            {
                this.AutoMap();

                foreach (var ignoredProperty in IgnoredProperties)
                {
                    var index = this.PropertyMaps.IndexOf(pm => pm.Data.Property.Name == ignoredProperty);
                    this.PropertyMaps.RemoveAt(index);
                }

                this.Map(m => m.EventStartSeconds).Index(0);
                this.Map(m => m.EventEndSeconds).Index(2);
                this.Map(m => m.EventDurationSeconds).Index(3);
                this.GetPropertyMap(m => ((EventBase)m).LowFrequencyHertz).Index(4);
                this.Map(m => m.HighFrequencyHertz).Index(5);
                this.References<Oblong.OblongClassMap>(m => m.Oblong);

                // Make sure HitElements is always in the last column!
                this.PropertyMap<AcousticEvent>(m => m.HitElements).Index(1000);
            }
        }

        /// <summary>
        /// Gets or sets units = seconds
        /// Time offset from start of current segment to start of event.
        /// NOTE: AcousticEvents do not have a notion of time offset wrt start of recording ; - only to start of current recording segment.
        /// Proxied to EventBase.EventStartSeconds
        /// </summary>
        public double TimeStart { get; set; }

        /// <summary>
        /// Gets or sets units = seconds
        /// Time offset from start of current segment to end of the event.
        /// Written into the csv file under column "EventEndSeconds"
        /// This field is NOT in EventBase. EventBase only requires TimeStart
        ///  because it is designed to also accomodate points.
        /// </summary>
        public double TimeEnd { get; set; }

        public double EventEndSeconds => this.TimeEnd + this.SegmentStartSeconds;

        public override double EventStartSeconds => this.TimeStart + this.SegmentStartSeconds;

        public void SetEventPositionRelative(
            TimeSpan segmentStartOffset,
            double eventStartSegment,
            double eventEndSegment)
        {
            this.TimeStart = eventStartSegment;
            this.TimeEnd = eventEndSegment;

            this.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
        }

        /// <summary>
        /// Gets or sets units = Hertz
        /// Proxied to EventBase.MinHz
        /// </summary>
        public new double LowFrequencyHertz
        {
            get
            {
                return base.LowFrequencyHertz ?? default(double);
            }

            set
            {
                base.LowFrequencyHertz = value;
            }
        }

        /// <summary>units = Hertz</summary>
        public double HighFrequencyHertz { get; set; }

        public double Bandwidth
        {
            get
            {
                return this.HighFrequencyHertz - this.LowFrequencyHertz + 1;
            }
        }

        public bool IsMelscale { get; set; }

        public Oblong Oblong { get; set; }

        /// <summary> Gets or sets required for conversions to & from MEL scale AND for drawing event on spectrum</summary>
        public int FreqBinCount { get; set; }

        /// <summary>
        /// required for freq-binID conversions
        /// </summary>
        public double FreqBinWidth { get; private set; }

        /// <summary> Frame duration in seconds</summary>
        public double FrameDuration { get; private set; }

        /// <summary> Time between frame starts in seconds. Inverse of FramesPerSecond</summary>
        public double FrameOffset { get; set; }

        /// <summary> Number of frame starts per second. Inverse of the frame offset</summary>
        public double FramesPerSecond { get; set; }

        //PROPERTIES OF THE EVENTS i.e. Name, SCORE ETC
        public string SpeciesName { get; set; }

        public string Name { get; set; }

        public string Name2 { get; set; }

        /// <summary> Average score through the event.</summary>
        public string ScoreComment { get; set; }

        /// <summary> Score normalised in range [0,1]. NOTE: Max is set = to five times user supplied threshold</summary>
        public double ScoreNormalised { get; set; }

        /// <summary> Max Possible Score: set = to 5x user supplied threshold. An arbitrary value used for score normalisation.</summary>
        public double Score_MaxPossible { get; private set; }

        public double Score_MaxInEvent { get; set; }

        public double Score_TimeOfMaxInEvent { get; set; }

        public string Score2Name { get; set; }

        /// <summary> second score if required</summary>
        public double Score2 { get; set; } // e.g. for Birgits recognisers

        /// <summary>
        /// Gets or sets a list of points that can be used to identifies features in spectrogram relative to the Event.
        /// i.e. Points can be outside of events and can have negative values.
        /// Point location is relative to the top left corner of the event.
        /// </summary>
        public List<Point> Points { get; set; }

        public double Periodicity { get; set; } // for events which have an oscillating acoustic energy - used for frog calls

        public double DominantFreq { get; set; } // the dominant freq in the event - used for frog calls

        // double I1MeandB; //mean intensity of pixels in the event prior to noise subtraction
        // double I1Var;    //,
        // double I2MeandB; // mean intensity of pixels in the event after Wiener filter, prior to noise subtraction
        // double I2Var;    //,
        double I3Mean;      // mean intensity of pixels in the event AFTER noise reduciton - USED FOR CLUSTERING
        double I3Var;       // variance of intensity of pixels in the event.

        //KIWI SCORES
        public double kiwi_durationScore;
        public double kiwi_hitScore;
        public double kiwi_snrScore;
        public double kiwi_sdPeakScore;
        public double kiwi_intensityScore;
        public double kiwi_gridScore;
        public double kiwi_chirpScore;
        public double kiwi_bandWidthScore;
        public double kiwi_deltaPeriodScore;
        public double kiwi_comboScore;

        /// <summary>Gets or sets a value indicating whether use this if want to filter or tag some members of a list for some purpose.</summary>
        public bool Tag { get; set; }

        /// <summary>Gets or sets assigned value when reading in a list of user identified events. Indicates a user assigned assessment of event intensity</summary>
        public int Intensity { get; set; }

        /// <summary>Gets or sets assigned value when reading in a list of user identified events. Indicates a user assigned assessment of event quality</summary>
        public int Quality { get; set; }

        public Color BorderColour { get; set; }

        public Color ScoreColour { get; set; }

        public AcousticEvent()
        {
            this.BorderColour = DefaultBorderColor;
            this.ScoreColour = DefaultScoreColor;
            this.HitColour = Color.FromArgb(128, this.BorderColour);
            this.IsMelscale = false;
        }

        public AcousticEvent(TimeSpan segmentStartOffset, double startTime, double eventDuration, double minFreq, double maxFreq)
            : this()
        {
            this.SetEventPositionRelative(segmentStartOffset, startTime, startTime + eventDuration);

            this.LowFrequencyHertz = minFreq;
            this.HighFrequencyHertz = maxFreq;

            // have no info to convert time/Hz values to coordinates
            this.Oblong = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcousticEvent"/> class.
        /// This constructor currently works ONLY for linear Hertz scale events.
        /// </summary>
        /// <param name="o">An oblong initialized with bin and frame numbers marking location of the event</param>
        /// <param name="NyquistFrequency">to set the freq scale</param>
        /// <param name="binCount">to set the freq scale</param>
        /// <param name="frameDuration">tseconds duration of a frame - to set the time scale</param>
        /// <param name="frameStep">seconds between frame starts i.e. frame step; i.e. inverse of frames per second. Sets the time scale for an event</param>
        /// <param name="frameCount">to set the time scale</param>
        public AcousticEvent(TimeSpan segmentStartOffset, Oblong o, int NyquistFrequency, int binCount, double frameDuration, double frameStep, int frameCount) : this()
        {
            this.Oblong = o;
            this.FreqBinWidth = NyquistFrequency / (double)binCount;
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
        /// <summary>Gets the event duration in seconds</summary>
        public double EventDurationSeconds => this.TimeEnd - this.TimeStart;

        /// <summary>
        /// Gets or sets which profile (combination of settings in a config file) produced this event
        /// </summary>
        public string Profile { get; set; }

        public void DoMelScale(bool doMelscale, int freqBinCount)
        {
            this.IsMelscale = doMelscale;
            this.FreqBinCount = freqBinCount;
        }

        public void SetTimeAndFreqScales(int samplingRate, int windowSize, int windowOffset)
        {
            double frameDuration, frameOffset, framesPerSecond;
            CalculateTimeScale(samplingRate, windowSize, windowOffset, out frameDuration, out frameOffset, out framesPerSecond);
            this.FrameDuration = frameDuration;    //frame duration in seconds
            this.FrameOffset = frameOffset;      //frame offset in seconds
            this.FramesPerSecond = framesPerSecond;  //inverse of the frame offset

            int binCount;
            double binWidth;
            CalculateFreqScale(samplingRate, windowSize, out binCount, out binWidth);
            this.FreqBinCount = binCount; //required for conversions to & from MEL scale
            this.FreqBinWidth = binWidth; //required for freq-binID conversions

            if (this.Oblong == null)
            {
                this.Oblong = ConvertEvent2Oblong(this);
            }
        }

        public void SetTimeAndFreqScales(double framesPerSec, double freqBinWidth)
        {
            //this.FrameDuration = frameDuration;     //frame duration in seconds
            this.FramesPerSecond = framesPerSec;      //inverse of the frame offset
            this.FrameOffset = 1 / framesPerSec;      //frame offset in seconds

            //this.FreqBinCount = binCount;           //required for conversions to & from MEL scale
            this.FreqBinWidth = freqBinWidth;         //required for freq-binID conversions

            if (this.Oblong == null)
            {
                this.Oblong = ConvertEvent2Oblong(this);
            }
        }

        /// <summary>
        /// Calculates the matrix/image indices of the acoustic event, when given the time/freq scales.
        /// This method called only by previous method:- Acousticevent.SetTimeAndFreqScales()
        /// Translate time/freq dimensions to coordinates in a matrix.
        /// columns of matrix are the freq bins. Origin is top left - as per matrix in the sonogram class.
        /// </summary>
        public static Oblong ConvertEvent2Oblong(AcousticEvent ae)
        {
            // Translate time dimension = frames = matrix rows.
            int topRow;
            int bottomRow;
            Time2RowIDs(ae.TimeStart, ae.EventDurationSeconds, ae.FrameOffset, out topRow, out bottomRow);

            //Translate freq dimension = freq bins = matrix columns.
            int leftCol;
            int rightCol;
            Freq2BinIDs(ae.IsMelscale, (int)ae.LowFrequencyHertz, (int)ae.HighFrequencyHertz, ae.FreqBinCount, ae.FreqBinWidth, out leftCol, out rightCol);

            return new Oblong(topRow, leftCol, bottomRow, rightCol);
        }

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

        public string WriteProperties()
        {
            return " min-max=" + this.LowFrequencyHertz + "-" + this.HighFrequencyHertz + ",  " + this.Oblong.ColumnLeft + "-" + this.Oblong.ColumnRight;
        }

        /// <summary>
        /// Draws an event on the image. Uses the fields already set on the audio event to determine correct placement.
        /// Fields requireed to be set include: `FramesPerSecond`, `FreqBinWidth`.
        /// </summary>
        public void DrawEvent(Bitmap sonogram)
        {
            Graphics g = Graphics.FromImage(sonogram);
            this.DrawEvent(g, sonogram, this.FramesPerSecond, this.FreqBinWidth, sonogram.Height);
        }

        /// <summary>
        /// Draws an event on the image. Allows for custom specification of variables.
        /// </summary>
        public void DrawEvent(Graphics g, Bitmap imageToReturn, double framesPerSecond, double freqBinWidth, int sonogramHeight)
        {
            Contract.Requires(this.BorderColour != null);
            Contract.Requires(this.HitElements == null || (this.HitElements != null && this.HitColour != null));
            var borderPen = new Pen(this.BorderColour);
            var scorePen = new Pen(this.ScoreColour);

            // calculate top and bottom freq bins
            int minFreqBin = (int)Math.Round(this.LowFrequencyHertz / freqBinWidth);
            int maxFreqBin = (int)Math.Round(this.HighFrequencyHertz / freqBinWidth);
            int height = maxFreqBin - minFreqBin + 1;
            int y = sonogramHeight - maxFreqBin - 1;

            // calculate start and end time frames
            int t1 = 0;
            int tWidth = 0;
            double duration = this.TimeEnd - this.TimeStart;
            if (duration != 0.0 && framesPerSecond != 0.0)
            {
                t1 = (int)Math.Round(this.TimeStart * framesPerSecond); // temporal start of event
                tWidth = (int)Math.Round(duration * framesPerSecond);
            }
            else if (this.Oblong != null)
            {
                t1 = this.Oblong.RowTop; // temporal start of event
                tWidth = this.Oblong.RowBottom - t1 + 1;
            }

            // 14-Feb-12 - Anthony - changed default brush so border would actually render with color
            g.DrawRectangle(borderPen, t1, y, tWidth, height);

            if (this.HitElements != null)
            {
                foreach (var hitElement in this.HitElements)
                {
                    imageToReturn.SetPixel(hitElement.X, sonogramHeight - hitElement.Y, this.HitColour.Value);
                }
            }

            //draw the score bar to indicate relative score
            int scoreHt = (int)Math.Round(height * this.ScoreNormalised);
            int y1 = y + height;
            int y2 = y1 - scoreHt;

            //g.DrawLine(scorePen, t1 + 1, y1, t1 + 1, y2);
            //g.DrawLine(scorePen, t1 + 2, y1, t1 + 2, y2);
            g.DrawLine(scorePen, t1, y1, t1, y2);
            g.DrawString(this.Name, new Font("Tahoma", 6), Brushes.Black, new PointF(t1, y - 1));

            // ################ draw quality: this is hack for Michael. Please keep this - Oct 2016
            //g.DrawString($"{this.Quality}", new Font("Tahoma", 6), Brushes.Black, new PointF(t1, y - 10));
        }

        /// <summary>
        /// Passed point is relative to top-left corner of the Acoustic Event.
        /// Oblong needs to be set for this method to work
        /// </summary>
        public void DrawPoint(Bitmap bmp, Point point, Color colour)
        {
            if (bmp == null)
            {
                return;
            }

            int maxFreqBin = (int)Math.Round(this.HighFrequencyHertz / this.FreqBinWidth);
            int row = bmp.Height - maxFreqBin - 1 + point.Y;
            int t1 = (int)Math.Round(this.TimeStart * this.FramesPerSecond); // temporal start of event
            int col = t1 + point.X;
            if (row >= bmp.Height)
            {
                row = bmp.Height - 1;
            }

            bmp.SetPixel(col, row, colour);
        }

        /// <summary>
        /// Returns the first event in the passed list which overlaps with this one IN THE SAME RECORDING.
        /// If no event overlaps return null.
        /// </summary>
        public AcousticEvent OverlapsEventInList(List<AcousticEvent> events)
        {
            foreach (AcousticEvent ae in events)
            {
                if ((this.FileName.Equals(ae.FileName)) && (this.Overlaps(ae)))
                {
                    return ae;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns true/false if this event time-overlaps the passed event.
        /// Overlap in frequency dimension is ignored.
        /// The overlap determination is made on the start and end time points.
        /// There are two possible overlaps to be checked
        /// </summary>
        public bool Overlaps(AcousticEvent ae)
        {
            if ((this.TimeStart < ae.TimeEnd) && (this.TimeEnd > ae.TimeStart))
            {
                return true;
            }

            if ((ae.TimeStart < this.TimeEnd) && (ae.TimeEnd > this.TimeStart))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the fractional overlap of two events.
        /// Translate time/freq dimensions to coordinates in a matrix.
        /// Freq dimension = bins   = matrix columns. Origin is top left - as per matrix in the sonogram class.
        /// Time dimension = frames = matrix rows.
        /// </summary>
        public static double EventFractionalOverlap(AcousticEvent event1, AcousticEvent event2)
        {
            //if (event1.EndTime < event2.StartTime) return 0.0;
            //if (event2.EndTime < event1.StartTime) return 0.0;
            //if (event1.MaxFreq < event2.MinFreq)   return 0.0;
            //if (event2.MaxFreq < event1.MinFreq)   return 0.0;
            //at this point the two events do overlap

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

        //#################################################################################################################
        //METHODS TO CONVERT BETWEEN FREQ BIN AND HERZ OR MELS

        /// <summary>
        /// converts frequency bounds of an event to left and right columns of object in sonogram matrix
        /// NOTE: binCount is required only if freq is in Mel scale
        /// </summary>
        /// <param name="doMelscale">mel scale</param>
        /// <param name="minFreq">lower freq bound</param>
        /// <param name="maxFreq">upper freq bound</param>
        /// <param name="nyquist">Nyquist freq in Herz</param>
        /// <param name="binWidth">frequency scale</param>
        /// <param name="leftCol">return bin index for lower freq bound</param>
        /// <param name="rightCol">return bin index for upper freq bound</param>
        public static void Freq2BinIDs(bool doMelscale, int minFreq, int maxFreq, int nyquist, double binWidth, out int leftCol, out int rightCol)
        {
            if (doMelscale)
            {
                Freq2MelsBinIDs(minFreq, maxFreq, binWidth, nyquist, out leftCol, out rightCol);
            }
            else
            {
                Freq2HerzBinIDs(minFreq, maxFreq, binWidth, out leftCol, out rightCol);
            }
        }

        public static void Freq2HerzBinIDs(int minFreq, int maxFreq, double binWidth, out int leftCol, out int rightCol)
        {
            leftCol = (int)Math.Round(minFreq / binWidth);
            rightCol = (int)Math.Round(maxFreq / binWidth);
        }

        public static void Freq2MelsBinIDs(int minFreq, int maxFreq, double binWidth, int nyquistFrequency, out int leftCol, out int rightCol)
        {
            int binCount = (int)(nyquistFrequency / binWidth) + 1;
            double maxMel = MFCCStuff.Mel(nyquistFrequency);
            int melRange = (int)(maxMel - 0 + 1);
            double binsPerMel = binCount / (double)melRange;
            leftCol = (int)Math.Round((double)MFCCStuff.Mel(minFreq) * binsPerMel);
            rightCol = (int)Math.Round((double)MFCCStuff.Mel(maxFreq) * binsPerMel);
        }

        //#################################################################################################################
        //METHODS TO CONVERT BETWEEN TIME BIN AND SECONDS

        public static void Time2RowIDs(double startTime, double duration, double frameOffset, out int topRow, out int bottomRow)
        {
            topRow = (int)Math.Round(startTime / frameOffset);
            bottomRow = (int)Math.Round((startTime + duration) / frameOffset);
        }

        public void SetNetIntensityAfterNoiseReduction(double mean, double var)
        {
            this.I3Mean = mean;
            this.I3Var = var;
        }

        /// <summary>
        /// returns the frame duration and offset duration in seconds
        /// </summary>
        /// <param name="samplingRate">signal samples per second</param>
        /// <param name="windowSize">number of signal samples in one window or frame.</param>
        /// <param name="windowOffset">number of signal samples between start of one frame and start of next frame.</param>
        /// <param name="frameDuration">units = seconds</param>
        /// <param name="frameOffset">units = second</param>
        /// <param name="framesPerSecond">number of frames in one second.</param>
        public static void CalculateTimeScale(int samplingRate, int windowSize, int windowOffset, out double frameDuration, out double frameOffset, out double framesPerSecond)
        {
            frameDuration = windowSize / (double)samplingRate;
            frameOffset = windowOffset / (double)samplingRate;
            framesPerSecond = 1 / frameOffset;
        }

        public static void CalculateFreqScale(int samplingRate, int windowSize, out int binCount, out double binWidth)
        {
            binCount = windowSize / 2;
            binWidth = samplingRate / (double)windowSize; //= Nyquist / binCount
        }

        public static void WriteEvents(List<AcousticEvent> eventList, ref StringBuilder sb)
        {
            if (eventList.Count == 0)
            {
                string line =
                    $"#     Event Name\t{"Start", 8:f3}\t{"End",6:f3}\t{"MinF"}\t{"MaxF"}\t{"Score1":f2}\t{"Score2":f1}\t{"SourceFile"}";
                sb.AppendLine(line);
                line = $"{"NoEvent"}\t{0.000, 8:f3}\t{0.000, 8:f3}\t{"N/A"}\t{"N/A"}\t{0.000:f2}\t{0.000:f1}\t{"N/A"}";
                sb.AppendLine(line);
            }
            else
            {
                AcousticEvent ae1 = eventList[0];
                string line =
                    $"#     Event Name\t{"Start",8:f3}\t{"End",6:f3}\t{"MinF"}\t{"MaxF"}\t{"Score":f2}\t{ae1.Score2Name:f1}\t{"SourceFile"}";
                sb.AppendLine(line);
                foreach (AcousticEvent ae in eventList)
                {
                    line = string.Format(
                        "{0}\t{1,8:f3}\t{2,8:f3}\t{3}\t{4}\t{5:f2}\t{6:f1}\t{7}",
                        ae.Name,
                        ae.TimeStart,
                        ae.TimeEnd,
                        ae.LowFrequencyHertz,
                        ae.HighFrequencyHertz,
                        ae.Score,
                        ae.Score2,
                        ae.FileName);
                    sb.AppendLine(line);
                }
            }
        }

        /// <summary>
        /// used to write lists of acousitc event data to an excell spread sheet.
        /// </summary>
        public static StringBuilder WriteEvents(List<AcousticEvent> eventList, string str)
        {
            StringBuilder sb = new StringBuilder();
            if (eventList.Count == 0)
            {
                string line = string.Format(
                    str + "\t{0}\t{1,8:f3}\t{2,8:f3}\t{3}\t{4}\t{5:f2}\t{6:f1}\t{7}", "NoEvent", 0.000, 0.000, "N/A", "N/A", 0.000, 0.000, "N/A");
                sb.AppendLine(line);
            }
            else
            {
                foreach (AcousticEvent ae in eventList)
                {
                    string line = string.Format(
                        str + "\t{0}\t{1,8:f3}\t{2,8:f3}\t{3}\t{4}\t{5:f2}\t{6:f1}\t{7}",
                        ae.Name,
                        ae.TimeStart,
                        ae.TimeEnd,
                        ae.LowFrequencyHertz,
                        ae.HighFrequencyHertz,
                        ae.Score,
                        ae.Score2,
                        ae.FileName);

                    sb.AppendLine(line);
                }
            }

            return sb;
        }

        /// <summary>
        /// Segments or not depending value of boolean doSegmentation
        /// </summary>
        /// <param name="sonogram">s</param>
        /// <param name="doSegmentation">segment? yes/no</param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="minHz">lower limit of bandwidth</param>
        /// <param name="maxHz">upper limit of bandwidth</param>
        /// <param name="smoothWindow">window for smoothing the acoustic intensity array</param>
        /// <param name="thresholdSD">segmentation threshold - standard deviations above 0 dB</param>
        /// <param name="minDuration">minimum duration of an event</param>
        /// <param name="maxDuration">maximum duration of an event</param>
        /// <returns></returns>
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

        public static Tuple<List<AcousticEvent>, double, double, double, double[]> GetSegmentationEvents(SpectrogramStandard sonogram, TimeSpan segmentStartOffset,
                                    int minHz, int maxHz, double smoothWindow, double thresholdSD, double minDuration, double maxDuration)
        {
            int nyquist = sonogram.SampleRate / 2;
            var tuple = SNR.SubbandIntensity_NoiseReduced(sonogram.Data, minHz, maxHz, nyquist, smoothWindow, sonogram.FramesPerSecond);
            double[] intensity = tuple.Item1; //noise reduced intensity array
            double Q = tuple.Item2;      //baseline dB in the original scale
            double oneSD = tuple.Item3;  //1 SD in dB around the baseline
            double dBThreshold = thresholdSD * oneSD;

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
                //ev.Name = callName;
            }

            return Tuple.Create(segmentEvents, Q, oneSD, dBThreshold, intensity);
        }

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
        } // end method GetEventsInFile(List<AcousticEvent> eventList, string fileName)

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
        /// recordings or files
        /// </summary>
        /// <param name="results"></param>
        /// <param name="labels"></param>
        /// <param name="tp"></param>
        /// <param name="fp"></param>
        /// <param name="fn"></param>
        /// <param name="precision"></param>
        /// <param name="recall"></param>
        /// <param name="accuracy"></param>
        /// <param name="resultsText"></param>
        public static void CalculateAccuracy(List<AcousticEvent> results, List<AcousticEvent> labels, out int tp, out int fp, out int fn,
                                         out double precision, out double recall, out double accuracy, out string resultsText)
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
                AcousticEvent overlapLabelEvent = ae.OverlapsEventInList(labelledEvents);//get overlapped labelled event
                if (overlapLabelEvent == null)
                {
                    fp++;
                    line = string.Format("False POSITIVE: {0,4} {1,15} {2,6:f1} ...{3,6:f1} {4,7:f1} {5,7:f1}\t{6,10:f2}", count, ae.Name, ae.TimeStart, end, ae.Score, ae.Score2, ae.EventDurationSeconds);
                }
                else
                {
                    tp++;
                    overlapLabelEvent.Tag = true; //tag because later need to determine fn
                    line = string.Format("True  POSITIVE: {0,4} {1,15} {2,6:f1} ...{3,6:f1} {4,7:f1} {5,7:f1}\t{6,10:f2}", count, ae.Name, ae.TimeStart, end, ae.Score, ae.Score2, ae.EventDurationSeconds);
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
                string hitFile = "";
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
                        count, ae.TimeStart, ae.TimeEnd, ae.Intensity, ae.Quality, ae.Name);
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

            if (((tp + fp) == 0))
            {
                precision = 0.0;
            }
            else
            {
                precision = tp / (double)(tp + fp);
            }

            if (((tp + fn) == 0))
            {
                recall = 0.0;
            }
            else
            {
                recall = tp / (double)(tp + fn);
            }

            accuracy = (precision + recall) / (float)2;

            resultsText = sb.ToString();
        } //end method

        /// <summary>
        /// Given two lists of AcousticEvents, one being labelled events and the other being predicted events,
        /// this method calculates the accuracy of the predictions in terms of tp, fp, fn etc.
        /// This method is similar to the one above except that it is assumed that all the events, both labelled and predicted
        /// come from the same recording.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="labels"></param>
        /// <param name="tp"></param>
        /// <param name="fp"></param>
        /// <param name="fn"></param>
        /// <param name="precision"></param>
        /// <param name="recall"></param>
        /// <param name="accuracy"></param>
        /// <param name="resultsText"></param>
        public static void CalculateAccuracyOnOneRecording(List<AcousticEvent> results, List<AcousticEvent> labels, out int tp, out int fp, out int fn,
                                         out double precision, out double recall, out double accuracy, out string resultsText)
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
                AcousticEvent overlapLabelEvent = ae.OverlapsEventInList(labelledEvents);//get overlapped labelled event
                if (overlapLabelEvent == null)
                {
                    fp++;
                    line = string.Format("False POSITIVE: {0,4} {1,15} {2,6:f1} ...{3,6:f1} {4,7:f1} {5,7:f1}\t{6,10:f2}", count, ae.Name, ae.TimeStart, end, ae.Score, ae.Score2, ae.EventDurationSeconds);
                }
                else
                {
                    tp++;
                    overlapLabelEvent.Tag = true; //tag because later need to determine fn
                    line = string.Format("True  POSITIVE: {0,4} {1,15} {2,6:f1} ...{3,6:f1} {4,7:f1} {5,7:f1}\t{6,10:f2}", count, ae.Name, ae.TimeStart, end, ae.Score, ae.Score2, ae.EventDurationSeconds);
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
                        count, ae.TimeStart, ae.TimeEnd, ae.Intensity, ae.Quality, ae.Name);
                    sb.Append(line + "\t" + ae.FileName + "\n");
                }
            }

            if (((tp + fp) == 0))
            {
                precision = 0.0;
            }
            else
            {
                precision = tp / (double)(tp + fp);
            }

            if (((tp + fn) == 0))
            {
                recall = 0.0;
            }
            else
            {
                recall = tp / (double)(tp + fn);
            }

            accuracy = (precision + recall) / (float)2;

            resultsText = sb.ToString();
        } //end method

//##############################################################################################################################################
//  THE NEXT THREE METHODS CONVERT BETWEEN SCORE ARRAYS AND ACOUSTIC EVENTS
//  THE NEXT TWO METHOD CONVERT AN ARRAY OF SCORE (USUALLY INTENSITY VALUES IN A SUB-BAND) TO ACOUSTIC EVENTS.
//  THE THIRD METHOD PRODUCES A SCORE ARRAY GIVEN A LIST OF EVENTS.

        /// <summary>
        /// Converts an array of sub-band intensity values to a list of AcousticEvents.
        /// This method does not constrain the maximum lewngth of detected events by setting maxDuration threshold to maximum value.
        /// </summary>
        /// <param name="values">the array of acoustic intensity values</param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="minHz">lower freq bound of the acoustic event</param>
        /// <param name="maxHz">upper freq bound of the acoustic event</param>
        /// <param name="framesPerSec">the time scale required by AcousticEvent class</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class</param>
        /// <param name="scoreThreshold"></param>
        /// <param name="minDuration">duration of event must exceed this to count as an event</param>
        /// <param name="maxDuration"></param>
        /// <param name="threshold">array value must exceed this dB threshold to count as an event</param>
        /// <param name="fileName">name of source file to be added to AcousticEvent class</param>
        /// <returns>a list of acoustic events</returns>
        //public static List<AcousticEvent> ConvertIntensityArray2Events(double[] values, int minHz, int maxHz,
        //                                                       double framesPerSec, double freqBinWidth,
        //                                                       double threshold, double minDuration, string fileName)
        //{
        //    double maxDuration = Double.MaxValue;
        //    return ConvertIntensityArray2Events(values, minHz, maxHz, framesPerSec, freqBinWidth, threshold, minDuration, maxDuration, fileName);
        //}

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

            for (int i = 0; i < count; i++)//pass over all frames
            {
                if ((isHit == false) && (values[i] > scoreThreshold))//start of an event
                {
                    isHit = true;
                    startTime = i * frameOffset;
                    startFrame = i;
                }
                else //check for the end of an event
                    if ((isHit == true) && (values[i] <= scoreThreshold))//this is end of an event, so initialise it
                    {
                        isHit = false;
                        double endTime = i * frameOffset;
                        double duration = endTime - startTime;
                        //if (duration < minDuration) continue; //skip events with duration shorter than threshold
                        if ((duration < minDuration) || (duration > maxDuration))
                        {
                            continue; //skip events with duration shorter than threshold
                        }

                        AcousticEvent ev = new AcousticEvent(segmentStartOffset, startTime, duration, minHz, maxHz);

                        ev.Name = "Acoustic Segment"; //default name
                        ev.SetTimeAndFreqScales(framesPerSec, freqBinWidth);

                        //obtain average intensity score.
                        double av = 0.0;
                        for (int n = startFrame; n <= i; n++)
                    {
                        av += values[n];
                    }

                        ev.Score = av / (double)(i - startFrame + 1);
                        events.Add(ev);
                    }
            }

            return events;
        }

        /// <summary>
        /// A general method to convert an array of score values to a list of AcousticEvents.
        /// The method uses the passed scoreThreshold in order to calculate a normalised score.
        /// Max possible score := threshold * 5.
        /// normalised score := score / maxPossibleScore.
        /// Some analysis techniques (e.g. OD) have their own methods for extracting events from score arrays.
        /// </summary>
        /// <param name="scores">the array of scores</param>
        /// <param name="minHz">lower freq bound of the acoustic event</param>
        /// <param name="maxHz">upper freq bound of the acoustic event</param>
        /// <param name="framesPerSec">the time scale required by AcousticEvent class</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class</param>
        /// <param name="scoreThreshold"></param>
        /// <param name="minDuration">duration of event must exceed this to count as an event</param>
        /// <param name="maxDuration">duration of event must be less than this to count as an event</param>
        /// <param name="segmentStartOffset"></param>
        /// <param name="threshold">score must exceed this threshold to count as an event</param>
        /// <param name="fileName">name of source file to be added to AcousticEvent class</param>
        /// <param name="callID">  name of the event to be added to AcousticEvent class</param>
        /// <returns>a list of acoustic events</returns>
        public static List<AcousticEvent> ConvertScoreArray2Events(
            double[] scores,
            int minHz,
            int maxHz,
            double framesPerSec,
            double freqBinWidth,
            double scoreThreshold,
            double minDuration,
            double maxDuration,
            TimeSpan segmentStartOffset)
        {
            int count = scores.Length;
            var events = new List<AcousticEvent>();
            double maxPossibleScore = 5 * scoreThreshold; // used to calcualte a normalised score bewteen 0 - 1.0
            bool isHit = false;
            double frameOffset = 1 / framesPerSec; // frame offset in fractions of second
            double startTime = 0.0;
            int startFrame = 0;

            for (int i = 0; i < count; i++) // pass over all frames
            {
                if ((isHit == false) && (scores[i] >= scoreThreshold)) //start of an event
                {
                    isHit = true;
                    startTime = i * frameOffset;
                    startFrame = i;
                }
                else // check for the end of an event
                if ((isHit == true) && (scores[i] <= scoreThreshold)) // this is end of an event, so initialise it
                {
                    isHit = false;
                    double endTime = i * frameOffset;
                    double duration = endTime - startTime;
                    // if (duration < minDuration) continue; //skip events with duration shorter than threshold
                    if ((duration < minDuration) || (duration > maxDuration))
                    {
                        continue; //skip events with duration shorter than threshold
                    }

                    // obtain an average score for the duration of the potential event.
                    double av = 0.0;
                    for (int n = startFrame; n <= i; n++)
                    {
                        av += scores[n];
                    }

                    av /= (double)(i - startFrame + 1);
                    if (av < scoreThreshold)
                    {
                        continue; //skip events whose score is < the threshold
                    }

                    AcousticEvent ev = new AcousticEvent(segmentStartOffset, startTime, duration, minHz, maxHz);

                    ev.SetTimeAndFreqScales(framesPerSec, freqBinWidth);
                    ev.Score = av;
                    ev.ScoreNormalised = ev.Score / maxPossibleScore; // normalised to the user supplied threshold
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
            } //end of pass over all frames

            return events;
        } //end method ConvertScoreArray2Events()

        /// <summary>
        /// Extracts an array of scores from a list of events.
        /// The events are required to have the passed name.
        /// The events are assumed to contain sufficient info about frame rate in order to populate the array.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="arraySize"></param>
        /// <param name="nameOfTargetEvent"></param>
        /// <returns></returns>
        public static double[] ExtractScoreArrayFromEvents(List<AcousticEvent> events, int arraySize, string nameOfTargetEvent)
        {
            double[] scores = new double[arraySize];
            if ((events == null) || (events.Count == 0))
            {
                return scores;
            }

            double windowOffset = events[0].FrameOffset;
            double frameRate = 1 / windowOffset; //frames per second

            int count = events.Count;
            foreach( AcousticEvent ae in events)
            {
                if (!ae.Name.Equals(nameOfTargetEvent))
                {
                    continue; //skip irrelevant events
                }

                int startFrame = (int)(ae.TimeStart * frameRate);
                int endFrame = (int)((ae.TimeStart + ae.EventDurationSeconds) * frameRate);

                for (int s = startFrame; s <= endFrame; s++)
                { scores[s] = ae.Score_MaxInEvent; }
            }

            return scores;
        } //end method

        //##############################################################################################################################################

        /// <summary>
        /// This method is used to do unit test on lists of events.
        /// First developed for frog recognizers - October 2016.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="opDir"></param>
        /// <param name="testName"></param>
        /// <param name="events"></param>
        public static void TestToCompareEvents(string fileName, DirectoryInfo opDir, string testName, List<AcousticEvent> events)
        {
            var testDir = new DirectoryInfo(opDir + $"\\UnitTest_{testName}");
            var benchmarkDir = new DirectoryInfo(testDir + "\\ExpectedOutput");
            if (!benchmarkDir.Exists)
            {
                benchmarkDir.Create();
            }

            var benchmarkFilePath = Path.Combine(benchmarkDir.FullName, fileName + ".TestEvents.csv");
            var eventsFilePath = Path.Combine(testDir.FullName,      fileName + ".Events.csv");
            var eventsFile = new FileInfo(eventsFilePath);
            Csv.WriteToCsv<EventBase>(eventsFile, events);

            LoggedConsole.WriteLine($"# EVENTS TEST: Camparing List of {testName} events with those in benchmark file:");
            var benchmarkFile = new FileInfo(benchmarkFilePath);
            if (!benchmarkFile.Exists)
            {
                LoggedConsole.WriteWarnLine("   A file of test/benchmark events does not exist.  Writing output as future events-test file");
                Csv.WriteToCsv<EventBase>(benchmarkFile, events);
            }
            else // compare the test events with benchmark
            {
                TestTools.FileEqualityTest("Compare acoustic events.", eventsFile, benchmarkFile);
            }
        }

        public static List<List<AcousticEvent>> ClusterEvents(AcousticEvent[] events)
        {
            LoggedConsole.WriteLine("# CLUSTERING EVENTS");
            var clusters = new List<List<AcousticEvent>>();

            var firstCluster = new List<AcousticEvent> {events[0]};
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
                    var newCluster = new List<AcousticEvent> {events[e]};
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
        } // AssignClusterIds

    }
}
