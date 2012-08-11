using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;


namespace AudioAnalysisTools
{

    /// <summary>
    /// NOTE: 21st June 2012.
    /// 
    /// This class contains methods to detect oscillations in a the sonogram of a signal. 
    /// The method Execute() returns all info about oscillaitons in the passed sonogram.
    /// This method should be called in preference to those in the class OscillationAnalysis.
    /// (The latter hsould be depracated.)
    /// </summary>
    public static class OscillationDetector
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sonogram">sonogram derived from the recording</param>
        /// <param name="minHz">min bound freq band to search</param>
        /// <param name="maxHz">max bound freq band to search</param>
        /// <param name="dctDuration">duration of DCT in seconds</param>
        /// <param name="minOscilFreq">ignore oscillation frequencies below this threshold</param>
        /// <param name="minAmplitude">ignore DCT amplitude values less than this minimum </param>
        /// <param name="opPath">set=null if do not want to save an image, which takes time</param>
        public static void Execute(SpectralSonogram sonogram, int minHz, int maxHz,
                                   double dctDuration, int minOscilFreq, int maxOscilFreq, double minAmplitude, double scoreThreshold,
                                   double minDuration, double maxDuration, out double[] scores, out List<AcousticEvent> events, out Double[,] hits)
        {

            //DETECT OSCILLATIONS
            hits = DetectOscillations(sonogram, minHz, maxHz, dctDuration, minOscilFreq, maxOscilFreq, minAmplitude);
            if (hits == null)
            {
                LoggedConsole.WriteLine("###### WARNING: DCT length too short to detect the maxOscilFreq");
                scores = null;
                events = null;
                return;
            }
            hits = RemoveIsolatedOscillations(hits);

            //EXTRACT SCORES AND ACOUSTIC EVENTS
            scores = GetODScores(hits, minHz, maxHz, sonogram.FBinWidth);
            scores = DataTools.filterMovingAverage(scores, 3);
            double[] oscFreq = GetODFrequency(hits, minHz, maxHz, sonogram.FBinWidth);
            events = ConvertODScores2Events(scores, oscFreq, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth, scoreThreshold,
                                          minDuration, sonogram.Configuration.SourceFName);
        }//end method


        /// <summary>
        /// Detects oscillations in a given freq bin.
        /// there are several important parameters for tuning.
        /// a) DCTLength: Good values are 0.25 to 0.50 sec. Do not want too long because DCT requires stationarity.
        ///     Do not want too short because too small a range of oscillations
        /// b) DCTindex: Sets lower bound for oscillations of interest. Index refers to array of coeff returned by DCT.
        ///     Array has same length as the length of the DCT. Low freq oscillations occur more often by chance. Want to exclude them.
        /// c) MinAmplitude: minimum acceptable value of a DCT coefficient if hit is to be accepted.
        ///     The algorithm is sensitive to this value. A lower value results in more oscillation hits being returned.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="minBin">min freq bin of search band</param>
        /// <param name="maxBin">max freq bin of search band</param>
        /// <param name="dctLength">number of values</param>
        /// <param name="DCTindex">Sets lower bound for oscillations of interest.</param>
        /// <param name="minAmplitude">threshold - do not accept a DCT value if its amplitude is less than this threshold</param>
        /// <returns></returns>
        public static Double[,] DetectOscillations(SpectralSonogram sonogram, int minHz, int maxHz,
                                                   double dctDuration, int minOscilFreq, int maxOscilFreq, double minAmplitude)
        {
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);

