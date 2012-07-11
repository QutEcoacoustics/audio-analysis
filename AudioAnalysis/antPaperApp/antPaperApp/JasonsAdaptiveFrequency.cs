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

    /// <summary>
    /// THIS is an adaptive sampling algorithm.
    /// It has NO random element in it - it only needs to be run once.
    /// </summary>
    public class JasonsAdaptiveFrequency
    {

        public JasonsAdaptiveFrequency(DirectoryInfo training, DirectoryInfo test, DirectoryInfo output)
        {
            Contract.Requires(training.Exists);
            Contract.Requires(test.Exists);

            // load and parse files

            // these are site wide analyses
            // i.e. sites are the repeatable experiment

            var trainingProfiles = Helpers.ReadFiles(training);

            var testProfiles = Helpers.ReadFiles(test);

            // record all the different "days" and "sites" we get
            var distinctDays = testProfiles.Select(sdp => sdp.Day).Distinct().ToArray();
            var distinctSites = testProfiles.Select(sdp => sdp.Site).Distinct().ToArray();

            // levels of testing to do
            var numSamples = new[] { 10, 20, 60, 100, 200 };
            var files = new Dictionary<int, StringBuilder>();
            foreach (var numSample in numSamples)
            {
                files.Add(numSample, new StringBuilder());
            }

            const int RandomRuns = 5;
            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 16 };
            Parallel.For(
                (long)0,
                RandomRuns,
                parallelOptions,
                (runIndex, loopState) =>
                    {

                        foreach (var testRun in numSamples)
                        {
                            var rand = new Random();
                            // eventually we will pick #testRun random samples
                            var samplesChosen = new List<KeyValuePair<int, int>>(testRun);
                            var speciesFound = new HashSet<string>();

                            // so sample from test data ordered on number of unique 
                            // species found in equivalent training minute
                            // i.e. min 300 in training, has 25 unique species, pick min 300 from test data
                            // however we do it adaptively based on the test profiles.

                            List<SiteDayProfile> adaptiveFilter = trainingProfiles;
                            for (int sample = 0; sample < testRun; sample++)
                            {
                                // first remove training samples where we have already found that species
                                adaptiveFilter =
                                    adaptiveFilter.Where(sdp => !speciesFound.Contains(sdp.SpeciesName)).ToList();

                                // for the first iteration, none should be removed
                                Contract.Assert(sample > 0 || adaptiveFilter.Count() == trainingProfiles.Count());

                                // then form ALL set from ramining profiles
                                var allSet = this.MakeAllSet(adaptiveFilter);

                                // choose samples out of the all set where precence *is* indicated
                                var presence = allSet.Where(kvp => kvp.Value == 1).ToList();

                                // randomly choose from presence subset
                                var randomChoice = rand.Next(presence.Count());
                                var randomChoiceMinute = presence[randomChoice];
                                samplesChosen.Add(randomChoiceMinute);



                                // now we have our random sample,
                                // we do our evaluation
                                // we take the minute from the randomly chosen training sample
                                // and grab all the unique species from the test data

                                foreach (var sdp in testProfiles)
                                {
                                    if (sdp.MinuteProfile[randomChoiceMinute.Key] == 1)
                                    {
                                        speciesFound.Add(sdp.SpeciesName);
                                    }
                                }

                                // loop back, try again
                            }

                            // make a summary result
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("*** Jason's adaptive bit (RUN NUMBER: " + runIndex + " )");
                            sb.AppendFormat("NumSamples,{0}\n", testRun);
                            sb.AppendLine(
                                "Species Found," + speciesFound.Aggregate((build, current) => build + "," + current));
                            sb.AppendLine(
                                "Species Found count," + speciesFound.Count.ToString(CultureInfo.InvariantCulture));
                            sb.AppendLine(
                                "Minutes sampled"
                                +
                                samplesChosen.Select(kvp => kvp.Key).Aggregate(
                                    "", (build, current) => build + "," + current));

                            lock (files)
                            {
                                files[testRun].AppendLine(sb.ToString());
                            }
                            
                        }
                    }); // end 1000 loops


            foreach (var stringBuilder in files)
            {
                File.AppendAllText(output + "\\" + stringBuilder.Key + ".txt", stringBuilder.Value.ToString());
            }

        }

        public Dictionary<int, int> MakeAllSet(List<SiteDayProfile>  profiles)
        {
            var allMinutes = new Dictionary<int, int>(1440);
            foreach (var profile in profiles)
            {
                foreach (var min in profile.MinuteProfile)
                {
                    if (min.Value == 1)
                    {
                        allMinutes[min.Key] = 1;

                    }
                }
            }

            return allMinutes;
        }


        

    }
}
