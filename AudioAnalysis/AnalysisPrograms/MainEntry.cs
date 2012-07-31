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
    using System.Linq;

    using AnalysisPrograms.Production;

    /// <summary>
    /// Main Entry for Analysis Programs.
    /// </summary>
    public class MainEntry
    {
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

            if (args.Length == 0)
            {
                Console.WriteLine("ERROR: You have called the AanalysisPrograms.MainEntry() method without command line arguments.");
            }
            else
            {
                string[] restOfArgs = args.Skip(1).ToArray();
                switch (args[0])
                {
                    // READY TO BE USED - REQUIRE CONFIG FILE
                    case "aed":           // acoustic event detection
                        AED.Dev(restOfArgs);
                        break;
                    case "analysesAvailable":           // acoustic event detection
                        AnalysesAvailable.Main(restOfArgs);
                        break;
                    case "canetoad":      // IAnalyser - detects canetoad calls as acoustic events
                        Canetoad.Dev(restOfArgs);  //Signed off: Michael Towsey 27th July 2012
                        break;
                    case "audio2csv":     // extracts acoustic indices from an audio recording (mp3 or wav) and prodcues a indices.csv file
                        AnalyseLongRecording.Main(restOfArgs);   //Signed off: Michael Towsey 27th July 2012
                        break;
                    case "audio2sonogram":        // produces a sonogram from an audio file. Can reduce dimensionality of the image.
                        Audio2Sonogram.Main(restOfArgs);   //Signed off: Michael Towsey 31st July 2012
                        break;
                    case "crow":               // IAnalyser - recognises the short crow "caw" - NOT the longer sigh.
                        Crow.Dev(restOfArgs);  //Signed off: Michael Towsey 27th July 2012
                        break;
                    case "indicesCsv2Image":        // produces a tracks image of column values in a csv file - one track per csv column.
                        IndicesCsv2Display.Main(restOfArgs);  //Signed off: Michael Towsey 27th July 2012
                        break;
                    case "epr":           // event pattern recognition - used for ground-parrots (BRAD)
                        GroundParrotRecogniser.Dev(restOfArgs);
                        break;
                    case "koalaMale":     // IAnalyser - detects the oscillating portion of a male koala bellow
                        KoalaMale.Dev(restOfArgs);  //Signed off: Michael Towsey 27th July 2012
                        break;
                    case "multiAnalyser": // IAnalyser - currently recognizes five different calls: human, crow, canetoad, machine and koala.
                        MultiAnalyser.Dev(restOfArgs);  //Signed off: Michael Towsey 27th July 2012
                        break;
                    case "human":         // IAnalyser - recognises human speech but not word recognition
                        Human1.Dev(restOfArgs);  //Signed off: Michael Towsey 27th July 2012
                        break;
                    case "kiwi":          // IAnalyser - little spotted kiwi calls from Andrew @ Victoria university. Versions 1 and 2 are obsolete.
                        LSKiwi3.Dev(restOfArgs);  //Signed off: Michael Towsey 27th July 2012
                        break;
                    case "kiwiROC":       // little spotted kiwi calls from Andrew @ Victoria university.
                        LSKiwiROC.Main(restOfArgs);  //Signed off: Michael Towsey 27th July 2012
                        break;
                    case "LewinsRail":    // IAnalyser - LewinsRail3 - yet to be tested on large data set but works OK on one or two available calls.
                        LewinsRail3.Dev(restOfArgs);  //Signed off: Michael Towsey 27th July 2012
                        break;
                    case "machines":      // IAnalyser - recognises Planes, Trains And Automobiles - works OK for planes not yet tested on train soun 
                        PlanesTrainsAndAutomobiles.Dev(restOfArgs);  //Signed off: Michael Towsey 27th July 2012
                        break;
                    case "snr":           // calculates signal to noise ratio
                        SnrAnalysis.Dev(restOfArgs);  //Signed off: Anthony, 25th July 2012
                        break;



                    // DEVELOPMENT PURPOSES ONLY - FOR MICHAEL'S USE
                    case "acousticIndices":      // extracts acoustic indices from one minute segment - for dev purposes only
                        Acoustic.Dev(restOfArgs);  //Signed off: Michael Towsey, 27th July 2012
                        break;
                    case "createtemplate_felt":  // extract an acoustic event and make a template for FELT
                        FeltTemplate_Create.Dev(restOfArgs);
                        break;
                    case "edittemplate_felt":    // edits the FELT template created above
                        FeltTemplate_Edit.Dev(restOfArgs);
                        break;
                    case "epr2":                 // event pattern recognition - used for ground-parrots (TOWSEY)
                        EPR.Dev(restOfArgs);
                        break;
                    case "felt":                 // find other acoustic events like this
                        FeltTemplates_Use.Dev(restOfArgs);
                        break;
                    case "frog_ribbit":          // frog calls
                        FrogRibit.Dev(restOfArgs);
                        break;
                    case "gratings":      // grid recognition
                        GratingDetection.Dev(restOfArgs);
                        break;
                    case "od":   // Oscillation Recogniser
                        OscillationRecogniser.Dev(restOfArgs);
                        break;
                    case "segment":  // segmentation of a recording
                        Segment.Dev(restOfArgs);
                        break;
                    case "species_accumulation_curves":      // species accumulation curves
                        SpeciesAccumulationCurve.Dev(restOfArgs);
                        //SpeciesAccumulationCurve.Executable(restOfArgs);
                        break;
                    //case "spt":  // spectral peak tracking
                    //    SPT.Dev(restOfArgs);
                    //    break;
                    //case "spr":  // syntactic pattern recognition
                    //    SPR.Dev(restOfArgs);
                    //    break;
                    case "test":      //
                        AnalysisTemplate.Dev(restOfArgs);
                        break;



                    // Production Analysis runs
                    case "production": // for running on mono or to run as fast as possible
                        Runner.Run(restOfArgs);
                        break;

                    default:
                        Console.WriteLine("Analysis option unrecognised>>>" + args[0]);
                        Console.WriteLine("Press any key to exit...");
                        Console.ReadLine();
                        break;
                }
            }
        }
    }
}
