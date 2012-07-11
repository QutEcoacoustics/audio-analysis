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
    /// THIS is a take on Jason's orginal work.
    /// It has a random element in it, and hence its results are repeated a thousand times.
    /// </summary>
    public class JasonsBasic
    {


        public JasonsBasic(DirectoryInfo training, DirectoryInfo test, DirectoryInfo output, int minBound, int maxBound, string testName)
        {
            Contract.Requires(training.Exists);
            Contract.Requires(test.Exists);

            // load and parse files

            // these are site wide analyses
            // i.e. sites are the repeatable experiment

            // this means that one of the days will be test data for each site
            ////var trainingProfiles = Helpers.ReadFiles(training);

            var testProfiles = Helpers.ReadFiles(test);

            // for a fair test we do not use the  training data
            //
            ////trainingProfiles.AddRange(testProfiles);
            var allProfiles = testProfiles;

            // record all the different "days" and "sites" we get
            var distinctDays = allProfiles.Select(sdp => sdp.Day).Distinct();
            var distinctSites = allProfiles.Select(sdp => sdp.Site).Distinct();

            // time bounds to constrain to

            // levels of testing to do
            var numSamples = Program.LevelsOfTestingToDo;
            var files = new Dictionary<int, StringBuilder>();
            foreach (var numSample in numSamples)
            {
                files.Add(numSample, new StringBuilder());
            }

            var randomRuns = 100;
            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 8 };
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
                            var speciesFound = new HashSet<string>();

                            // so sample randomly from all data

                            for (int sample = 0; sample < testRun; sample++)
                            {
                                // randomly choose from all samples within the time bounds
                                var randomChoice = rand.Next(minBound, maxBound);

                                // there could be multiple results from that minute, randomly choose one
                                // if each profile is a species * day * site tuple
                                // we want to filter down to one site and one day
                                var randomSite = distinctSites.GetRandomElement();
                                var randomDay = distinctDays.GetRandomElement();

                                // MUST BE CAREFUL TO ONLY GRAB '1' SAMPLE
                                var restrictedProfiles =
                                    allProfiles.Where(sdp => sdp.Site == randomSite && sdp.Day == randomDay);

                                randomSamplesChosen.Add(Tuple.Create(randomChoice, randomSite, randomDay));

                                Contract.Assert(restrictedProfiles.All(sdp => sdp.Day == restrictedProfiles.First().Day));
                                Contract.Assert(restrictedProfiles.All(sdp => sdp.Site == restrictedProfiles.First().Site));

                                // now we have our random sample,
                                // we do our evaluation
                                // grab all the species in the left over profiles
                                foreach (var profile in restrictedProfiles)
                                {
                                    // last step, see if this species, in this day, at this site, actually occurs at chosen minute

                                    if (profile.MinuteProfile[randomChoice] == 1)
                                    {
                                        speciesFound.Add(profile.SpeciesName);
                                    }
                                }


                                // loop back, try again
                            }

                            // make a summary result
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine("*** "+testName+ " (RUN NUMBER: " + randomRunIndex + " )");
                            sb.AppendFormat("NumSamples,{0}\n", testRun);
                            sb.AppendLine(
                                "Species Found," + speciesFound.Aggregate((build, current) => build + "," + current));
                            sb.AppendLine(
                                "Species Found count," + speciesFound.Count.ToString(CultureInfo.InvariantCulture));
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

        public Dictionary<int, int> MakeAllSet(List<SiteDaySpeciesProfile>  profiles)
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
