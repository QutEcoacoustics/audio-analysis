﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainEntry.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the MainEntry type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using AnalysisPrograms.Production;

    /// <summary>
    /// Main Entry for Analysis Programs.
    /// </summary>
    public class MainEntry
    {
        private static readonly Dictionary<string, Action<string[]>> KnownAnalyses;

        static MainEntry()
        {
            // STATIC CONSTRUCTOR
            
            KnownAnalyses = new Dictionary<string, Action<string[]>> (StringComparer.InvariantCultureIgnoreCase)
                {
                    // acoustic event detection
                    { "aed", AED.Dev },

                    // returns list of available analyses
                    // Signed off: Michael Towsey 1st August 2012
                    { "analysesAvailable", strings => AnalysesAvailable.Main(strings) },

                    // IAnalyser - detects canetoad calls as acoustic events
                    // Signed off: Michael Towsey 27th July 2012
                    { "canetoad", Canetoad.Dev },

                    // extracts acoustic indices from an audio recording (mp3 or wav) and prodcues a indices.csv file
                    // Signed off: Michael Towsey 27th July 2012
                    { "audio2csv", strings => AnalyseLongRecording.Main(strings) },

                    // produces a sonogram from an audio file. Can reduce dimensionality of the image.
                    // Signed off: Michael Towsey 31st July 2012
                    { "audio2sonogram", strings => Audio2Sonogram.Main(strings) },

                    // IAnalyser - recognises the short crow "caw" - NOT the longer sigh.
                    // Signed off: Michael Towsey 27th July 2012
                    { "crow", Crow.Dev },

                    // produces a tracks image of column values in a csv file - one track per csv column.
                    // Signed off: Michael Towsey 27th July 2012
                    { "indicesCsv2Image", strings => IndicesCsv2Display.Main(strings) },

                    // event pattern recognition - used for ground-parrots (BRAD)
                    { "epr", GroundParrotRecogniser.Dev },

                    // IAnalyser - detects the oscillating portion of a male koala bellow
                    // Signed off: Michael Towsey 27th July 2012
                    { "koalaMale", KoalaMale.Dev },

                    // IAnalyser - currently recognizes five different calls: human, crow, canetoad, machine and koala.
                    // Signed off: Michael Towsey 27th July 2012
                    { "multiAnalyser", MultiAnalyser.Dev },

                    // IAnalyser - recognises human speech but not word recognition
                    // Signed off: Michael Towsey 27th July 2012
                    { "human", Human1.Dev },

                    // IAnalyser - little spotted kiwi calls from Andrew @ Victoria university. Versions 1 and 2 are obsolete.
                    // Signed off: Michael Towsey 27th July 2012
                    { "kiwi", LSKiwi3.Dev },

                    // little spotted kiwi calls from Andrew @ Victoria university.
                    // Signed off: Michael Towsey 27th July 2012
                    { "kiwiROC", LSKiwiROC.Main },

                    // IAnalyser - LewinsRail3 - yet to be tested on large data set but works OK on one or two available calls.
                    // Signed off: Michael Towsey 27th July 2012
                    { "LewinsRail", LewinsRail3.Dev },

                    // IAnalyser - recognises Planes, Trains And Automobiles - works OK for planes not yet tested on train soun 
                    // Signed off: Michael Towsey 27th July 2012
                    { "machines", PlanesTrainsAndAutomobiles.Dev },

                    // calculates signal to noise ratio
                    // Signed off: Anthony, 25th July 2012
                    { "snr", SnrAnalysis.Dev },




                    // DEVELOPMENT PURPOSES ONLY - FOR MICHAEL'S USE

                    // extracts acoustic indices from one minute segment - for dev purposes only
                    // Signed off: Michael Towsey, 27th July 2012
                    { "acousticIndices", Acoustic.Dev },

                    // extract an acoustic event and make a template for FELT
                    { "createtemplate_felt", FeltTemplate_Create.Dev },

                    // edits the FELT template created above
                    { "edittemplate_felt", FeltTemplate_Edit.Dev },

                    // event pattern recognition - used for ground-parrots (TOWSEY)
                    { "epr2", EPR.Dev },

                    // find other acoustic events like this
                    { "felt", FeltTemplates_Use.Dev },

                    // frog calls
                    { "frog_ribbit", FrogRibit.Dev },

                    // grid recognition
                    { "gratings", GratingDetection.Dev },

                    // Oscillation Recogniser
                    { "od", OscillationRecogniser.Dev },

                    // IAnalyser - detects rain
                    { "rain", Rain.Dev },

                    // IAnalyser - detects Gastric Brooding Frog
                    { "rheobatrachus", RheobatrachusSilus.Dev },

                    // segmentation of a recording
                    { "segment", Segment.Dev },

                    // species accumulation curves
                    // SpeciesAccumulationCurve.Executable
                    { "species_accumulation_curves", SpeciesAccumulationCurve.Dev },
                    
                    // spectral peak tracking
                    /////{ "spt", SPT.Dev },

                    // syntactic pattern recognition
                    ////{ "spr", SPR.Dev },

                    // ???
                    { "test", AnalysisTemplate.Dev },

                    // Production Analysis runs
                    // for running on mono or to run as fast as possible
                    { "production", Runner.Run },
                };
        }

        /// <summary>
        /// Program entry.
        /// </summary>
        /// <param name="args">
        /// Analysis Program arguments.
        /// </param>
        public static void Main(string[] args)
        {
            //var analysers = AnalysisCoordinator.GetAnalysers(typeof(MainEntry).Assembly);
            //analysers.FirstOrDefault(a => a.Identifier == analysisIdentifier);

#if DEBUG
            if (!Debugger.IsAttached)
            {
                Console.WriteLine("Do you wish to debug? Attach now or press [Y] to attach. Press any key other key to continue.");
                if (Console.ReadKey(true).KeyChar.ToString().ToLower() == "y")
                {
                    Debugger.Launch();
                }

                if (Debugger.IsAttached)
                {
                    Console.WriteLine("\t>>> Attach sucessful");
                }
                Console.WriteLine();
            }
#endif
            

            if (args.Length == 0)
            {
                Console.WriteLine("ERROR: You have called the AanalysisPrograms.MainEntry() method without command line arguments.");
                Usage();
            }
            else
            {
                var firstArg = args[0].ToLower();
                string[] restOfArgs = args.Skip(1).ToArray();

                if (KnownAnalyses.ContainsKey(firstArg))
                {
                    var analysisFunc = KnownAnalyses[firstArg];
                    analysisFunc(restOfArgs);
                }
                else
                {
                    // default
                    Console.WriteLine("ERROR: Analysis option unrecognised: " + args[0]);
                    Usage();
                }
            }

#if DEBUG
            // if Michael is debugging with visual studio, this will prevent the window closing.
            Process parentProcess = ProcessExtensions.ParentProcessUtilities.GetParentProcess();
            if (parentProcess.ProcessName == "devenv")
            {
                Console.WriteLine("Exit hung, press any key to quit.");
                Console.ReadLine();
            }
#endif
        }

        private static void Usage()
        {
            Console.Write(
@"
USAGE:
>   analysisPrograms.exe analysisOption [...]

Valid analysis options are:
");
            Console.WriteLine("\t" + string.Join(", ", KnownAnalyses.Keys));
            Console.WriteLine();
        }
    }
}
