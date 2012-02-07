using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;


namespace AudioAnalysisTools
{
    public static class OscillationAnalysis
    {


        /// <summary>
        /// FINDS OSCILLATIONS IN A SONOGRAM
        /// But first it segments the sonogram based on acoustic energy in freq band of interest.
        /// </summary>
        /// <param name="sonogram">sonogram derived from the recording</param>
        /// <param name="minHz">min bound freq band to search</param>
        /// <param name="maxHz">max bound freq band to search</param>
        /// <param name="dctDuration">duration of DCT in seconds</param>
        /// <param name="dctThreshold">minimum amplitude of DCT </param>
        /// <param name="minOscilFreq">ignore oscillation frequencies below this threshold</param>
        /// <param name="maxOscilFreq">ignore oscillation frequencies greater than this </param>
        /// <param name="scoreThreshold">used for FP/FN</param>
        /// <param name="minDuration">ignore hits whose duration is shorter than this</param>
        /// <param name="maxDuration">ignore hits whose duration is longer than this</param>
        /// <param name="scores">return an array of scores over the entire recording</param>
        /// <param name="events">return a list of acoustic events</param>
        /// <param name="hits">a matrix that show where there is an oscillation of sufficient amplitude in the correct range.
        ///                    Values in the matrix are the oscillation rate. i.e. if OR = 2.0 = 2 oscillations per second. </param>
        public static void Execute(SpectralSonogram sonogram, bool doSegmentation, int minHz, int maxHz,
                                   double dctDuration, double dctThreshold, bool normaliseDCT, int minOscilFreq, int maxOscilFreq, 
                                   double scoreThreshold, double minDuration, double maxDuration,
                                   out double[] scores, out List<AcousticEvent> events, out Double[,] hits, out double[] intensity,
                                   out TimeSpan totalTime)
        {
            DateTime startTime1 = DateTime.Now;

            //EXTRACT SEGMENTATIOn EVENTS
            //DO SEGMENTATION
            double smoothWindow = 1 / (double)minOscilFreq; //window = max oscillation period
            Log.WriteLine(" Segmentation smoothing window = {0:f2} seconds", smoothWindow);
            double thresholdSD = 0.1;       //Set threshold to 1/5th of a standard deviation of the background noise.
            maxDuration = Double.MaxValue;  //Do not constrain maximum length of events.

            var tuple = AcousticEvent.GetSegmentationEvents(sonogram, doSegmentation, minHz, maxHz, smoothWindow, thresholdSD, minDuration, maxDuration);
            var segmentEvents = tuple.Item1;
            intensity = tuple.Item5;
            Log.WriteLine("Number of segments={0}", segmentEvents.Count);
            TimeSpan span1 = DateTime.Now.Subtract(startTime1); 
            Log.WriteLine(" SEGMENTATION COMP TIME = " + span1.TotalMilliseconds.ToString() + "ms");            
            DateTime startTime2 = DateTime.Now; 

            //DETECT OSCILLATIONS
            hits = DetectOscillationsInSonogram(sonogram, minHz, maxHz, dctDuration, dctThreshold, normaliseDCT, minOscilFreq, maxOscilFreq, segmentEvents);
            hits = RemoveIsolatedOscillations(hits);

            //EXTRACT SCORES AND ACOUSTIC EVENTS
            scores = GetOscillationScores(hits, minHz, maxHz, sonogram.FBinWidth);
            double[] oscFreq = GetOscillationFrequency(hits, minHz, maxHz, sonogram.FBinWidth);
            events = ConvertODScores2Events(scores, oscFreq, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth, sonogram.Configuration.FreqBinCount, scoreThreshold,
                                            minDuration, maxDuration, sonogram.Configuration.SourceFName);

            //events = segmentEvents;  //#################################### to see segment events in output image.
            DateTime endTime2 = DateTime.Now;
            TimeSpan span2 = endTime2.Subtract(startTime2);
            Log.WriteLine(" TOTAL COMP TIME = " + span2.ToString()+"s");
            totalTime = endTime2.Subtract(startTime1);
        }//end method



