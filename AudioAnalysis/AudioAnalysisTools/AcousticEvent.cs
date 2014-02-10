using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using System.Text.RegularExpressions;

namespace AudioAnalysisTools
{
    using Acoustics.Shared;

    public class AcousticEvent
    {
        public static Color DEFAULT_BORDER_COLOR = Color.Crimson;
        public static Color DEFAULT_SCORE_COLOR = Color.Black;

        //DIMENSIONS OF THE EVENT
        /// <summary>in seconds</summary>
        public double Duration;
        /// <summary>in seconds</summary>
        public double TimeStart { get; set; } //within current recording     
        /// <summary>in seconds</summary>
        public double TimeEnd { get; set; }   //within current recording
        /// <summary>units = Hertz</summary>
        public int MinFreq { get; set; }

        /// <summary>units = Hertz</summary>
        public int MaxFreq { get; set; }

        public int FreqRange { get { return (this.MaxFreq - this.MinFreq + 1); } }
        public bool IsMelscale { get; set; }
        public Oblong oblong { get; set; }

        /// <summary> required for conversions to & from MEL scale AND for drawing event on spectrum</summary>
        public int FreqBinCount { get; set; } 
        public double FreqBinWidth { get; private set; }    //required for freq-binID conversions
        /// <summary> Frame duration in seconds</summary>
        public double FrameDuration { get; private set; }
        /// <summary> Time between frame starts in seconds. Inverse of FramesPerSecond</summary>
        public double FrameOffset { get; set; }
        /// <summary> Number of frame starts per second. Inverse of the frame offset</summary>
        public double FramesPerSecond { get; set; }


        //PROPERTIES OF THE EVENTS i.e. Name, SCORE ETC
        public string Name { get; set; }
        public string Name2 { get; set; }

        public string SourceFileName { get; set; }
        public double SourceFileDuration { get; set; }
        /// <summary> Average score through the event.</summary>
        public double Score { get; set; }
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


        public double Periodicity  { get; set; } // for events which have an oscillating acoustic energy - used for frog calls
        public double DominantFreq { get; set; } // the dominant freq in the event - used for frog calls

        // double I1MeandB; //mean intensity of pixels in the event prior to noise subtraction 
        // double I1Var;    //,
        // double I2MeandB; // mean intensity of pixels in the event after Wiener filter, prior to noise subtraction 
        // double I2Var;    //,
        double I3Mean;      // mean intensity of pixels in the event AFTER noise reduciton - USED FOR CLUSTERING
        double I3Var;       // variance of intensity of pixels in the event.

        //SIX KIWI SCORES
        public double kiwi_durationScore, kiwi_hitScore, kiwi_snrScore, kiwi_sdPeakScore;
        public double kiwi_intensityScore, kiwi_gridScore, kiwi_chirpScore, kiwi_bandWidthScore, kiwi_deltaPeriodScore, kiwi_comboScore;



        /// <summary>Use this if want to filter or tag some members of a list for some purpose.</summary>
        public bool Tag { get; set; }
        /// <summary>Assigned value when reading in a list of user identified events. Indicates a user assigned assessment of event intensity</summary>
        public int Intensity { get; set; } 
        /// <summary>Assigned value when reading in a list of user identified events. Indicates a user assigned assessment of event quality</summary>
        public int Quality { get; set; }  

        /// <summary>
        /// <para>Populate this with any information that should be stored for verification or
        /// checks required for research papers.</para>
        /// <para>Do not include Name, NormalisedScore, StartTime, EndTime, MinFreq or MaxFreq, as these are stored by default.</para>
        /// </summary>
        public List<ResultProperty> ResultPropertyList { get; set; }

        public Color BorderColour { get; set; }
        public Color ScoreColour  { get; set; }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public AcousticEvent(double startTime, double duration, double minFreq, double maxFreq)
        {
            this.TimeStart = startTime;
            this.Duration = duration;
            this.TimeEnd = startTime + duration;
            this.MinFreq = (int)minFreq;
            this.MaxFreq = (int)maxFreq;
            this.IsMelscale = false;
            oblong = null;// have no info to convert time/Hz values to coordinates
        }

        /// <summary>
        /// This constructor currently works ONLY for linear Herz scale events
        /// </summary>
        /// <param name="o"></param>
        /// <param name="frameOffset">seconds between frame starts i.e. inverse of frames per second. Sets the time scale for an event</param>
        /// <param name="binWidth">sets the frequency scale for an event</param>
        public AcousticEvent(Oblong o, double frameOffset, double binWidth)
        {
            this.oblong = o;
            this.FreqBinWidth = binWidth;
            this.FrameOffset = frameOffset;
            this.IsMelscale = false;

            double startTime; double duration;
            RowIDs2Time(o.r1, o.r2, frameOffset, out startTime, out duration);
            this.TimeStart = startTime;
            this.Duration = duration;
            this.TimeEnd = startTime + duration;
            int minF; int maxF;
            HerzBinIDs2Freq(o.c1, o.c2, binWidth, out minF, out maxF);
            this.MinFreq = minF;
            this.MaxFreq = maxF;
        }

