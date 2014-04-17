using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioAnalysisTools.StandardSpectrograms;
using AudioAnalysisTools.WavTools;
using AudioAnalysisTools.DSP;
using System.IO;
using MathNet.Numerics;



namespace QutBioacosutics.Xie
{
    using AudioAnalysisTools;
    using log4net;
    using TowseyLibrary;
    using System.Drawing;
    using System.Drawing.Imaging;
    using QutBioacosutics.Xie.Configuration;

    public static class Main
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Main));

        public static void Entry(dynamic configuration, FileInfo source)
        {

            Log.Info("Enter into Jie's personal workspace");

            /*
             * Warning! The `configuration` variable is dynamic.
             * Do not use it outside of this method. Extract all params below.
             */
            
            //***************************************************************//
            //Parameters setting

            // Peak parameters
            double amplitudeThreshold = configuration.amplitude_threshold;   // Decibel---the minimum amplitude value
            int range = configuration.range;                                 // Frame---the distance in either side for selecting peaks
            int distance = configuration.distance;                           // Frame---remove near peaks
            double binToreance = configuration.binToreance;                  // Bin---the fluctuation of the dominant frequency bin   
            int frameThreshold = configuration.frame_threshold;              // Frame---frame numbers of the silence   
           
            // Track parameters
            double trackThreshold = configuration.track_threshold;            // Used for calculating the percent of peaks in one track    
            int maximumDuration = configuration.maximum_duration;             // Minimum duration of tracks
            int minimumDuration = configuration.minimum_duration;             // Maximum duration of tracks   
            double maximumDiffBin = configuration.maximum_diffBin;            // Difference between the highest and lowest bins   
            int duraionThreshold = configuration.duraion_threshold;           // Frame---threshold of minimum duration

            // Harmonic parameters
            int colThreshold = configuration.col_threshold;                   // ???    
            int zeroBinIndex = configuration.zero_binIndex;                   // ???

            // Path for output
            string imagePath = configuration.image_path;

            // Canetoad parameters---class
            int minimumOscillationNumberCanetoad = configuration.minimumOscillationNumberCanetoad;
            int maximumOscillationNumberCanetoad = configuration.maximumOscillationNumberCanetoad;
            int minimumFrequencyCanetoad = configuration.MinimumFrequencyCanetoad;
            int maximumFrequencyCanetoad = configuration.MaximumFrequencyCanetoad;
            double dct_DurationCanetoad = configuration.Dct_DurationCanetoad;
            double dct_ThresholdCanetoad = configuration.Dct_ThresholdCanetoad;

            // Gracillenta parameters---class




            // Nasuta parameters---class



            //****************************************************************//


            // Path of loaded recording

            string path = @"C:\Jie\data\Segment_JCU_01\020313_429min.wav";

            var recording = new AudioRecording(path);

            // Step.1 Generate spectrogarm
            // A. Generate spectrogram for extracting tracks and entropy
          
            var spectrogramConfig = new SonogramConfig() { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.9, WindowSize = 512 };
            var spectrogram = new SpectrogramStandard(spectrogramConfig, recording.GetWavReader());

            // B. Generate spectrogram for extracting oscillation rate
           
            var spectrogramConfigOsc = new SonogramConfig() { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.5, WindowSize = 512 };
            var spectrogramOsc = new SpectrogramStandard(spectrogramConfigOsc, recording.GetWavReader());


            // Step.2 Produce features




            //*************************************************************//
            var canetoadConfig = new CanetoadConfiguration 
            {
                MinimumOscillationNumberCanetoad = minimumOscillationNumberCanetoad,
                MaximumOscillationNumberCanetoad = maximumOscillationNumberCanetoad,
                MinimumFrequencyCanetoad = minimumFrequencyCanetoad,
                MaximumFrequencyCanetoad = maximumFrequencyCanetoad,
                Dct_DurationCanetoad = dct_DurationCanetoad,
                Dct_ThresholdCanetoad = dct_ThresholdCanetoad,
            };



            // A. Tracks


            // B. Entropy


            // C. Oscillation rate




            // 1. Cane_toad detection


            // 2. Gracillenta detection


            // 3. Nasuta detection
            
                

            //D. Harmonic

            // Step.3 Draw spectrogram

            double[,] spectrogramMatrix = DataTools.normalise(spectrogram.Data);
            int rows = spectrogramMatrix.GetLength(0);
            int cols = spectrogramMatrix.GetLength(1);

            Color[] grayScale = ImageTools.GrayScale();
            Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int greyId = (int)Math.Floor(spectrogramMatrix[r, c] * 255);
                    if (greyId < 0) greyId = 0;
                    else
                        if (greyId > 255) greyId = 255;

                    greyId = 255 - greyId;
                    bmp.SetPixel(c, r, grayScale[greyId]);
                }
            }

            bmp.Save(imagePath);


            // Step.4 Draw false-color spectrogram




            Log.Info("OK");
            
            
            
            
            
            
            
            
            
            
            
            //FileInfo path = ((string)configuration.file).ToFileInfo();

            //if (source != null)
            //{
            //    path = source;
            //}

            //Log.Info(@path);

            //string outpath;
            //var fileName = path.Name;

            //outpath = Path.GetFileNameWithoutExtension(fileName);

            //string outPath = Path.Combine("c:\\jie\\output\\csv", outpath);
            //string outPath2 = Path.ChangeExtension(outPath, ".csv");
            
            //string imagePath = outPath2;
            //string imagePath = configuration.image_path;
            //string ipDirStr = @"C:\Jie\output\index1";
            //string opDirStr = @"C:\Jie\output\index1";
            //double amplitudeThreshold = configuration.amplitude_threshold;
            //int range = configuration.range;
            //int distance = configuration.distance;
            //double binToreance = configuration.binToreance;
            //int frameThreshold = configuration.frameThreshold;
            //int duraionThreshold = configuration.duraionThreshold;
            //double trackThreshold = configuration.trackThreshold;
            //int maximumDuration = configuration.maximumDuration;
            //int minimumDuration = configuration.minimumDuration;
            //double maximumDiffBin = configuration.maximumDiffBin;

            //int colThreshold = configuration.colThreshold;
            //int zeroBinIndex = configuration.zeroBinIndex;

            // Change seconds to framesize



            ////..........................................................//
            ////Draw False color spectrogram
            //string fileName = "frogs_DATE";
            //var ipDir = new DirectoryInfo(ipDirStr);
            //var opDir = new DirectoryInfo(opDirStr);
            //int startMinute = 19 * 60;

            //LDSpectrogramConfig spgConfig = new LDSpectrogramConfig(fileName, ipDir, opDir);
            ////spgConfig.ColourMap = "TRC-OSC-HAR";
            //spgConfig.ColourMap = "OSC-HAR-TRC";
            //spgConfig.MinuteOffset = startMinute;
            //spgConfig.FrameWidth = 256;
            ////spgConfig.SampleRate = 17640;
            //spgConfig.SampleRate = 22050;
            //FileInfo path = new FileInfo(Path.Combine(opDir.FullName, "LDSpectrogramConfig.yml"));
            //spgConfig.WritConfigToYAML(path);
            ////LDSpectrogramRGB.DrawFalseColourSpectrograms(spgConfig);
            //XieFunction.DrawFalseColourSpectrograms(spgConfig);



            ////..........................................................//
            ////Read csc files and save them to make three indexes

            //var trackResult = new double[726, 257];
            //var longtrackResult = new double[726, 257];
            //var oscillationResult = new double[726, 257];
            //var harmonicResult = new double[726, 257];

            //var csvFiles = Directory.GetFiles("C:\\Jie\\output\\csv");

            //var csvCount = csvFiles.Count();

            //for (int csvIndex = 0; csvIndex < csvCount; csvIndex++)
            //{
            //    var csvfile = CsvTools.ReadCSVFile2Matrix(csvFiles[csvIndex]);


            //    string fullName = Path.GetFileNameWithoutExtension(csvFiles[csvIndex]);

            //    string num = Path.GetFileNameWithoutExtension(fullName);
            //    int numVal = 0;
            //    if (num.Length == 11)
            //    {
            //        string subnum = num.Substring(7, 1);
            //        numVal = Int32.Parse(subnum);
            //    }

            //    if (num.Length == 12)
            //    {
            //        string subnum = num.Substring(7, 2);
            //        numVal = Int32.Parse(subnum);
            //    }

            //    if (num.Length == 13)
            //    {
            //        string subnum = num.Substring(7, 3);
            //        numVal = Int32.Parse(subnum);
            //    }


            //    for (int i = 0; i < csvfile.GetLength(0); i++)
            //    {
            //        trackResult[numVal, i] = csvfile[i, 0];
            //        longtrackResult[numVal, i] = csvfile[i, 1];
            //        oscillationResult[numVal, i] = csvfile[i, 2];
            //        harmonicResult[numVal, i] = csvfile[i, 3];
            //    }
            //}

            //FileTools.WriteMatrix2File(trackResult, @"C:\Jie\output\index2\track.csv");
            //FileTools.WriteMatrix2File(oscillationResult, @"C:\Jie\output\index2\oscillation.csv");
            //FileTools.WriteMatrix2File(harmonicResult, @"C:\Jie\output\index2\harmonic.csv");

            
            ////Write 3 index matirxes to csv file
            //int csvRow = trackResult.GetLength(0);
            //int csvCol = trackResult.GetLength(1);

            //for (int c = 0; c < csvCol; c++)
            //{
            //    var lines = new string[csvRow + 1];
            //    for (int r = 0; r < csvRow; r++)
            //    {
            //        lines[r] = trackResult[r, c].ToString();
            //    }

            //    FileTools.WriteTextFile(@"C:\\Jie\\output\\index\\track.csv", lines);
            //}

            //FileTools.WriteTextFile(@"C:\\Jie\\output\\index\\track.csv", trackResult);

            // bool noiseReduction = (int)configuration.do_noise_reduction == 1;

            //float noiseThreshold = configuration.noise_threshold;
            //Dictionary<string, string> complexSettings = configuration.complex_settings;
            //string[] simpleArray = configuration.array_example;
            //int[] simpleArray2 = configuration.array_example_2;
            //int? missingValue = configuration.i_dont_exist;
            //string doober = configuration.doobywacker;

            // the following will always throw an exception
            //int missingValue2 = configuration.i_also_dont_exist;

            // Execute analysis

            //var fileEntries = Directory.GetFiles("C:\\Jie\\data\\Segment_JCU_01");

            //var fileCount = fileEntries.Count();



            //for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
            //{
            //    //string path = fileEntries[319];
            //    string path = @"C:\Jie\data\canetoad2.wav";

            //    string num = Path.GetFileNameWithoutExtension(path);

            //    string outpath = Path.GetFileNameWithoutExtension(num);

            //    string outPath = Path.Combine("c:\\jie\\output\\csv", outpath);
            //    string outPath2 = Path.ChangeExtension(outPath, ".csv");


            //    //int numVal = 0;
            //    //if (num.Length == 11)
            //    //{
            //    //    string subnum = num.Substring(7, 1);
            //    //    numVal = Int32.Parse(subnum);
            //    //}

            //    //if (num.Length == 12)
            //    //{
            //    //    string subnum = num.Substring(7, 2);
            //    //    numVal = Int32.Parse(subnum);
            //    //}

            //    //if (num.Length == 13)
            //    //{
            //    //    string subnum = num.Substring(7, 3);
            //    //    numVal = Int32.Parse(subnum);
            //    //}

            //    var recording = new AudioRecording(path);

            //    // Generate a spectrogram
            //    //var recording = new AudioRecording(path);
            //    var spectrogramConfig = new SonogramConfig() { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.9, WindowSize = 512 };
            //    var spectrogram = new SpectrogramStandard(spectrogramConfig, recording.GetWavReader());

            //    // Rotate the spectrogram to make it more suitable for me
            //    var spectrogramMatrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogram.Data);

            //    int rows = spectrogram.Data.GetLength(1);
            //    int cols = spectrogram.Data.GetLength(0);

            //    // Find short tracks

            //    var peakMatrix = new double[spectrogram.Data.GetLength(1), spectrogram.Data.GetLength(0)];
            //    var localPeaks = new FindLocalPeaks();
            //    peakMatrix = localPeaks.LocalPeaks(spectrogram.Data, amplitudeThreshold, range, distance);

            //    var multipleTracks = new ExtractTracks();
            //    var results = multipleTracks.GetTracks(peakMatrix, binToreance, frameThreshold, duraionThreshold, trackThreshold, maximumDuration, minimumDuration, maximumDiffBin);
            //    var trackMatrix = new double[rows, cols];

            //    var trackFeature = new double[rows];
            //    trackFeature = results.Item1;

            //    // Normalize the track duration
            //    var norTArray = new double[trackFeature.Length];

            //    for (int i = 0; i < trackFeature.Length; i++)
            //    {
            //        norTArray[i] = trackFeature[i] / cols;
            //    }

            //    trackMatrix = results.Item2;


            //    // Find long tracks

            //    //var spectrogramConfigLongTrack = new SonogramConfig() { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.5, WindowSize = 512 };
            //    //var spectrogramLongTrack = new SpectrogramStandard(spectrogramConfigLongTrack, recording.GetWavReader());

            //    //// Smooth the spectrogram for extracting long tracks
            //    //var LongTrackSmoothMatrix = ImageTools.GaussianBlur_5cell(spectrogramLongTrack.Data);

            //    //int longrows = spectrogramLongTrack.Data.GetLength(1);
            //    //int longcols = spectrogramLongTrack.Data.GetLength(0);

            //    //var peakLongMatrix = new double[longrows, longcols];
            //    //var fingLongPeaks = new FindLocalPeaks();
            //    //peakLongMatrix = fingLongPeaks.LocalLongPeaks(LongTrackSmoothMatrix, 3, 9, 19);

            //    //// Extract long tracks with wide band

            //    //var trackLongMatrix = new double[longrows, longcols];
            //    //var trackLongArray = new double[longrows];
            //    //var multipleLongTracks = new ExtractTracks();

            //    //var resultsLong = multipleLongTracks.GetLongTracks(peakLongMatrix, 3, frameThreshold, duraionThreshold, trackThreshold, 40, 20);
            //    //trackLongArray = resultsLong.Item1;

            //    //// Normalization
            //    //var norLongTArray = new double[trackLongArray.Length];
            //    //for (int i = 0; i < trackLongArray.Length; i++)
            //    //{
            //    //    norLongTArray[i] = trackLongArray[i] / longcols;
            //    //}

            //    //trackLongMatrix = resultsLong.Item2;

            //    // Find oscillation 

            //    var spectrogramConfigOscillation = new SonogramConfig() { NoiseReductionType = NoiseReductionType.NONE, WindowOverlap = 0.5, WindowSize = 512 };
            //    var spectrogramOscillation = new SpectrogramStandard(spectrogramConfigOscillation, recording.GetWavReader());


            //    var oscRate = new FindOscillation();
            //    var oscRateResult = oscRate.OscillationRate(spectrogramOscillation, 400, 900, 0.5, 10, 15, 0.75);

                


            //    double[,] spectrogramMatrix1 = DataTools.normalise(spectrogramOscillation.Data);
            //    int rows1 = spectrogramMatrix1.GetLength(0);
            //    int cols1 = spectrogramMatrix1.GetLength(1);

            //    Color[] grayScale = ImageTools.GrayScale();
            //    Bitmap bmp = new Bitmap(cols1, rows1, PixelFormat.Format24bppRgb);

            //    for (int r = 0; r < rows1; r++)
            //    {
            //        for (int c = 0; c < cols1; c++)
            //        {
            //            int greyId = (int)Math.Floor(spectrogramMatrix1[r, c] * 255);
            //            if (greyId < 0) greyId = 0;
            //            else
            //                if (greyId > 255) greyId = 255;

            //            greyId = 255 - greyId;
            //            bmp.SetPixel(c, r, grayScale[greyId]);
            //        }
            //    }

            //    for (int i = 0; i < rows1; i++)
            //    {
            //        for (int j = 0; j < cols1; j++)
            //        {
            //            if (oscRateResult[i, j] != 0)
            //            {
            //                bmp.SetPixel(j, i, Color.Blue);
            //            }

            //        }
            //    }


            //    bmp.Save(imagePath);





            //    var Oscillation = new FindOscillation();
            //    var oscillationArray = Oscillation.getOscillation(spectrogramOscillation.Data, zeroBinIndex);

            //    // Find harmonic

            //    var Harmonic = new FindHarmonics();
            //    var harmonicMatrix = Harmonic.getHarmonic(trackMatrix, colThreshold, zeroBinIndex);
            //    // Change harmonicMarix to array
            //    var harmonicArray = new double[harmonicMatrix.GetLength(0)];
            //    for (int i = 0; i < harmonicMatrix.GetLength(0); i++)
            //    {
            //        var temp = 0.0;
            //        for (int j = 0; j < harmonicMatrix.GetLength(1); j++)
            //        {
            //            temp = temp + harmonicMatrix[i, j];
            //        }
            //        harmonicArray[i] = temp;
            //    }

            //    // Normalization
            //    var norHArray = new double[harmonicArray.Length];
            //    for (int i = 0; i < harmonicArray.Length; i++)
            //    {
            //        norHArray[i] = harmonicArray[i] / spectrogram.Data.GetLength(0);
            //    }

            //    var FrogIndexList = new List<FrogIndex>();
            //    for (int i = (norHArray.Length - 1); i > 0; i--)
            //    {
            //        var FrogIndex = new FrogIndex();
            //        FrogIndex.Track = norTArray[i];
            //        //FrogIndex.LongTrack = norLongTArray[i];
            //        FrogIndex.Oscillation = oscillationArray[i];
            //        FrogIndex.Harmonic = norHArray[i];

            //        FrogIndexList.Add(FrogIndex);
            //    }

            //    //var FrogIndex = new List<List<string>>();

            //    //FrogIndex.Add(new List<string> { norTArray.ToString(), norOscArray.ToString(), norHArray.ToString() });

            //    FileInfo fileInfo = new FileInfo(outPath2);

            //    CsvTools.WriteResultsToCsv(fileInfo, FrogIndexList);


            //    // Write the index to three matrix

            //    //for (int r = 0; r < norHArray.Length; r++)
            //    //{
            //    //    trackResult[r, numVal] = norTArray[r];
            //    //}

            //    //for (int r = 0; r < norHArray.Length; r++)
            //    //{
            //    //    oscillationResult[r, numVal] = oscillationArray[r];
            //    //}

            //    //for (int r = 0; r < norHArray.Length; r++)
            //    //{
            //    //    harmonicResult[r, numVal] = norHArray[r];
            //    //}

            //    //Log.Info(numVal);
            //}

            //// Write 3 index into csv file
            ////var FrogIndexList = new List<FrogIndex>();
            ////var FrogIndex = new FrogIndex();

            ////FrogIndex.Track = trackResult;
            ////FrogIndex.Oscillation = oscillationResult;
            ////FrogIndex.Harmonic = harmonicResult;

            ////FrogIndexList.Add(FrogIndex);

            ////FileInfo fileInfo = new FileInfo(imagePath);
            ////CsvTools.WriteResultsToCsv(fileInfo, FrogIndexList);

            //Log.Info("Analysis complete");


            //var matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogramLongTrack.Data);
            
            // the dominant frequency of Litoria fallax is 4750Hz, thus the frequency band of this frog is 4500Hz - 5000Hz, the duration is 430ms, the cycles per second is 70.
            //var fallaxLowBin = (int)(4500 / spectrogramLongTrack.FBinWidth);
            //var fallaxHighBin = (int)(5000 / spectrogramLongTrack.FBinWidth);

            //var fallaxminDuration = (int) (spectrogramLongTrack.FramesPerSecond * 400 / 1000);
            //var fallaxmaxDuration = (int) (spectrogramLongTrack.FramesPerSecond * 460 / 1000);

            //// find the maximum in the specify frequency band
            //var fallaxMatrix = new double[rows, cols];
            //var findnasutaPeaks = new FindLocalPeaks();
            //fallaxMatrix = findnasutaPeaks.MaximumOfBand(matrix, amplitudeThreshold, fallaxHighBin, fallaxLowBin);

            //var image = ImageTools.DrawMatrix(fallaxMatrix);
            //image.Save(imagePath);



            //var fallaxTracks = new ExtractTracks();
            //var fallaxresult = fallaxTracks.GetTracks(fallaxMatrix, 3, frameThreshold, duraionThreshold, trackThreshold, fallaxmaxDuration, fallaxminDuration, 10);



            //var rowLow = (int)(2000 / spectrogramLongTrack.FBinWidth);
            //var rowHigh = (int)(2500 / spectrogramLongTrack.FBinWidth);

            //var tempArray = new double[cols - 1];

            //for (int c = 0; c < (cols - 1); c++)
            //{
            //    double energyDiff = 0;
            //    for (int r = rowLow; r < rowHigh; r++)
            //    {
            //        //var tempDiff = matrix[r, c] * Math.Pow(matrix[r, c] - matrix[r, c + 1], 2);
            //        var tempDiff = Math.Pow(matrix[r, c], 2);
            //        energyDiff = energyDiff + tempDiff;
            //    }

            //    tempArray[c] = energyDiff;
            //}

            ////var result = DataTools.AutoCorrelation(tempArray,0,tempArray.Length);

            //DataTools.writeBarGraph(tempArray);

 

            //double[,] spectrogramMatrix = DataTools.normalise(spectrogramLongTrack.Data);            
            //int rows = spectrogramMatrix.GetLength(0);
            //int cols = spectrogramMatrix.GetLength(1);

            //Color[] grayScale = ImageTools.GrayScale();
            //Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            //for (int r = 0; r < rows; r++)
            //{
            //    for (int c = 0; c < cols; c++)
            //    {
            //        int greyId = (int)Math.Floor(spectrogramMatrix[r, c] * 255);
            //        if (greyId < 0) greyId = 0;
            //        else
            //            if (greyId > 255) greyId = 255;

            //        greyId = 255 - greyId;
            //        bmp.SetPixel(c, r, grayScale[greyId]);
            //    }
            //}

            //bmp.Save(imagePath);


            //var image = ImageTools.DrawMatrix(spectrogramLongTrack.Data);
            //image.Save(imagePath);



            // find the oscillation through all the recordings

            //spectrogramOscillation.Data = MatrixTools.MatrixRotate90Anticlockwise(spectrogramOscillation.Data);




            //var image = ImageTools.DrawMatrix(oscillationArray);
            //image.Save(@"C:\Jie\output\a.png");


            //var norOscArray = oscillationArray;
            


            //peakMatrix = MatrixTools.MatrixRotate90Anticlockwise(peakMatrix);
            //var image = ImageTools.DrawMatrix(trackLongMatrix);
            //image.Save(imagePath);

            

            //var image = ImageTools.DrawMatrix(peakMatrix);
            //image.Save(imagePath);



            //var image = ImageTools.DrawMatrix(trackMatrix);
            //image.Save(imagePath);

            // find the harmonic structure based on tracks

            //trackMatrix = MatrixTools.MatrixRotate90Anticlockwise(trackMatrix);
            //double[,] spectrogramMatrix = DataTools.normalise(spectrogramOscillation.Data);
            //spectrogramMatrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogramMatrix);

            //int rows = spectrogramMatrix.GetLength(0);
            //int cols = spectrogramMatrix.GetLength(1);

            //Color[] grayScale = ImageTools.GrayScale();
            //Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            //for (int r = 0; r < rows; r++)
            //{
            //    for (int c = 0; c < cols; c++)
            //    {
            //        int greyId = (int)Math.Floor(spectrogramMatrix[r, c] * 255);
            //        if (greyId < 0) greyId = 0;
            //        else
            //            if (greyId > 255) greyId = 255;

            //        greyId = 255 - greyId;
            //        bmp.SetPixel(c, r, grayScale[greyId]);
            //    }
            //}

            //for (int i = 0; i < rows; i++)
            //{
            //    for (int j = 0; j < cols; j++)
            //    {
            //        if (trackMatrix[i, j] == 1)
            //        {
            //            bmp.SetPixel(j, i, Color.Blue);
            //        }

            //    }
            //}

            //for (int i = 0; i < rows; i++)
            //{
            //    for (int j = 0; j < cols; j++)
            //    {
            //        if (trackMatrix[i, j] == 2)
            //        {
            //            bmp.SetPixel(j, i, Color.Red);
            //        }

            //    }
            //}

            //bmp.Save(imagePath);

            //double[,] spectrogramLongMatrix = DataTools.normalise(LongTrackSmoothMatrix);
            //spectrogramLongMatrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogramLongMatrix);

            //int rows = spectrogramLongMatrix.GetLength(0);
            //int cols = spectrogramLongMatrix.GetLength(1);

            //Color[] grayScale = ImageTools.GrayScale();
            //Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);
            ////bmp.Save(imagePath);

            // save the arrays (1-trackArray,2-oscillationArray,3-harmonicArray) to CSV file.
            //var NormaliseHarmonicArray = norHArray.Reverse();
            //var NormaliseOscillationArray = norOscArray.Reverse();
            //var NormaliseTrackArray = norTArray.Reverse();

            //var FrogIndexList = new List<FrogIndex>();
            //for (int i = (norHArray.Length - 1); i > 0; i--)
            //{
            //    var FrogIndex = new FrogIndex();
            //    FrogIndex.Track = norTArray[i];
            //    //FrogIndex.LongTrack = norLongTArray[i];
            //    FrogIndex.Oscillation = norOscArray[i];
            //    FrogIndex.Harmonic = norHArray[i];

            //    FrogIndexList.Add(FrogIndex);
            //}

            ////var FrogIndex = new List<List<string>>();

            ////FrogIndex.Add(new List<string> { norTArray.ToString(), norOscArray.ToString(), norHArray.ToString() });

            //FileInfo fileInfo = new FileInfo(imagePath);

            //CsvTools.WriteResultsToCsv(fileInfo, FrogIndexList);


        }

        
    }
}
