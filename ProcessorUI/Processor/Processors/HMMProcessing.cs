using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using AudioAnalysis;
using AudioTools;
using QutSensors.Processor.WebServices;
using QutSensors.Data;

namespace QutSensors.Processor
{
    public class HMMProcessing : Processor
    {
        public HMMProcessing(ProcessorSettings settings)
            : base(settings)
        {
        }

        public override IEnumerable<ProcessorJobItemResult> Process(TempFile inputFile, ProcessorJobItemDescription item, out TimeSpan? duration)
        {
            // Phase 1.0 - will be refactored post demo

            // get duration
            duration = DShowConverter.GetDuration(inputFile.FileName, item.MimeType);
            if (duration == null)
            {
                OnLog("Unable to calculate length");
                throw new Exception("Unable to calculate length");
            }

            // settings file defines which label or labels we are interested in (Labels=foo,bar)
            string[] desiredLabels = (settings["Labels"] ?? "").Split(',');
            int? threshold = settings.HasSetting("Threshold") ? (int?)int.Parse(settings["Threshold"]) : null;

            // process the file
            using (var temporaryDirectory = new TempDirectory()) { 
            using (var tempModelFile = new TempFile(".zip")) {
            using (var converted = DShowConverter.ConvertTo(inputFile.FileName, item.MimeType, MimeTypes.WavMimeType, null, null) as BufferedDirectShowStream)
            {
                File.WriteAllBytes(tempModelFile.FileName, (byte[])item.Job.Parameters.BinaryDeserialize());

                HMMBuilder.Program.Execute(
                    temporaryDirectory.DirectoryName,       // temporary directory
                    tempModelFile.FileName,                 // model file
                    converted.BufferFile.FileName           // input file
                );

                // output we are interested in will be in [x axis time component];
                // -> temporaryDirectory\results\TestScan.mlf
                //      #!MLF!#
                //      "filename"
                //      <start time in 100ns> <end time in 100ns> <label> <score>
                //      (above line repeated for each match)
                //      .
                //
                //      Need to look through the match lines and find the corresponding label (or labels) we are interested in
                //
                // -> temporaryDirectory\<tempModelFile>\mfccConfig [y axis frequency component]
                //      LOFREQ = <n>
                //      HIFREQ = <n>

                // identify frequency band (same for all labels)
                string tempModelFileBase = new FileInfo(tempModelFile.FileName).Name;
                string mfccConfigFile = String.Format("{0}\\{1}\\mfccConfig", temporaryDirectory.DirectoryName, tempModelFileBase.Substring(0, tempModelFileBase.Length - 4));
                int minFrequency = 800;
                int maxFrequency = 8000;
                if (File.Exists(mfccConfigFile))
                {
                    string[] lines = File.ReadAllLines(mfccConfigFile);
                    foreach (string line in lines)
                    {
                        string[] tokens = line.Split('=');
                        if (tokens.Length != 2)
                            continue;

                        string key = tokens[0].Trim();
                        string val = tokens[1].Trim();

                        if (key.ToLower().Equals("lofreq"))
                            minFrequency = int.Parse(val);
                        else if (key.ToLower().Equals("hifreq"))
                            maxFrequency = int.Parse(val);
                    }
                }

                // identify the matches
                var events = new List<AcousticEvent>();
                string mlfFile = String.Format("{0}\\results\\TestScan.mlf", temporaryDirectory.DirectoryName);
                if (!File.Exists(mlfFile))
                    throw new Exception("No MLF output file");

                Regex matchRegex = new Regex(@"^([0-9]+)\s+([0-9]+)\s+([^\s]+)\s+([0-9.\-]+)\s*$");
                foreach (string line in File.ReadAllLines(mlfFile))
                {
                    Match match;
                    if ((match = matchRegex.Match(line)).Success)
                    {
                        string matchedLabel = match.Groups[3].Value;

                        if (desiredLabels.Contains(matchedLabel))
                        {
                            TimeSpan start = TimeSpan.FromTicks(long.Parse(match.Groups[1].Value));
                            TimeSpan end = TimeSpan.FromTicks(long.Parse(match.Groups[2].Value));
                            double score = double.Parse(match.Groups[4].Value);

                            if (threshold.HasValue && score > threshold.Value)
                            {
                                events.Add(
                                    new AcousticEvent(
                                        start.TotalSeconds,
                                        end.Subtract(start).TotalSeconds,
                                        minFrequency,
                                        maxFrequency
                                    )
                                );
                            }
                        }
                    }
                }

                // mangle the output into something we can put into the database
                StringReader reader = new StringReader(ResultSerializer.SerializeHMMResult(events).InnerXml);
                XElement element = XElement.Load(reader);

                var result = new List<ProcessorJobItemResult>();
                result.Add(new ProcessorJobItemResult()
                {
                    Start = 0,
                    Stop = (int)duration.Value.TotalMilliseconds,
                    Results = element,

                    // for the time being we will rank on event count (occurance of particular label/labels however we should
                    // probabably change this to take into account match score (in addition or in place of)
                    RankingScoreValue = events.Count,
                    RankingScoreName = "Event Count",
                    RankingScoreLocation = 0.0
                });
                return result;
            }
            }
            }
        }
    }
}
