using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnalysisPrograms.Processing;

namespace AnalysisPrograms
{
    class MainEntry
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("ERROR: YOU HAVE CALLED THE AanalysisPrograms.MainEntry() method without command line arguments.");
            }
            else
            {
                string[] r = args.Skip(1).ToArray();
                switch (args[0])
                {
                    case "segment":  // segmentation of a recording
                        Segment.Dev(r);
                        break;
                    case "snr":      // signal to noise ratio
                        SnrAnalysis.Dev(r);
                        break;
                    case "aed":      // acoustic event detection
                        AED.Dev(r);
                        break;


                    case "od":   // Oscillation Recogniser
                        OscillationRecogniser.Dev(r);
                        break;
                    case "hd":   // Harmonic Recogniser
                        HarmonicRecogniser.Dev(r);
                        break;
                    case "epr":  // event pattern recognition - used for ground-parrots
                        GroundParrotRecogniser.Dev(r);
                        break;
                    case "spt":  // spectral peak tracking, currently not used
                        Spt.Dev(r);
                        break;
                    case "mfcc-od": // special use of MFCCs and OD for calls haveing oscillating character ie Lewin's Rail
                        MFCC_OD.Dev(r);
                        break;
                    case "htk":  // run an HTK template over a recording
                        HTKRecogniser.Dev(r);
                        break;
                    case "createtemplate_mfccod": // Create a template that extracts mfccs and uses OD. Used for Lewin's Rail recognition
                        Create_MFCC_OD_Template.Dev(r);
                        break;
                    case "processing": // for running on the processing cluster
                        ProcessingUtils.Run(r);
                        break;
                    case "localrun": // audio conversion tests
                        AudioConversion.Convert(r);
                        break;
                    default:
                        Console.WriteLine("Analysis option unrecognised");
                        Console.ReadLine();
                        break;
                }
            }
        }
    }
}