        /// <summary>
        /// FINDS OSCILLATIONS IN A SONOGRAM
        /// SAME METHOD AS ABOVE BUT .....
        /// 1) WITHOUT CALCULATING THE COMPUTATION TIME
        /// 2) WITHOUT DOING SEGMENTATION
        /// </summary>
        /// <param name="sonogram">sonogram derived from the recording</param>
        /// <param name="minHz">min bound freq band to search</param>
        /// <param name="maxHz">max bound freq band to search</param>
        /// <param name="dctDuration">duration of DCT in seconds</param>
        /// <param name="dctThreshold">minimum amplitude of DCT </param>
        /// <param name="minOscilFreq">ignore oscillation frequencies below this threshold</param>
        /// <param name="maxOscilFreq">ignore oscillation frequencies greater than this </param>
        /// <param name="scoreThreshold">used for FP/FN</param>
        /// <param name="minDuration">ignore hits whose duration is shorter than this</param>
        /// <param name="maxDuration">ignore hits whose duration is longer than this</param>
        /// <param name="scores">return an array of scores over the entire recording</param>
        /// <param name="events">return a list of acoustic events</param>
        /// <param name="hits">a matrix to be superimposed over the final sonogram which shows where DCY exceed threshold</param>
        public static void Execute(SpectralSonogram sonogram, int minHz, int maxHz,
                                   double dctDuration, double dctThreshold, bool normaliseDCT, double minOscilFreq, double maxOscilFreq,
                                   double scoreThreshold, double minDuration, double maxDuration,
                                   out double[] scores, out List<AcousticEvent> events, out Double[,] hits, out double[] oscFreq)
        {
            //convert the entire recording to an acoustic event - this is the legacy of previous experimentation!!!!!!!!!
            List<AcousticEvent> segmentEvents = new List<AcousticEvent>();
            var ae = new AcousticEvent(0.0, sonogram.Duration.TotalSeconds, minHz, maxHz);
            ae.SetTimeAndFreqScales(sonogram.FramesPerSecond, sonogram.FBinWidth);
            segmentEvents.Add(ae);

            //DETECT OSCILLATIONS
            hits = DetectOscillationsInSonogram(sonogram, minHz, maxHz, dctDuration, dctThreshold, normaliseDCT, minOscilFreq, maxOscilFreq, segmentEvents);
            hits = RemoveIsolatedOscillations(hits);

            //EXTRACT SCORES AND ACOUSTIC EVENTS
            scores = GetOscillationScores(hits, minHz, maxHz, sonogram.FBinWidth);
            scores = DataTools.filterMovingAverage(scores, 3);
            oscFreq = GetOscillationFrequency(hits, minHz, maxHz, sonogram.FBinWidth);
            events = ConvertODScores2Events(scores, oscFreq, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth, sonogram.Configuration.FreqBinCount, scoreThreshold,
                                            minDuration, maxDuration, sonogram.Configuration.SourceFName);
        }//end method




        /// <summary>
        /// Detects oscillations in a given freq bin.
        /// there are several important parameters for tuning.
        /// a) dctDuration: Good values are 0.25 to 0.50 sec. Do not want too long because DCT requires stationarity.
        ///     Do not want too short because too small a range of oscillations
        /// b) dctThreshold: minimum acceptable value of a DCT coefficient if hit is to be accepted.
        ///     The algorithm is sensitive to this value. A lower value results in more oscillation hits being returned.
        /// c) Min and Max Oscillaitons: Sets lower & upper bound for oscillations of interest.
        ///     Array has same length as the length of the DCT. Low freq oscillations occur more often by chance. Want to exclude them.
        /// </summary>
        /// <param name="sonogram"></param>
        /// <param name="minHz">min freq bin of search band</param>
        /// <param name="maxHz">max freq bin of search band</param>
        /// <param name="dctDuration"></param>
        /// <param name="dctThreshold"></param>
        /// <param name="minOscilFreq"></param>
        /// <param name="maxOscilFreq"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        public static Double[,] DetectOscillationsInSonogram(SpectralSonogram sonogram, int minHz, int maxHz, double dctDuration, double dctThreshold,
                                                    bool normaliseDCT, double minOscilFreq, double maxOscilFreq, List<AcousticEvent> events)
        {
            if (events == null) return null;
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);

