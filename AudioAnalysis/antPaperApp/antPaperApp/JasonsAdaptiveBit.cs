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

    public class SiteDaySpeciesProfile
    {
        public Dictionary<int, int> MinuteProfile;

        public DateTime Day;

        public string SpeciesName;

        public string Site;
    }

    /// <summary>
    /// THIS is an adaptive sampling algorithm.
    /// It has a random element in it, and hence its results are repeated a thousand times.
    /// </summary>
    public class JasonsAdaptiveBit
    {


        public JasonsAdaptiveBit(DirectoryInfo training, DirectoryInfo test, DirectoryInfo output)
        {
            Contract.Requires(training.Exists);
            Contract.Requires(test.Exists);

            // load and parse files

            // these are site wide analyses
            // i.e. sites are the repeatable experiment

            var trainingProfiles = Helpers.ReadFiles(training);

            var testProfiles = Helpers.ReadFiles(test);

            // record all the different "days" and "sites" we get
            var distinctDaysTest = testProfiles.Select(sdp => sdp.Day).Distinct().ToArray();
            var distinctSitesTest = testProfiles.Select(sdp => sdp.Site).Distinct().ToArray();

            // levels of testing to do
            var numSamples = Program.LevelsOfTestingToDo;
            var files = new Dictionary<int, StringBuilder>();
            foreach (var numSample in numSamples)
            {
                files.Add(numSample, new StringBuilder());
            }

            const int randomRuns = 100;
            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 16 };
            Parallel.For(
                (long)0,
                randomRuns,
                parallelOptions,
                (randomRunIndex, loopState) =>
                    {

                        foreach (var testRun in numSamples)
                        {
                            var rand = new Random();
                            // eventually we will pick #testRun random samples
                            var randomSamplesChosen = new List<Tuple<int, string, DateTime>>(testRun);
                            var speciesFoundInTestData = new HashSet<string>();

                            // so sample randomly from test data
                            // however we do it adaptively based on the species we "discover" in test profiles.

                            List<SiteDaySpeciesProfile> adaptiveFilter = trainingProfiles;
                            for (int sample = 0; sample < testRun; sample++)
                            {
                                // first remove training samples where we have already found that species
                                adaptiveFilter =
                                    adaptiveFilter.Where(sdp => !speciesFoundInTestData.Contains(sdp.SpeciesName)).ToList();

                                // for the first iteration, none should be removed
                                Contract.Assert(sample > 0 || adaptiveFilter.Count() == trainingProfiles.Count());

                                // then form ALL set from remaining profiles
                                var allSet = MakeAllSet(adaptiveFilter);

                                // choose samples out of the all set where precence *is* indicated
                                var presence = allSet.Where(kvp => kvp.Value == 1).ToList();

                                // randomly choose from presence subset
                                var randomChoice = rand.Next(presence.Count());
                                var randomChoiceMinute = presence[randomChoice];

                                // there could be multiple results from that minute, randomly choose one
                                // if each profile is a species * day * site tuple
                                // we want to filter down to one site and one day
                                var randomSite = distinctSitesTest.GetRandomElement();
                                var randomDay = distinctDaysTest.GetRandomElement();

                                // now we have our random sample,
                                // we do our evaluation
                                // we take the minute from the randomly chosen training sample
                                // and grab all the unique species from the test data
                                // MUST BE CAREFUL TO ONLY GRAB '1' SAMPLE

                                var restrictedTestProfiles =
                                    testProfiles.Where(sdp => sdp.Site == randomSite && sdp.Day == randomDay).ToArray();

                                randomSamplesChosen.Add(Tuple.Create(randomChoiceMinute.Key, randomSite, randomDay));

                                Contract.Assert(restrictedTestProfiles.All(sdp=> sdp.Day == restrictedTestProfiles.First().Day));
                                Contract.Assert(restrictedTestProfiles.All(sdp => sdp.Site == restrictedTestProfiles.First().Site));

                                foreach (var sdp in restrictedTestProfiles)
                                {
                                    if (sdp.MinuteProfile[randomChoiceMinute.Key] == 1)
                                    {
                                        speciesFoundInTestData.Add(sdp.SpeciesName);
                                    }
                                }

                                // loop back, try again
                            }

                            // make a summary result

                            var foundCount = speciesFoundInTestData.Count;
                            if (foundCount == 0)
                            {
                                speciesFoundInTestData.Add("[[[NONE!]]]");
                            }

                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("*** Jason's adaptive bit (RUN NUMBER: " + randomRunIndex + " )");
                            sb.AppendFormat("NumSamples,{0}\n", testRun);
                            sb.AppendLine(
                                "Species Found," + speciesFoundInTestData.Aggregate((build, current) => build + "," + current));
                            sb.AppendLine(
                                "Species Found count," + foundCount.ToString(CultureInfo.InvariantCulture));
                            sb.AppendLine(
                                "Minutes sampled (minute, site, day)"
                                +
                                randomSamplesChosen.Aggregate(
                                    "",
                                    (build, current) =>
                                    build
                                    +
                                    string.Format(
                                        ", ({0}, {1}, {2})",
                                        current.Item1,
                                        current.Item2,
                                        current.Item3.ToShortDateString())));

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

        public static Dictionary<int, int> MakeAllSet(List<SiteDaySpeciesProfile>  profiles, bool sum = false)
        {
            var allMinutes = new Dictionary<int, int>(1440);
            foreach (var profile in profiles)
            {
                foreach (var min in profile.MinuteProfile)
                {
                    if (min.Value == 1)
                    {
                       if (sum)
                       {
                           if (allMinutes.ContainsKey(min.Key))
                           {
                               allMinutes[min.Key] = allMinutes[min.Key] + 1;
                           }
                           else
                           {
                               allMinutes[min.Key] = 1;   
                           }
                       }
                       else
                       {
                           allMinutes[min.Key] = 1;    
                           
                       }
                    }
                }
            }

            return allMinutes;
        }


        

    }
}
