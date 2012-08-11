using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;
using System.Drawing;


namespace AnalysisPrograms
{
    
    
    /// <summary>
    /// This program runs an alternative version of Event Pattern Recognition (EPR) 
    /// It can be used to detect Ground Parrots.
    /// It was developed by Michael Towsey in order to address difficulties in the original EPR algorithm - see more below.
    /// COMMAND LINE ARGUMENTS:
    /// string recordingPath = args[0];   //the recording to be scanned
    /// string iniPath       = args[1];   //the initialisation file containing parameters for AED and EPR
    /// string targetName    = args[2];   //prefix of name of the created output files 
    /// 
    /// The program currently produces only ONE output file: an image of the recording to be scanned with an energy track and two score tracks.
    ///     1) Energy track  - Measure of the total energy in the user defined frequency band, one value per frame. 
    ///     2) Score Track 1 - Oscillation score - Requires user defined parameters to detect the repeated chirp of a Ground Parrot.
    ///                        Is a way to cut down the search space. Only deploy template at places where high Oscillation score. 
    ///     3) Score Track 2 - Template score - dB centre-surround difference of each template rectangle.
    ///                        Currently the dB Score is averaged over the 15 AEs in the groundparrot template.
    ///                        
    /// THE EXISTING ALGORITHM:
    /// 1) Convert the signal to dB spectrogram
    /// 2) AED: i) noise removal
    ///        ii) convert to binary using dB threshold.
    ///       iii) Use spidering algorithm to marquee acoustic events.
    ///        iv) Split over-size events where possible
    ///         v) Remove under-size events.
    /// 3) EPR: i) Align first AE of template to first 'valid' AE in spectrogram. A valid AE is one whose lower left vertex lies in the
    ///            user-defined freq band. Currently align lower left vertex for groundparrot recogniser.
    ///        ii) For each AE in template find closest AE in spectrogram. (Least euclidian distance) 
    ///       iii) For each AE in template, calculate percent overlap to closest AE in spectrogram.
    ///        iv) Apply threshold to adjust the FP-FN trade-off.
    ///         
    /// PROBLEM WITH EXISTING ALGORITHM:
    /// AED:
    ///        i) Major problem is that the AEs found by AED depend greatly on the user-supplied dB threshold.
    ///           If threshold too low, the components of the ground parrot call get incorporated into a single larger AE.
    ///       ii) The spidering algorithm (copied by Brad from MatLab into F#) is computationally expensive.    
    /// EPR:
    ///        i) EPR is hard coded for groundparrots. In particular the configuration of AEs in the template is hard-coded.
    ///       ii) EPR is hard coded to align the template using the lower-left vertex of the first AE.
    ///           This is suitable for the rising cadence of a groundparrot call - but not if descending.
    ///           
    /// POSSIBLE SOLUTIONS TO EPR AND AED
    /// AED:
    ///        i) Need an approach whose result does not depend critically on the user-supplied dB threshold.
    ///           SOLUTION: Try multiple thresholds starting high and dropping in 2dB steps - pick largest score.
    ///       ii) Use oscillation detection (OD) to find locations where good change of ground parrot.
    ///              This only works if the chirps are repeated at fixed interval.
    /// EPR:
    ///        i) Instead of aligning lower-left of AEs align the centroid.
    ///       ii) Only consider AEs whose centroid lies in the frequency band.
    ///      iii) Only consider AEs whose area is 'similar' to first AE of template. 
    ///       iv) Only find overlaps for AEs 2-15 if first overlap exceeds the threshold.
    ///       
    ///  ###############################################################################################################
    /// TWO NEW EPR ALGORITHMS BELOW:
    /// IDEA 1:
    /// Note: NOT all the above ideas have been implemented. Just a few.
    ///       The below does NOT implement AED and does not attempt noise removal to avoid the dB thresholding problem.
    ///       The below uses a different EPR metric
    /// 1) Convert the signal to dB spectrogram   
    /// 2) Detect energy oscillations in the user defined frequency band.
    ///         i) Calulate the dB power in freq band of each frame.
    /// 3) DCT:
    ///         i) Use Discrete Cosine Transform to detect oscillations in band energy.
    ///        ii) Only apply template where the DCT score exceeds a threshold (normalised). Align start of template to high dB frame.
    /// 4) TEMPLATE SCORE       
    ///         i) Calculate dB score for first AE in template. dB score = max_dB - surround_dB
    ///                                    where max_dB = max dB value for all pixels in first template AE.
    ///        ii) Do not proceed if dB score below threshold else calculate dB score for remaining template AEs.
    ///       iii) Calculate average dB score over all 15 AEs in template.
    /// 
    /// COMMENT ON NEW ALGORITHM
    /// 1) It is very fast.
    /// 2) Works well where call shows up as energy oscillation.
    /// 3) Is not as accurate because the dB score has less discrimination than original EPR.
    /// BUT COULD COMBINE THE TWO APPROACHES.
    /// 
    /// ###############################################################################################################
    /// IDEA 2: ANOTHER EPR ALGORITHM
    /// 1) Convert the signal to dB spectrogram   
    /// 2) Detect energy oscillations in the user defined frequency band.
    ///         i) Calulate the dB power in freq band of each frame.
    ///        
    /// 3) DCT: OPTIONAL
    /// ONLY PROCEED IF HAVE HIGH dB SCORE and HIGH DCT SCORE
    /// 
    /// 4) NOISE COMPENSAION
    ///         Subtract modal noise but DO NOT truncate -dB values to zero.
    ///
    /// 5) AT POINTS THAT EXCEED dB and DCT thresholds
    ///    i) loop through dB thresholds from 10dB down to 3dB in steps of 1-2dB.
    ///   ii) determine valid AEs in freq band and within certain time range of current position.
    ///       A valid AE has two attributes: a) entirely within freq band and time width; b) >= 70% overlap with first template AE
    ///  iii) Accumulate a list of valid starting point AEs
    ///  iv) Apply template to each valid start point.
    ///         a) extract AEs at dB threshold apporpriate to the valid AE
    ///         a) align centroid of valid AE with centroid of first template AE
    ///         c) calculate the overlap score
    ///       
    /// 
    /// 
    /// ###############################################################################################################
    /// HOW TO CALL F# METHODS FROM C# CODE.
    /// Use: Can call Brad's F# methods for AED and EPR, especially the spidering method.
    /// 
    /// var binaryMatrix = ConvertSpectrogram2Binary(spectrogram, threshold);
    /// var matrix       = Microsoft.FSharp.Math.MatrixModule.ofArray2D(binaryMatrix);
    /// var aeList       = QutSensors.AudioAnalysis.AED.GetAcousticEvents.getAcousticEvents(matrix);
    /// 
    /// more info here:
    /// http://stackoverflow.com/questions/271966/about-using-f-to-create-a-matrix-assembly-usable-from-c
    ///
    /// More generally can use F# for matrix manipulations as follows:
    /// 
    /// using System; 
    /// using System.Text; 
    /// using Microsoft.FSharp.Math; 
    ///  
    /// namespace CSharp 
    /// { 
    ///   class Program 
    ///   { 
    ///     static void Main(string[] args) 
    ///     { 
    ///       // declare two matrices in C# type
    ///       double[,] x = { { 1.0, 2.0 }, { 4.0, 5.0 } }; 
    ///       double[,] y = { { 1.0, 2.0 }, { 7.0, 8.0 } }; 
    ///       // convert the two matrices to F# type
    ///       Matrix<double> m1 = MatrixModule.of_array2(x); 
    ///       Matrix<double> m2 = MatrixModule.of_array2(y); 
    ///       // perform the F# operation
    ///       var mp = m1 * m2; 
    ///       // convert the F# output back to C# type. 
    ///       var output = mp.ToArray2(); 
    ///       LoggedConsole.WriteLine(output.ToString()); 
    ///       Console.ReadKey(); 
    ///     } 
    /// } 
    /// 
    /// 
    /// ###############################################################################################################
    /// </summary>
    class EPR
    {