        public void DoMelScale(bool doMelscale, int freqBinCount)
        {
            this.IsMelscale = doMelscale;
            this.FreqBinCount = freqBinCount;
        }

        public void SetTimeAndFreqScales(int samplingRate, int windowSize, int windowOffset)
        {
            double frameDuration, frameOffset, framesPerSecond;
            CalculateTimeScale(samplingRate, windowSize, windowOffset,
                                         out frameDuration, out frameOffset, out framesPerSecond);
            this.FrameDuration = frameDuration;    //frame duration in seconds
            this.FrameOffset = frameOffset;      //frame offset in seconds
            this.FramesPerSecond = framesPerSecond;  //inverse of the frame offset

            int binCount;
            double binWidth;
            CalculateFreqScale(samplingRate, windowSize, out binCount, out binWidth);
            this.FreqBinCount = binCount; //required for conversions to & from MEL scale
            this.FreqBinWidth = binWidth; //required for freq-binID conversions

            if (this.oblong == null) this.oblong = AcousticEvent.ConvertEvent2Oblong(this);

        }

        public void SetTimeAndFreqScales(double framesPerSec, double freqBinWidth)
        {
            //this.FrameDuration = frameDuration;     //frame duration in seconds
            this.FramesPerSecond = framesPerSec;      //inverse of the frame offset
            this.FrameOffset = 1 / framesPerSec;      //frame offset in seconds

            //this.FreqBinCount = binCount;           //required for conversions to & from MEL scale
            this.FreqBinWidth = freqBinWidth;         //required for freq-binID conversions

            if (this.oblong == null) this.oblong = AcousticEvent.ConvertEvent2Oblong(this);
        }


        /// <summary>
        /// calculates the matrix/image indices of the acoustic event, when given the time/freq scales.
        /// This method called only by previous method:- SetTimeAndFreqScales(int samplingRate, int windowSize, int windowOffset)
        /// </summary>
        /// <returns></returns>
        public static Oblong ConvertEvent2Oblong(AcousticEvent ae)
        {
            //translate time/freq dimensions to coordinates in a matrix.
            //columns of matrix are the freq bins. Origin is top left - as per matrix in the sonogram class.
            //Translate time dimension = frames = matrix rows.
            int topRow; int bottomRow;
            Time2RowIDs(ae.TimeStart, ae.Duration, ae.FrameOffset, out topRow, out bottomRow);

            //Translate freq dimension = freq bins = matrix columns.
            int leftCol; int rightCol;
            Freq2BinIDs(ae.IsMelscale, ae.MinFreq, ae.MaxFreq, ae.FreqBinCount, ae.FreqBinWidth, out leftCol, out rightCol);

            return new Oblong(topRow, leftCol, bottomRow, rightCol);
        }


        /// <summary>
        /// Sets the passed score and also a value normalised between a min and a max.
        /// </summary>
        /// <param name="score"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void SetScores(double score, double min, double max)
        {
            this.Score = score;
            this.ScoreNormalised = (score - min) / (max - min);
            if (this.ScoreNormalised > 1.0) this.ScoreNormalised = 1.0;
            if (this.ScoreNormalised < 0.0) this.ScoreNormalised = 0.0;
        }

        public string WriteProperties()
        {
            return " min-max=" + this.MinFreq + "-" + this.MaxFreq + ",  " + oblong.c1 + "-" + oblong.c2;
        }




        public void DrawEvent(Graphics g, double framesPerSecond, double freqBinWidth, int sonogramHeight)
        {
            Pen p1 = new Pen(AcousticEvent.DEFAULT_BORDER_COLOR); // default colour
            Pen p2 = new Pen(AcousticEvent.DEFAULT_SCORE_COLOR);
            if (this.BorderColour != null) p1 = new Pen(this.BorderColour);

            //calculate top and bottom freq bins
            int minFreqBin = (int)Math.Round(this.MinFreq / freqBinWidth);
            int maxFreqBin = (int)Math.Round(this.MaxFreq / freqBinWidth);
            int height = maxFreqBin - minFreqBin + 1;
            int y = sonogramHeight - maxFreqBin - 1;

            //calculate start and end time frames
            int t1 = 0;
            int tWidth = 0;
            double duration = this.TimeEnd - this.TimeStart;
            if ((duration != 0.0) && (framesPerSecond != 0.0))
            {
                t1 = (int)Math.Round(this.TimeStart * framesPerSecond); //temporal start of event
                tWidth = (int)Math.Round(duration * framesPerSecond);
            }
            else if (this.oblong != null)
            {
                t1 = this.oblong.r1; //temporal start of event
                tWidth = this.oblong.r2 - t1 + 1;
            }

            // 14-Feb-12 - Anthony - changed default brush so border would actually render with color
            g.DrawRectangle(p1, t1, y, tWidth, height);

            //draw the score bar to indicate relative score
            int scoreHt = (int)Math.Round(height * this.ScoreNormalised);
            int y1 = y + height;
            int y2 = y1 - scoreHt;
            g.DrawLine(p2, t1 + 1, y1, t1 + 1, y2);
            g.DrawLine(p2, t1 + 2, y1, t1 + 2, y2);
            //g.DrawLine(p2, t1 + 3, y1, t1 + 3, y2);
            g.DrawString(this.Name, new Font("Tahoma", 8), Brushes.Black, new PointF(t1, y - 1));
        }








