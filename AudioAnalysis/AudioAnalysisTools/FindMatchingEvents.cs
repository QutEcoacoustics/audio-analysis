using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
//using System.IO;
using TowseyLib;
using AudioAnalysisTools;
using AudioAnalysisTools.Sonogram;


namespace AudioAnalysisTools
{
    public class FindMatchingEvents
    {



        public static double[,] ReadImage2BinaryMatrixDouble(string fileName)
        {
            Bitmap bitmap = ImageTools.ReadImage2Bitmap(fileName);
            int height = bitmap.Height;  //height
            int width  = bitmap.Width;    //width
            var matrix = new double[height, width];

            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                {
                    Color color = bitmap.GetPixel(c, r);
                    if ((color.R < 255) && (color.G < 255) && (color.B < 255)) matrix[r, c] = 1; // init an ON CELL = +1
                    else matrix[r, c] = -1; // init OFF CELL = -1
                }
            return matrix;
        }

        public static double[,] ReadImage2TrinaryMatrix(string fileName)
        {
            Bitmap bitmap = ImageTools.ReadImage2Bitmap(fileName);
            int height = bitmap.Height;  //height
            int width  = bitmap.Width;    //width
            var matrix = new double[height, width];

            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                {
                    Color color = bitmap.GetPixel(c, r);
                    if ((color.R < 255) && (color.G < 255) && (color.B < 255)) matrix[r, c] = 1;
                    else if ((color.G < 255) && (color.B < 255)) matrix[r, c] = 0;
                    else matrix[r, c] = -1;
                }
            return matrix;
        }


        public static char[,] ReadTextFile2CharMatrix(string fileName)
        {
            List<string> lines = FileTools.ReadTextFile(fileName);
            int rows = lines.Count;
            int cols = lines[0].Length;
            var matrix = new char[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    matrix[r, c] = (lines[r].ToCharArray())[c];
                }
            return matrix;
        }




        /// <summary>
        /// Use this method to find match in sonogram to a symbolic definition of a bird call.
        /// That is, the template should be matrix of binary or trinary values.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="dynamicRange"></param>
        /// <param name="sonogram"></param>
        /// <param name="segments"></param>
        /// <param name="minHz"></param>
        /// <param name="maxHz"></param>
        /// <param name="dBThreshold">Not used in calculation. Only used to speed up loop over the spectrogram.</param>
        /// <returns></returns>
        public static System.Tuple<double[]> Execute_Bi_or_TrinaryMatch(double[,] template, SpectralSonogram sonogram, 
                                                                         List<AcousticEvent> segments, int minHz, int maxHz, double dBThreshold)
        {
            Log.WriteLine("SEARCHING FOR EVENTS LIKE TARGET.");
            if (segments == null) return null;
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);
            int templateHeight = template.GetLength(0);
            int templateWidth  = template.GetLength(1);
            int cellCount      = templateHeight * templateWidth;
            //var image = BaseSonogram.Data2ImageData(target);
            //ImageTools.DrawMatrix(image, 1, 1, @"C:\SensorNetworks\Output\FELT_Currawong\target.png");

            // ######### Following line normalises template scores for comparison between templates.
            // ######### Ensures OP=0 for featureless sonogram #########
            // ######### template score = (average dB of on-template-cells - average dB of off-template-cells). 
            WriteTemplate2Console(template);
            var tuple1 = NormaliseBiTrinaryMatrix(template);
            template = tuple1.Item1;
            int positiveCount = tuple1.Item2;
            int negativeCount = tuple1.Item3;
            Log.WriteLine("TEMPLATE: Number of POS cells/total cells = {0}/{1}", positiveCount, cellCount);
            Log.WriteLine("TEMPLATE: Number of NEG cells/total cells = {0}/{1}", negativeCount, cellCount);

