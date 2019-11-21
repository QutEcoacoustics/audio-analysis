// <copyright file="Sandpit.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Accord.Statistics.Kernels;
    using Acoustics.Tools.Wav;
    using AnalyseLongRecordings;
    using AnalysisPrograms.Draw.Zooming;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using McMaster.Extensions.CommandLineUtils;
    using Production.Arguments;
    using TowseyLibrary;
    using Log = TowseyLibrary.Log;

    /// <summary>
    /// Activity Code for this class:= sandpit
    ///
    /// Activity Codes for other tasks to do with spectrograms and audio files:
    ///
    /// audio2csv - Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-colour spectrograms.
    /// audio2sonogram - Calls AnalysisPrograms.Audio2Sonogram.Main(): Produces a sonogram from an audio file - EITHER custom OR via SOX.Generates multiple spectrogram images and oscilllations info
    /// indicescsv2image - Calls DrawSummaryIndexTracks.Main(): Input csv file of summary indices. Outputs a tracks image.
    /// colourspectrogram - Calls DrawLongDurationSpectrograms.Execute():  Produces LD spectrograms from matrices of indices.
    /// drawzoomingspectrograms - Calls DrawZoomingSpectrograms.Execute():  Produces LD spectrograms on different time scales.
    /// differencespectrogram - Calls DifferenceSpectrogram.Execute():  Produces Long duration difference spectrograms
    ///
    /// audiofilecheck - Writes information about audio files to a csv file.
    /// snr - Calls SnrAnalysis.Execute():  Calculates signal to noise ratio.
    /// audiocutter - Cuts audio into segments of desired length and format
    /// createfoursonograms.
    /// </summary>
    public class Sandpit
    {
        public const string CommandName = "Sandpit";

        [Command(
        CommandName,
        Description = "[UNMAINTAINED] Michael's personal experimental area.",
        ShowInHelpText = false)]
        public class Arguments : SubCommandBase
        {
            public override Task<int> Execute(CommandLineApplication app)
            {
                var tStart = DateTime.Now;
                Log.Verbosity = 1;
                Log.WriteLine("# Start Time = " + tStart.ToString(CultureInfo.InvariantCulture));

                //AnalyseFrogDataSet();
                //Audio2CsvOverOneFile();
                //Audio2CsvOverMultipleFiles();

                // used to get files from availae for Black rail and Least Bittern papers.
                //CodeToExtractFeatureVectorOfIndices();
                //CodeToGetLdfcSpectrogramsFromAvailae();
                //CodeToPlaceScoreTracksUnderLdfcSpectrograms();
                //CodeToPlaceScoreTracksUnderSingleImage();

                //ConcatenateIndexFilesAndSpectrograms();
                //ConcatenateGreyScaleSpectrogramImages();
                //ConcatenateMarineImages();
                //ConcatenateImages();
                //ConcatenateTwelveImages();
                //CubeHelixDrawTestImage();
                //DrawLongDurationSpectrogram();
                //DrawClusterSequence();
                //DrawStandardSpectrograms();
                //DrawZoomingSpectrogramPyramid();

                //Test_DrawFourSpectrograms();


                //ExtractSpectralFeatures();
                //HerveGlotinMethods();
                //KarlHeinzFrommolt();
                //OTSU_TRHESHOLDING();
                //ResourcesForEventPatternRecognition();
                //ResourcesForRheobatrachusSilusRecogniser();
                //TestAnalyseLongRecordingUsingArtificialSignal();
                //TestArbimonSegmentationAlgorithm();
                //TestDrawHistogram();
                //TestEigenValues();
                //TestChannelIntegrity();
                //TestDct();
                //Statistics.TestGetNthPercentileBin();

                //TEST_FilterMovingAverage();
                //TestImageProcessing();
                //TestMatrix3dClass();
                //TestsOfFrequencyScales();
                //TestReadingFileOfSummaryIndices();
                //TestStructureTensor();
                //TestWavelets();
                //TestFft2D();
                //TestTernaryPlots();
                //TestDirectorySearchAndFileSearch();
                //TestNoiseReduction();
                //ReadSpectralIndicesFromTwoFalseColourSpectrogramRibbons();
                //Oscillations2014.TESTMETHOD_DrawOscillationSpectrogram();
                //Oscillations2014.TESTMETHOD_GetSpectralIndex_Osc();
                //Test_DrawFourSpectrograms();
                //TestLinearFunction();

                Console.WriteLine("# Finished Sandpit Task!    Press any key to exit.");
                return this.Ok();
            }
        }
    }
}
