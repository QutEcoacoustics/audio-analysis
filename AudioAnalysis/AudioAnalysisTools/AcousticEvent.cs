using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using System.Text.RegularExpressions;
using QutSensors.Shared;

namespace AudioAnalysisTools
{
    public class AcousticEvent
    {

        //DIMENSIONS OF THE EVENT
        public double StartTime { get; set; }       // (s),
        public double Duration;                     // in seconds
        public double EndTime { get; private set; } // (s),
        public int MinFreq;  // Hertz
        public int MaxFreq;  // Hertz
        public int FreqRange { get { return (MaxFreq - MinFreq + 1); } }
        public bool IsMelscale { get; set; }
        public Oblong oblong { get; private set; }

        public int FreqBinCount { get; private set; }     //required for conversions to & from MEL scale
        public double FreqBinWidth { get; private set; }    //required for freq-binID conversions
        public double FrameDuration { get; private set; }    //frame duration in seconds
        public double FrameOffset { get; private set; }    //time between frame starts in seconds
        public double FramesPerSecond { get; private set; }  //inverse of the frame offset


        //PROPERTIES OF THE EVENTS i.e. Name, SCORE ETC
        public string Name { get; set; }
        public string SourceFile { get; set; }
        public double Score { get; set; }
        public string ScoreComment { get; set; }
        public string Score2Name { get; set; }
        public double Score2 { get; set; } //second score if required e.g. for Birgits recognisers
        public double NormalisedScore { get; private set; } //score normalised in range [0,1].
        //double I1MeandB; //mean intensity of pixels in the event prior to noise subtraction 
        //double I1Var;  //,
        //double I2MeandB; //mean intensity of pixels in the event after Wiener filter, prior to noise subtraction 
        //double I2Var;  //,
        double I3Mean;   //mean intensity of pixels in the event AFTER noise reduciton 
        double I3Var;    //variance of intensity of pixels in the event.

        public int Intensity { get; set; } //subjective assesment of event intenisty
        public int Quality { get; set; }   //subjective assessment of event quality
        public bool Tag { get; set; } //use this if want to filter or tag some members of a list for some purpose

        /// <summary>
        /// <para>Populate this with any information that should be stored for verification or
        /// checks required for research papers.</para>
        /// <para>Do not include Name, NormalisedScore, StartTime, EndTime, MinFreq or MaxFreq, as these are stored by default.</para>
        /// </summary>
        public List<ResultProperty> ResultPropertyList { get; set; }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public AcousticEvent(double startTime, double duration, double minFreq, double maxFreq)
        {
            this.StartTime = startTime;
            this.Duration = duration;
            this.EndTime = startTime + duration;
            this.MinFreq = (int)minFreq;
            this.MaxFreq = (int)maxFreq;
            this.IsMelscale = false;
            oblong = null;// have no info to convert time/Hz values to coordinates
        }


        /// <summary>
        /// This constructor currently works ONLY for linear Herz scale events
        /// </summary>
        /// <param name="o"></param>
        /// <param name="binWidth"></param>
        public AcousticEvent(Oblong o, double frameOffset, double binWidth)
        {
            this.oblong = o;
            this.FreqBinWidth = binWidth;
            this.FrameOffset = frameOffset;
            this.IsMelscale = false;

            double startTime; double duration;
            RowIDs2Time(o.r1, o.r2, frameOffset, out startTime, out duration);
            this.StartTime = startTime;
            this.Duration = duration;
            this.EndTime = startTime + duration;
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

            if (this.oblong == null) this.oblong = ConvertEvent2Oblong();

        }

        public void SetTimeAndFreqScales(double framesPerSec, double freqBinWidth)
        {
            //this.FrameDuration = frameDuration;     //frame duration in seconds
            this.FramesPerSecond = framesPerSec;      //inverse of the frame offset
            this.FrameOffset = 1 / framesPerSec;      //frame offset in seconds

            //this.FreqBinCount = binCount;           //required for conversions to & from MEL scale
            this.FreqBinWidth = freqBinWidth;         //required for freq-binID conversions

            if (this.oblong == null) this.oblong = ConvertEvent2Oblong();
        }