        // GROUND PARROT
        // epr2 "C:\SensorNetworks\WavFiles\GroundParrot\Aug2010_Site1\audio\DM420013_0342m_00s__0344m_00s.mp3" C:\SensorNetworks\Output\EPR_GroundParrot\EPR_GroundParrot_Params.txt gp1


        // Keys to recognise identifiers in PARAMETERS - INI file. 
        public static string key_CALL_NAME          = "CALL_NAME";
        public static string key_DO_SEGMENTATION    = "DO_SEGMENTATION";
        public static string key_EVENT_START        = "EVENT_START";
        public static string key_EVENT_END          = "EVENT_END";
        public static string key_MIN_HZ             = "MIN_HZ";
        public static string key_MAX_HZ             = "MAX_HZ";
        public static string key_TEMPLATE_MIN_INTENSITY = "TEMPLATE_MIN_INTENSITY";
        public static string key_TEMPLATE_MAX_INTENSITY = "TEMPLATE_MAX_INTENSITY";
        public static string key_FRAME_OVERLAP      = "FRAME_OVERLAP";
        public static string key_SMOOTH_WINDOW      = "SMOOTH_WINDOW";
        public static string key_SOURCE_RECORDING   = "SOURCE_RECORDING";
        public static string key_SOURCE_DIRECTORY   = "SOURCE_DIRECTORY";
        public static string key_MIN_DURATION       = "MIN_DURATION";
        public static string key_DECIBEL_THRESHOLD  = "DECIBEL_THRESHOLD";        // Used when extracting analog template from spectrogram.
        public static string key_TEMPLATE_THRESHOLD = "TEMPLATE_THRESHOLD";       // Value in 0-1. Used when preparing binary, trinary and syntactic templates.
        public static string key_DONT_CARE_NH       = "DONT_CARE_BOUNDARY";       // Used when preparing trinary template.
        public static string key_LINE_LENGTH        = "SPR_LINE_LENGTH";          // Used when preparing syntactic PR template.
        public static string key_DRAW_SONOGRAMS     = "DRAW_SONOGRAMS";




