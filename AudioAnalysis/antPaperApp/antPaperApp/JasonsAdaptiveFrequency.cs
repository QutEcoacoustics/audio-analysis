using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace antPaperApp
{
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Count = Int32;
    using Minute = Int32;


    /// <summary>
    /// THIS is an adaptive sampling algorithm.
    /// It has a random element in it, and hence its results are repeated a thousand times.
    /// THis algorithm choose the training samples with most unique calls first.
    /// The minute chosen, is then evaluated against ONE test sample (which has to be randomly chosen from the remaining test sites/days)
    /// </summary>
    public class JasonsAdaptiveFrequency
    {

        public JasonsAdaptiveFrequency(DirectoryInfo training, DirectoryInfo test, DirectoryInfo output, int variance = 1)
        {
            Contract.Requires(training.Exists);
            Contract.Requires(test.Exists);
            Contract.Requires(variance >= 1);

            // load and parse files

            // these are site wide analyses
            // i.e. sites are the repeatable experiment

            var trainingProfiles = Helpers.ReadFiles(training);

            var testProfiles = Helpers.ReadFiles(test);

            // record all the different "days" and "sites" we get in testData
            var testDayCombos = JasonsAdaptiveBit.SiteDayCombos(testProfiles);


            // levels of testing to do
            var numSamples = Program.LevelsOfTestingToDo;
            var outputFiles = new Dictionary<int, StringBuilder>();
            foreach (var numSample in numSamples)
            {
                outputFiles.Add(numSample, new StringBuilder());
            }

            const int RandomRuns = 100;

            Parallel.For(
                (long)0,
                RandomRuns,
                Program.ParallelOptions,
                (runIndex, loopState) =>
                    {

                        foreach (var testRun in numSamples)
                        {
                            
                            // eventually we will pick #testRun random samples
                            var samplesChosen = new List<Tuple<int, string, DateTime, Count>>(testRun);
                            var speciesFoundInTestData = new HashSet<string>();

                            // so sample from test data ordered on number of unique 
                            // species found in equivalent training minute
                            // i.e. min 300 in training, has 25 unique species, pick min 300 from test data (there will be serveral matching minutes, pick only one)
                            // however we do it adaptively based on the species we "discover" in test profiles.

                            List<SiteDaySpeciesProfile> adaptiveFilter = trainingProfiles;
                            for (int sample = 0; sample < testRun; sample++)
                            {
                                // first remove training samples where we have already found that species
                                adaptiveFilter =
                                    adaptiveFilter.Where(sdp => !speciesFoundInTestData.Contains(sdp.SpeciesName)).ToList();

                                // for the first iteration, none should be removed
                                Contract.Assert(sample > 0 || adaptiveFilter.Count() == trainingProfiles.Count());

                                // then form super profile from remaining profiles
                                // IMPORTANT  - the all set will sum and not AND

                                // group into sites and days
                                var allset = JasonsAdaptiveBit.MakeAllSet(adaptiveFilter, true);

                                // ORDER samples based on the most unique species occuring
                                var orderedUniqueSpeciesCount = allset.OrderByDescending(m => m.Value).ToList();

                                // choose the first sample with the most unique species
                                var trainingChoice = orderedUniqueSpeciesCount.First();

                                // or if a variance is set, choose a sample N randomly within the top V species
                                if (variance > 1)
                                {
                                    trainingChoice = orderedUniqueSpeciesCount.Take(variance).GetRandomElement();
                                }


                                // there could be multiple results from that minute, randomly choose one
                                // if each profile is a species * day * site tuple
                                // we want to filter down to one site and one day
                                var randomTestDay = testDayCombos.GetRandomElement();
                                var randomSite = randomTestDay.Item1;
                                var randomDay = randomTestDay.Item2;

                                samplesChosen.Add(Tuple.Create(trainingChoice.Key, randomSite, randomDay, trainingChoice.Value));



                                // now we have our sample,
                                // we do our evaluation
                                // we take the minute from the chosen training sample
                                // and grab all the unique species from the test data
                                var restrictedTestProfiles =
                                    testProfiles.Where(sdp => sdp.Site == randomSite && sdp.Day == randomDay).ToArray();

                                Contract.Assert(restrictedTestProfiles.All(sdp => sdp.Day == restrictedTestProfiles.First().Day));
                                Contract.Assert(restrictedTestProfiles.All(sdp => sdp.Site == restrictedTestProfiles.First().Site));

                                foreach (var sdp in restrictedTestProfiles)
                                {
                                    if (sdp.MinuteProfile[trainingChoice.Key] == 1)
                                    {
                                        speciesFoundInTestData.Add(sdp.SpeciesName);
                                    }
                                }

                                // loop back, try again
                            }

                            var foundCount = speciesFoundInTestData.Count;
                            if (foundCount == 0)
                            {
                                speciesFoundInTestData.Add("[[[NONE!]]]");
                            }

                            // make a summary result
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("*** Jason's unique frequency (RUN NUMBER: " + runIndex + " )");
                            sb.AppendFormat("NumSamples,{0}\n", testRun);
                            sb.AppendLine(
                                "Species Found," + speciesFoundInTestData.Aggregate((build, current) => build + "," + current));
                            sb.AppendLine(
                                "Species Found count," + foundCount.ToString(CultureInfo.InvariantCulture));
                            sb.AppendLine("Minutes sampled (minute, site, day, #numTrainingSpecies)"
                                +
                                samplesChosen.Aggregate(
                                    "",
                                    (build, current) =>
                                    build
                                    +
                                    string.Format(
                                        ", ({0}, {1}, {2}, {3})",
                                        current.Item1,
                                        current.Item2,
                                        current.Item3.ToShortDateString(),
                                        current.Item4)
                                        ));

                            lock (outputFiles)
                            {
                                outputFiles[testRun].AppendLine(sb.ToString());
                            }
                            
                        }
                    }); // end 1000 loops


            foreach (var stringBuilder in outputFiles)
            {
                File.AppendAllText(output + "\\" + stringBuilder.Key + ".txt", stringBuilder.Value.ToString());
            }

        }

        public class AggregatedSiteDayMinute
        {
            public DateTime Day { get; set; }

            public string Site { get; set; }

            public Minute Minute { get; set; }

            public HashSet<string> Species { get; set; }
        }

        public static List<AggregatedSiteDayMinute> Group(List<SiteDaySpeciesProfile> profiles)
        {
            var result = new List<AggregatedSiteDayMinute>();
            const int MinutesInADay = 1440;
            foreach (var daySpeciesProfile in profiles)
            {
                for (int minute = 0; minute < MinutesInADay; minute++)
                {
                    AggregatedSiteDayMinute aggregatedSDP;

                    aggregatedSDP = result.SingleOrDefault(sdp => sdp.Site == daySpeciesProfile.Site && sdp.Day == daySpeciesProfile.Day && sdp.Minute == minute);

                    if (aggregatedSDP == null)
                    {
                        aggregatedSDP = new AggregatedSiteDayMinute
                            {
                                Site = daySpeciesProfile.Site,
                                Day = daySpeciesProfile.Day,
                                Minute = minute,
                                Species = new HashSet<string>()
                            };
                        
                        result.Add(aggregatedSDP);
                    }

                    // add all the species that can be found
                   

                    if (daySpeciesProfile.MinuteProfile[minute] == 1)
                    {
                        aggregatedSDP.Species.Add(daySpeciesProfile.SpeciesName);
                    }
                }
            }


            return result;

        }



    }
}