            double[] scores = new double[sonogram.FrameCount];

            
            foreach (AcousticEvent av in segments)
            {
                Log.WriteLine("SEARCHING SEGMENT.");
                int startRow = (int)Math.Floor(av.TimeStart * sonogram.FramesPerSecond);
                int endRow   = (int)Math.Floor(av.TimeEnd   * sonogram.FramesPerSecond);
                if (endRow >= sonogram.FrameCount) endRow = sonogram.FrameCount;
                int stopRow = endRow - templateWidth -1;
                if (stopRow <= startRow) stopRow = startRow +1;  //want minimum of one row

                for (int r = startRow; r < stopRow; r++)
                {
                    double max = -double.MaxValue;
                    //int maxOnCount= 0; //used to display % ON-count and maybe to modify the score.
                    int binBuffer = 10;
                    for (int bin = -binBuffer; bin < +binBuffer; bin++) 
                    {
                        int c = minBin + bin; 
                        if(c < 0) c = 0;
                        double crossCor = 0.0;
                        //int onCount = 0;

                        for (int j = 0; j < templateWidth; j++)
                        {
                            int c0 = c + templateHeight - 1;
                            for (int i = 0; i < templateHeight; i++)
                            {
                                crossCor += sonogram.Data[r + j, c0 - i] *   template[i, j];
                                //if ((sonogram.Data[r + j, c0 - i] > 0.0) && (template[i, j] > 0)) onCount++;
                            }
                        }
                        //var image = BaseSonogram.Data2ImageData(matrix);
                        //ImageTools.DrawMatrix(image, 1, 1, @"C:\SensorNetworks\Output\FELT_CURLEW\compare.png");

                        if (crossCor > max)
                        {
                            max = crossCor;
                            //maxOnCount = onCount;
                        }
                    } // end freq bins

                    //following line yields score = av of PosCells - av of NegCells.
                    scores[r] = max / (double)positiveCount;

                    // display percent onCount
                    //int pcOnCount = maxOnCount * 100 / positiveCount;
                    //if (r % 100 == 0) { LoggedConsole.WriteLine("{0} - {1:f3}", r, scores[r]); }
                    //if (scores[r] >= dBThreshold) { LoggedConsole.WriteLine("r={0} score={1}  %on={2}.", r, scores[r], pcOnCount); }
                    if (r % 100 == 0) { LoggedConsole.Write("."); }
                    if (scores[r] < dBThreshold) r += 3; //skip where score is low

                } // end of rows in segment
                LoggedConsole.WriteLine("\nFINISHED SEARCHING SEGMENT FOR ACOUSTIC EVENT."); 
            } // foreach (AcousticEvent av in segments)

            var tuple = System.Tuple.Create(scores);
            return tuple;
        }//Execute


        /// <summary>
        /// Use this method to find match in sonogram to a symbolic definition of a bird call.
        /// That is, the template should be matrix of binary or trinary values.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="dynamicRange"></param>
        /// <param name="sonogram"></param>
        /// <param name="segments"></param>
        /// <param name="minHz"></param>
        /// <param name="maxHz"></param>
        /// <param name="dBThreshold"></param>
        /// <returns></returns>
        public static System.Tuple<double[]> Execute_Spr_Match(char[,] template, SpectralSonogram sonogram,
                                                               List<AcousticEvent> segments, int minHz, int maxHz, double dBThreshold)
        {
            int lineLength = 10;
            //dBThreshold = 9;

            Log.WriteLine("SEARCHING FOR EVENTS LIKE TARGET (SPR).");
            if (segments == null) return null;
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);
            int templateFrames   = template.GetLength(0);  // time axis - SPR template is same orientation as the sonogram data matrix
            int templateFreqBins = template.GetLength(1);  // freq axis
            int cellCount = templateFrames * templateFreqBins;


            int positiveCount = SprTools.CountTemplateChars(template);
            int negativeCount = cellCount - positiveCount;
            Log.WriteLine("TEMPLATE: Number of POS cells/total cells = {0}/{1}", positiveCount, cellCount);
            // Log.WriteLine("TEMPLATE: Number of NEG cells/total cells = {0}/{1}", negativeCount, cellCount);
            char[,] charogram = SprTools.Target2SymbolicTracks(sonogram.Data, dBThreshold, lineLength);

            //var m = DataTools.MatrixTranspose(charogram);
            FileTools.WriteMatrix2File(charogram, "C:\\SensorNetworks\\Output\\FELT_MultiOutput_5templates\\char_ogram.txt"); //view the char-ogram

            double[] scores = new double[sonogram.FrameCount];


