// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioConversion.cs" company="MQUTeR">
//   -
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Processing
{
    using System;
    using System.IO;
    using System.Reflection;

    using AudioTools;

    /// <summary>
    /// The audio conversion.
    /// </summary>
    internal class AudioConversion
    {
        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="args">
        /// Working directory.
        /// </param>
        public static void Convert(string[] args)
        {
            Write(
                "File Name, Source, BitsPerSample, BytesPerSample, Channels, Epsilon, SampleRate, SampleCount, Time",
                true,
                true);

            var dir = args[0];
            foreach (var path in Directory.GetFiles(dir, "*"))
            {
                var file = new FileInfo(path);
                Write(file.Name);

                UseWavReader(file);
                UseAudioInfo(file);
                UseDSGetDuration(file);
            }

            Console.ReadLine();
        }

        private static void UseWavReader(FileInfo file)
        {
            Write("WavReader");

            try
            {
                using (var reader = new WavReader(file.FullName))
                {
                    Write(reader.BitsPerSample.ToString());
                    Write(reader.BytesPerSample.ToString());
                    Write(reader.Channels.ToString());
                    Write(reader.Epsilon.ToString());
                    Write(reader.SampleRate.ToString());
                    Write(reader.Samples.Length.ToString());
                    Write(reader.Time.ToString(), true, false);
                }
            }
            catch
            {
                Write("WavReader could not read file.");
            }
        }

        private static void UseAudioInfo(FileInfo file)
        {
            Write("AudioInfo");

            try
            {
                var audioInfo = DShowConverter.GetAudioInfo(file.FullName, null);

                Write(audioInfo.BitsPerSample.ToString());
                Write(audioInfo.BytesPerSample.ToString());
                Write(audioInfo.Channels.ToString());
                Write(string.Empty);
                Write(audioInfo.SamplesPerSecond.ToString());
                Write(audioInfo.SampleCount.ToString());
            }
            catch
            {
                Write("AudioInfo could not read file.");
            }
        }

        private static void UseDSGetDuration(FileInfo file)
        {
            Write("DShowConverter.GetDuration");

            try
            {
                var duration = DShowConverter.GetDuration(file.FullName, null);

                Write(duration.ToString(), true, false);
            }
            catch
            {
                Write("DShowConverter.GetDuration could not read file.");
            }
        }

        private static void Write(string data, bool appendNewLine, bool overwriteFile)
        {
            Console.WriteLine(data);

            var path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "file_test_logging.txt");

            if (appendNewLine)
            {
                data += Environment.NewLine;
            }

            if (overwriteFile)
            {
                File.WriteAllText(path, data);
            }
            else
            {
                File.AppendAllText(path, data);
            }
        }

        private static void Write(string data)
        {
            Write(data + ", ", false, false);
        }

        public static void Segment(string audioFilePath)
        {

        }
    }
}