        /// <summary>
        /// calculates the matrix/image indices of the acoustic event, when given the time/freq scales.
        /// This method called only by previous method:- SetTimeAndFreqScales(int samplingRate, int windowSize, int windowOffset)
        /// </summary>
        /// <returns></returns>
        public Oblong ConvertEvent2Oblong()
        {
            //translate time/freq dimensions to coordinates in a matrix.
            //columns of matrix are the freq bins. Origin is top left - as per matrix in the sonogram class.
            //Translate time dimension = frames = matrix rows.
            int topRow; int bottomRow;
            Time2RowIDs(this.StartTime, this.Duration, this.FrameOffset, out topRow, out bottomRow);

            //Translate freq dimension = freq bins = matrix columns.
            int leftCol; int rightCol;
            Freq2BinIDs(this.IsMelscale, this.MinFreq, this.MaxFreq, this.FreqBinCount, this.FreqBinWidth, out leftCol, out rightCol);

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
            this.NormalisedScore = (score - min) / (max - min);
            if (this.NormalisedScore > 1.0) this.NormalisedScore = 1.0;
            if (this.NormalisedScore < 0.0) this.NormalisedScore = 0.0;
        }

        public string WriteProperties()
        {
            return " min-max=" + MinFreq + "-" + MaxFreq + ",  " + oblong.c1 + "-" + oblong.c2;
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
                if ((this.SourceFile.Equals(ae.SourceFile)) && (this.Overlaps(ae))) return ae;
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
            if ((this.StartTime < ae.EndTime) && (this.EndTime > ae.StartTime))
                return true;
            if ((ae.StartTime < this.EndTime) && (ae.EndTime > this.StartTime))
                return true;
            return false;
        }

        //#################################################################################################################
        //METHODS TO CONVERT BETWEEN FREQ BIN AND HERZ OR MELS 

