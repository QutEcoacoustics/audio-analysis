using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.IO;
using TowseyLib;
using AudioAnalysisTools;

namespace AudioAnalysisTools
{
    public class FindMatchingEvents
    {

        /// <summary>
        /// Given the target event in form of a matrix, find other events in the passed recording that are like the target. 
        /// </summary>
        /// <param name="target">target as matrix of values</param>
        /// <param name="dynamicRange">in dB used to prepare the target. Use same to prepare recording.</param>
        /// <param name="recording"></param>
        /// <param name="doSegmentation"></param>
        /// <param name="minHz"></param>
        /// <param name="maxHz"></param>
        /// <param name="frameOverlap"></param>
        /// <param name="smoothWindow">only used for segmentation.</param>
        /// <param name="thresholdSD">threshold expressed as standard deviations of the background noise</param>
        /// <param name="minDuration"></param>
        /// <returns></returns>
        public static System.Tuple<SpectralSonogram, List<AcousticEvent>, double[], double> ExecuteFELT(double[,] target, double dynamicRange, AudioRecording recording,
                   bool doSegmentation, int minHz, int maxHz, double frameOverlap, double smoothWindow, double thresholdSD, double minDuration)
        {
            //i: CHECK RECORDING
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            //ii: MAKE SONOGRAM
            Log.WriteLine("Start sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowOverlap = frameOverlap;
            sonoConfig.DoMelScale = false;
            sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            sonoConfig.DynamicRange = dynamicRange;
            sonoConfig.mfccConfig.CcCount = 12;                 //Number of mfcc coefficients
            //sonoConfig.mfccConfig.DoMelScale = false;
            //sonoConfig.mfccConfig.IncludeDelta = true;
            //sonoConfig.mfccConfig.IncludeDoubleDelta = false;
            //sonoConfig.DeltaT = 2;

            AmplitudeSonogram basegram = new AmplitudeSonogram(sonoConfig, recording.GetWavReader());
            SpectralSonogram sonogram = new SpectralSonogram(basegram);  //spectrogram has dim[N,257]
            //BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();
            Log.WriteLine("Signal: Duration={0}, Sample Rate={1}", sonogram.Duration, sr);
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap);
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);

            //iii: SUBTRACT MODAL NOISE
            //double[] modalNoise = SNR.CalculateModalNoise(sonogram.Data); //calculate modal noise profile
            //modalNoise = DataTools.filterMovingAverage(modalNoise, 7);    //smooth the noise profile
            //sonogram.Data = SNR.RemoveModalNoise(sonogram.Data, modalNoise); //sets neg values to zero

            //iv: DO SEGMENTATION
            double maxDuration = Double.MaxValue;  //Do not constrain maximum length of events.
            var tuple = AcousticEvent.GetSegmentationEvents((SpectralSonogram)sonogram, doSegmentation, minHz, maxHz, smoothWindow, thresholdSD, minDuration, maxDuration);
            var segmentEvents = tuple.Item1;
            var intensity = tuple.Item5;
            
            //iv SCORE SONOGRAM FOR EVENTS LIKE TARGET
            var tuple2 = FindMatchingEvents.Execute_StewartGage(target, dynamicRange, (SpectralSonogram)sonogram, segmentEvents, minHz, maxHz, minDuration);
            //var tuple2 = FindMatchingEvents.Execute_SobelEdges(target, dynamicRange, (SpectralSonogram)sonogram, segmentEvents, minHz, maxHz, minDuration);
            //var tuple2 = FindMatchingEvents.Execute_MFCC_XCOR(target, dynamicRange, sonogram, segmentEvents, minHz, maxHz, minDuration);
            var scores = tuple2.Item1;

            double Q, SD;
            scores = SNR.NoiseSubtractMode(scores, out Q, out SD);
            Log.WriteLine("Match Scores: baseline score and SD: Q={0:f4}, SD={1:f4}", Q, SD);
            double matchThreshold = thresholdSD * SD;

            //v: EXTRACT EVENTS
            int targetLength = target.GetLength(0);
            var eventScores = scores;
            for (int i = 1; i < scores.Length - targetLength; i++)
            {
                if (scores[i] > matchThreshold)
                {
                    for (int j = 0; j < targetLength; j++) if (scores[i] > scores[i - 1])
                        eventScores[i + j] = scores[i];
                }
            }


