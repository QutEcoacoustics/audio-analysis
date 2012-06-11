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
    using System.Configuration;
    using System.IO;
    using System.Linq;

    using AnalysisPrograms.Processing;

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
            if (args.Length == 0)
            {
                Console.WriteLine("ERROR: You have called the AanalysisPrograms.MainEntry() method without command line arguments.");
            }
            else
            {
                string[] restOfArgs = args.Skip(1).ToArray();
                switch (args[0])
                {
                    // READY TO BE USED - REQUIRE PARAMS FILE ONLY
                    case "aed":      // acoustic event detection
                        AED.Dev(restOfArgs);
                        break;
                    case "crow":     // recognises uhman speech but not word recognition
                        Crow.Dev(restOfArgs);
                        break;
                    case "epr":  // event pattern recognition - used for ground-parrots (BRAD)
                        GroundParrotRecogniser.Dev(restOfArgs);
                        break;
                    case "gratings":  // grid recognition
                        GratingDetection.Dev(restOfArgs);
                        break;
                    //case "grids":  // grid recognition
                    //    BarsAndStripes.Dev(restOfArgs);
                    //    break;
                    case "harmonics":   // general harmonics recogniser
                        Harmonics.Dev(restOfArgs);
                        break;
                    case "hd":   // Harmonic Recogniser
                        HarmonicRecogniser.Dev(restOfArgs);
                        break;
                    case "human":     // recognises uhman speech but not word recognition
                        Human2.Dev(restOfArgs);
                        break;
                    case "kiwi":  // little spotted kiwi calls from Andrew @ Victoria university.
                        LSKiwi2.Dev(restOfArgs);
                        break;
                    case "LewinsRail":  //LewinsRail3
                        LewinsRail3.Dev(restOfArgs);
                        break;
                    case "machines":     // recognises Planes, Trains And Automobiles 
                        PlanesTrainsAndAutomobiles.Dev(restOfArgs);
                        break;
                    case "od":   // Oscillation Recogniser
                        OscillationRecogniser.Dev(restOfArgs);
                        break;
                    case "segment":  // segmentation of a recording
                        Segment.Dev(restOfArgs);
                        break;
                    case "snr":      // signal to noise ratio
                        SnrAnalysis.Dev(restOfArgs);
                        break;
                    case "test":      //
                        AnalysisTemplate.Dev(restOfArgs);
                        break;



                    // UNDER DEVELOPMENT - FOR MICHAEL'S USE ONLY
                    //case "createtemplate_mfccod": // Create a template that extracts mfccs and uses OD. Used for Lewin's Rail recognition
                    //    Create_MFCC_OD_Template.Dev(restOfArgs);
                    //    break;
                    //case "BarsAndStripes":     // recognises Planes, Trains And Automobiles 
                    //    BarsAndStripes.Dev(restOfArgs);
                    //    break;                       
                    case "felt":     // find other acoustic events like this
                        FeltTemplates_Use.Dev(restOfArgs);
                        break;
					case "createtemplate_felt":   // extract an acoustic event and make a template for FELT
                        FeltTemplate_Create.Dev(restOfArgs);
                        break;
                    case "edittemplate_felt":     // edits the FELT template created above
                        FeltTemplate_Edit.Dev(restOfArgs);
                        break;
                    case "frog_ribbit":  // frog calls
                        FrogRibit.Dev(restOfArgs);
                        break;
                    case "spt":  // spectral peak tracking
                        SPT.Dev(restOfArgs);
                        break;
                    case "spr":  // syntactic pattern recognition
                        SPR.Dev(restOfArgs);
                        break;
                    case "richness_indices":      // richness_indices
                        AcousticIndices.Dev(restOfArgs);
                        //RichnessIndices2.Executable(restOfArgs);
                        break;
                    case "species_accumulation_curves":      // species accumulation curves
                        SpeciesAccumulationCurve.Dev(restOfArgs);
                        //SpeciesAccumulationCurve.Executable(restOfArgs);
                        break;
                    case "dimred":   // dimensionality reduction
                        DimReduction.Dev(restOfArgs);
                        break;
                    case "epr2": // event pattern recognition - used for ground-parrots (TOWSEY)
                        EPR.Dev(restOfArgs);
                        break;

                    // Analysis runs - FOR MARK'S USE ONLY
                    case "processing": // for running on the processing cluster
                        ProcessingUtils.Run(restOfArgs);
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
