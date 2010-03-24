using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioTools;
using System.IO;

namespace AnalysisPrograms.Processing
{
    internal class AudioConversion
    {
        public static void Convert(string[] args)
        {


            string dir = args[0];
            foreach (var path in Directory.GetFiles(dir))
            {
                Console.WriteLine("File: " + path);
                
                using (var reader = new WavReader(path))
                {
                    Console.WriteLine(reader.BitsPerSample);
                    Console.WriteLine(reader.BytesPerSample);
                    Console.WriteLine(reader.Channels);
                    Console.WriteLine(reader.Epsilon);
                    Console.WriteLine(reader.SampleRate);
                    Console.WriteLine(reader.Samples.Length);
                    Console.WriteLine(reader.Time);
                }
                Console.WriteLine();
            }


        }
    }
}