        public static void Dev(string[] args)
        {
            string title = "# EVENT PATTERN RECOGNITION.";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            Log.Verbosity = 1;
            Segment.CheckArguments(args);

            string recordingPath = args[0];
            string iniPath       = args[1]; // path of the ini or params file
            string targetName    = args[2]; // prefix of name of created files 

            string recordingFileName = Path.GetFileName(recordingPath);
            string recordingDirectory= Path.GetDirectoryName(recordingPath);
            string outputDir         = Path.GetDirectoryName(iniPath) + "\\";
            string targetPath        = outputDir + targetName + "_target.txt";
            string targetNoNoisePath = outputDir + targetName + "_targetNoNoise.txt";
            string noisePath         = outputDir + targetName + "_noise.txt";
            string targetImagePath   = outputDir + targetName + "_target.png";
            string paramsPath        = outputDir + targetName + "_params.txt";

            Log.WriteIfVerbose("# Output folder =" + outputDir);

            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(recordingPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            //ii: READ PARAMETER VALUES FROM INI FILE
            var config = new ConfigDictionary(iniPath);
            Dictionary<string, string> dict = config.GetTable();

            // framing parameters
            //double frameOverlap      = FeltTemplates_Use.FeltFrameOverlap;   // default = 0.5
            double frameOverlap = Double.Parse(dict[key_FRAME_OVERLAP]);
            
            //frequency band
            int minHz = Int32.Parse(dict[key_MIN_HZ]);
            int maxHz = Int32.Parse(dict[key_MAX_HZ]);

            // oscillation OD parameters
            double dctDuration = Double.Parse(dict[OscillationRecogniser.key_DCT_DURATION]);   // 2.0; // seconds
            double dctThreshold = Double.Parse(dict[OscillationRecogniser.key_DCT_THRESHOLD]);  // 0.5;
            int minOscilFreq    = Int32.Parse(dict[OscillationRecogniser.key_MIN_OSCIL_FREQ]);  // 4;
            int maxOscilFreq    = Int32.Parse(dict[OscillationRecogniser.key_MAX_OSCIL_FREQ]);  // 5;
            bool normaliseDCT = false; 
            
            //double dBThreshold       = Double.Parse(dict[key_DECIBEL_THRESHOLD]);   //threshold to set MIN DECIBEL BOUND
            int DRAW_SONOGRAMS       = Int32.Parse(dict[key_DRAW_SONOGRAMS]);       //options to draw sonogram

            // iii initialize the sonogram config class.
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            //sonoConfig.WindowSize = windowSize;
            sonoConfig.WindowOverlap = frameOverlap;


            // iv: generate the sonogram
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f2}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap);
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);

