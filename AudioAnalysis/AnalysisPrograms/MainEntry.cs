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
                Console.WriteLine("ERROR: YOU HAVE CALLED THE AanalysisPrograms.MainEntry() method without command line arguments.");
            }
            else
            {
                string[] restOfArgs = args.Skip(1).ToArray();
                switch (args[0])
                {
                    // READY TO BE USED - REQUIRE PARAMS FILE ONLY
                    case "segment":  // segmentation of a recording
                        Segment.Dev(restOfArgs);
                        break;
                    case "snr":      // signal to noise ratio
                        SnrAnalysis.Dev(restOfArgs);
                        break;
                    case "aed":      // acoustic event detection
                        AED.Dev(restOfArgs);
                        break;
                    case "od":   // Oscillation Recogniser
                        OscillationRecogniser.Dev(restOfArgs);
                        break;
                    case "epr":  // event pattern recognition - used for ground-parrots
                        GroundParrotRecogniser.Dev(restOfArgs);
                        break;
                    case "spt":  // spectral peak tracking
                        SPT.Dev(restOfArgs);
                        break;
                    case "spr":  // syntactic pattern recognition
                        SPR.Dev(restOfArgs);
                        break;
                    case "hd":   // Harmonic Recogniser
                        HarmonicRecogniser.Dev(restOfArgs);
                        break;

                    // READY TO BE USED - REQUIRE PARAMS FILE AND ZIPPED RESOURCES FILE.
                    case "mfcc-od": // special use of MFCCs and OD for calls haveing oscillating character ie Lewin's Rail
                        MFCC_OD.Dev(restOfArgs);
                        break;
                    case "htk":  // run an HTK template over a recording
                        HTKRecogniser.Dev(restOfArgs);
                        break;

                    // UNDER DEVELOPMENT
                    case "dimred":  // dimensionality reduction
                        DimReduction.Dev(restOfArgs);
                        break;
                    case "felt":     // find other acoustic events like this
                        FindEventsLikeThis.Dev(restOfArgs);
                        break;


                    // Analysis development - FOR MICHAEL'S USE ONLY
                    case "createtemplate_mfccod": // Create a template that extracts mfccs and uses OD. Used for Lewin's Rail recognition
                        Create_MFCC_OD_Template.Dev(restOfArgs);
                        break;
                    case "createtemplate_felt":   // extract an acoustic event and make a template for FELT
                        CreateFeltTemplate.Dev(restOfArgs);
                        break;


                    // Analysis runs - FOR MARK'S USE ONLY
                    case "processing": // for running on the processing cluster
                        ProcessingUtils.Run(restOfArgs);
                        break;
                    case "localrun": // audio conversion tests
                        AudioConversion.Convert(restOfArgs);
                        break;
                    default:
                        Console.WriteLine("Analysis option unrecognised>>>" + args[0]);
                        Console.ReadLine();
                        break;
                }
            }
        }
    }
}
