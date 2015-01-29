﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Actions.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the MainEntryArguments type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Production
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using AnalysisPrograms.AnalyseLongRecordings;

    using AudioAnalysisTools;

    using Dong.Felt;

    using FELT.Runner;

    using PowerArgs;

    using SammonProjection;

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

        #region FourMainTasks
        [ArgDescription("Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-colour spectrograms.")]
        public AnalyseLongRecording.Arguments Audio2CsvArgs { get; set; }
        public static Action<AnalyseLongRecording.Arguments> Audio2Csv()
        {
            // 2. Analyses long audio recording (mp3 or wav) as per passed config file. Outputs an events.csv file AND an indices.csv file
            // Signed off: Michael Towsey 4th December 2012
            return AnalyseLongRecording.Execute;
        }

        [ArgDescription("Calls AnalysisPrograms.Audio2Sonogram.Main(): Produces a sonogram from an audio file - EITHER custom OR via SOX.")]
        public Audio2Sonogram.Arguments Audio2SonogramArgs { get; set; }
        public static Action<Audio2Sonogram.Arguments> Audio2Sonogram()
        {
            // 3. Produces a sonogram from an audio file - EITHER custom OR via SOX
            // Signed off: Michael Towsey 31st July 2012
            return AnalysisPrograms.Audio2Sonogram.Main;
        }

        [ArgDescription("Calls DrawSummaryIndexTracks.Main(): Input csv file of summary indices. Outputs a tracks image.")]
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

        #region Analyses of single, short (one minute) segments of audio

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

        [ArgDescription("Calls DrawLongDurationSpectrograms.Execute():  Produces LD spectrograms from matrices of indices.")]
        public DrawLongDurationSpectrograms.Arguments ColourSpectrogramArgs { get; set; }
        public static Action<DrawLongDurationSpectrograms.Arguments> ColourSpectrogram()
        {
            return DrawLongDurationSpectrograms.Execute;
        }

        [ArgDescription("Calls DrawZoomingSpectrograms.Execute():  Produces LD spectrograms on different time scales.")]
        public DrawZoomingSpectrograms.Arguments ZoomingSpectrogramsArgs { get; set; }
        public static Action<DrawZoomingSpectrograms.Arguments> ZoomingSpectrograms()
        {
            return DrawZoomingSpectrograms.Execute;
        }
        

        [ArgDescription("Calls DifferenceSpectrogram.Execute():  Produces ")]
        public DifferenceSpectrogram.Arguments DifferenceSpectrogramArgs { get; set; }
        public static Action<DifferenceSpectrogram.Arguments> DifferenceSpectrogram()
        {
            return AnalysisPrograms.DifferenceSpectrogram.Execute;
        }

        [ArgDescription("Calls Canetoad.Dev(): Detects canetoad calls as acoustic events in a short (one minute) recording segment.")]
        public Canetoad.Arguments CanetoadArgs { get; set; }
        public static Action<Canetoad.Arguments> Canetoad()
        {
            // IAnalyser - detects canetoad calls as acoustic events
            // Execute() signed off: Michael Towsey 27th July 2012
            return AnalysisPrograms.Canetoad.Dev;
        }

        [ArgDescription("Calls Crow.Dev(): Detects Crow calls - the short 'caw' NOT the longer sigh.")]
        public Crow.Arguments CrowArgs { get; set; }
        public static Action<Crow.Arguments> Crow()
        {
            // IAnalyser - recognises the short crow "caw" - NOT the longer sigh.
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

        [ArgDescription("Calls EPR.Execute():  Event Pattern Recognition - used for ground-parrots (TOWSEY version).")]
        public EPR.Arguments Epr2Args { get; set; }
        public static Action<EPR.Arguments> Epr2()
        {
            // event pattern recognition - used for ground-parrots (TOWSEY)
            return EPR.Execute;
        }

        [ArgDescription("Calls FeltTemplate_Create.Execute():  FIND EVENTS LIKE THIS: started by TOWSEY but unfinished.")]
        public FeltTemplate_Create.Arguments FeltCreateTemplateArgs { get; set; }
        public static Action<FeltTemplate_Create.Arguments> FeltCreateTemplate()
        {
            // extract an acoustic event and make a template for FELT
            return FeltTemplate_Create.Execute;
        }

        [ArgDescription("Calls FeltTemplate_Edit.Execute():  FIND EVENTS LIKE THIS: started by TOWSEY but unfinished.")]
        public FeltTemplate_Edit.Arguments FeltEditTemplateArgs { get; set; }
        public static Action<FeltTemplate_Edit.Arguments> FeltEditTemplate()
        {
            // edits the FELT template created above
            return FeltTemplate_Edit.Execute;
        }

        [ArgDescription("Calls FeltTemplates_Use.Execute():  FIND EVENTS LIKE THIS: started by TOWSEY but unfinished.")]
        public FeltTemplates_Use.Arguments FeltArgs { get; set; }
        public static Action<FeltTemplates_Use.Arguments> Felt()
        {
            // find other acoustic events like this
            return FeltTemplates_Use.Execute;
        }

        public Object TruskingerFeltArgs { get; set; }
        public static Action<Object> TruskingerFelt()
        {
            // anthony's attempt at FELT
            // this runs his suggestion tool, and the actual FELT analysis
            // DOES NOT CURRENTLY WORK
            return FELT.Runner.Main.ProgramEntry;
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

        public RheobatrachusSilus.Arguments RheobatrachusArgs { get; set; }
        public static Action<RheobatrachusSilus.Arguments> Rheobatrachus()
        {
            // IAnalyser - detects Gastric Brooding Frog
            return RheobatrachusSilus.Dev;
        }

        [ArgDescription("Calls GratingDetection.Execute():  Alternative to oscillation detection. NOT REALLY USEFUL any more!")]
        public GratingDetection.Arguments GratingsArgs { get; set; }
        public static Action<GratingDetection.Arguments> Gratings()
        {
            // grid recognition
            return GratingDetection.Execute;
        }

        [ArgDescription("Calls Human1.Dev():  Recognises human speech but does not do word recognition.")]
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

        public LSKiwiROC.Arguments KiwiRocArgs { get; set; }
        public static Action<LSKiwiROC.Arguments> KiwiRoc()
        {
            // SEPARATE PROCESSING TASK FOR KIWI OUTPUT 
            // little spotted kiwi calls from Andrew @ Victoria university.
            // Signed off: Michael Towsey 27th July 2012
            return AnalysisPrograms.LSKiwiROC.Main;
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

        [ArgDescription("Calls SnrAnalysis.Execute():  Calculates signal to noise ratio.")]
        public SnrAnalysis.Arguments SnrArgs { get; set; }
        public static Action<SnrAnalysis.Arguments> Snr()
        {
            // calculates signal to noise ratio
            // Signed off:  Anthony, 25th July 2012
            return AnalysisPrograms.SnrAnalysis.Execute;
        }

        [ArgDescription("Calls OscillationRecogniser.Execute():  od = Oscillation Detection")]
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

        [ArgDescription("Calls SPR.Execute():  spr = Syntactic Pattern Recognition. Probably not useful anymore.")]
        public SPR.Arguments SprArgs { get; set; }
        public static Action<SPR.Arguments> Spr()
        {
            // syntactic pattern recognition
            return AnalysisPrograms.SPR.Execute;
        }

        [ArgDescription("Calls SammonProgram.Dev():  Produced by Anthony. Not yet used in anger - but should do.")]
        public SammonProgram.Arguments SammonProjectionArgs { get; set; }
        public static Action<SammonProgram.Arguments> SammonProjection()
        {
            // an investigation into sammon projections
            return SammonProgram.Dev;
        }

        [ArgDescription("Calls FeltAnalysis.Dev(): Xueyan's work area.")]
        public FeltAnalysis.Arguments DongArgs { get; set; }
        public static Action<FeltAnalysis.Arguments> Dong()
        {
            // Xueyan's FELT
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
            // Michael's play area
            return AnalysisPrograms.Sandpit.Dev;
        }

        [ArgDescription("Calls AnalysisTemplate.Dev():  A template for producing IAnalysis classes.")]
        public AnalysisTemplate.Arguments TestArgs { get; set; }
        public static Action<AnalysisTemplate.Arguments> Test()
        {
            // A template for producing IAnalysis classes.
            return AnalysisTemplate.Dev;
        }

        [ArgDescription("Test only. This option should be deprecated!")]
        public AnalysesAvailable.Arguments AnalysesAvailableArgs { get; set; }
        public static Action<AnalysesAvailable.Arguments> AnalysesAvailable()
        {
            // 1. Returns list of available analyses
            // Signed off: Michael Towsey 1st August 2012
            return AnalysisPrograms.AnalysesAvailable.Main;
        }

        [ArgDescription("Test only. ")]
        public DummyAnalyser.Arguments DummyArgs { get; set; }
        public static Action<DummyAnalyser.Arguments> Dummy()
        {
            return AnalysisPrograms.DummyAnalyser.Execute;
        }

        public FileRenamer.Arguments FileRenamerArgs { get; set; }
        public static Action<FileRenamer.Arguments> FileRenamer()
        {
            return AnalysisPrograms.FileRenamer.Execute;
        }

        public Create4Sonograms.Arguments Create4SonogramsArgs { get; set; }
        public static Action<Create4Sonograms.Arguments> Create4Sonograms()
        {
            return AnalysisPrograms.Create4Sonograms.Main;
        }

        [ArgDescription("Calls AnalysisPrograms.OscillationsGeneric.Main(): Searches for oscillations")]
        public OscillationsGeneric.Arguments oscillationsGenericArgs { get; set; }
        public static Action<OscillationsGeneric.Arguments> oscillationsGeneric()
        {
            return AnalysisPrograms.OscillationsGeneric.Main;
        }



        public Audio2InputForConvCNN.Arguments CreateConvCnnSonogramsArgs { get; set; }

        public static Action<Audio2InputForConvCNN.Arguments> CreateConvCnnSonograms()
        {
            return Audio2InputForConvCNN.Execute;
        }


        #endregion
    }
}