            List<AcousticEvent> matchEvents = AcousticEvent.ConvertIntensityArray2Events(eventScores, minHz, maxHz, sonogram.FramesPerSecond,
                                                   sonogram.FBinWidth, matchThreshold, minDuration, maxDuration, recording.FileName);
            return System.Tuple.Create(sonogram, matchEvents, scores, matchThreshold);
        }//end ExecuteFELT



        public static System.Tuple<double[]> Execute_StewartGage(double[,] target, double dynamicRange, SpectralSonogram sonogram, 
                                    List<AcousticEvent> segments, int minHz, int maxHz, double minDuration)
        {
            Log.WriteLine("SEARCHING FOR EVENTS LIKE TARGET.");
            if (segments == null) return null;
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);
            int targetLength = target.GetLength(0);

            //adjust target's dynamic range to that set by user 
            //target = SNR.SetDynamicRange(target, 0.0, dynamicRange); //set event's dynamic range
            double[] v1 = DataTools.Matrix2Array(target);
            //v1 = DataTools.normalise2UnitLength(v1);
            //string imagePath2 = @"C:\SensorNetworks\Output\FELT_Currawong\target.png";
            //var image = BaseSonogram.Data2ImageData(target);
            //ImageTools.DrawMatrix(image, 1, 1, imagePath2);

            double[] scores = new double[sonogram.FrameCount];
            foreach (AcousticEvent av in segments)
            {
                Log.WriteLine("SEARCHING SEGMENT.");
                int startRow = (int)Math.Round(av.StartTime * sonogram.FramesPerSecond);
                int endRow   = (int)Math.Round(av.EndTime   * sonogram.FramesPerSecond);
                if (endRow >= sonogram.FrameCount) endRow = sonogram.FrameCount;
                int stopRow = endRow - targetLength;
                if (stopRow <= startRow) stopRow = startRow +1;  //want minimum of one row
                int offset = targetLength / 2;

                for (int r = startRow; r < stopRow; r++)
                {
                    double[,] matrix = DataTools.Submatrix(sonogram.Data, r, minBin, r + targetLength - 1, maxBin);
                    //matrix = SNR.SetDynamicRange(matrix, 0.0, dynamicRange); //set event's dynamic range

                    //string imagePath2 = @"C:\SensorNetworks\Output\FELT_CURLEW\compare.png";
                    //var image = BaseSonogram.Data2ImageData(matrix);
                    //ImageTools.DrawMatrix(image, 1, 1, imagePath2);

                    double[] v2 = DataTools.Matrix2Array(matrix);
                    //v2 = DataTools.normalise2UnitLength(v2);
                    double crossCor = DataTools.DotProduct(v1, v2);
                    //scores[r + offset] = crossCor / v1.Length; // use this score if not going to extend
                    scores[r] = crossCor / v1.Length;            // use this score if going to extend
                    //Log.WriteLine("row={0}\t{1:f10}", r, scores[r]); 
                } // end of rows in segment
                //for (int r = stopRow; r < endRow; r++) scores[r] = scores[stopRow-1]; //fill in end of segment
            } // foreach (AcousticEvent av in segments)