            int dctLength = (int)Math.Round(sonogram.FramesPerSecond * dctDuration);
            int minIndex = (int)(minOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi
            int maxIndex = (int)(maxOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi

            if (maxIndex > dctLength) return null;       //safety check

            int rows = sonogram.Data.GetLength(0);
            int cols = sonogram.Data.GetLength(1);
            Double[,] hits = new Double[rows, cols];
            Double[,] matrix = sonogram.Data;

            double[,] cosines = Speech.Cosines(dctLength, dctLength); //set up the cosine coefficients
            //following two lines write matrix of cos values for checking.
            //string txtPath = @"C:\SensorNetworks\Output\cosines.txt";
            //FileTools.WriteMatrix2File_Formatted(cosines, txtPath, "F3");

            //following two lines write bmp image of cos values for checking.
            //string bmpPath = @"C:\SensorNetworks\Output\cosines.png";
            //ImageTools.DrawMatrix(cosines, bmpPath, true);



            for (int c = minBin; c <= maxBin; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows - dctLength; r++)
                {
                    var array = new double[dctLength];
                    //accumulate J columns of values
                    for (int i = 0; i < dctLength; i++)
                        for (int j = 0; j < 5; j++) array[i] += matrix[r + i, c + j];

                    array = DataTools.SubtractMean(array);
                    //     DataTools.writeBarGraph(array);

                    double[] dct = Speech.DCT(array, cosines);
                    for (int i = 0; i < dctLength; i++) dct[i] = Math.Abs(dct[i]);//convert to absolute values
                    dct[0] = 0.0; dct[1] = 0.0; dct[2] = 0.0; dct[3] = 0.0; dct[4] = 0.0;//remove low freq oscillations from consideration
                    dct = DataTools.normalise2UnitLength(dct);
                    //dct = DataTools.normalise(dct); //another option to normalise
                    int indexOfMaxValue = DataTools.GetMaxIndex(dct);
                    double oscilFreq = indexOfMaxValue / dctDuration * 0.5; //Times 0.5 because index = Pi and not 2Pi

                    //DataTools.MinMax(dct, out min, out max);
                    //      DataTools.writeBarGraph(dct);

                    //mark DCT location with oscillation freq, only if oscillation freq is in correct range and amplitude
                    if ((indexOfMaxValue >= minIndex) && (indexOfMaxValue <= maxIndex) && (dct[indexOfMaxValue] > minAmplitude))
                    {
                        for (int i = 0; i < dctLength; i++) hits[r + i, c] = oscilFreq;
                    }
                    r += 5; //skip rows
                }
                c++; //do alternate columns
            }
            return hits;
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
        /// Converts the hits derived from the oscilation detector into a score for each frame.
        /// NOTE: The oscillation detector skips every second row, so score must be adjusted for this.
        /// </summary>
        /// <param name="hits">sonogram as matrix showing location of oscillation hits</param>
        /// <param name="minHz">lower freq bound of the acoustic event</param>
        /// <param name="maxHz">upper freq bound of the acoustic event</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class</param>
        /// <returns></returns>
        public static double[] GetODScores(double[,] hits, int minHz, int maxHz, double freqBinWidth)
        {
            int rows = hits.GetLength(0);
            int cols = hits.GetLength(1);
            int minBin = (int)(minHz / freqBinWidth);
            int maxBin = (int)(maxHz / freqBinWidth);
            int binCount = maxBin - minBin + 1;
            double hitRange = binCount * 0.5 * 0.8; //set hit range slightly < half the bins. Half because only scan every second bin.
            var scores = new double[rows];
            for (int r = 0; r < rows; r++)
            {
                int score = 0;
                for (int c = minBin; c <= maxBin; c++)//traverse columns in required band
                {
                    if (hits[r, c] > 0) score++;
                }
                scores[r] = score / hitRange; //normalise the hit score in [0,1]
                if (scores[r] > 1.0) scores[r] = 1.0;
            }
            return scores;
        }//end method GetODScores()


        public static double[] GetODFrequency(double[,] hits, int minHz, int maxHz, double freqBinWidth)
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
                for (int c = minBin; c <= maxBin; c++)//traverse columns in required band
                {
                    if (hits[r, c] > 0)
                    {
                        freq += hits[r, c];
                        count ++;
                    }
                }
                if (count == 0) oscFreq[r] = 0;
                else            oscFreq[r] = freq / (double)count; //return the average frequency
                //if (oscFreq[r] > 1.0) oscFreq[r] = 1.0;
            }
            return oscFreq;
        }//end method GetODFrequency()

        /// <summary>
        /// Converts the Oscillation Detector score array to a list of AcousticEvents. 
        /// </summary>
        /// <param name="scores">the array of OD scores</param>
        /// <param name="minHz">lower freq bound of the acoustic event</param>
        /// <param name="maxHz">upper freq bound of the acoustic event</param>
        /// <param name="framesPerSec">the time scale required by AcousticEvent class</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class</param>
        /// <param name="scoreThreshold">OD score must exceed this threshold to count as an event</param>
        /// <param name="durationThreshold">duration of event must exceed this duration to count as an event</param>
        /// <param name="fileName">name of source file to be added to AcousticEvent class</param>
        /// <returns></returns>
        public static List<AcousticEvent> ConvertODScores2Events(double[] scores, double[] oscFreq, int minHz, int maxHz, double framesPerSec, double freqBinWidth,
                                                               double maxThreshold, double durationThreshold, string fileName)
        {
            //double minThreshold = 0.1;
            //double scoreThreshold = minThreshold; //set this to the minimum threshold to start with
            double scoreThreshold = maxThreshold; //set this to the minimum threshold to start with
            int count = scores.Length;
            //int minBin = (int)(minHz / freqBinWidth);
            //int maxBin = (int)(maxHz / freqBinWidth);
            //int binCount = maxBin - minBin + 1;
            var events = new List<AcousticEvent>();
            bool isHit = false;
            double frameOffset = 1 / framesPerSec;
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
                else  //check for the end of an event
                    if ((isHit == true) && (scores[i] < scoreThreshold))//this is end of an event, so initialise it
                    {
                        isHit = false;
                        double endTime = i * frameOffset;
                        //double duration = endTime - startTime;
                        double duration = (i - startFrame + 1)  * frameOffset;
                        if (duration < durationThreshold) continue; //skip events with duration shorter than threshold
                        //LoggedConsole.WriteLine("startFrame={0}   startTime={1:f2}s    endFrame={2}   endTime={3:f2}s ", startFrame, startTime, i, endTime);
                        AcousticEvent ev = new AcousticEvent(startTime, duration, minHz, maxHz);
                        ev.Name = "OscillationEvent"; //default name
                        //ev.SetTimeAndFreqScales(framesPerSec, freqBinWidth);
                        ev.SourceFileName = fileName;
                        //obtain average score.
                        double av = 0.0;
                        for (int n = startFrame; n <= i; n++) av += scores[n];
                        ev.Score = av / (double)(i - startFrame + 1);
                        //obtain oscillation freq.
                        av = 0.0;
                        for (int n = startFrame; n <= i; n++) av += oscFreq[n];
                        ev.Score2 = av / (double)(i - startFrame + 1);
                        events.Add(ev);
                    }

                //adapt the threshold
                //if ((scores[i] >= maxThreshold) && (maxThreshold >= scoreThreshold)) scoreThreshold *= 1.01;
                //else
                //if ((scores[i] <= minThreshold) && (minThreshold <= scoreThreshold)) scoreThreshold *= 0.95;
                

            } //end of pass over all frames
            return events;
        }//end method ConvertODScores2Events()


        public static double CalculateRequiredFrameOverlap(int sr, int framewidth, /*double dctDuration, */ double maxOscilation)
        {
            double optimumFrameRate = 3 * maxOscilation; //so that max oscillation sits in 3/4 along the array of DCT coefficients
            //double frameOffset = sr / (double)optimumFrameRate;
            int frameOffset = (int)(sr / (double)optimumFrameRate);  //do this AND NOT LINE ABOVE OR ELSE GET CUMULATIVE ERRORS IN time scale

            double overlap = (framewidth - frameOffset) / (double)framewidth;
            return overlap;
        }

    }//end class 
} //AudioAnalysisTools
