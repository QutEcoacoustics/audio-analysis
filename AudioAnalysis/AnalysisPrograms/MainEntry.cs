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
    using System.Text;

    using PowerArgs;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

#if DEBUG
    using Acoustics.Shared.Debugging;
#endif

    using AnalysisPrograms.Production;
    using System.IO;
    using Acoustics.Tools.Audio;
    using Acoustics.Tools;
    using System.Security.Cryptography;
    using Dong.Felt;
    using log4net;

    /// <summary>
    /// Main Entry for Analysis Programs.
    /// </summary>
    public static partial class MainEntry
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /*ATA
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
                    //{ "analysesAvailable", strings => AnalysesAvailable.Main(strings) },

                    // 2. Analyses long audio recording (mp3 or wav) as per passed config file. Outputs an events.csv file AND an indices.csv file
                    // Signed off: Michael Towsey 4th December 2012
                    //{ "audio2csv", AnalyseLongRecording.Execute },

                    // 3. Produces a sonogram from an audio file - EITHER custom OR via SOX
                    // Signed off: Michael Towsey 31st July 2012
                    //{ "audio2sonogram", Audio2Sonogram.Main },

                    // 4. Produces a tracks image of column values in a csv file - one track per csv column.
                    // Signed off: Michael Towsey 27th July 2012
                    //{ "indicesCsv2Image", IndicesCsv2Display.Main },



                    // ########### ANALYSES for INDIVIDUAL CALLS - Called through DEV() or EXCECUTE() ############

                    // extracts acoustic indices from one minute segment
                    // { "acousticIndices", Acoustic.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    //{ "acousticIndices", Acoustic.Execute },

                    // IAnalyser - detects canetoad calls as acoustic events
                    // { "canetoad", Canetoad.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    //{ "canetoad", Canetoad.Execute },

                    // IAnalyser - recognises the short crow "caw" - NOT the longer sigh.
                    // { "crow", Crow.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    //{ "crow", strings => Crow.Execute(strings) },

                    // IAnalyser - recognises human speech but not word recognition
                    // { "human", Human1.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    //{ "human", strings => Human1.Execute(strings) },

                    // IAnalyser - little spotted kiwi calls from Andrew @ Victoria university. Versions 1 and 2 are obsolete.
                    // { "kiwi", LSKiwi3.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    //{ "kiwi", LSKiwi3.Execute },

                    // IAnalyser - detects the oscillating portion of a male koala bellow
                    //  { "koalaMale", KoalaMale.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    //{ "koalaMale", KoalaMale.Execute },

                    // IAnalyser - LewinsRail3 - yet to be tested on large data set but works OK on one or two available calls.
                    // { "LewinsRail", LewinsRail3.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    //{ "LewinsRail", strings => LewinsRail3.Execute(strings) },

                    // IAnalyser - recognises Planes, Trains And Automobiles - works OK for planes not yet tested on train soun 
                    // { "machines", PlanesTrainsAndAutomobiles.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    //{ "machines", PlanesTrainsAndAutomobiles.Execute },

                    // IAnalyser - currently recognizes five different calls: human, crow, canetoad, machine and koala.
                    // { "multiAnalyser", MultiAnalyser.Dev },
                    // Execute() signed off: Michael Towsey 27th July 2012
                    //{ "multiAnalyser", MultiAnalyser.Execute },

                    // calculates signal to noise ratio - CANNOT CALL FROM COMMAND LINE
                    // Signed off:  Anthony, 25th July 2012
                     //{ "snr", SnrAnalysis.Dev },


                    // ########### SEPARATE PROCESSING TASK FOR KIWI OUTPUT ###########

                    // little spotted kiwi calls from Andrew @ Victoria university.
                    // Signed off: Michael Towsey 27th July 2012
                    //{ "kiwiROC", LSKiwiROC.Main },

                    // ########### ANALYSES UNDER DEVELOPMENT - OUTPUT NOT GUARANTEED ###########

                    // acoustic event detection
                    //{ "aed", AED.Dev },

                    // extract an acoustic event and make a template for FELT
                    //{ "createtemplate_felt", FeltTemplate_Create.Dev },

                    // edits the FELT template created above
                    //{ "edittemplate_felt", FeltTemplate_Edit.Dev },

                    // event pattern recognition - used for ground-parrots (BRAD)
                    // { "epr", GroundParrotRecogniser.Dev },

                    // event pattern recognition - used for ground-parrots (TOWSEY)
                    // { "epr2", EPR.Dev },

                    // find other acoustic events like this
                    //{ "felt", FeltTemplates_Use.Dev },

                    // anthony's attempt at FELT
                    // this runs his suggestion tool, and the actual FELT analysis
                    //{ "truskinger.felt", strings => FELT.Runner.Main.ProgramEntry(strings) },

                    // Xueyan's FELT
                    //{ "dong.felt", FeltAnalysis.Dev },

                    // frog calls
                    //{ "frog_ribbit", FrogRibit.Dev },

                    // IAnalyser - detects Gastric Brooding Frog
                    //{ "frogs", Frogs.Dev },

                    // grid recognition
                    //{ "gratings", GratingDetection.Dev },

                    // Oscillation Recogniser
                    //{ "od", OscillationRecogniser.Dev },

                    // Production Analysis runs - for running on mono or to run as fast as possible
                    //{ "production", Runner.Run },

                    // IAnalyser - detects rain
                    //{ "rain", Rain.Dev },

                    // IAnalyser - detects Gastric Brooding Frog
                    //{ "rheobatrachus", RheobatrachusSilus.Dev },

                    // segmentation of a recording
                    //{ "segment", Segment.Dev },

                    // species accumulation curves
                    // SpeciesAccumulationCurve.Executable
                    //{ "species_accumulation_curves", SpeciesAccumulationCurve.Dev },
                    
                    // spectral peak tracking
                    /////{ "spt", SPT.Dev },

                    // syntactic pattern recognition
                    ////{ "spr", SPR.Dev },

                    // A template for producing IAnalysis classes.
                    //{ "test", AnalysisTemplate.Dev },

                    // a investigation into sammon projections
                    //{ "sammon_projection", SammonProgram.Dev },

                    // Michael's play area
                    //{ "sandpit", Sandpit.Dev },
                };
        }*/



        public static int Main(string[] args)
        {
            Copyright();

            AttachExceptionHandler();

            NoConsole.Log.Info("Executable called with these arguments: {1}{0}{1}".Format2(Environment.CommandLine, Environment.NewLine));

            // HACK: Remove the following two line when argument refactoring is done
            var options = DebugOptions.Yes;
            AttachDebugger(ref options);

            Arguments = ParseArguments(args);

            var debugOptions = Arguments.Args.DebugOption;
            AttachDebugger(ref debugOptions);

            // note: Exception handling can be found in CurrentDomainOnUnhandledException
            Execute(Arguments);

            /*ATA

            
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

                    // execute the analysis
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
            */
            HangBeforeExit();

            // finally return error level
            NoConsole.Log.Info("ERRORLEVEL: " + ExceptionLookup.Ok);
            return ExceptionLookup.Ok;
        }

        private static void AudioFileInfo(string[] args)
        {
            IEnumerable<string> files = null;

            var outputFile = new FileInfo(args[0]);
            var input = args[1];


            if (Directory.Exists(input))
            {
                // three args: 
                // first is output file path
                // second is whether to recurse or not
                // third is dir containing audio files
                // e.g. AnalysisPrograms.exe "<dir path>" recurse "<output file path>"
                // e.g. AnalysisPrograms.exe "<dir path>" norecurse "<output file path>"

                var recurse = args[2] == "recurse";

                files = Directory.EnumerateFiles(input, "*.*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            }
            else if (File.Exists(input))
            {
                // at least two args:
                // first is output file path
                // second is text file path containing audio file paths (one per line)

                // skip the output file
                files = File.ReadLines(input).Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Trim(' ', '"')).Distinct().OrderBy(l => l);
            }

            var mau = new MasterAudioUtility();
            var stopwatch = new Stopwatch();

            var headers = "\"" + string.Join("\", \"",
                "SourceFile",
                "SampleRate (hertz)",
                "BitsPerSecond",
                "BitsPerSample",
                "ChannelCount",
                "Duration (sec)",
                "MediaType",
                "FileSize (bytes)",
                "SHA256 Hash",
                "Identifier") + "\"";

            using (var fs = File.Open(outputFile.FullName, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var sw = new StreamWriter(fs))
            {
                sw.WriteLine(headers);

                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);

                        stopwatch.Restart();
                        var info = mau.Info(fileInfo);
                        stopwatch.Stop();

                        var infoTime = stopwatch.Elapsed;

                        stopwatch.Restart();
                        var hash = SHA256Hash(fileInfo);
                        stopwatch.Stop();

                        Console.WriteLine("info: {1} hash: {2} for {0}.", fileInfo.Name, infoTime, stopwatch.Elapsed);

                        var output = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}",
                            CsvSafeString(info.SourceFile != null ? info.SourceFile.ToString() : string.Empty),
                            CsvSafeString(info.SampleRate.HasValue ? info.SampleRate.Value.ToString() : string.Empty),
                            CsvSafeString(info.BitsPerSecond.HasValue ? info.BitsPerSecond.Value.ToString() : string.Empty),
                            CsvSafeString(info.BitsPerSample.HasValue ? info.BitsPerSample.Value.ToString() : string.Empty),
                            CsvSafeString(info.ChannelCount.HasValue ? info.ChannelCount.Value.ToString() : string.Empty),
                            CsvSafeString(info.Duration.HasValue ? info.Duration.Value.TotalSeconds.ToString() : string.Empty),
                            CsvSafeString(info.MediaType),
                            CsvSafeString(info.SourceFile.Length.ToString()),
                            CsvSafeString(hash),
                            GetIdentifierFromPath(info.SourceFile.FullName)
                            );

                        sw.WriteLine(output);

                        sw.Flush();
                        fs.Flush();
                    }
                    catch (Exception ex)
                    {
                        if (Log.IsWarnEnabled)
                        {
                            Log.Warn("Error processing " + file, ex);
                        }
                    }
                }
            }

        }

        private static string CsvSafeString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                value = string.Empty;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static string SHA256Hash(FileInfo file)
        {
            var filePath = file.FullName;

            // see http://stackoverflow.com/questions/1177607/what-is-the-fastest-way-to-create-a-checksum-for-large-files-in-c-sharp/1177744#1177744
            // for info about the buffer size

            // buffer of 1.2gb
            var bufferSize = 1200000;

            //using (FileStream stream = File.OpenRead(filePath))
            using (var stream = new BufferedStream(File.OpenRead(filePath), bufferSize))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty).ToLowerInvariant();
            }
        }

        private static Guid GetIdentifierFromPath(string path)
        {
            var fileName = Path.GetFileName(path);
            if (fileName.IndexOf('_') > -1)
            {
                var guid = fileName.Substring(0, fileName.IndexOf('_'));

                Guid result;
                if (Guid.TryParse(guid, out result))
                {
                    return result;
                }
            }

            return Guid.Empty;
        }

        /*ATA
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
        }*/
    }
}