        /// <summary>
        /// Returns the first event in the passed list which overlaps with this one IN THE SAME RECORDING.
        /// If no event overlaps return null.
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public AcousticEvent OverlapsEventInList(List<AcousticEvent> events)
        {
            foreach (AcousticEvent ae in events)
            {
                if ((this.SourceFileName.Equals(ae.SourceFileName)) && (this.Overlaps(ae))) return ae;
            }
            return null;
        }

        /// <summary>
        /// Returns true/false if this event time-overlaps the passed event.
        /// Overlap in frequency dimension is ignored.
        /// The overlap determination is made on the start and end time points.
        /// There are two possible overlaps to be checked
        /// </summary>
        /// <param name="ae"></param>
        /// <returns></returns>
        public bool Overlaps(AcousticEvent ae)
        {
            if ((this.TimeStart < ae.TimeEnd) && (this.TimeEnd > ae.TimeStart))
                return true;
            if ((ae.TimeStart < this.TimeEnd) && (ae.TimeEnd > this.TimeStart))
                return true;
            return false;
        }

        /// <summary>
        /// Returns the fractional overlap of two events.
        /// Translate time/freq dimensions to coordinates in a matrix.
        /// Freq dimension = bins   = matrix columns. Origin is top left - as per matrix in the sonogram class.
        /// Time dimension = frames = matrix rows.
        /// </summary>
        /// <param name="event1">an acoustic event</param>
        /// <param name="event2">an acoustic event</param>
        /// <returns></returns>
        public static double EventFractionalOverlap(AcousticEvent event1, AcousticEvent event2)
        {
            //if (event1.EndTime < event2.StartTime) return 0.0;
            //if (event2.EndTime < event1.StartTime) return 0.0;
            //if (event1.MaxFreq < event2.MinFreq)   return 0.0;
            //if (event2.MaxFreq < event1.MinFreq)   return 0.0;
            //at this point the two events do overlap

            int timeOverlap = Oblong.RowOverlap(event1.oblong, event2.oblong);
            if (timeOverlap == 0) return 0.0;
            int hzOverlap   = Oblong.ColumnOverlap(event1.oblong, event2.oblong);
            if (hzOverlap   == 0) return 0.0;

            int overlapArea = timeOverlap * hzOverlap;
            double fractionalOverlap1 = overlapArea / (double)event1.oblong.Area();
            double fractionalOverlap2 = overlapArea / (double)event2.oblong.Area();

            if (fractionalOverlap1 > fractionalOverlap2) return fractionalOverlap1;
            else                                         return fractionalOverlap2;
        }

        //#################################################################################################################
        //METHODS TO CONVERT BETWEEN FREQ BIN AND HERZ OR MELS 

        /// <summary>
        /// converts frequency bounds of an event to left and right columns of object in sonogram matrix
        /// NOTE: binCount is required only if freq is in Mel scale
        /// </summary>
        /// <param name="doMelscale"></param>
        /// <param name="minFreq">lower freq bound</param>
        /// <param name="maxFreq">upper freq bound</param>
        /// <param name="Nyquist">Nyquist freq in Herz</param>
        /// <param name="binWidth">frequency scale</param>
        /// <param name="leftCol">return bin index for lower freq bound</param>
        /// <param name="rightCol">return bin index for upper freq bound</param>
        public static void Freq2BinIDs(bool doMelscale, int minFreq, int maxFreq, int Nyquist, double binWidth,
                                                                                              out int leftCol, out int rightCol)
        {
            if (doMelscale)
                Freq2MelsBinIDs(minFreq, maxFreq, binWidth, Nyquist, out leftCol, out rightCol);
            else
                Freq2HerzBinIDs(minFreq, maxFreq, binWidth, out leftCol, out rightCol);
        }
        public static void Freq2HerzBinIDs(int minFreq, int maxFreq, double binWidth, out int leftCol, out int rightCol)
        {
            leftCol = (int)Math.Round(minFreq / binWidth);
            rightCol = (int)Math.Round(maxFreq / binWidth);
        }
        public static void Freq2MelsBinIDs(int minFreq, int maxFreq, double binWidth, int nyquistFrequency, out int leftCol, out int rightCol)
        {
            int binCount = (int)(nyquistFrequency / binWidth) + 1;
            double maxMel = Speech.Mel(nyquistFrequency);
            int melRange = (int)(maxMel - 0 + 1);
            double binsPerMel = binCount / (double)melRange;
            leftCol = (int)Math.Round((double)Speech.Mel(minFreq) * binsPerMel);
            rightCol = (int)Math.Round((double)Speech.Mel(maxFreq) * binsPerMel);
        }

