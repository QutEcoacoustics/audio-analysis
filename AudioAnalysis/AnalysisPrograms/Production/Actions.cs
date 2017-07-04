// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Actions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the MainEntryArguments type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable StyleCop.SA1201
namespace AnalysisPrograms.Production
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using AnalyseLongRecordings;
    using AudioAnalysisTools;
    using Dong.Felt;
    using Draw.Zooming;
    using EventStatistics;
    using PowerArgs;
    using Recognizers.Base;

    /// <summary>
    /// Defines the various actions (sub programs) that we can run.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1516:ElementsMustBeSeparatedByBlankLine", Justification = "Reviewed. Suppression is OK here.")]
    [ArgAllowNullActions]
    public partial class MainEntryArguments
    {
        #region meta

        [ArgDescription("Prints the available program actions")]
        public object ListArgs { get; set; }
        public static void List(object obj)
        {
            MainEntry.PrintUsage(null, MainEntry.Usages.ListAvailable);
        }

        [ArgDescription("Prints the full help for the program and all actions")]
        [ArgExample("help spt", "will print help for the spt action")]
        public HelpArguments HelpArgs { get; set; }
        public static void Help(HelpArguments args)
        {
            if (args != null && args.ActionName.IsNotWhitespace())
            {
                MainEntry.PrintUsage(null, MainEntry.Usages.Single, args.ActionName);
            }
            else
            {
                MainEntry.PrintUsage(null, MainEntry.Usages.All);
            }
        }

        #endregion

        #region ImportantTasks
        [ArgDescription("List available IAnalyzers available for use with audio2csv or eventRecognizer")]
        public object AnalysesAvailableArgs { get; set; }
        public static Action<object> AnalysesAvailable()
        {
            // 1. Returns list of available analyses
            // Signed off: Anthony Truskinger 2016
            return AnalysisPrograms.AnalysesAvailable.Execute;
        }

        [ArgDescription("Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-colour spectrograms.")]
        public AnalyseLongRecording.Arguments Audio2CsvArgs { get; set; }
        public static Action<AnalyseLongRecording.Arguments> Audio2Csv()
        {
            // 2. Analyses long audio recording (mp3 or wav) as per passed config file. Outputs an events.csv file AND an indices.csv file
            // Signed off: Michael Towsey 4th December 2012
            return AnalyseLongRecording.Execute;
        }

        [ArgDescription("Calls AnalysisPrograms.Audio2Sonogram.Main(): Produces a standard spectrogram from an audio file - EITHER custom OR via SOX.")]
        public Audio2Sonogram.Arguments Audio2SonogramArgs { get; set; }
        public static Action<Audio2Sonogram.Arguments> Audio2Sonogram()
        {
            // 3. Produces a sonogram from an audio file - EITHER custom OR via SOX
            // Signed off: Michael Towsey 31st July 2012
            return AnalysisPrograms.Audio2Sonogram.Main;
        }

        [ArgDescription("Calls DrawSummaryIndexTracks.Main(): Input a csv file of summary indices. Outputs a tracks image.")]
        public DrawSummaryIndexTracks.Arguments IndicesCsv2ImageArgs { get; set; }
        public static Action<DrawSummaryIndexTracks.Arguments> IndicesCsv2Image()
        {
            // 4. Produces a tracks image of column values in a csv file - one track per csv column.
            // Signed off: Michael Towsey 27th July 2012
            return DrawSummaryIndexTracks.Main;
        }

        [ArgDescription("Event statistics accepts a list of events to analyze and returns a data file of statistics")]
        public EventStatistics.EventStatisticsAnalysis.Arguments EventStatisticsArgs { get; set; }
        public static Action<EventStatisticsAnalysis.Arguments> EventStatistics()
        {
            return EventStatisticsAnalysis.Execute;
        }

        #endregion

        #region Analyses of single, short (one minute) segments of audio

        [ArgDescription("Calls MultiAnalyser.Execute():  Entry point for running multiple species recognizers at same time. Only use on short recordings (< 2mins)")]
        public MultiAnalyser_OBSOLETE.Arguments MultiAnalyserArgs { get; set; }
        public static Action<MultiAnalyser_OBSOLETE.Arguments> MultiAnalyser()
        {
            // IAnalyser - currently recognizes five different calls: human, crow, canetoad, machine and koala.
            // Execute() signed off: Michael Towsey 27th July 2012
            return MultiAnalyser_OBSOLETE.Dev;
        }

        [ArgDescription("The entry point for all species or event recognizers. Only to be used on short recordings (< 2 mins).")]
        public RecognizerEntry.Arguments EventRecognizerArgs { get; set; }
        public static Action<RecognizerEntry.Arguments> EventRecognizer()
        {
            return RecognizerEntry.Execute;
        }

        public AudioCutter.Arguments AudioCutterArgs { get; set; }
        public static Action<AudioCutter.Arguments> AudioCutter()
        {
            return AnalysisPrograms.AudioCutter.Execute;
        }

        public AudioFileCheck.Arguments AudioFileCheckArgs { get; set; }
        public static Action<AudioFileCheck.Arguments> AudioFileCheck()
        {
            return AnalysisPrograms.AudioFileCheck.Execute;
        }

        [ArgDescription("Calls Acoustic.Dev(): Extracts spectral and summary acoustic indices from a short (one minute) recording segment.")]
        public Acoustic.Arguments AcousticIndicesArgs { get; set; }
        public static Action<Acoustic.Arguments> AcousticIndices()
        {
            // extracts acoustic indices from one minute
            // Execute() signed off: Michael Towsey 27th July 2012
            return Acoustic.Dev;
        }

        [ArgDescription("Calls AED.Execute():  ACOUSTIC EVENT DETECTION.")]
        public Aed.Arguments AedArgs { get; set; }
        public static Action<Aed.Arguments> Aed()
        {
            return AnalysisPrograms.Aed.Execute;
        }

        [ArgDescription("Calls ConcatenateIndexFiles.Execute():  Concatenates multiple consecutive index.csv files.")]
        public ConcatenateIndexFiles.Arguments ConcatenateIndexFilesArgs { get; set; }
        public static Action<ConcatenateIndexFiles.Arguments> ConcatenateIndexFiles()
        {
            return AnalysisPrograms.ConcatenateIndexFiles.Execute;
        }

        [ArgDescription("Calls DrawEasyImage.Execute():  Concatenates multiple consecutive index.csv files.")]
        public DrawEasyImage.Arguments DrawEasyImageArgs { get; set; }
        public static Action<DrawEasyImage.Arguments> DrawEasyImage()
        {
            return AnalysisPrograms.DrawEasyImage.Execute;
        }

        [ArgDescription("Calls DrawLongDurationSpectrograms.Execute():  Produces long-duration false-colour spectrograms from matrices of spectral indices.")]
        public DrawLongDurationSpectrograms.Arguments ColourSpectrogramArgs { get; set; }
        public static Action<DrawLongDurationSpectrograms.Arguments> ColourSpectrogram()
        {
            return DrawLongDurationSpectrograms.Execute;
        }

        [ArgDescription("Calls DrawZoomingSpectrograms.Execute():  Produces long-duration false-colour spectrograms on different time scales.")]
        public DrawZoomingSpectrograms.Arguments ZoomingSpectrogramsArgs { get; set; }
        public static Action<DrawZoomingSpectrograms.Arguments> ZoomingSpectrograms()
        {
            return DrawZoomingSpectrograms.Execute;
        }

        [ArgDescription("Calls DifferenceSpectrogram.Execute():  Produces a false-colour spectrogram that show only the differences between two spectrograms.")]
        public DifferenceSpectrogram.Arguments DifferenceSpectrogramArgs { get; set; }
        public static Action<DifferenceSpectrogram.Arguments> DifferenceSpectrogram()
        {
            return AnalysisPrograms.DifferenceSpectrogram.Execute;
        }

        public object TruskingerFeltArgs { get; set; }
        public static Action<object> TruskingerFelt()
        {
            // Anthony's attempt at FELT
            // this runs his suggestion tool, and the actual FELT analysis
            // DOES NOT CURRENTLY WORK
            return FELT.Runner.Main.ProgramEntry;
        }

        [ArgDescription("Calls Human1.Dev():  Recognises human speech but does not do word recognition.")]
        public Human1.Arguments HumanArgs { get; set; }
        public static Action<Human1.Arguments> Human()
        {
            // IAnalyser - recognises human speech but not word recognition
            // Execute() signed off: Michael Towsey 27th July 2012
            return Human1.Dev;
        }

        [ArgDescription("Calls LSKiwi3.Dev():  Only of use for Little Brown Kiwi recordings from New Zealand.")]
        public LSKiwi3.Arguments KiwiArgs { get; set; }
        public static Action<LSKiwi3.Arguments> Kiwi()
        {
            // IAnalyser - little spotted kiwi calls from Andrew @ Victoria university. Versions 1 and 2 are obsolete.
            // Execute() signed off: Michael Towsey 27th July 2012
            return LSKiwi3.Dev;
        }

        [ArgDescription("Calls LSKiwiROC.Main():  DEPRACATED. Only used in 2012 to analyse output from LSKiwi3.Dev().")]
        public LSKiwiROC.Arguments KiwiRocArgs { get; set; }
        public static Action<LSKiwiROC.Arguments> KiwiRoc()
        {
            // SEPARATE PROCESSING TASK FOR KIWI OUTPUT
            // little spotted kiwi calls from Andrew @ Victoria university.
            // Signed off: Michael Towsey 27th July 2012
            return LSKiwiROC.Main;
        }

        [ArgDescription("Calls KoalaMale.Dev():  Dates back to 2012. Still current.")]
        public KoalaMale.Arguments KoalaMaleArgs { get; set; }
        public static Action<KoalaMale.Arguments> KoalaMale()
        {
            // IAnalyser - detects the oscillating portion of a male koala bellow
            // Execute() signed off: Michael Towsey 27th July 2012
            return AnalysisPrograms.KoalaMale.Dev;
        }

        [ArgDescription("Calls SnrAnalysis.Execute():  Calculates signal to noise ratio.")]
        public SnrAnalysis.Arguments SnrArgs { get; set; }
        public static Action<SnrAnalysis.Arguments> Snr()
        {
            // calculates signal to noise ratio
            // Signed off:  Anthony, 25th July 2012
            return SnrAnalysis.Execute;
        }

        [ArgDescription("Calls OscillationRecogniser.Execute():  od = Oscillation Detection")]
        public OscillationRecogniser.Arguments OdArgs { get; set; }
        public static Action<OscillationRecogniser.Arguments> Od()
        {
            // Oscillation Recogniser
            return OscillationRecogniser.Execute;
        }

        [ArgDescription("Calls OscillationsGeneric.Main(): Searches for oscillations")]
        public OscillationsGeneric.Arguments oscillationsGenericArgs { get; set; }
        public static Action<OscillationsGeneric.Arguments> oscillationsGeneric()
        {
            return OscillationsGeneric.Main;
        }

        public Runner.Arguments ProductionArgs { get; set; }
        public static Action<Runner.Arguments> Production()
        {
            // Production Analysis runs - for running on mono or to run as fast as possible
            return Runner.Run;
        }

        [ArgDescription("Calls Rain.Dev():  Used to recognise one minute segments of rain. Revise code if intend to use.")]
        public Rain_OBSOLETE.Arguments RainArgs { get; set; }
        public static Action<Rain_OBSOLETE.Arguments> Rain()
        {
            // IAnalyser - detects rain
            return Rain_OBSOLETE.Dev;
        }

        [ArgDescription("Calls LewinsRail3.Dev():  Dates back to 2012. Revise code if intend to use.")]
        public LewinsRail3OBSOLETE.Arguments LewinsRailArgs { get; set; }
        public static Action<LewinsRail3OBSOLETE.Arguments> LewinsRail()
        {
            // IAnalyser - LewinsRail3 - yet to be tested on large data set but works OK on one or two available calls.
            // Execute() signed off: Michael Towsey 27th July 2012
            return LewinsRail3OBSOLETE.Dev;
        }

        [ArgDescription("Calls PlanesTrainsAndAutomobiles.Execute():  Dates back to 2013. Revise code if intend to use.")]
        public PlanesTrainsAndAutomobiles.Arguments MachinesArgs { get; set; }
        public static Action<PlanesTrainsAndAutomobiles.Arguments> Machines()
        {
            // IAnalyser - recognises Planes, Trains And Automobiles - works OK for planes not yet tested on train sounds
            // Execute() signed off: Michael Towsey 27th July 2012
            return PlanesTrainsAndAutomobiles.Dev;
        }

        public Segment.Arguments SegmentArgs { get; set; }
        public static Action<Segment.Arguments> Segment()
        {
            // segmentation of a recording
            return AnalysisPrograms.Segment.Execute;
        }

        [ArgDescription("Calls SpeciesAccumulationCurve.Execute():  SHOULD NOT BE IN THIS LIST!")]
        public SpeciesAccumulationCurve.Arguments SpeciesAccumulationCurveArgs { get; set; }
        public static Action<SpeciesAccumulationCurve.Arguments> SpeciesAccumulationCurve()
        {
            // species accumulation curves
            return AnalysisPrograms.SpeciesAccumulationCurve.Execute;
        }


        [ArgDescription("Calls SPT.Execute():  spt = Spectral Peak Tracking. Probably not useful anymore.")]
        public SPT.Arguments SptArgs { get; set; }
        public static Action<SPT.Arguments> Spt()
        {
            // spectral peak tracking
            return SPT.Execute;
        }

        [ArgDescription("Calls SPR.Execute():  spr = Syntactic Pattern Recognition. OBSOLETE.")]
        public SPR_OBSOLETE.Arguments SprArgs { get; set; }
        public static Action<SPR_OBSOLETE.Arguments> Spr()
        {
            // syntactic pattern recognition
            return SPR_OBSOLETE.Execute;
        }

        [ArgDescription("Calls FeltAnalysis.Dev(): Xueyan's work area.")]
        public FeltAnalysis.Arguments DongArgs { get; set; }
        public static Action<FeltAnalysis.Arguments> Dong()
        {
            return FeltAnalysis.Dev;
        }

        [ArgDescription("Calls XiesAnalysis.Execute(): Jie's work area.")]
        public XiesAnalysis.Arguments XiesAnalysisArgs { get; set; }
        public static Action<XiesAnalysis.Arguments> XiesAnalysis()
        {
            return AnalysisPrograms.XiesAnalysis.Execute;
        }

        [ArgDescription("Calls AnalysisPrograms.Sandpit.Dev(): Michael's experimental area.")]
        public Sandpit.Arguments SandpitArgs { get; set; }
        public static Action<Sandpit.Arguments> Sandpit()
        {
            return AnalysisPrograms.Sandpit.Dev;
        }

        [ArgDescription("Calls AnalysisTemplate.Dev():  A template for producing IAnalysis classes.")]
        public AnalysisTemplate.Arguments TestArgs { get; set; }
        public static Action<AnalysisTemplate.Arguments> Test()
        {
            // A template for producing IAnalysis classes.
            return AnalysisTemplate.Dev;
        }

        [ArgDescription("Calls Create4Sonograms.Main(). Creates a set of four spectrograms derived using different algorithms.")]
        public Create4Sonograms.Arguments Create4SonogramsArgs { get; set; }
        public static Action<Create4Sonograms.Arguments> Create4Sonograms()
        {
            return AnalysisPrograms.Create4Sonograms.Main;
        }

        [ArgDescription("Test only. ")]
        public DummyAnalyser.Arguments DummyArgs { get; set; }
        public static Action<DummyAnalyser.Arguments> Dummy()
        {
            return DummyAnalyser.Execute;
        }

        public FileRenamer.Arguments FileRenamerArgs { get; set; }
        public static Action<FileRenamer.Arguments> FileRenamer()
        {
            return AnalysisPrograms.FileRenamer.Execute;
        }

        [ArgDescription("Calls Crow.Dev(): Detects Crow calls - the short 'caw' NOT the longer sigh.")]
        public Crow.Arguments CrowArgs { get; set; }
        public static Action<Crow.Arguments> Crow()
        {
            // IAnalyser - recognizes the short crow "caw" - NOT the longer sigh.
            // Execute() signed off: Michael Towsey 27th July 2012
            return AnalysisPrograms.Crow.Dev;
        }

        [ArgDescription("Calls GroundParrotRecogniser.Dev():  event pattern recognition - used for ground-parrots (BRAD version).")]
        public GroundParrotRecogniser.Arguments EprArgs { get; set; }
        public static Action<GroundParrotRecogniser.Arguments> Epr()
        {
            // event pattern recognition - used for ground-parrots (BRAD)
            return GroundParrotRecogniser.Dev;
        }

        [ArgDescription("Calls EPR.Execute():  Event Pattern Recognition - used for ground-parrots (TOWSEY version). Revise code if intend to use.")]
        public EPR.Arguments Epr2Args { get; set; }
        public static Action<EPR.Arguments> Epr2()
        {
            // event pattern recognition - used for ground-parrots (TOWSEY)
            return EPR.Execute;
        }

        [ArgDescription("DEPRACATED:  All frog recognizers should now enter through EventRecognizer.Execute() or Multirecognizer.Execute().")]
        public CanetoadOld_OBSOLETE.Arguments CanetoadArgs { get; set; }
        public static Action<CanetoadOld_OBSOLETE.Arguments> Canetoad()
        {
            // IAnalyser - detects canetoad calls as acoustic events
            // Execute() signed off: Michael Towsey 27th July 2012
            return CanetoadOld_OBSOLETE.Dev;
        }

        [ArgDescription("No further practical use. Used in 2014 to prepare short recordings of bird calls for analysis by Convolution Neural Networks.")]
        public Audio2InputForConvCNN.Arguments CreateConvCnnSonogramsArgs { get; set; }
        public static Action<Audio2InputForConvCNN.Arguments> CreateConvCnnSonograms()
        {
            return Audio2InputForConvCNN.Execute;
        }

        [ArgDescription("DEPRACATED:  All frog recognizers should now enter through EventRecognizer.Execute() or Multirecognizer.Execute().")]
        public FrogRibit_OBSOLETE.Arguments FrogRibitArgs { get; set; }
        public static Action<FrogRibit_OBSOLETE.Arguments> FrogRibit()
        {
            // frog calls
            return FrogRibit_OBSOLETE.Dev;
        }

        [ArgDescription("DEPRACATED:  All frog recognizers should now enter through EventRecognizer.Execute() or Multirecognizer.Execute().")]
        public Frogs_OBSOLETE.Arguments FrogArgs { get; set; }
        public static Action<Frogs_OBSOLETE.Arguments> Frog()
        {
            // IAnalyser - detects Gastric Brooding Frog
            return Frogs_OBSOLETE.Dev;
        }

        [ArgDescription("DEPRACATED:  All frog recognizers should now enter through EventRecognizer.Execute() or Multirecognizer.Execute().")]
        public RheobatrachusSilus.Arguments RheobatrachusArgs { get; set; }
        public static Action<RheobatrachusSilus.Arguments> Rheobatrachus()
        {
            // IAnalyser - detects Gastric Brooding Frog
            return RheobatrachusSilus.Dev;
        }

        [ArgDescription("DEPRACATED. Calls GratingDetection.Execute():  An attempt to find alternative to oscillation detection. NOT USEFUL any more!")]
        public GratingDetection_OBSOLETE.Arguments GratingsArgs { get; set; }
        public static Action<GratingDetection_OBSOLETE.Arguments> Gratings()
        {
            // grid recognition
            return GratingDetection_OBSOLETE.Execute;
        }

        [ArgDescription("DEPRACATED:  Calls FeltTemplate_Create.Execute():  FIND EVENTS LIKE THIS: started by TOWSEY but unfinished.")]
        public FeltTemplate_Create.Arguments FeltCreateTemplateArgs { get; set; }
        public static Action<FeltTemplate_Create.Arguments> FeltCreateTemplate()
        {
            // extract an acoustic event and make a template for FELT
            return FeltTemplate_Create.Execute;
        }

        [ArgDescription("DEPRACATED:  Calls FeltTemplate_Edit.Execute():  FIND EVENTS LIKE THIS: started by TOWSEY but unfinished.")]
        public FeltTemplate_Edit.Arguments FeltEditTemplateArgs { get; set; }
        public static Action<FeltTemplate_Edit.Arguments> FeltEditTemplate()
        {
            // edits the FELT template created above
            return FeltTemplate_Edit.Execute;
        }

        [ArgDescription("DEPRACATED:  Calls FeltTemplates_Use.Execute():  FIND EVENTS LIKE THIS: started by TOWSEY but unfinished.")]
        public FeltTemplates_Use.Arguments FeltArgs { get; set; }
        public static Action<FeltTemplates_Use.Arguments> Felt()
        {
            // find other acoustic events like this
            return FeltTemplates_Use.Execute;
        }

        #endregion
    }
}