            foreach (AcousticEvent av in segments)
            {
                Log.WriteLine("SEARCHING SEGMENT.");
                int startRow = (int)Math.Floor(av.TimeStart * sonogram.FramesPerSecond);
                int endRow   = (int)Math.Floor(av.TimeEnd * sonogram.FramesPerSecond);
                if (endRow >= sonogram.FrameCount) endRow = sonogram.FrameCount;
                int stopRow = endRow - templateFrames - 1;
                if (stopRow <= startRow) stopRow = startRow + 1;  //want minimum of one row

                for (int r = startRow; r < stopRow; r++)
                {
                    double maxSimilarity = -double.MaxValue;
                    int binBuffer = 10;
                    // calculate similarity at one frame position
                    for (int bin = -binBuffer; bin < +binBuffer; bin++)
                    {
                        int c = minBin + bin;
                        if (c < 0) c = 0;
                        double onSum = 0.0;
                        double offSum = 0.0;

                        // calculate onSum and offSum
                        for (int j = 0; j < templateFreqBins; j++) //freq axis
                        {
                            for (int i = 0; i < templateFrames; i++)
                            {
                                if (charogram[r + i, c + j] == '-') continue;
                                else
                                    if (template[i, j] == '-') offSum += sonogram.Data[r + i, c + j];
                                    else
                                    {
                                        //char c1 = charogram[r + i, c + j];
                                        //char c2 = template[i, j];
                                        //int difference = (int)c1 - (int)c2;
                                        int diff = SprTools.SymbolDifference(charogram[r + i, c + j], template[i, j]);
                                        onSum += ((90 - diff) / (double)90 * sonogram.Data[r + i, c + j]);
                                    }
                            }
                        } // calculate similarity
                        double similarity = (onSum / (double)positiveCount) - (offSum / (double)negativeCount); 
                        if (similarity > maxSimilarity) maxSimilarity = similarity;
                    } // end freq bins

                    //following line yields score = av of PosCells - av of NegCells.
                    scores[r] = maxSimilarity;

                    //if (r % 100 == 0) { LoggedConsole.WriteLine("{0} - {1:f3}", r, scores[r]); }
                    //if (scores[r] >= dBThreshold) { LoggedConsole.WriteLine("r={0} score={1}.", r, scores[r]); }
                    if (r % 100 == 0) { LoggedConsole.Write("."); }
                    if (scores[r] < dBThreshold) r += 3; //skip where score is low

                } // end of rows in segment
                LoggedConsole.WriteLine("\nFINISHED SEARCHING SEGMENT FOR ACOUSTIC EVENT.");
            } // foreach (AcousticEvent av in segments)

