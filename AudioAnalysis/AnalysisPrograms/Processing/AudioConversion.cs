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

            Write("File Name, Source, BitsPerSample, BytesPerSample, Channels, Epsilon, SampleRate, SampleCount, Time", true, true);


            string dir = args[0];
            foreach (var path in Directory.GetFiles(dir,"*.ogg"))
            {
                using (var reader = new WavReader(path))
                {
                    Write(path);
                    Write("WavReader");
                    Write(reader.BitsPerSample.ToString());
                    Write(reader.BytesPerSample.ToString());
                    Write(reader.Channels.ToString());
                    Write(reader.Epsilon.ToString());
                    Write(reader.SampleRate.ToString());
                    Write(reader.Samples.Length.ToString());
                    Write(reader.Time.ToString(), true, false);
                }


                Write(path);

                var audioInfo = DShowConverter.GetAudioInfo(path, null);

                Write("AudioInfo");
                Write(audioInfo.BitsPerSample.ToString());
                Write(audioInfo.BytesPerSample.ToString());
                Write(audioInfo.Channels.ToString());
                Write(string.Empty);
                Write(audioInfo.SamplesPerSecond.ToString());
                Write(audioInfo.SampleCount.ToString());

                var duration = DShowConverter.GetDuration(path, null);

                Write(duration.ToString(), true, false);


            }

            Console.ReadLine();
        }

        private static void Write(string data, bool appendNewLine, bool overwriteFile)
        {
            var path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "file_test_logging.txt");

            if (appendNewLine) data += Environment.NewLine;

            if (overwriteFile) File.WriteAllText(path, data);
            else File.AppendAllText(path, data);
        }

        private static void Write(string data)
        {
            Write(data + ", ", false, false);
        }
    }
}
