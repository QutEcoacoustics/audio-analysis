// --------------------------------------------------------------------------------------------------------------------
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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using Acoustics.Shared.Debugging;

    using AnalysisPrograms.Production;

    using log4net;

    /// <summary>
    /// Main Entry for Analysis Programs.
    /// </summary>
    public class MainEntry
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Dictionary<string, Action<string[]>> KnownAnalyses;

        // STATIC CONSTRUCTOR
        static MainEntry()
        {
            // creation of known analyses dictionary, ignore case on the key for flexibility on matching in commandline
            KnownAnalyses = new Dictionary<string, Action<string[]>>(StringComparer.InvariantCultureIgnoreCase)
                {
                    // ########### FOUR TASKS ############

                    // 1. Returns list of available analyses
                    // Signed off: Michael Towsey 1st August 2012
                    { "analysesAvailable", strings => AnalysesAvailable.Main(strings) },

                    // 2. Analyses long audio recording (mp3 or wav) as per passed config file. Outputs an events.csv file AND an indices.csv file
                    // Signed off: Michael Towsey 27th July 2012
                    { "audio2csv", strings => AnalyseLongRecording.Main(strings) },

                    // 3. Produces a sonogram from an audio file - EITHER custom OR via SOX
                    // Signed off: Michael Towsey 31st July 2012
                    { "audio2sonogram", Audio2Sonogram.Main },

                    // 4. Produces a tracks image of column values in a csv file - one track per csv column.
                    // Signed off: Michael Towsey 27th July 2012
                    { "indicesCsv2Image", strings => IndicesCsv2Display.Main(strings) },



                    // ########### ANALYSES for INDIVIDUAL CALLS - Called through DEV() or EXCECUTE() ############

                    // extracts acoustic indices from one minute segment
                    // { "acousticIndices", Acoustic.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    { "acousticIndices", Acoustic.Execute },

                    // IAnalyser - detects canetoad calls as acoustic events
                    // { "canetoad", Canetoad.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    { "canetoad", Canetoad.Execute },

                    // IAnalyser - recognises the short crow "caw" - NOT the longer sigh.
                    // { "crow", Crow.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    { "crow", strings => Crow.Execute(strings) },

                    // IAnalyser - recognises human speech but not word recognition
                    // { "human", Human1.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    { "human", strings => Human1.Execute(strings) },

                    // IAnalyser - little spotted kiwi calls from Andrew @ Victoria university. Versions 1 and 2 are obsolete.
                    // { "kiwi", LSKiwi3.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    { "kiwi", LSKiwi3.Execute },

                    // IAnalyser - detects the oscillating portion of a male koala bellow
                    //  { "koalaMale", KoalaMale.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    { "koalaMale", KoalaMale.Execute },

                    // IAnalyser - LewinsRail3 - yet to be tested on large data set but works OK on one or two available calls.
                    // { "LewinsRail", LewinsRail3.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    { "LewinsRail", strings => LewinsRail3.Execute(strings) },

                    // IAnalyser - recognises Planes, Trains And Automobiles - works OK for planes not yet tested on train soun 
                    // { "machines", PlanesTrainsAndAutomobiles.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    { "machines", PlanesTrainsAndAutomobiles.Execute },

                    // IAnalyser - currently recognizes five different calls: human, crow, canetoad, machine and koala.
                    // { "multiAnalyser", MultiAnalyser.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    { "multiAnalyser", MultiAnalyser.Execute },

                    // calculates signal to noise ratio - CANNOT CALL FROM COMMAND LINE
                    // Signed off:  Anthony, 25th July 2012
                     { "snr", SnrAnalysis.Dev },


                    // ########### SEPARATE PROCESSING TASK FOR KIWI OUTPUT ###########

                    // little spotted kiwi calls from Andrew @ Victoria university.
                    // Signed off: Michael Towsey 27th July 2012
                    { "kiwiROC", LSKiwiROC.Main },

                    // ########### ANALYSES UNDER DEVELOPMENT - OUTPUT NOT GUARANTEED ###########

                    // acoustic event detection
                    { "aed", AED.Dev },

                    // extract an acoustic event and make a template for FELT
                    { "createtemplate_felt", FeltTemplate_Create.Dev },

                    // edits the FELT template created above
                    { "edittemplate_felt", FeltTemplate_Edit.Dev },

                    // event pattern recognition - used for ground-parrots (BRAD)
                    // { "epr", GroundParrotRecogniser.Dev },

                    // event pattern recognition - used for ground-parrots (TOWSEY)
                    // { "epr2", EPR.Dev },

                    // find other acoustic events like this
                    { "felt", FeltTemplates_Use.Dev },

                    // anthony's attempt at FELT
                    // this runs his suggestion tool, and the actual FELT analysis
                    { "truskinger.felt", strings => FELT.Runner.Main.ProgramEntry(strings) },

                    // frog calls
                    { "frog_ribbit", FrogRibit.Dev },

                    // IAnalyser - detects Gastric Brooding Frog
                    { "frogs", Frogs.Dev },

                    // grid recognition
                    { "gratings", GratingDetection.Dev },

                    // Oscillation Recogniser
                    { "od", OscillationRecogniser.Dev },

                    // Production Analysis runs - for running on mono or to run as fast as possible
                    { "production", Runner.Run },

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

                    // A template for producing IAnalysis classes.
                    { "test", AnalysisTemplate.Dev },
                };
        }

        /// <summary>
        /// Program entry.
        /// </summary>
        /// <param name="args">
        /// Analysis Program arguments.
        /// </param>
        /// <returns>
        /// The exit code (error level) for the application.
        /// </returns>
        public static int Main(string[] args)
        {
            ////var analysers = AnalysisCoordinator.GetAnalysers(typeof(MainEntry).Assembly);
            ////analysers.FirstOrDefault(a => a.Identifier == analysisIdentifier);

            AttachDebugger(ref args);

            AttachExceptionHandler();

            Log.Debug("Executable called with these arguments: {1}{0}{1}".Format2(Environment.CommandLine, Environment.NewLine));

            // note: Exception handling moved to CurrentDomainOnUnhandledException
            if (args.Length == 0)
            {
                const string Msg = "ERROR: You have called the AnalysisPrograms.MainEntry() method without command line arguments.";
                LoggedConsole.WriteErrorLine(Msg);
                Usage();
                throw new CommandMainArgumentMissingException();
            }
            else
            {
                var firstArg = args[0].ToLower();
                string[] restOfArgs = args.Skip(1).ToArray();

                if (KnownAnalyses.ContainsKey(firstArg))
                {
                    var analysisFunc = KnownAnalyses[firstArg];

                    //! excute the analysis
                    analysisFunc(restOfArgs);
                }
                else
                {
                    // default
                    LoggedConsole.WriteLine("ERROR: Analysis option unrecognised: " + args[0]);
                    Usage();

                    throw new AnalysisOptionUnknownCommandException();
                }
            }

            HangBeforeExit();

            // finally return error level
            Log.Debug("ERRORLEVEL: " + (int)CommandLineException.KnownReturnCodes.Ok);
            return (int)CommandLineException.KnownReturnCodes.Ok;
        }

        private static void HangBeforeExit()
        {
#if DEBUG
            // if Michael is debugging with visual studio, this will prevent the window closing.
            Process parentProcess = ProcessExtensions.ParentProcessUtilities.GetParentProcess();
            if (parentProcess.ProcessName == "devenv")
            {
                LoggedConsole.WriteLine("Exit hung, press any key to quit.");
                Console.ReadLine();
            }
#endif
        }

        private static void AttachExceptionHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            int returnCode;
            var ex = (Exception)unhandledExceptionEventArgs.ExceptionObject;
            if (ex is CommandLineException)
            {
                var cex = (CommandLineException)ex;
                returnCode = (int)cex.ReturnCode;
                ////LoggedConsole.WriteLine(cex.Message);
            }
            else 
            {
                Log.Fatal("Unhandled exception ->", ex);
                returnCode = (int)CommandLineException.KnownReturnCodes.SpecialExceptionErrorLevel;
            }

            // finally return error level
            Log.Debug("ERRORLEVEL: " + returnCode);

            if (Debugger.IsAttached)
            {
                // no dot exit, we want the exception to be raised to Window's Exception handling
                // this will allow the debugger to appropriately break on the right line
                Environment.ExitCode = returnCode;
            }
            else
            {
                // If debugger is not attached, we *do not* want to raise the error to the Windows level
                // Everything has already been logged, just exit with appropriate errorlevel
                Environment.Exit(returnCode);
            }
        }

        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1027:TabsMustNotBeUsed", Justification = "Reviewed. Suppression is OK here.")]
        private static void AttachDebugger(ref string[] args)
        {
            string noDebug = "-nodebug".ToLowerInvariant();
            string debug = "-debug".ToLowerInvariant();
            var attach = false;
            if (args.Length > 0)
            {
                if (args[0].ToLowerInvariant() == noDebug)
                {
                    args = args.Skip(1).ToArray();
                    return;
                }

                // no conflict here, if it was nodebug this method would of exited already
                if (args[0].ToLowerInvariant() == debug)
                {
                    args = args.Skip(1).ToArray();
                    attach = true;
                }
            }
#if DEBUG
            if (!Debugger.IsAttached)
            {
                if (!attach)
                {
                    // then prompt manually
                    LoggedConsole.WriteLine(
                        "Do you wish to debug? Attach now or press [Y] to attach. Press any key other key to continue.");
                    attach = Console.ReadKey(true).KeyChar.ToString(CultureInfo.InvariantCulture).ToLower() == "y";
                }

                if (attach)
                {
                    var vsProcess =
                        VisualStudioAttacher.GetVisualStudioForSolutions(
                            new List<string> { "AudioAnalysis2012.sln", "AudioAnalysis.sln" });

                    if (vsProcess != null)
                    {
                        VisualStudioAttacher.AttachVisualStudioToProcess(vsProcess, Process.GetCurrentProcess());
                    }
                    else
                    {
                        // try and attach the old fashioned way
                        Debugger.Launch();
                    }

                    if (Debugger.IsAttached)
                    {
                        LoggedConsole.WriteLine("\t>>> Attach sucessful");
                    }

                    LoggedConsole.WriteLine();
                }
            }
#endif
        }

        private static void Usage()
        {
            LoggedConsole.Write(
@"
USAGE:
>   analysisPrograms.exe [-nodebug] analysisOption [...]

Valid analysis options are:
");
            LoggedConsole.WriteLine("\t" + string.Join(", ", KnownAnalyses.Keys));
            LoggedConsole.WriteLine();
            LoggedConsole.WriteLine("The first argument must be one of the above analysis options.");
            LoggedConsole.WriteLine("The remaining arguments depend on your analysis option.");

            // I'll leave this note here for Michael, why do you do these things? just run the console app and copy the output... or pipe the output into a file... or open the most recent log file!
            ////TowseyLib.FileTools.WriteTextFile(@"C:\temp.txt", string.Join(", ", KnownAnalyses.Keys));
        }
    }
}