            // v: extract the subband energy array
            Log.WriteLine("# Start extracting target event.");
            double[] dBArray = SNR.DecibelsInSubband(sonogram.Data, minHz, maxHz, sonogram.FBinWidth);
            for (int i=0; i<sonogram.FrameCount; i++) dBArray[i] /= binCount; // get average dB energy
            double Q = 0.0;
            double SD = 0.0;
            dBArray = SNR.NoiseSubtractMode(dBArray, out Q, out SD);
            double maxDB = 6.0;
            double dBThreshold = (2 * SD) / maxDB;  //set dB threshold to 2xSD above background noise
            dBArray = SNR.NormaliseDecibelArray_ZeroOne(dBArray, maxDB);
            dBArray = DataTools.filterMovingAverage(dBArray, 7);
            //Log.WriteLine("Q ={0}", Q);
            //Log.WriteLine("SD={0}", SD);
            //Log.WriteLine("Th={0}", dBThreshold); //normalised threshhold

            // #############################################################################################################################################
            // vi: look for oscillation at required OR for ground parrots.
            double[] odScores = OscillationAnalysis.DetectOscillationsInScoreArray(dBArray, dctDuration, sonogram.FramesPerSecond, dctThreshold,
                                                    normaliseDCT, minOscilFreq, maxOscilFreq);
            //odScores = SNR.NoiseSubtractMode(odScores, out Q, out SD);
            double maxOD = 1.0;
            odScores = SNR.NormaliseDecibelArray_ZeroOne(odScores, maxOD);
            odScores = DataTools.filterMovingAverage(odScores, 5);
            //odScores = DataTools.normalise(odScores); //normalise 0 - 1
            //double odThreshold = (10 * SD) / maxOD;   //set od threshold to 2xSD above background noise
            //double odThreshold = dctThreshold;
            double odThreshold = 0.4;
            Log.WriteLine("Max={0}", odScores.Max());
            //Log.WriteLine("Q  ={0}", Q);
            //Log.WriteLine("SD ={0}", SD);
            Log.WriteLine("Th ={0}", dctThreshold); //normalised threshhold


            // #############################################################################################################################################
            // vii: LOOK FOR GROUND PARROTS USING TEMPLATE
            var template = GroundParrotRecogniser.ReadGroundParrotTemplateAsList(sonogram.FrameOffset, (int)sonogram.FBinWidth);
            double[] gpScores = DetectEPR(template, sonogram, odScores, odThreshold);
            gpScores = DataTools.normalise(gpScores); //normalise 0 - 1

            // #############################################################################################################################################

            // iv: SAVE extracted event as matrix of dB intensity values
            //FileTools.WriteMatrix2File(template, targetPath);                  // write template values to file PRIOR to noise removal.
            //FileTools.WriteMatrix2File(templateMinusNoise, targetNoNoisePath); // write template values to file AFTER to noise removal.
            //FileTools.WriteArray2File(noiseSubband, noisePath);

