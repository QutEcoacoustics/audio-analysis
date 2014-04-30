using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms.Production
{
    using System.Reflection;

    using Dong.Felt;

    using FELT.Runner;

    using PowerArgs;

    using SammonProjection;
    using AudioAnalysisTools;

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
            if (args != null && args.ActionName.NotWhitespace())
            {
                MainEntry.PrintUsage(null, MainEntry.Usages.Single, args.ActionName);
            }
            else
            {
                MainEntry.PrintUsage(null, MainEntry.Usages.All);
            }
        }

        #endregion

        #region FourMainTasks
        [ArgDescription("Testing")]
        public AnalysesAvailable.Arguments AnalysesAvailableArgs { get; set; }
        public static Action<AnalysesAvailable.Arguments> AnalysesAvailable()
        {
            // 1. Returns list of available analyses
            // Signed off: Michael Towsey 1st August 2012
            return AnalysisPrograms.AnalysesAvailable.Main;
        }

        public AnalyseLongRecording.Arguments Audio2CsvArgs { get; set; }
        public static Action<AnalyseLongRecording.Arguments> Audio2Csv()
        {
            // 2. Analyses long audio recording (mp3 or wav) as per passed config file. Outputs an events.csv file AND an indices.csv file
            // Signed off: Michael Towsey 4th December 2012
            return AnalyseLongRecording.Execute;
        }

        public Audio2Sonogram.Arguments Audio2SonogramArgs { get; set; }
        public static Action<Audio2Sonogram.Arguments> Audio2Sonogram()
        {
            // 3. Produces a sonogram from an audio file - EITHER custom OR via SOX
            // Signed off: Michael Towsey 31st July 2012
            return AnalysisPrograms.Audio2Sonogram.Main;
        }

        public DrawSummaryIndexTracks.Arguments IndicesCsv2ImageArgs { get; set; }
        public static Action<DrawSummaryIndexTracks.Arguments> IndicesCsv2Image()
        {
            // 4. Produces a tracks image of column values in a csv file - one track per csv column.
            // Signed off: Michael Towsey 27th July 2012
            return DrawSummaryIndexTracks.Main;
        }

        //public DrawSummaryIndices.Arguments IndicesCsv2ImageArgs { get; set; }
        //public static Action<DrawSummaryIndices.Arguments> IndicesCsv2Image()
        //{
        //    // 4. Produces a tracks image of summary indices in a csv file - one track per summary index.
        //    // Signed off: Michael Towsey 29th April 2014
        //    return DrawSummaryIndices.Main;
        //}
        

        #endregion

        #region Analyses for Individual Calls

        public Acoustic.Arguments AcousticIndicesArgs { get; set; }
        public static Action<Acoustic.Arguments> AcousticIndices()
        {
            // extracts acoustic indices from one minute 
            // Execute() signed off: Michael Towsey 27th July 2012
            return Acoustic.Dev;
        }

        public Canetoad.Arguments CanetoadArgs { get; set; }
        public static Action<Canetoad.Arguments> Canetoad()
        {
            // IAnalyser - detects canetoad calls as acoustic events
            // Execute() signed off: Michael Towsey 27th July 2012
            return AnalysisPrograms.Canetoad.Dev;
        }

        public Crow.Arguments CrowArgs { get; set; }
        public static Action<Crow.Arguments> Crow()
        {
            // IAnalyser - recognises the short crow "caw" - NOT the longer sigh.
            // Execute() signed off: Michael Towsey 27th July 2012
            return AnalysisPrograms.Crow.Dev;
        }

        public Human1.Arguments HumanArgs { get; set; }
        public static Action<Human1.Arguments> Human()
        {
            // IAnalyser - recognises human speech but not word recognition
            // Execute() signed off: Michael Towsey 27th July 2012
            return AnalysisPrograms.Human1.Dev;
        }

        public LSKiwi3.Arguments KiwiArgs { get; set; }
        public static Action<LSKiwi3.Arguments> Kiwi()
        {
            // IAnalyser - little spotted kiwi calls from Andrew @ Victoria university. Versions 1 and 2 are obsolete.
            // Execute() signed off: Michael Towsey 27th July 2012
            return AnalysisPrograms.LSKiwi3.Dev;
        }

        public KoalaMale.Arguments KoalaMaleArgs { get; set; }
        public static Action<KoalaMale.Arguments> KoalaMale()
        {
            // IAnalyser - detects the oscillating portion of a male koala bellow
            // Execute() signed off: Michael Towsey 27th July 2012
            return AnalysisPrograms.KoalaMale.Dev;
        }

        public LewinsRail3.Arguments LewinsRailArgs { get; set; }
        public static Action<LewinsRail3.Arguments> LewinsRail()
        {
            // IAnalyser - LewinsRail3 - yet to be tested on large data set but works OK on one or two available calls.
            // Execute() signed off: Michael Towsey 27th July 2012
            return AnalysisPrograms.LewinsRail3.Dev;
        }

        public PlanesTrainsAndAutomobiles.Arguments MachinesArgs { get; set; }
        public static Action<PlanesTrainsAndAutomobiles.Arguments> Machines()
        {
            // IAnalyser - recognises Planes, Trains And Automobiles - works OK for planes not yet tested on train sounds
            // Execute() signed off: Michael Towsey 27th July 2012
            return AnalysisPrograms.PlanesTrainsAndAutomobiles.Dev;
        }

        public MultiAnalyser.Arguments MultiAnalyserArgs { get; set; }
        public static Action<MultiAnalyser.Arguments> MultiAnalyser()
        {
            // IAnalyser - currently recognizes five different calls: human, crow, canetoad, machine and koala.
            // Execute() signed off: Michael Towsey 27th July 2012
            return AnalysisPrograms.MultiAnalyser.Dev;
        }

        public SnrAnalysis.Arguments SnrArgs { get; set; }
        public static Action<SnrAnalysis.Arguments> Snr()
        {
            // calculates signal to noise ratio
            // Signed off:  Anthony, 25th July 2012
            return AnalysisPrograms.SnrAnalysis.Execute;
        }

        public LSKiwiROC.Arguments KiwiRocArgs { get; set; }
        public static Action<LSKiwiROC.Arguments> KiwiRoc()
        {
            // SEPARATE PROCESSING TASK FOR KIWI OUTPUT 
            // little spotted kiwi calls from Andrew @ Victoria university.
            // Signed off: Michael Towsey 27th July 2012
            return AnalysisPrograms.LSKiwiROC.Main;
        }

        public AED.Arguments AedArgs { get; set; }
        public static Action<AED.Arguments> Aed()
        {
            return AED.Execute;
        }

        public FeltTemplate_Create.Arguments FeltCreateTemplateArgs { get; set; }
        public static Action<FeltTemplate_Create.Arguments> FeltCreateTemplate()
        {
            // extract an acoustic event and make a template for FELT
            return FeltTemplate_Create.Execute;
        }

        public FeltTemplate_Edit.Arguments FeltEditTemplateArgs { get; set; }
        public static Action<FeltTemplate_Edit.Arguments> FeltEditTemplate()
        {
            // edits the FELT template created above
            return FeltTemplate_Edit.Execute;
        }

        public FeltTemplates_Use.Arguments FeltArgs { get; set; }
        public static Action<FeltTemplates_Use.Arguments> Felt()
        {
            // find other acoustic events like this
            return FeltTemplates_Use.Execute;
        }

        public GroundParrotRecogniser.Arguments EprArgs { get; set; }
        public static Action<GroundParrotRecogniser.Arguments> Epr()
        {
            // event pattern recognition - used for ground-parrots (BRAD)
            return GroundParrotRecogniser.Dev;
        }

        public EPR.Arguments Epr2Args { get; set; }
        public static Action<EPR.Arguments> Epr2()
        {
            // event pattern recognition - used for ground-parrots (TOWSEY)
            return EPR.Execute;
        }

        public Object TruskingerFeltArgs { get; set; }
        public static Action<Object> TruskingerFelt()
        {
            // anthony's attempt at FELT
            // this runs his suggestion tool, and the actual FELT analysis
            // DOES NOT CURRENTLY WORK
            return FELT.Runner.Main.ProgramEntry;
        }

        public FeltAnalysis.Arguments DongArgs { get; set; }
        public static Action<FeltAnalysis.Arguments> Dong()
        {
            // Xueyan's FELT
            return FeltAnalysis.Dev;
        }

        public FrogRibit.Arguments FrogRibitArgs { get; set; }
        public static Action<FrogRibit.Arguments> FrogRibit()
        {
            // frog calls
            return AnalysisPrograms.FrogRibit.Dev;
        }

        public Frogs.Arguments FrogArgs { get; set; }
        public static Action<Frogs.Arguments> Frog()
        {
            // IAnalyser - detects Gastric Brooding Frog
            return Frogs.Dev;
        }

        public GratingDetection.Arguments GratingsArgs { get; set; }
        public static Action<GratingDetection.Arguments> Gratings()
        {
            // grid recognition
            return GratingDetection.Execute;
        }

        public OscillationRecogniser.Arguments OdArgs { get; set; }
        public static Action<OscillationRecogniser.Arguments> Od()
        {
            // Oscillation Recogniser
            return OscillationRecogniser.Execute;
        }

        public Runner.Arguments ProductionArgs { get; set; }
        public static Action<Runner.Arguments> Production()
        {
            // Production Analysis runs - for running on mono or to run as fast as possible
            return Runner.Run;
        }

        public Rain.Arguments RainArgs { get; set; }
        public static Action<Rain.Arguments> Rain()
        {
            // IAnalyser - detects rain
            return AnalysisPrograms.Rain.Dev;
        }

        public RheobatrachusSilus.Arguments RheobatrachusArgs { get; set; }
        public static Action<RheobatrachusSilus.Arguments> Rheobatrachus()
        {
            // IAnalyser - detects Gastric Brooding Frog
            return RheobatrachusSilus.Dev;
        }

        public Segment.Arguments SegmentArgs { get; set; }
        public static Action<Segment.Arguments> Segment()
        {
            // segmentation of a recording
            return AnalysisPrograms.Segment.Execute;
        }

        public SpeciesAccumulationCurve.Arguments SpeciesAccumulationCurveArgs { get; set; }
        public static Action<SpeciesAccumulationCurve.Arguments> SpeciesAccumulationCurve()
        {
            // species accumulation curves
            return AnalysisPrograms.SpeciesAccumulationCurve.Execute;
        }

        
        public SPT.Arguments SptArgs { get; set; }
        public static Action<SPT.Arguments> Spt()
        {
            // spectral peak tracking
            return SPT.Execute;
        }

        public SPR.Arguments SprArgs { get; set; }
        public static Action<SPR.Arguments> Spr()
        {
            // syntactic pattern recognition
            return AnalysisPrograms.SPR.Execute;
        }

        public AnalysisTemplate.Arguments TestArgs { get; set; }
        public static Action<AnalysisTemplate.Arguments> Test()
        {
            // A template for producing IAnalysis classes.
            return AnalysisTemplate.Dev;
        }

        public SammonProgram.Arguments SammonProjectionArgs { get; set; }
        public static Action<SammonProgram.Arguments> SammonProjection()
        {
            // an investigation into sammon projections
            return SammonProgram.Dev;
        }

        public Sandpit.Arguments SandpitArgs { get; set; }
        public static Action<Sandpit.Arguments> Sandpit()
        {
            // Michael's play area
            return AnalysisPrograms.Sandpit.Dev;
        }

        public AudioFileCheck.Arguments AudioFileCheckArgs { get; set; }
        public static Action<AudioFileCheck.Arguments> AudioFileCheck()
        {
            return AnalysisPrograms.AudioFileCheck.Execute;
        }

        public DrawLongDurationSpectrograms.Arguments ColourSpectrogramArgs { get; set; }
        public static Action<DrawLongDurationSpectrograms.Arguments> ColourSpectrogram()
        {
            return DrawLongDurationSpectrograms.Execute;
        }

        public AudioCutter.Arguments AudioCutterArgs { get; set; }
        public static Action<AudioCutter.Arguments> AudioCutter()
        {
            return AnalysisPrograms.AudioCutter.Execute;
        }        
        
        public XiesAnalysis.Arguments XiesAnalysisArgs { get; set; }
        public static Action<XiesAnalysis.Arguments> XiesAnalysis()
        {
            return AnalysisPrograms.XiesAnalysis.Execute;
        }

        public DifferenceSpectrogram.Arguments DifferenceSpectrogramArgs { get; set; }
        public static Action<DifferenceSpectrogram.Arguments> DifferenceSpectrogram()
        {
            return AnalysisPrograms.DifferenceSpectrogram.Execute;
        }


        public DummyAnalyser.Arguments DummyArgs { get; set; }
        public static Action<DummyAnalyser.Arguments> Dummy()
        {
            return AnalysisPrograms.DummyAnalyser.Execute;
        }

        #endregion
    }
}