            int dctLength = (int)Math.Round(sonogram.FramesPerSecond * dctDuration);
            int minIndex = (int)(minOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi
            int maxIndex = (int)(maxOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi
            if (maxIndex > dctLength) maxIndex = dctLength; //safety check in case of future changes to code.

            int rows = sonogram.Data.GetLength(0);
            int cols = sonogram.Data.GetLength(1);
            Double[,] hits = new Double[rows, cols];

            double[,] cosines = Speech.Cosines(dctLength, dctLength); //set up the cosine coefficients
            //following two lines write matrix of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Sonograms\cosines.txt";
            //FileTools.WriteMatrix2File_Formatted(cosines, fPath, "F3");

            //following two lines write bmp image of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Output\cosines.bmp";
            //ImageTools.DrawMatrix(cosines, fPath);

            foreach (AcousticEvent av in events)
            {
                int startRow = (int)Math.Round(av.StartTime * sonogram.FramesPerSecond);
                int endRow   = (int)Math.Round(av.EndTime * sonogram.FramesPerSecond);
                if (endRow >= sonogram.FrameCount) endRow = sonogram.FrameCount - 1;
                endRow   -= dctLength;
                if (endRow <= startRow) endRow = startRow +1;  //want minimum of one row

                for (int c = minBin; c <= maxBin; c++)//traverse columns
                {
                    for (int r = startRow; r < endRow; r++)
                    {
                        var array = new double[dctLength];
                        //accumulate J columns of values
                        int N = 5; //average five rows
                        for (int i = 0; i < dctLength; i++)
                        { for (int j = 0; j < N; j++) array[i] += sonogram.Data[r + i, c + j]; }
                        for (int i = 0; i < dctLength; i++) array[i] /= N;

                        array = DataTools.SubtractMean(array);
                        //     DataTools.writeBarGraph(array);

                        double[] dct = Speech.DCT(array, cosines);
                        for (int i = 0; i < dctLength; i++) dct[i] = Math.Abs(dct[i]);//convert to absolute values
                        for (int i = 0; i < 5; i++) dct[i] = 0.0;   //remove low freq oscillations from consideration
                        if(normaliseDCT) dct = DataTools.normalise2UnitLength(dct);
                        int indexOfMaxValue = DataTools.GetMaxIndex(dct);
                        double oscilFreq = indexOfMaxValue / dctDuration * 0.5; //Times 0.5 because index = Pi and not 2Pi
                        //      DataTools.writeBarGraph(dct);
                        //Console.WriteLine("oscilFreq = " + oscilFreq);

                        //mark DCT location with oscillation freq, only if oscillation freq is in correct range and amplitude
                        if ((indexOfMaxValue >= minIndex) && (indexOfMaxValue <= maxIndex) && (dct[indexOfMaxValue] > dctThreshold))
                        {
                            for (int i = 0; i < dctLength; i++) hits[r + i, c]   = oscilFreq;
                            for (int i = 0; i < dctLength; i++) hits[r + i, c+1] = oscilFreq; //write alternate column - MUST DO THIS BECAUSE doing alternate columns
                        }
                        r += 6; //skip rows
                    }
                    c++; //do alternate columns
                }
            } //foreach (AcousticEvent av in events)
            return hits;
        }


        /// <summary>
        /// Calls the above method but converts integer oscillations rate to doubles
        /// </summary>
        /// <returns></returns>
        public static Double[,] DetectOscillationsInSonogram(SpectralSonogram sonogram, int minHz, int maxHz, double dctDuration, double dctThreshold,
                                                           bool normaliseDCT, int minOscilFreq, int maxOscilFreq, List<AcousticEvent> events)
        {
            Double[,] hits = DetectOscillationsInSonogram(sonogram, minHz, maxHz, dctDuration, dctThreshold, normaliseDCT, (double)minOscilFreq, (double)maxOscilFreq, events);
            return hits;
        }


        public static double[] DetectOscillationsInScoreArray(double[] scoreArray, double dctDuration, double timeScale, double dctThreshold,
                                                    bool normaliseDCT, int minOscilFreq, int maxOscilFreq)
        {
            int dctLength = (int)Math.Round(timeScale * dctDuration);
            int minIndex = (int)(minOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi
            int maxIndex = (int)(maxOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi
            if (maxIndex > dctLength) maxIndex = dctLength; //safety check in case of future changes to code.

            int length = scoreArray.Length;
            double[] hits = new Double[length];

            double[,] cosines = Speech.Cosines(dctLength, dctLength); //set up the cosine coefficients
            //following two lines write matrix of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Sonograms\cosines.txt";
            //FileTools.WriteMatrix2File_Formatted(cosines, fPath, "F3");

            //following two lines write bmp image of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Output\cosines.bmp";
            //ImageTools.DrawMatrix(cosines, fPath);

            for (int r = 0; r < length - dctLength; r++)
            {
                var array = new double[dctLength];
                //transfer values
                for (int i = 0; i < dctLength; i++) array[i] = scoreArray[r+i];

                array = DataTools.SubtractMean(array);
                //     DataTools.writeBarGraph(array);

                double[] dct = Speech.DCT(array, cosines);
                for (int i = 0; i < dctLength; i++) dct[i] = Math.Abs(dct[i]);//convert to absolute values
                for (int i = 0; i < 5; i++) dct[i] = 0.0;   //remove low freq oscillations from consideration
                if (normaliseDCT) dct = DataTools.normalise2UnitLength(dct);
                int indexOfMaxValue = DataTools.GetMaxIndex(dct);
                double oscilFreq = indexOfMaxValue / dctDuration * 0.5; //Times 0.5 because index = Pi and not 2Pi
                //      DataTools.writeBarGraph(dct);
                //Console.WriteLine("oscilFreq = " + oscilFreq);

                //mark DCT location with oscillation freq, only if oscillation freq is in correct range and amplitude
                if ((indexOfMaxValue >= minIndex) && (indexOfMaxValue <= maxIndex) && (dct[indexOfMaxValue] > dctThreshold))
                {
                    hits[r]     = dct[indexOfMaxValue];
                    hits[r + 1] = dct[indexOfMaxValue]; // because skipping rows.
                    //for (int i = 0; i < dctLength; i++) if (hits[r + i] < dct[indexOfMaxValue]) hits[r + i] = dct[indexOfMaxValue];
                }
                r += 1; //skip rows
            }
            return hits;
        }



        /// <summary>
        ///  fills the gaps in an array of scores
        /// </summary>
        /// <param name="oscillations"></param>
        /// <param name="fillDuration">duration in seconds</param>
        /// <param name="timeScale">frames per Second</param>
        /// <returns></returns>
        public static double[] FillScoreArray(double[] oscillations, double fillDuration, double timeScale)
        {                   
            int L = oscillations.Length;
            var ret = new double[L];
            int fillLength = (int)Math.Round(timeScale * fillDuration);
            for (int i = 0; i < L - fillLength; i++)
            {
                    for (int j = 0; j < fillLength; j++) if (ret[i + j] < oscillations[i]) ret[i + j] = oscillations[i];
                i += 1; //skip rows
            }
            return ret;
        }

        /// <summary>
        /// Removes single lines of hits from Oscillation matrix.
        /// </summary>
        /// <param name="matrix">the Oscillation matrix</param>
        /// <returns></returns>
        public static Double[,] RemoveIsolatedOscillations(Double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            Double[,] cleanMatrix = matrix;

            for (int c = 3; c < cols - 3; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows; r++)
                {
                    if (cleanMatrix[r, c] == 0.0) continue;
                    if ((matrix[r, c - 2] == 0.0) && (matrix[r, c + 2] == 0))  //+2 because alternate columns
                        cleanMatrix[r, c] = 0.0;
                }
            }
            return cleanMatrix;
        } //end method RemoveIsolatedOscillations()


        /// <summary>
        /// Converts the hits in the "hit matrix" derived from the oscilation detector into a score for each frame.
        /// Score is normalised - the fraction of bins in the correct frequncy band that have an oscilation hit.
        /// </summary>
        /// <param name="hits">sonogram as matrix showing location of oscillation hits</param>
        /// <param name="minHz">lower freq bound of the acoustic event</param>
        /// <param name="maxHz">upper freq bound of the acoustic event</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class</param>
        /// <returns></returns>
        public static double[] GetOscillationScores(double[,] hits, int minHz, int maxHz, double freqBinWidth)
        {
            int rows = hits.GetLength(0);
            int cols = hits.GetLength(1);
            int minBin = (int)(minHz / freqBinWidth);
            int maxBin = (int)(maxHz / freqBinWidth);
            int binCount = maxBin - minBin + 1;
            //double hitRange = binCount * 0.5 * 0.9; //set hit range slightly < half the bins. Half because only scan every second bin.
            double hitRange = binCount * 0.9; //set hit range slightly less than bin count 
            var scores = new double[rows];
            for (int r = 0; r < rows; r++)
            {
                int score = 0;
                for (int c = minBin; c <= maxBin; c++)//traverse columns in required freq band
                {
                    if (hits[r, c] > 0) score++; //add up number of freq bins where have a hit
                }
                scores[r] = score / hitRange; //normalise the hit score in [0,1]
                if (scores[r] > 1.0) scores[r] = 1.0;
            }
            return scores;
        }//end method GetOscillationScores()


        /// <summary>
        /// for each frame, returns the average oscilation rate for those freq bins that register a hit. 
        /// </summary>
        /// <param name="hits"></param>
        /// <param name="minHz"></param>
        /// <param name="maxHz"></param>
        /// <param name="freqBinWidth"></param>
        /// <returns></returns>
        public static double[] GetOscillationFrequency(double[,] hits, int minHz, int maxHz, double freqBinWidth)
        {
            int rows = hits.GetLength(0);
            int cols = hits.GetLength(1);
            int minBin = (int)(minHz / freqBinWidth);
            int maxBin = (int)(maxHz / freqBinWidth);
            int binCount = maxBin - minBin + 1;

            var oscFreq = new double[rows]; //to store the oscillation frequency
            for (int r = 0; r < rows; r++)
            {
                double freq = 0;
                int count = 0;
                for (int c = minBin; c <= maxBin; c++)//traverse columns in required frequency band
                {
                    if (hits[r, c] > 0)
                    {
                        freq += hits[r, c];
                        count ++;
                    }
                }
                if (count == 0) oscFreq[r] = 0;
                else            oscFreq[r] = freq / (double)count; //return the average frequency
            }
            return oscFreq;
        }//end method GetOscillationFrequency()


        /// <summary>
        /// Converts the Oscillation Detector score array to a list of AcousticEvents. 
        /// NOTE: Method assumes passed score array was normalised.
        /// See the CousticEvent class for a generic version of this method.
        /// </summary>
        /// <param name="scores">the array of OD scores</param>
        /// <param name="oscFreq"></param>
        /// <param name="minHz">lower freq bound of the acoustic event</param>
        /// <param name="maxHz">upper freq bound of the acoustic event</param>
        /// <param name="framesPerSec">the time scale required by AcousticEvent class</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class</param>
        /// <param name="scoreThreshold">OD score must exceed this threshold to count as an event</param>
        /// <param name="minDuration">duration of event must exceed this to count as an event</param>
        /// <param name="maxDuration">duration of event must be less than this to count as an event</param>
        /// <param name="fileName">name of source file to be added to AcousticEvent class</param>
        /// <returns></returns>
        public static List<AcousticEvent> ConvertODScores2Events(double[] scores, double[] oscFreq, int minHz, int maxHz,
                                                               double framesPerSec, double freqBinWidth, int freqBinCount,
                                                               double scoreThreshold, double minDuration, double maxDuration, 
                                                               string fileName)
        {
            int count = scores.Length;

            var events = new List<AcousticEvent>();
            bool isHit = false;
            double frameOffset = 1 / framesPerSec; //frame offset in fractions of second
            double startTime = 0.0;
            int startFrame = 0;

            for (int i = 0; i < count; i++)//pass over all frames
            {
                if ((isHit == false) && (scores[i] >= scoreThreshold))//start of an event
                {
                    isHit = true;
                    startTime = i * frameOffset;
                    startFrame = i;
                }
                else  // check for the end of an event
                    if ((isHit == true) && (scores[i] < scoreThreshold))//this is end of an event, so initialise it
                    {
                        isHit = false;
                        double endTime = i * frameOffset;
                        double duration = endTime - startTime;
                        if ((duration < minDuration) || (duration > maxDuration)) continue; //skip events with duration shorter than threshold
                        AcousticEvent ev = new AcousticEvent(startTime, duration, minHz, maxHz);
                        ev.Name = "OscillationEvent"; //default name
                        ev.SetTimeAndFreqScales(framesPerSec, freqBinWidth);
                        ev.SourceFile = fileName;
                        ev.FreqBinCount = freqBinCount; //required for drawing event on the spectrogram

                        // obtain average score.
                        double av = 0.0;
                        for (int n = startFrame; n <= i; n++) av += scores[n];
                        av /= (double)(i - startFrame + 1);
                        ev.SetScores(av, 0.0, 1.0);  // assumes passed score array was normalised.

                        //calculate average oscillation freq and assign to ev.Score2 
                        ev.Score2Name = "OscRate"; //score2 name
                        av = 0.0;
                        for (int n = startFrame; n <= i; n++) av += oscFreq[n];
                        ev.Score2 = av / (double)(i - startFrame + 1);
                        events.Add(ev);
                    }
            } // end of pass over all frames
            return events;
        } // end method ConvertODScores2Events()


        /// <summary>
        /// returns the periodicity in an array of values
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double[] PeriodicityAnalysis(double[] array)
        {
            //DataTools.writeBarGraph(array);
            var A = DataTools.AutoCorrelation(array, 0, array.Length * 3 / 4); // do 2/3rds of maximum possible lag
            int dctLength = A.Length;

            A = DataTools.SubtractMean(A);
            //DataTools.writeBarGraph(A);

            double[,] cosines = Speech.Cosines(dctLength, dctLength); //set up the cosine coefficients
            double[] dct = Speech.DCT(A, cosines);
            
            for (int i = 0; i < dctLength; i++) dct[i] = Math.Abs(dct[i]);//convert to absolute values
            //DataTools.writeBarGraph(dct);
            for (int i = 0; i < 3; i++) dct[i] = 0.0;   //remove low freq oscillations from consideration
            dct = DataTools.normalise2UnitLength(dct);
            var peaks = DataTools.GetPeaks(dct);
            // remove non-peak values and low values
            for (int i = 0; i < dctLength; i++) if ((!peaks[i]) || (dct[i]< 0.2)) dct[i] = 0.0;
            DataTools.writeBarGraph(dct);

            //get periodicity of highest three values
            int peakCount = 3;
            var period = new double[peakCount];
            var maxIndex = new double[peakCount];
            for (int i = 0; i < peakCount; i++)
            {
                int indexOfMaxValue = DataTools.GetMaxIndex(dct);
                maxIndex[i] = indexOfMaxValue;
                //double oscilFreq = indexOfMaxValue / dctDuration * 0.5; //Times 0.5 because index = Pi and not 2Pi
                if ((double)indexOfMaxValue == 0) period[i] = 0.0;
                else                              period[i] = dctLength / (double)indexOfMaxValue * 2;
                dct[indexOfMaxValue] = 0.0; // remove value for next iteration
            }
            Console.WriteLine("Max indices = {0:f0},  {1:f0},  {2:f0}.", maxIndex[0] ,maxIndex[1] ,maxIndex[2] );
            return period;
        }


    }//end class
}