            var tuple = System.Tuple.Create(scores);
            return tuple;
        } //Execute_Spr_Match()


        public static System.Tuple<double> Execute_One_Spr_Match(char[,] template, double[,] dataMatrix, double dBThreshold)
        {
            int lineLength = 10;

            int templateFrames = template.GetLength(0);  // time axis - SPR template is same orientation as the sonogram data matrix
            int templateFreqBins = template.GetLength(1);  // freq axis
            int cellCount = templateFrames * templateFreqBins;


            int positiveCount = SprTools.CountTemplateChars(template);
            int negativeCount = cellCount - positiveCount;
            Log.WriteLine("TEMPLATE: Number of POS cells/total = {0}/{1}", positiveCount, cellCount);
            char[,] charogram = SprTools.Target2SymbolicTracks(dataMatrix, dBThreshold, lineLength);

            FileTools.WriteMatrix2File(charogram, "C:\\SensorNetworks\\Output\\FELT_LewinsRail1\\charogram.txt"); //view the char-ogram

            double onSum  = 0.0;
            double offSum = 0.0;

            // calculate onSum and offSum
            for (int j = 0; j < templateFreqBins; j++) //freq axis
            {
                for (int i = 0; i < templateFrames; i++)
                {
                    if (charogram[i, j] == '-') continue;
                    else
                        if (template[i, j] == '-') offSum += dataMatrix[i, j];
                        else
                        {
                            //char c1 = charogram[i, j];
                            //char c2 = template[i, j];
                            //int difference = (int)c1 - (int)c2;
                            int diff = SprTools.SymbolDifference(charogram[i, j], template[i, j]);
                            //LoggedConsole.WriteLine("{0},{1}  diff={2}", i,j, diff);
                            onSum += ((90 - diff) / (double)90 * dataMatrix[i, j]);
                        }
                }
            } // calculate similarity
            //following line yields score = av of PosCells - av of NegCells.
            double similarity = (onSum / (double)positiveCount) - (offSum / (double)negativeCount);

            var tuple = System.Tuple.Create(similarity);
            return tuple;
        } //Execute_One_Spr_Match()




        public static int CountPositives(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            int count = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                    if (m[r, c] == 1.0) count++;
            }
            return count;
        }

        /// <summary>
        /// Normalises a binary matrix of -1,+1 or trinary matrix of -1,0,+1 so that the sum of +1 cells = sum of -1 cells.
        /// Change the -1 cells by a ratio.
        /// The purpose is to use the normalised matrix for pattern matching such that the matrix returns a zero value for uniform background noise.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static System.Tuple<double[,], int, int> NormaliseBiTrinaryMatrix(double[,] target)
        {
            int rows = target.GetLength(0);
            int cols = target.GetLength(1);
            var m = new double[rows, cols];
            int posCount = 0;
            int negCount = 0; 
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                    if (target[r, c] > 0) posCount++;
                    else 
                    if (target[r, c] < 0) negCount++;
            }
            double ratio = posCount / (double)negCount;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (target[r, c] < 0) m[r, c] = -ratio;
                    else                  m[r, c] = target[r, c];
                }
            }
            return System.Tuple.Create(m, posCount, negCount);
        }


        /// <summary>
        /// Use this method when want to match defined shape in target using cross-correlation.
        /// This was the method used by Stewart Gage.
        /// First set target and source to same dynamic range.
        /// Then normalise target and source to unit-length.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="dynamicRange"></param>
        /// <param name="sonogram"></param>
        /// <param name="segments"></param>
        /// <param name="minHz"></param>
        /// <param name="maxHz"></param>
        /// <param name="minDuration"></param>
        /// <returns></returns>
        public static System.Tuple<double[]> Execute_StewartGage(double[,] target, double dynamicRange, SpectralSonogram sonogram,
                                    List<AcousticEvent> segments, int minHz, int maxHz, double minDuration)
        {
            Log.WriteLine("SEARCHING FOR EVENTS LIKE TARGET.");
            if (segments == null) return null;
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);
            int targetLength = target.GetLength(0);

            //adjust target's dynamic range to that set by user 
            target = SNR.SetDynamicRange(target, 0.0, dynamicRange); //set event's dynamic range
            double[] v1 = DataTools.Matrix2Array(target);
            v1 = DataTools.normalise2UnitLength(v1);
            //var image = BaseSonogram.Data2ImageData(target);
            //ImageTools.DrawMatrix(image, 1, 1, @"C:\SensorNetworks\Output\FELT_Currawong\target.png");

            double[] scores = new double[sonogram.FrameCount];
            foreach (AcousticEvent av in segments)
            {
                Log.WriteLine("SEARCHING SEGMENT.");
                int startRow = (int)Math.Round(av.TimeStart * sonogram.FramesPerSecond);
                int endRow = (int)Math.Round(av.TimeEnd * sonogram.FramesPerSecond);
                if (endRow >= sonogram.FrameCount) endRow = sonogram.FrameCount;
                int stopRow = endRow - targetLength;
                if (stopRow <= startRow) stopRow = startRow + 1;  //want minimum of one row
                int offset = targetLength / 2;

                for (int r = startRow; r < stopRow; r++)
                {
                    double[,] matrix = DataTools.Submatrix(sonogram.Data, r, minBin, r + targetLength - 1, maxBin);
                    matrix = SNR.SetDynamicRange(matrix, 0.0, dynamicRange); //set event's dynamic range
                    //var image = BaseSonogram.Data2ImageData(matrix);
                    //ImageTools.DrawMatrix(image, 1, 1, @"C:\SensorNetworks\Output\FELT_CURLEW\compare.png");

                    double[] v2 = DataTools.Matrix2Array(matrix);
                    v2 = DataTools.normalise2UnitLength(v2);
                    scores[r] = DataTools.DotProduct(v1, v2);  //the Cross Correlation
                } // end of rows in segment
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
                int startRow = (int)Math.Round(av.TimeStart * sonogram.FramesPerSecond);
                int endRow = (int)Math.Round(av.TimeEnd * sonogram.FramesPerSecond);
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
            var result1 = BaseSonogram.Data2ImageData(target);
            var image   = result1.Item1;
            ImageTools.DrawMatrix(image, 1, 1, imagePath2);


            double[] scores = new double[sonogram.FrameCount];
            foreach (AcousticEvent av in segments)
            {
                Log.WriteLine("SEARCHING SEGMENT.");
                int startRow = (int)Math.Round(av.TimeStart * sonogram.FramesPerSecond);
                int endRow = (int)Math.Round(av.TimeEnd * sonogram.FramesPerSecond);
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

        public static void WriteTemplate2Console(double[,] template)
        {
            LoggedConsole.WriteLine("\n############## TEMPLATE #################");
            int rows = template.GetLength(0);
            int cols = template.GetLength(1);
            for (int r = 0; r < rows; r++)
            {
                var sb = new StringBuilder();
                for (int c = 0; c < cols; c++)
                {
                    if (template[r, c] > 0)  sb.Append('#');
                    else if (template[r, c] == 0) sb.Append('.');
                    else sb.Append('-');
                }
                LoggedConsole.WriteLine(sb.ToString());
            }

        } // WriteTemplate2Console()



    } //end class FindEvents
}