        /// <summary>
        /// converts left and right column IDs to min and max frequency bounds of an event
        /// WARNING!!! ONLY WORKS FOR LINEAR HERZ SCALE. NEED TO WRITE ANOTHER METHOD FOR MEL SCALE ############################
        /// </summary>
        /// <param name="leftCol"></param>
        /// <param name="rightCol"></param>
        /// <param name="minFreq"></param>
        /// <param name="maxFreq"></param>
        public static void HerzBinIDs2Freq(int leftCol, int rightCol, double binWidth, out int minFreq, out int maxFreq)
        {
            minFreq = (int)Math.Round(leftCol * binWidth);
            maxFreq = (int)Math.Round(rightCol * binWidth);
            //if (doMelscale) //convert min max Hz to mel scale
            //{
            //}
        }




        //#################################################################################################################
        //METHODS TO CONVERT BETWEEN TIME BIN AND SECONDS 
        public static void RowIDs2Time(int topRow, int bottomRow, double frameOffset, out double startTime, out double duration)
        {
            startTime = topRow * frameOffset;
            double end = (bottomRow + 1) * frameOffset;
            duration = end - startTime;
        }

        public static void Time2RowIDs(double startTime, double duration, double frameOffset, out int topRow, out int bottomRow)
        {
            topRow = (int)Math.Round(startTime / frameOffset);
            bottomRow = (int)Math.Round((startTime + duration) / frameOffset);
        }

        public void SetNetIntensityAfterNoiseReduction(double mean, double var)
        {
            this.I3Mean = mean; //
            this.I3Var = var;  //
        }