            var tuple = System.Tuple.Create(scores);
            return tuple;
        }//Execute

        public static System.Tuple<double[]> Execute_SobelEdges(double[,] target, double dynamicRange, SpectralSonogram sonogram,
                                    List<AcousticEvent> segments, int minHz, int maxHz, double minDuration)
        {
            Log.WriteLine("SEARCHING FOR EVENTS LIKE TARGET.");
            if (segments == null) return null;
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);
            int targetLength = target.GetLength(0);

            //adjust target's dynamic range to that set by user 
            target = SNR.SetDynamicRange(target, 3.0, dynamicRange); //set event's dynamic range
            double[,] edgeTarget = ImageTools.SobelEdgeDetection(target, 0.4);
            double[] v1 = DataTools.Matrix2Array(edgeTarget);
            v1 = DataTools.normalise2UnitLength(v1);

            //string imagePath2 =  @"C:\SensorNetworks\Output\FELT_Currawong\edgeTarget.png";
            //var image = BaseSonogram.Data2ImageData(edgeTarget);
            //ImageTools.DrawMatrix(image, 1, 1, imagePath2);

            double[] scores = new double[sonogram.FrameCount];
            foreach (AcousticEvent av in segments)
            {
                Log.WriteLine("SEARCHING SEGMENT.");
                int startRow = (int)Math.Round(av.StartTime * sonogram.FramesPerSecond);
                int endRow = (int)Math.Round(av.EndTime * sonogram.FramesPerSecond);
                if (endRow >= sonogram.FrameCount) endRow = sonogram.FrameCount;
                int stopRow = endRow - targetLength;
                if (stopRow <= startRow) stopRow = startRow + 1;  //want minimum of one row

                for (int r = startRow; r < stopRow; r++)
                {
                    double[,] matrix = DataTools.Submatrix(sonogram.Data, r, minBin, r + targetLength - 1, maxBin);
                    matrix = SNR.SetDynamicRange(matrix, 3.0, dynamicRange); //set event's dynamic range
                    double[,] edgeMatrix = ImageTools.SobelEdgeDetection(matrix, 0.4);

                    //string imagePath2 = @"C:\SensorNetworks\Output\FELT_Gecko\compare.png";
                    //var image = BaseSonogram.Data2ImageData(matrix);
                    //ImageTools.DrawMatrix(image, 1, 1, imagePath2);

                    double[] v2 = DataTools.Matrix2Array(edgeMatrix);
                    v2 = DataTools.normalise2UnitLength(v2);
                    double crossCor = DataTools.DotProduct(v1, v2);
                    scores[r] = crossCor;
                    //Log.WriteLine("row={0}\t{1:f10}", r, crossCor);
                } //end of rows in segment
                for (int r = stopRow; r < endRow; r++) scores[r] = scores[stopRow - 1]; //fill in end of segment
            } //foreach (AcousticEvent av in segments)

            var tuple = System.Tuple.Create(scores);
            return tuple;
        }//Execute



        public static System.Tuple<double[]> Execute_MFCC_XCOR(double[,] target, double dynamicRange, SpectralSonogram sonogram,
                                    List<AcousticEvent> segments, int minHz, int maxHz, double minDuration)
        {
            Log.WriteLine("SEARCHING FOR EVENTS LIKE TARGET.");
            if (segments == null) return null;
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);
            int targetLength = target.GetLength(0);

            //set up the matrix of cosine coefficients 
            int coeffCount = 12; //only use first 12 coefficients.
            int binCount = target.GetLength(1);  //number of filters in filter bank
            double[,] cosines = Speech.Cosines(binCount, coeffCount + 1); //set up the cosine coefficients

            //adjust target's dynamic range to that set by user 
            target = SNR.SetDynamicRange(target, 3.0, dynamicRange); //set event's dynamic range
            target = Speech.Cepstra(target, coeffCount, cosines);
            double[] v1 = DataTools.Matrix2Array(target);
            v1 = DataTools.normalise2UnitLength(v1);

            string imagePath2 =  @"C:\SensorNetworks\Output\FELT_Currawong\target.png";
            var image = BaseSonogram.Data2ImageData(target);
            ImageTools.DrawMatrix(image, 1, 1, imagePath2);


            double[] scores = new double[sonogram.FrameCount];
            foreach (AcousticEvent av in segments)
            {
                Log.WriteLine("SEARCHING SEGMENT.");
                int startRow = (int)Math.Round(av.StartTime * sonogram.FramesPerSecond);
                int endRow = (int)Math.Round(av.EndTime * sonogram.FramesPerSecond);
                if (endRow >= sonogram.FrameCount) endRow = sonogram.FrameCount - 1;
                endRow -= targetLength;
                if (endRow <= startRow) endRow = startRow + 1;  //want minimum of one row

                for (int r = startRow; r < endRow; r++)
                {
                    double[,] matrix = DataTools.Submatrix(sonogram.Data, r, minBin, r + targetLength - 1, maxBin);
                    matrix = SNR.SetDynamicRange(matrix, 3.0, dynamicRange); //set event's dynamic range

                    //string imagePath2 = @"C:\SensorNetworks\Output\FELT_Gecko\compare.png";
                    //var image = BaseSonogram.Data2ImageData(matrix);
                    //ImageTools.DrawMatrix(image, 1, 1, imagePath2);
                    matrix = Speech.Cepstra(matrix, coeffCount, cosines);

                    double[] v2 = DataTools.Matrix2Array(matrix);
                    v2 = DataTools.normalise2UnitLength(v2);
                    double crossCor = DataTools.DotProduct(v1, v2);
                    scores[r] = crossCor;
                } //end of rows in segment
            } //foreach (AcousticEvent av in segments)

            var tuple = System.Tuple.Create(scores);
            return tuple;
        }//Execute


    } //end class FindEvents
}
