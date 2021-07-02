// <copyright file="EPR.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AnalysisPrograms.Production.Arguments;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using McMaster.Extensions.CommandLineUtils;
    using TowseyLibrary;

    /// <summary>
    /// ######### 2021 NOTE:
    /// This class was refactored in July 2021 to remove all dependencies on the Oscillations2010 class. The 2010 class was long out of date.
    /// Instead, the Execute() method now calls the Oscillations2019 class which implements a similar but improved algorithm.
    /// However some work remains to be done to remove the previous system of config dictionaries.
    ///
    /// ######## THE FOLLOWING NOTES DATE BACK TO 2010-2011.
    /// This program runs an alternative version of Event Pattern Recognition (EPR)
    /// It can be used to detect Ground Parrots.
    /// It was developed by Michael Towsey in 20010 to address difficulties in the original EPR algorithm - see more below.
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
    ///         c) calculate the overlap score.
    /// </summary>
    public class EPR
    {
        public const string CommandName = "EPR";

        [Command(
            CommandName,
            Description = "[UNMAINTAINED] Event Pattern Recognition - used for ground-parrots (TOWSEY version). Revise code if intend to use.")]
        public class Arguments : SourceAndConfigArguments
        {
            [Option(Description = "prefix of name of the created output files")]
            [LegalFilePath]
            [Required]
            public string Target { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                EPR.Execute(this);
                return this.Ok();
            }
        }

        public static void Execute(Arguments arguments)
        {
            MainEntry.WarnIfDeveloperEntryUsed();

            string title = "# EVENT PATTERN RECOGNITION.";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            Log.Verbosity = 1;

            string targetName = arguments.Target; // prefix of name of created files

            var input = arguments.Source;

            string recordingFileName = input.Name;
            string recordingDirectory = input.DirectoryName;
            DirectoryInfo outputDir = arguments.Config.ToFileInfo().Directory;
            FileInfo targetPath = outputDir.CombineFile(targetName + "_target.txt");
            FileInfo targetNoNoisePath = outputDir.CombineFile(targetName + "_targetNoNoise.txt");
            FileInfo noisePath = outputDir.CombineFile(targetName + "_noise.txt");
            FileInfo targetImagePath = outputDir.CombineFile(targetName + "_target.png");
            FileInfo paramsPath = outputDir.CombineFile(targetName + "_params.txt");

            Log.WriteIfVerbose("# Output folder =" + outputDir);

            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(input.FullName);

            //if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz(); THIS METHOD CALL IS OBSOLETE
            int sr = recording.SampleRate;

            //ii: READ PARAMETER VALUES FROM INI FILE
            var config = new ConfigDictionary(arguments.Config);
            Dictionary<string, string> dict = config.GetTable();

            // framing parameters
            //double frameOverlap      = FeltTemplates_Use.FeltFrameOverlap;   // default = 0.5
            double frameOverlap = double.Parse(dict["FRAME_OVERLAP"]);

            //the search band band
            int minHz = int.Parse(dict["MIN_HZ"]);
            int maxHz = int.Parse(dict["MAX_HZ"]);

            // oscillation OD parameters
            double dctDuration = double.Parse(dict["dctDuration"]);   // 2.0; // seconds
            double dctThreshold = double.Parse(dict["dctThreshold"]);  // 0.5;
            int minOscFreq = int.Parse(dict["minOscillationFrequency"]);  // 4;
            int maxOscFreq = int.Parse(dict["maxOscillationFrequency"]);  // 5;
            double decibelThreshold = double.Parse(dict["decibelThreshold"]);

            // iii initialize the spectrogram config with default values..
            SonogramConfig sonoConfig = new SonogramConfig
            {
                SourceFName = recording.BaseName,

                //sonoConfig.WindowSize = windowSize;
                WindowOverlap = frameOverlap,
            };

            // iv: generate the spectrogram
            var spectrogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            Log.WriteLine(
                "Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f2}%, Offset={3:f1}ms, Frames/s={4:f1}",
                spectrogram.Configuration.WindowSize,
                spectrogram.FrameCount,
                spectrogram.FrameDuration * 1000,
                spectrogram.FrameStep * 1000,
                spectrogram.FramesPerSecond,
                frameOverlap);

            int binCount = (int)(maxHz / spectrogram.FBinWidth) - (int)(minHz / spectrogram.FBinWidth) + 1;
            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);

            // v: extract the subband energy array
            // smooth the spectra in all time-frames.
            spectrogram.Data = MatrixTools.SmoothRows(spectrogram.Data, 3);

            // extract array of decibel values, frame averaged over required frequency band
            var decibelArray = SNR.CalculateFreqBandAvIntensity(spectrogram.Data, minHz, maxHz, spectrogram.NyquistFrequency);

            // can noise reduce the decibel array here if it helps.
            // but only if noise reduction not already done earlier.

            // #############################################################################################################################################
            // vi: look for oscillation at required oscillation rate for ground parrots.
            // The following call to the Oscillations2010 class is no longer available. Class deprecated on 2 July 2021.
            // Instead use the below call to the Oscillations2019 class.
            /*
            double[] odScores = Oscillations2010.DetectOscillationsInScoreArray(
                dBArray,
                dctDuration,
                sonogram.FramesPerSecond,
                dctThreshold,
                normaliseDCT,
                minOscilRate,
                maxOscilRate);
            */

            // vi: look for oscillation at required oscillation rate for ground parrots.
            var framesPerSecond = spectrogram.FramesPerSecond;
            Oscillations2019.DetectOscillations(
                decibelArray,
                framesPerSecond,
                decibelThreshold,
                dctDuration,
                minOscFreq,
                maxOscFreq,
                dctThreshold,
                out var dctScores,
                out var oscFreq);

            double maxOctScore = 1.0;
            dctScores = SNR.NormaliseDecibelArray_ZeroOne(dctScores, maxOctScore);
            dctScores = DataTools.filterMovingAverage(dctScores, 5);

            // #############################################################################################################################################
            // vii: LOOK FOR GROUND PARROTS USING TEMPLATE
            var template = GroundParrotRecogniser.ReadGroundParrotTemplateAsList(spectrogram);
            double[] gpScores = DetectEPR(template, spectrogram, dctScores, dctThreshold);
            gpScores = DataTools.normalise(gpScores); //NormaliseMatrixValues 0 - 1

            // #############################################################################################################################################
            // viii: SAVE image of extracted event in the original sonogram
            string sonogramImagePath = outputDir + Path.GetFileNameWithoutExtension(recordingFileName) + ".png";
        }

        public static double[] DetectEPR(List<AcousticEvent> template, BaseSonogram sonogram, double[] odScores, double odThreshold)
        {
            int length = sonogram.FrameCount;
            double[] eprScores = new double[length];
            Oblong ob1 = template[0].Oblong; // the first chirp in template
            Oblong obZ = template[template.Count - 1].Oblong; // the last  chirp in template
            int templateLength = obZ.RowBottom;

            for (int frame = 0; frame < length - templateLength; frame++)
            {
                if (odScores[frame] < odThreshold)
                {
                    continue;
                }

                // get best freq band and max score for the first rectangle.
                double maxScore = -double.MaxValue;
                int freqBinOffset = 0;
                for (int bin = -5; bin < 15; bin++)
                {
                    Oblong ob = new Oblong(ob1.RowTop + frame, ob1.ColumnLeft + bin, ob1.RowBottom + frame, ob1.ColumnRight + bin);
                    double score = GetLocationScore(sonogram, ob);
                    if (score > maxScore)
                    {
                        maxScore = score;
                        freqBinOffset = bin;
                    }
                }

                //if location score exceeds threshold of 6 dB then get remaining scores.
                if (maxScore < 6.0)
                {
                    continue;
                }

                foreach (AcousticEvent ae in template)
                {
                    Oblong ob = new Oblong(ae.Oblong.RowTop + frame, ae.Oblong.ColumnLeft + freqBinOffset, ae.Oblong.RowBottom + frame, ae.Oblong.ColumnRight + freqBinOffset);
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
        public static double GetLocationScore(BaseSonogram sonogram, Oblong ob)
        {
            double max = -double.MaxValue;
            for (int r = ob.RowTop; r < ob.RowBottom; r++)
            {
                for (int c = ob.ColumnLeft; c < ob.ColumnRight; c++)
                {
                    if (sonogram.Data[r, c] > max)
                    {
                        max = sonogram.Data[r, c];
                    }
                }
            }

            //calculate average boundary value
            int boundaryLength = 2 * (ob.RowBottom - ob.RowTop + 1 + ob.ColumnRight - ob.ColumnLeft + 1);
            double boundaryValue = 0.0;
            for (int r = ob.RowTop; r < ob.RowBottom; r++)
            {
                boundaryValue += sonogram.Data[r, ob.ColumnLeft] + sonogram.Data[r, ob.ColumnRight];
            }

            for (int c = ob.ColumnLeft; c < ob.ColumnRight; c++)
            {
                boundaryValue += sonogram.Data[ob.RowTop, c] + sonogram.Data[ob.RowBottom, c];
            }

            boundaryValue /= boundaryLength;

            double score = max - boundaryValue;
            if (score < 0.0)
            {
                score = 0.0;
            }

            return score;
        }
    } // end class
}