        /// <summary>
        /// returns the frame duration and offset duration in seconds
        /// </summary>
        /// <param name="samplingRate">signal samples per second</param>
        /// <param name="windowSize">number of signal samples in one window or frame.</param>
        /// <param name="windowOffset">number of signal samples between start of one frame and start of next frame.</param>
        /// <param name="frameDuration">units = seconds</param>
        /// <param name="frameOffset">units = seconds</param>
        /// <param name="framesPerSecond">number of frames in one second.</param>
        public static void CalculateTimeScale(int samplingRate, int windowSize, int windowOffset,
                                                        out double frameDuration, out double frameOffset, out double framesPerSecond)
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
                string line = String.Format("#{0}\t{1,8:f3}\t{2,6:f3}\t{3}\t{4}\t{5:f2}\t{6:f1}\t{7}",
                                            "     Event Name", "Start", "End", "MinF", "MaxF", "Score1", "Score2", "SourceFile");
                sb.AppendLine(line);
                line = String.Format("{0}\t{1,8:f3}\t{2,8:f3}\t{3}\t{4}\t{5:f2}\t{6:f1}\t{7}",
                                     "NoEvent", 0.000, 0.000, "N/A", "N/A", 0.000, 0.000, "N/A");
                sb.AppendLine(line);
            }
            else
            {
                AcousticEvent ae1 = eventList[0];
                string line = String.Format("#{0}\t{1,8:f3}\t{2,6:f3}\t{3}\t{4}\t{5:f2}\t{6:f1}\t{7}",
                                            "     Event Name", "Start", "End", "MinF", "MaxF", "Score", ae1.Score2Name, "SourceFile");
                sb.AppendLine(line);
                foreach (AcousticEvent ae in eventList)
                {
                    line = String.Format("{0}\t{1,8:f3}\t{2,8:f3}\t{3}\t{4}\t{5:f2}\t{6:f1}\t{7}",
                                         ae.Name, ae.TimeStart, ae.TimeEnd, ae.MinFreq, ae.MaxFreq, ae.Score, ae.Score2, ae.SourceFileName);
                    sb.AppendLine(line);
                }
            }
        }

        /// <summary>
        /// used to write lists of acousitc event data to an excell spread sheet.
        /// </summary>
        /// <param name="eventList"></param>
        /// <param name="str"></param>
        public static StringBuilder WriteEvents(List<AcousticEvent> eventList, string str)
        {
            StringBuilder sb = new StringBuilder();
            if (eventList.Count == 0)
            {
                string line = String.Format(str + "\t{0}\t{1,8:f3}\t{2,8:f3}\t{3}\t{4}\t{5:f2}\t{6:f1}\t{7}",
                                     "NoEvent", 0.000, 0.000, "N/A", "N/A", 0.000, 0.000, "N/A");
                sb.AppendLine(line);
            }
            else
            {
                foreach (AcousticEvent ae in eventList)
                {
                    string line = String.Format(str + "\t{0}\t{1,8:f3}\t{2,8:f3}\t{3}\t{4}\t{5:f2}\t{6:f1}\t{7}",
                                         ae.Name, ae.TimeStart, ae.TimeEnd, ae.MinFreq, ae.MaxFreq, ae.Score, ae.Score2, ae.SourceFileName);
                    sb.AppendLine(line);
                }
            }
            return sb;
        }


        /// <summary>
        /// Segments or not depending value of boolean doSegmentation
        /// </summary>
        /// <param name="sonogram"></param>
        /// <param name="doSegmentation">segment? yes/no</param>
        /// <param name="minHz">lower limit of bandwidth</param>
        /// <param name="maxHz">upper limit of bandwidth</param>
        /// <param name="smoothWindow">window for smoothing the acoustic intensity array</param>
        /// <param name="thresholdSD">segmentation threshold - standard deviations above 0 dB</param>
        /// <param name="minDuration">minimum duration of an event</param>
        /// <param name="maxDuration">maximum duration of an event</param>
        /// <returns></returns>
        public static System.Tuple<List<AcousticEvent>, double, double, double, double[]> GetSegmentationEvents(SpectralSonogram sonogram,
                            bool doSegmentation, int minHz, int maxHz, double smoothWindow, double thresholdSD, double minDuration, double maxDuration)
        {
            if (!doSegmentation)//by-pass segmentation and make entire recording just one event.
            {
                double oneSD = 0.0; 
                double dBThreshold = 0.0;
                double[] intensity = null;
                List<AcousticEvent> segmentEvents = new List<AcousticEvent>();
                var ae = new AcousticEvent(0.0, sonogram.Duration.TotalSeconds, minHz, maxHz);
                ae.SetTimeAndFreqScales(sonogram.FramesPerSecond, sonogram.FBinWidth);
                segmentEvents.Add(ae);
                return System.Tuple.Create(segmentEvents, 0.0, oneSD, dBThreshold, intensity);
            }

            var tuple = GetSegmentationEvents(sonogram, minHz, maxHz, smoothWindow, thresholdSD, minDuration, maxDuration);
            return tuple; 
        }

        public static System.Tuple<List<AcousticEvent>, double, double, double, double[]> GetSegmentationEvents(SpectralSonogram sonogram, 
                                    int minHz, int maxHz, double smoothWindow, double thresholdSD, double minDuration, double maxDuration)
        {
            int nyquist = sonogram.SampleRate / 2;
            var tuple = SNR.SubbandIntensity_NoiseReduced(sonogram.Data, minHz, maxHz, nyquist, smoothWindow, sonogram.FramesPerSecond);
            double[] intensity = tuple.Item1; //noise reduced intensity array
            double Q = tuple.Item2;      //baseline dB in the original scale
            double oneSD = tuple.Item3;  //1 SD in dB around the baseline 
            double dBThreshold = thresholdSD * oneSD;
            var segmentEvents = AcousticEvent.ConvertIntensityArray2Events(intensity, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth,           
                                                           dBThreshold, minDuration, maxDuration);
            foreach (AcousticEvent ev in segmentEvents)
            {
                ev.SourceFileName = sonogram.Configuration.SourceFName;
                //ev.Name = callName;
            }

            return System.Tuple.Create(segmentEvents, Q, oneSD, dBThreshold, intensity);
        }



        /// <summary>
        /// returns all the events in a list that occur in the recording with passed file name.
        /// </summary>
        /// <param name="eventList"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static List<AcousticEvent> GetEventsInFile(List<AcousticEvent> eventList, string fileName)
        {
            var events = new List<AcousticEvent>();
            foreach (AcousticEvent ae in eventList)
            {
                if (ae.SourceFileName.Equals(fileName)) events.Add(ae);
            }
            return events;
        } // end method GetEventsInFile(List<AcousticEvent> eventList, string fileName)


        /// <summary>
        /// <summary>
        /// Reads a text file containing a list of acoustic events (one per line) and returns list of events.
        /// The file must contain a header.
        /// The format is tab separated words as follows:
        /// words[0]=file name; words[1]=recording date; words[2]=time; words[3]=start; words[4]=end; 
        /// words[5]=tag; words[6]=quality; words[7]=intensity 
        /// 
        /// NOTE: if match argument = null, method will return all events.
        /// </summary>
        /// <param name="path">path of file containing the acoustic events</param>
        /// <param name="match">file/recording name to match</param>
        /// <param name="labelsText">info to return as text</param>
        /// <returns>a list of Acoustic events</returns>
        public static List<AcousticEvent> GetAcousticEventsFromLabelsFile(string path, string match, out string labelsText)
        {
            var sb = new StringBuilder();
            var events = new List<AcousticEvent>();
            List<string> lines = FileTools.ReadTextFile(path);
            int minFreq = 0; //dummy value - never to be used
            int maxfreq = 0; //dummy value - never to be used
            string line = "\nList of LABELLED events in file: " + Path.GetFileName(path);
            //LoggedConsole.WriteLine(line);
            sb.Append(line + "\n");
            line = "  #   #  \ttag \tstart  ...   end  intensity quality  file";
            //LoggedConsole.WriteLine(line);
            sb.Append(line + "\n");
            int count = 0;
            for (int i = 1; i < lines.Count; i++) //skip the header line in labels data
            {
                string[] words = Regex.Split(lines[i], @"\t");
                if ((words.Length < 8) || (words[4].Equals(null)) || (words[4].Equals("")))
                    continue; //ignore entries that do not have full data
                if (! match.Equals(words[5]))  continue;  //ignore events without required tag
                //if (!file.StartsWith(match))) continue;  //ignore events not from the required file

                string file = words[0];
                string date = words[1];
                string time = words[2];
                double start = Double.Parse(words[3]);
                double end = Double.Parse(words[4]);
                string tag = words[5];
                int quality = Int32.Parse(words[6]);
                int intensity = Int32.Parse(words[7]);
                count++;
                line = String.Format("{0,3} {1,3} {2,10}{3,6:f1} ...{4,6:f1}{5,10}{6,10}\t{7}",
                                        count, i, tag, start, end, intensity, quality, file);
                //LoggedConsole.WriteLine(line);
                sb.Append(line + "\n");

                var ae = new AcousticEvent(start, (end - start), minFreq, maxfreq);
                ae.Score = intensity;
                ae.Name = tag;
                ae.SourceFileName = file;
                ae.Intensity = intensity;
                ae.Quality = quality;
                events.Add(ae);
            }
            labelsText = sb.ToString();
            return events;
        } //end method GetLabelsInFile(List<string> labels, string file)


        public static List<AcousticEvent> GetTaggedEventsInFile(List<AcousticEvent> labeledEvents, string filename)
        {
            var events = new List<AcousticEvent>();
            foreach (AcousticEvent ae in events)
            {
                if(ae.SourceFileName.Equals(filename)) events.Add(ae);
            }
            return events;
        }


        /// <summary>
        /// merges two acoustic events in a list if they are separated by fewer than S seconds.
        /// Method assumes that the events in list are in temporal order.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="secondsGap"></param>
        public static void MergeAdjacentEvents(List<AcousticEvent> events, int secondsGap)
        {
            for (int e = events.Count-2; e >=0; e--)
            {
                if ((events[e+1].TimeStart - events[e].TimeEnd) <= secondsGap)
                {
                    events[e].TimeEnd = events[e + 1].TimeEnd;
                    events[e].Duration = events[e].TimeEnd - events[e].TimeStart;
                    events[e].oblong.r2 = events[e + 1].oblong.r2;
                    events.RemoveRange(e + 1, 1);
                } 
            }
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
            string header = String.Format("\nScore Category:    #{0,12}name{0,3}start{0,6}end{0,2}score1{0,2}score2{0,5}duration{0,6}source file", space);
            LoggedConsole.WriteLine(header);
            string line = null;
            var sb = new StringBuilder(header + "\n");
            string previousSourceFile = "  ";

            foreach (AcousticEvent ae in results)
            {
                count++;
                double end = ae.TimeStart + ae.Duration; //calculate end time of the result event
                var labelledEvents = AcousticEvent.GetEventsInFile(labels, ae.SourceFileName); //get all & only those labelled events in same file as result ae
                resultsSourceFiles.Add(ae.SourceFileName);   //keep list of source files that the detected events come from
                AcousticEvent overlapLabelEvent = ae.OverlapsEventInList(labelledEvents);//get overlapped labelled event
                if (overlapLabelEvent == null)
                {
                    fp++;
                    line = String.Format("False POSITIVE: {0,4} {1,15} {2,6:f1} ...{3,6:f1} {4,7:f1} {5,7:f1}\t{6,10:f2}", count, ae.Name, ae.TimeStart, end, ae.Score, ae.Score2, ae.Duration);
                }
                else
                {
                    tp++;
                    overlapLabelEvent.Tag = true; //tag because later need to determine fn
                    line = String.Format("True  POSITIVE: {0,4} {1,15} {2,6:f1} ...{3,6:f1} {4,7:f1} {5,7:f1}\t{6,10:f2}", count, ae.Name, ae.TimeStart, end, ae.Score, ae.Score2, ae.Duration);
                }
                if (previousSourceFile != ae.SourceFileName)
                {
                    LoggedConsole.WriteLine(line + "\t" + ae.SourceFileName);
                    sb.Append(line + "\t" + ae.SourceFileName + "\n");
                    previousSourceFile = ae.SourceFileName;
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
                if (resultsSourceFiles.Contains(ae.SourceFileName))
                    hitFile = "**";
                if (ae.Tag == false)
                {
                    fn++;
                    line = String.Format("False NEGATIVE: {0,4} {5,15} {1,6:f1} ...{2,6:f1}    intensity={3}     quality={4}",
                                         count, ae.TimeStart, ae.TimeEnd, ae.Intensity, ae.Quality, ae.Name);
                    if (previousSourceFile != ae.SourceFileName)
                    {
                        LoggedConsole.WriteLine(line + "\t" + ae.SourceFileName + " " + hitFile);
                        sb.Append(line + "\t" + ae.SourceFileName + " " + hitFile + "\n");
                        previousSourceFile = ae.SourceFileName;
                    }
                    else
                    {
                        LoggedConsole.WriteLine(line + "\t  ||   ||   ||   ||   ||   ||");
                        sb.Append(line + "\t  ||   ||   ||   ||   ||   ||\n");
                    }
                }
            }

            if (fn == 0) line = "NO FALSE NEGATIVES.";
            else
                line = "** This FN event occured in a recording which also scored a tp or fp hit.";
            LoggedConsole.WriteLine(line);
            sb.Append(line + "\n");

            if (((tp + fp) == 0)) precision = 0.0;
            else precision = tp / (double)(tp + fp);
            if (((tp + fn) == 0)) recall = 0.0;
            else recall = tp / (double)(tp + fn);
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
            string header = String.Format("PREDICTED EVENTS:  #{0,12}name{0,3}start{0,6}end{0,2}score1{0,2}score2{0,5}duration{0,6}source file", space);
            //LoggedConsole.WriteLine(header);
            string line = null;
            var sb = new StringBuilder(header + "\n");

            foreach (AcousticEvent ae in results)
            {
                count++;
                double end = ae.TimeStart + ae.Duration; //calculate end time of the result event
                var labelledEvents = AcousticEvent.GetEventsInFile(labels, ae.SourceFileName); //get all & only those labelled events in same file as result ae
                resultsSourceFiles.Add(ae.SourceFileName);   //keep list of source files that the detected events come from
                AcousticEvent overlapLabelEvent = ae.OverlapsEventInList(labelledEvents);//get overlapped labelled event
                if (overlapLabelEvent == null)
                {
                    fp++;
                    line = String.Format("False POSITIVE: {0,4} {1,15} {2,6:f1} ...{3,6:f1} {4,7:f1} {5,7:f1}\t{6,10:f2}", count, ae.Name, ae.TimeStart, end, ae.Score, ae.Score2, ae.Duration);
                }
                else
                {
                    tp++;
                    overlapLabelEvent.Tag = true; //tag because later need to determine fn
                    line = String.Format("True  POSITIVE: {0,4} {1,15} {2,6:f1} ...{3,6:f1} {4,7:f1} {5,7:f1}\t{6,10:f2}", count, ae.Name, ae.TimeStart, end, ae.Score, ae.Score2, ae.Duration);
                }
                sb.Append(line + "\t" + ae.SourceFileName + "\n");

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
                    line = String.Format("False NEGATIVE: {0,4} {5,15} {1,6:f1} ...{2,6:f1}    intensity={3}     quality={4}",
                                         count, ae.TimeStart, ae.TimeEnd, ae.Intensity, ae.Quality, ae.Name);
                    sb.Append(line + "\t" + ae.SourceFileName + "\n");
                }
            }

            if (((tp + fp) == 0)) precision = 0.0;
            else precision = tp / (double)(tp + fp);
            if (((tp + fn) == 0)) recall = 0.0;
            else recall = tp / (double)(tp + fn);
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
        /// <param name="minHz">lower freq bound of the acoustic event</param>
        /// <param name="maxHz">upper freq bound of the acoustic event</param>
        /// <param name="framesPerSec">the time scale required by AcousticEvent class</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class</param>
        /// <param name="threshold">array value must exceed this dB threshold to count as an event</param>
        /// <param name="minDuration">duration of event must exceed this to count as an event</param>
        /// <param name="fileName">name of source file to be added to AcousticEvent class</param>
        /// <returns>a list of acoustic events</returns>
        //public static List<AcousticEvent> ConvertIntensityArray2Events(double[] values, int minHz, int maxHz,
        //                                                       double framesPerSec, double freqBinWidth,
        //                                                       double threshold, double minDuration, string fileName)
        //{
        //    double maxDuration = Double.MaxValue; 
        //    return ConvertIntensityArray2Events(values, minHz, maxHz, framesPerSec, freqBinWidth, threshold, minDuration, maxDuration, fileName);
        //}
        


        public static List<AcousticEvent> ConvertIntensityArray2Events(double[] values, int minHz, int maxHz,
                                                               double framesPerSec, double freqBinWidth, double scoreThreshold, double minDuration, double maxDuration)
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
                else  //check for the end of an event
                    if ((isHit == true) && (values[i] <= scoreThreshold))//this is end of an event, so initialise it
                    {
                        isHit = false;
                        double endTime = i * frameOffset;
                        double duration = endTime - startTime;
                        //if (duration < minDuration) continue; //skip events with duration shorter than threshold
                        if ((duration < minDuration) || (duration > maxDuration)) continue; //skip events with duration shorter than threshold
                        AcousticEvent ev = new AcousticEvent(startTime, duration, minHz, maxHz);
                        ev.Name = "Acoustic Segment"; //default name
                        ev.SetTimeAndFreqScales(framesPerSec, freqBinWidth);

                        //obtain average intensity score.
                        double av = 0.0;
                        for (int n = startFrame; n <= i; n++) av += values[n];
                        ev.Score = av / (double)(i - startFrame + 1);
                        events.Add(ev);
                    }
            } //end of pass over all frames
            return events;
        }//end method ConvertScores2Events()


        /// <summary>
        /// A general method to convert an array of score values to a list of AcousticEvents.
        /// The method uses the passed scoreThreshold in order to calculate a normalised score.
        /// Max possible score := threshold * 5
        /// normalised score := score / maxPossibleScore.
        /// Some analysis techniques (e.g. OD) have their own methods for extracting events from score arrays.
        /// </summary>
        /// <param name="scores">the array of scores</param>
        /// <param name="minHz">lower freq bound of the acoustic event</param>
        /// <param name="maxHz">upper freq bound of the acoustic event</param>
        /// <param name="framesPerSec">the time scale required by AcousticEvent class</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class</param>
        /// <param name="threshold">score must exceed this threshold to count as an event</param>
        /// <param name="minDuration">duration of event must exceed this to count as an event</param>
        /// <param name="maxDuration">duration of event must be less than this to count as an event</param>
        /// <param name="fileName">name of source file to be added to AcousticEvent class</param>
        /// <param name="callID">  name of the event to be added to AcousticEvent class</param>
        /// <returns>a list of acoustic events</returns>
        public static List<AcousticEvent> ConvertScoreArray2Events(double[] scores, int minHz, int maxHz, double framesPerSec, double freqBinWidth,
                                                                   double scoreThreshold, double minDuration, double maxDuration)
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
                if ((isHit == false) && (scores[i] >= scoreThreshold))//start of an event
                {
                    isHit = true;
                    startTime = i * frameOffset;
                    startFrame = i;
                }
                else  // check for the end of an event
                    if ((isHit == true) && (scores[i] <= scoreThreshold)) // this is end of an event, so initialise it
                    {
                        isHit = false;
                        double endTime = i * frameOffset;
                        double duration = endTime - startTime;
                        // if (duration < minDuration) continue; //skip events with duration shorter than threshold
                        if ((duration < minDuration) || (duration > maxDuration)) continue; //skip events with duration shorter than threshold

                        // obtain an average score for the duration of the potential event.
                        double av = 0.0;
                        for (int n = startFrame; n <= i; n++) av += scores[n];
                        av /= (double)(i - startFrame + 1);
                        if (av < scoreThreshold) continue; //skip events whose score is < the threshold


                        AcousticEvent ev = new AcousticEvent(startTime, duration, minHz, maxHz);
                        ev.SetTimeAndFreqScales(framesPerSec, freqBinWidth);
                        ev.Score = av;
                        ev.ScoreNormalised = ev.Score / maxPossibleScore; // normalised to the user supplied threshold
                        if (ev.ScoreNormalised > 1.0) ev.ScoreNormalised = 1.0;
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
        }//end method ConvertScoreArray2Events()



        /// <summary>
        /// Extracts an array of scores from a list of events.
        /// The events are required to have the passed name.
        /// The events are assumed to contain sufficient info about frame rate in order to populate the array.
        /// This method is only called when visualising HTK scores
        /// </summary>
        /// <param name="events"></param>
        /// <param name="frameCount">the size of the array to return</param>
        /// <param name="windowOffset"></param>
        /// <param name="targetClass"></param>
        /// <param name="scoreThreshold"></param>
        /// <param name="qualityMean"></param>
        /// <param name="qualitySD"></param>
        /// <param name="qualityThreshold"></param>
        /// <returns></returns>
        public static double[] ExtractScoreArrayFromEvents(List<AcousticEvent> events, int arraySize, string targetName)
        //public static double[] ExtractScoreArray(List<AcousticEvent> events, string iniFile, int arraySize, string targetName)
        {
            double[] scores = new double[arraySize];
            if ((events == null) || (events.Count == 0)) return scores;
 
            double windowOffset = events[0].FrameOffset;
            double frameRate = 1 / windowOffset; //frames per second

            //for (int i = 0; i < arraySize; i++) scores[i] = Double.NaN; //init to NaNs.
            int count = events.Count;

            //double avScore = 0.0;
            //double avDuration = 0.0;
            //double avFrames = 0.0;
            for (int i = 0; i < count; i++)
            {
                if (!events[i].Name.Equals(targetName)) continue; //skip irrelevant events

                //           double scoreThreshold = config.GetDouble(vocalName + "HTK_THRESHOLD");
                //           double qualityMean = config.GetDouble(vocalName + "DURATION_MEAN");
                //           double qualitySD = config.GetDouble(vocalName + "DURATION_SD");
                //           double qualityThreshold = config.GetDouble("Key_SD_THRESHOLD");
                int startFrame = (int)(events[i].TimeStart * frameRate);
                int endFrame = (int)((events[i].TimeStart + events[i].Duration) * frameRate);
                double frameLength = events[i].Duration * frameRate;

                //avScore    += events[i].Score;
                //avDuration += events[i].Duration;
                //avFrames   += frameLength;

                for (int s = startFrame; s <= endFrame; s++) scores[s] = events[i].ScoreNormalised;
            }
            return scores;
        } //end method

        //##############################################################################################################################################


    }
}
