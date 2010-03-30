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
                    case "aed":  //acoustic event detection
                        AED.Dev(r);
                        break;
                    case "od":   //Oscillation Recogniser
                        OscillationRecogniser.Dev(r);
                        break;
                    case "hd":   //Harmonic Recogniser
                        HarmonicRecogniser.Dev(r);
                        break;
                    case "groundparrot":
                        GroundParrotRecogniser.Dev(r); //event pattern recognition
                        break;
                    case "snr":   //signal to noise ratio
                        SnrAnalysis.Dev(r);
                        break;
                    case "htk":   //run an HTK template over a recording
                        HTKRecogniser.Dev(r);
                        break;
                    case "spt": // spectral peak tracking, currently not used
                        Spt.Dev(r);
                        break;
                    case "kekkek": // special use of MFCCs and OD for Lewin's Rail recognition
                        OD_KekKek.Dev(r);
                        break;
                    case "processing": // for running on the processing cluster
                        ProcessingUtils.Run(r);
                        break;
                    case "localrun":
                        AudioConversion.Convert(r);
                        break;
                    default:
                        Console.WriteLine("unrecognised");
                        break;
                }
            }
        }
    }
}