            // v: SAVE image of extracted event in the original sonogram 
            string sonogramImagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
            DrawSonogram(sonogram, sonogramImagePath, dBArray, dBThreshold / maxDB, odScores, dctThreshold, gpScores, template);


            Log.WriteLine("# Finished everything!");
            Console.ReadLine();
        } // Dev()



        public static double[] DetectEPR(List<AcousticEvent> template, BaseSonogram sonogram, double[] odScores, double odThreshold)
        {
            int length = sonogram.FrameCount;
            double[] eprScores = new double[length];
            Oblong ob1 = template[0].oblong; // the first chirp in template
            Oblong obZ = template[template.Count-1].oblong; // the last  chirp in template
            int templateLength = obZ.r2;

            for (int frame = 0; frame < length - templateLength; frame++)
            {
                if (odScores[frame] < odThreshold) continue;

                // get best freq band and max score for the first rectangle.
                double maxScore = -Double.MaxValue;
                int freqBinOffset = 0;
                for (int bin = -5; bin < 15; bin++)
                {
                    Oblong ob = new Oblong(ob1.r1 + frame, ob1.c1 + bin, ob1.r2 + frame, ob1.c2 + bin);
                    double score = GetLocationScore(sonogram, ob);
                    if (score > maxScore)
                    {
                        maxScore = score;
                        freqBinOffset = bin;
                    }
                }

                //if location score exceeds threshold of 6 dB then get remaining scores.
                if (maxScore < 6.0) continue;

                foreach(AcousticEvent ae in template)
                {
                    Oblong ob = new Oblong(ae.oblong.r1 + frame, ae.oblong.c1 + freqBinOffset, ae.oblong.r2 + frame, ae.oblong.c2 + freqBinOffset);
                    double score = GetLocationScore(sonogram, ob);
                    eprScores[frame] += score;
                }
                eprScores[frame] /= template.Count;

            }
            return eprScores;
        }

        /// <summary>
        /// reutrns the difference between the maximum dB value in a retangular location and the average of the boundary dB values.
        /// </summary>
        /// <param name="sonogram"></param>
        /// <param name="ob"></param>
        /// <returns></returns>
        public static double GetLocationScore(BaseSonogram sonogram, Oblong ob)
        {
            double max = -Double.MaxValue;
            for (int r = ob.r1; r < ob.r2; r++)
                for (int c = ob.c1; c < ob.c2; c++)
                {
                    if (sonogram.Data[r, c] > max) max = sonogram.Data[r, c];
                }

            //calculate average boundary value
            int boundaryLength = 2 * (ob.r2 - ob.r1 + 1 + ob.c2 - ob.c1 + 1);
            double boundaryValue = 0.0;
            for (int r = ob.r1; r < ob.r2; r++) boundaryValue += (sonogram.Data[r, ob.c1] + sonogram.Data[r, ob.c2]);
            for (int c = ob.c1; c < ob.c2; c++) boundaryValue += (sonogram.Data[ob.r1, c] + sonogram.Data[ob.r2, c]);
            boundaryValue /= boundaryLength;

            double score = max - boundaryValue;
            if (score < 0.0) score = 0.0;
            return score;
        }

        public static void DrawSonogram(BaseSonogram sonogram, string path, double[] normalizedDBArray, double dBThreshold,
                                                       double[] odScores, double odThreshold, double[] gpScores, List<AcousticEvent> list)
        {
            Log.WriteLine("# Save image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetScoreTrack(normalizedDBArray, 0, 1.0, dBThreshold));
                image.AddTrack(Image_Track.GetScoreTrack(odScores, 0, 1.0, odThreshold));
                image.AddTrack(Image_Track.GetScoreTrack(gpScores, 0, 1.0, 0.3));
                //image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                //var aes = new List<AcousticEvent>();
                //aes.Add(ae);
                image.AddEvents(list, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond); 
                image.Save(path);
            }
        } //end DrawSonogram

    } // end class
}