        /// <summary>
        /// converts frequency bounds of an event to left and right columns of object in sonogram matrix
        /// </summary>
        /// <param name="minF"></param>
        /// <param name="maxF"></param>
        /// <param name="leftCol"></param>
        /// <param name="rightCol"></param>
        public static void Freq2BinIDs(bool doMelscale, int minFreq, int maxFreq, int binCount, double binWidth,
                                                                                              out int leftCol, out int rightCol)
        {
            if (doMelscale)
                Freq2MelsBinIDs(minFreq, maxFreq, binWidth, binCount, out leftCol, out rightCol);
            else
                Freq2HerzBinIDs(minFreq, maxFreq, binWidth, out leftCol, out rightCol);
        }
        public static void Freq2HerzBinIDs(int minFreq, int maxFreq, double binWidth, out int leftCol, out int rightCol)
        {
            leftCol = (int)Math.Round(minFreq / binWidth);
            rightCol = (int)Math.Round(maxFreq / binWidth);
        }
        public static void Freq2MelsBinIDs(int minFreq, int maxFreq, double binWidth, int binCount, out int leftCol, out int rightCol)
        {
            double nyquistFrequency = binCount * binWidth;
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
                                         ae.Name, ae.StartTime, ae.EndTime, ae.MinFreq, ae.MaxFreq, ae.Score, ae.Score2, ae.SourceFile);
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
                                         ae.Name, ae.StartTime, ae.EndTime, ae.MinFreq, ae.MaxFreq, ae.Score, ae.Score2, ae.SourceFile);
                    sb.AppendLine(line);
                }
            }
            return sb;
        }


        /// <summary>
        /// Segments or not depending value of boolean doSegmentation
        /// </summary>
        /// <param name="sonogram"></param>
        /// <param name="doSegmentation"></param>
        /// <param name="minHz"></param>
        /// <param name="maxHz"></param>
        /// <param name="smoothWindow"></param>
        /// <param name="thresholdSD"></param>
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
            double[] intensity = tuple.Item1;
            double Q = tuple.Item2;
            double oneSD = tuple.Item3;
            double dBThreshold = thresholdSD * oneSD;
            var segmentEvents = AcousticEvent.ConvertIntensityArray2Events(intensity, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth,           
                                                           dBThreshold, minDuration, maxDuration, sonogram.Configuration.SourceFName);
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
                if (ae.SourceFile.Equals(fileName)) events.Add(ae);
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
            //Console.WriteLine(line);
            sb.Append(line + "\n");
            line = "  #   #  \ttag \tstart  ...   end  intensity quality  file";
            //Console.WriteLine(line);
            sb.Append(line + "\n");
            int count = 0;
            for (int i = 1; i < lines.Count; i++) //skip the header line in labels data
            {
                string[] words = Regex.Split(lines[i], @"\t");
                if ((words.Length < 8) || (words[4].Equals(null)) || (words[4].Equals("")))
                    continue; //ignore entries that do not have full data
                string file = words[0];
                if ((match != null) && (!file.StartsWith(match))) continue;  //ignore events not from the required file

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
                //Console.WriteLine(line);
                sb.Append(line + "\n");

                var ae = new AcousticEvent(start, (end - start), minFreq, maxfreq);
                ae.Score = intensity;
                ae.Name = tag;
                ae.SourceFile = file;
                ae.Intensity = intensity;
                ae.Quality = quality;
                events.Add(ae);
            }
            labelsText = sb.ToString();
            return events;
        } //end method GetLabelsInFile(List<string> labels, string file)




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
            Console.WriteLine(header);
            string line = null;
            var sb = new StringBuilder(header + "\n");
            string previousSourceFile = "  ";

            foreach (AcousticEvent ae in results)
            {
                count++;
                double end = ae.StartTime + ae.Duration; //calculate end time of the result event
                var labelledEvents = AcousticEvent.GetEventsInFile(labels, ae.SourceFile); //get all & only those labelled events in same file as result ae
                resultsSourceFiles.Add(ae.SourceFile);   //keep list of source files that the detected events come from
                AcousticEvent overlapLabelEvent = ae.OverlapsEventInList(labelledEvents);//get overlapped labelled event
                if (overlapLabelEvent == null)
                {
                    fp++;
                    line = String.Format("False POSITIVE: {0,4} {1,15} {2,6:f1} ...{3,6:f1} {4,7:f1} {5,7:f1}\t{6,10:f2}", count, ae.Name, ae.StartTime, end, ae.Score, ae.Score2, ae.Duration);
                }
                else
                {
                    tp++;
                    overlapLabelEvent.Tag = true; //tag because later need to determine fn
                    line = String.Format("True  POSITIVE: {0,4} {1,15} {2,6:f1} ...{3,6:f1} {4,7:f1} {5,7:f1}\t{6,10:f2}", count, ae.Name, ae.StartTime, end, ae.Score, ae.Score2, ae.Duration);
                }
                if (previousSourceFile != ae.SourceFile)
                {
                    Console.WriteLine(line + "\t" + ae.SourceFile);
                    sb.Append(line + "\t" + ae.SourceFile + "\n");
                    previousSourceFile = ae.SourceFile;
                }
                else
                {
                    Console.WriteLine(line + "\t  ||   ||   ||   ||   ||   ||");
                    sb.Append(line + "\t  ||   ||   ||   ||   ||   ||\n");
                }


            }//end of looking for true and false positives



            //Now calculate the FALSE NEGATIVES. These are the labelled events not tagged in previous search.
            Console.WriteLine();
            sb.Append("\n");
            fn = 0;
            count = 0;
            previousSourceFile = " "; //this is just a device to achieve a formatting hwich is easier to interpret
            foreach (AcousticEvent ae in labels)
            {
                count++;
                string hitFile = "";
                //check if this FN event is in a file that score tp of fp hit. 
                if (resultsSourceFiles.Contains(ae.SourceFile))
                    hitFile = "**";
                if (ae.Tag == false)
                {
                    fn++;
                    line = String.Format("False NEGATIVE: {0,4} {5,15} {1,6:f1} ...{2,6:f1}    intensity={3}     quality={4}",
                                         count, ae.StartTime, ae.EndTime, ae.Intensity, ae.Quality, ae.Name);
                    if (previousSourceFile != ae.SourceFile)
                    {
                        Console.WriteLine(line + "\t" + ae.SourceFile + " " + hitFile);
                        sb.Append(line + "\t" + ae.SourceFile + " " + hitFile + "\n");
                        previousSourceFile = ae.SourceFile;
                    }
                    else
                    {
                        Console.WriteLine(line + "\t  ||   ||   ||   ||   ||   ||");
                        sb.Append(line + "\t  ||   ||   ||   ||   ||   ||\n");
                    }
                }
            }

            if (fn == 0) line = "NO FALSE NEGATIVES.";
            else
                line = "** This FN event occured in a recording which also scored a tp or fp hit.";
            Console.WriteLine(line);
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
            //Console.WriteLine(header);
            string line = null;
            var sb = new StringBuilder(header + "\n");

            foreach (AcousticEvent ae in results)
            {
                count++;
                double end = ae.StartTime + ae.Duration; //calculate end time of the result event
                var labelledEvents = AcousticEvent.GetEventsInFile(labels, ae.SourceFile); //get all & only those labelled events in same file as result ae
                resultsSourceFiles.Add(ae.SourceFile);   //keep list of source files that the detected events come from
                AcousticEvent overlapLabelEvent = ae.OverlapsEventInList(labelledEvents);//get overlapped labelled event
                if (overlapLabelEvent == null)
                {
                    fp++;
                    line = String.Format("False POSITIVE: {0,4} {1,15} {2,6:f1} ...{3,6:f1} {4,7:f1} {5,7:f1}\t{6,10:f2}", count, ae.Name, ae.StartTime, end, ae.Score, ae.Score2, ae.Duration);
                }
                else
                {
                    tp++;
                    overlapLabelEvent.Tag = true; //tag because later need to determine fn
                    line = String.Format("True  POSITIVE: {0,4} {1,15} {2,6:f1} ...{3,6:f1} {4,7:f1} {5,7:f1}\t{6,10:f2}", count, ae.Name, ae.StartTime, end, ae.Score, ae.Score2, ae.Duration);
                }
                sb.Append(line + "\t" + ae.SourceFile + "\n");

            }//end of looking for true and false positives



            //Now calculate the FALSE NEGATIVES. These are the labelled events not tagged in previous search.
            //Console.WriteLine();
            sb.Append("\n");
            count = 0;
            foreach (AcousticEvent ae in labels)
            {
                count++;
                if (ae.Tag == false)
                {
                    fn++;
                    line = String.Format("False NEGATIVE: {0,4} {5,15} {1,6:f1} ...{2,6:f1}    intensity={3}     quality={4}",
                                         count, ae.StartTime, ae.EndTime, ae.Intensity, ae.Quality, ae.Name);
                    sb.Append(line + "\t" + ae.SourceFile + "\n");
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
        
        /// <summary>
        /// Converts an array of sub-band acoustic intensity values to a list of AcousticEvents.
        /// USE THIS METHOD WHEN SEGMENTING A SIGNAL ON ACOUSTIC INTENSITY
        /// </summary>
        /// <param name="values">the array of acoustic intensity values</param>
        /// <param name="minHz">lower freq bound of the acoustic event</param>
        /// <param name="maxHz">upper freq bound of the acoustic event</param>
        /// <param name="framesPerSec">the time scale required by AcousticEvent class</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class</param>
        /// <param name="threshold">array value must exceed this dB threshold to count as an event</param>
        /// <param name="minDuration">duration of event must exceed this to count as an event</param>
        /// <param name="maxDuration">duration of event must be less than this to count as an event</param>
        /// <param name="fileName">name of source file to be added to AcousticEvent class</param>
        /// <returns>a list of acoustic events</returns>
        public static List<AcousticEvent> ConvertIntensityArray2Events(double[] values, int minHz, int maxHz,
                                                               double framesPerSec, double freqBinWidth,
                                                               double scoreThreshold, double minDuration, double maxDuration, string fileName)
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
                        ev.SourceFile = fileName;

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

            double windowOffset = events[0].FrameOffset;
            double frameRate = 1 / windowOffset; //frames per second

            // string[] files = new string[1];
            // files[0] = iniFile;
            // Configuration config = new Configuration(files);

            double[] scores = new double[arraySize];
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
                int startFrame = (int)(events[i].StartTime * frameRate);
                int endFrame = (int)((events[i].StartTime + events[i].Duration) * frameRate);
                double frameLength = events[i].Duration * frameRate;

                //avScore    += events[i].Score;
                //avDuration += events[i].Duration;
                //avFrames   += frameLength;

                for (int s = startFrame; s <= endFrame; s++) scores[s] = events[i].NormalisedScore;
            }
            return scores;
        } //end method

        //##############################################################################################################################################


    }
}
