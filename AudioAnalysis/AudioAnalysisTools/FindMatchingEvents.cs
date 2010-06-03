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


        public static System.Tuple<BaseSonogram, List<AcousticEvent>, double[]> ExecuteFELT(double[,] target, double dynamicRange, AudioRecording recording,
                   bool doSegmentation, int minHz, int maxHz, double frameOverlap, double smoothWindow, double eventThreshold, double minDuration)
        {
            //i: CHECK RECORDING
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            //ii: MAKE SONOGRAM
            Log.WriteLine("Start sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.WindowOverlap = frameOverlap;
            sonoConfig.SourceFName = recording.FileName;
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();
            Log.WriteLine("Signal: Duration={0}, Sample Rate={1}", sonogram.Duration, sr);
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap);
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);

            //iii: SUBTRACT MODAL NOISE
            double[] modalNoise = SNR.CalculateModalNoise(sonogram.Data); //calculate modal noise profile
            modalNoise = DataTools.filterMovingAverage(modalNoise, 7);    //smooth the noise profile
            sonogram.Data = SNR.SubtractModalNoise(sonogram.Data, modalNoise);

            //iv: DO SEGMENTATION
            double thresholdSD = 0.1;              //Set threshold to 1/5th of a standard deviation of the background noise.
            double maxDuration = Double.MaxValue;  //Do not constrain maximum length of events.
            var tuple = AcousticEvent.GetSegmentationEvents((SpectralSonogram)sonogram, doSegmentation, minHz, maxHz, smoothWindow, thresholdSD, minDuration, maxDuration);
            var segmentEvents = tuple.Item1;
            var intensity = tuple.Item5;

            //iv DETECT EVENTS LIKE TARGET
            var tuple2 = FindMatchingEvents.Execute(target, dynamicRange, (SpectralSonogram)sonogram, segmentEvents, minHz, maxHz, eventThreshold, minDuration);
            var scores = tuple2.Item1;
            double Q, SD;
            scores = SNR.NoiseSubtractMode(scores, out Q, out SD);
            Log.WriteLine("Match Scores: baseline score and SD: Q={0:f4}, SD={1:f4}", Q, SD);
            double matchThreshold = 2 * SD;
            List<AcousticEvent> matchEvents = AcousticEvent.ConvertIntensityArray2Events(scores, minHz, maxHz, sonogram.FramesPerSecond,
                                                   sonogram.FBinWidth, matchThreshold, minDuration, maxDuration, recording.FileName);
            return System.Tuple.Create(sonogram, matchEvents, scores);
        }//end ExecuteFELT



        public static System.Tuple<double[]> Execute(double[,] target, double dynamicRange, SpectralSonogram sonogram, 
                                    List<AcousticEvent> segments, int minHz, int maxHz, double eventThreshold, double minDuration)
        {
            Log.WriteLine("SEARCHING FOR EVENTS LIKE TARGET.");
            //adjust target's dynamic range to that set by user 
            target = SNR.SetDynamicRange(target, 0.0, dynamicRange); //set event's dynamic range
            double[] v1 = DataTools.Matrix2Array(target);
            v1 = DataTools.normalise2UnitLength(v1);
            //string imagePath2 =  @"C:\SensorNetworks\Output\FELT_Gecko\target.png";
            //var image = BaseSonogram.Data2ImageData(target);
            //ImageTools.DrawMatrix(image, 1, 1, imagePath2);

            if (segments == null) return null;
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);
            int targetLength = target.GetLength(0);

            double[] scores = new double[sonogram.FrameCount];
            foreach (AcousticEvent av in segments)
            {
                Log.WriteLine("SEARCHING SEGMENT.");
                int startRow = (int)Math.Round(av.StartTime * sonogram.FramesPerSecond);
                int endRow   = (int)Math.Round(av.EndTime   * sonogram.FramesPerSecond);
                if (endRow >= sonogram.FrameCount) endRow = sonogram.FrameCount - 1;
                endRow   -= targetLength;
                if (endRow <= startRow) endRow = startRow +1;  //want minimum of one row

                for (int r = startRow; r < endRow; r++)
                {
                    double[,] matrix = DataTools.Submatrix(sonogram.Data, r, minBin, r + targetLength - 1, maxBin);
                    matrix = SNR.SetDynamicRange(matrix, 0.0, dynamicRange); //set event's dynamic range

                    //string imagePath2 = @"C:\SensorNetworks\Output\FELT_Gecko\compare.png";
                    //var image = BaseSonogram.Data2ImageData(matrix);
                    //ImageTools.DrawMatrix(image, 1, 1, imagePath2);

                    double[] v2 = DataTools.Matrix2Array(matrix);
                    v2 = DataTools.normalise2UnitLength(v2);
                    double crossCor = DataTools.DotProduct(v1, v2);
                    //scores[r] = crossCor;

                    for (int i = 0; i < targetLength; i++) if (scores[r + i] < crossCor) scores[r + i] = crossCor;
                    //Log.WriteLine("row={0}\t{1:f10}", r, crossCor);
                } //end of rows in segment
            } //foreach (AcousticEvent av in segments)

            var tuple = System.Tuple.Create(scores);
            return tuple;
        }//Execute



    } //end class FindEvents
}
