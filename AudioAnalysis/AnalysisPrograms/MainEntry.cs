using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms
{
    class MainEntry
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage");
            }
            else
            {
                string[] r = args.Skip(1).ToArray();
                switch (args[0])
                {
                    case "groundparrot":
                        GroundParrotRecogniser.Main(r);
                        break;
                    case "snr":
                        Main_SNR.Main(r);
                        break;
                    case "canetoad":
                        CaneToadAnalysis.Manage_CaneToadRecogniser(r);
                        break;

                    default:
                        Console.WriteLine("unrecognised");
                        break;
                }
            }
        }
    }
}
