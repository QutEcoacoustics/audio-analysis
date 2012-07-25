using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace antPaperApp
{
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// THIS is an adaptive sampling algorithm.
    /// It has NO random element in it!.
    /// THis algorithm choose the test samples with most extreme acoustic profiles first.
    /// HOWEVER it ALSO adaptively filters like JasonsAdaptiveFrequency
    /// The minute chosen, is then evaluated against ONE test sample
    /// 
    /// 
    /// </summary>
    public class JasonAndMike_TheirPowersCombined_AreCaptainPLANET
    {

        public JasonAndMike_TheirPowersCombined_AreCaptainPLANET(DirectoryInfo trainingTagProfiles, DirectoryInfo testTagProfiles, DirectoryInfo trainingIndicies, DirectoryInfo testIndicies, DirectoryInfo output, int variance = 1)
        {
            Contract.Requires(trainingTagProfiles.Exists);
            Contract.Requires(testTagProfiles.Exists);
            Contract.Requires(trainingIndicies.Exists);
            Contract.Requires(testIndicies.Exists);
            Contract.Requires(output.Exists);
            Contract.Requires(variance >= 1);


            // load and parse files

            var trainingProfiles = Helpers.ReadFiles(trainingTagProfiles);
            var testProfiles = Helpers.ReadFiles(testTagProfiles);

            var trainingIndiciesRows = Helpers.ReadIndiciesFiles(trainingIndicies);
            var testIndiciesRows = Helpers.ReadIndiciesFiles(testIndicies);

            // record all the different "days" and "sites" we get in testData
            var testDayCombos = JasonsAdaptiveBit.SiteDayCombos(testProfiles);

            // misc setup

            var numSamples = Program.LevelsOfTestingToDo;
            var outputFiles = new Dictionary<int, StringBuilder>();
            foreach (var numSample in numSamples)
            {
                outputFiles.Add(numSample, new StringBuilder());
            }

            // NOT RANDOM (do 8 to ensure consistent results)
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
                            var samplesChosen = new List<Tuple<int, string, DateTime, double>>(testRun);
                            var speciesFoundInTestData = new HashSet<string>();

                            // so sample from test data, a sample chosen by this process:
                            // form a profile of species call from training data.
                            // each profile is a uniqe tuple of site * day * minute * (unique species count, acoustic profile)
                            // -- training
                            //      filter out tuples where species has been found before
                            //      order by extremeness (acoustic profiles & unique specices count zscored)
                            //      pick the most extreme
                            // -- testing
                            //      filter test profiles to only one site/day
                            //          do this by picking most extreme audio profile
                            //      count species, loop


                            List<SiteDaySpeciesProfile> adaptiveFilter = trainingProfiles;
                            for (int sample = 0; sample < testRun; sample++)
                            {
                                // first remove training samples where we have already found that species
                                adaptiveFilter =
                                    adaptiveFilter.Where(sdp => !speciesFoundInTestData.Contains(sdp.SpeciesName)).ToList();

                                // using acoustic indicies as well is too stable,  filter out any minutes that have been checked previously
                                // note, filtering out one minute here, means several sites&days will not get their minutes checked
                                //if (samplesChosen.Count > 1)
                                //{
                                //    foreach (var siteDaySpeciesProfile in adaptiveFilter)
                                //    {
                                //        foreach (var tuple in samplesChosen)
                                //        {
                                //            siteDaySpeciesProfile.MinuteProfile[tuple.Item1] = 0;
                                //        }
                                //    }
                                //}

                                // for the first iteration, none should be removed
                                Contract.Assert(sample > 0 || adaptiveFilter.Count() == trainingProfiles.Count());

                                // then form super profile from remaining profiles
                                // IMPORTANT  - the all set will sum and not AND

                                // group into sites and days
                                var allset = JasonsAdaptiveBit.MakeAllSet(adaptiveFilter, true);

                                // sync with indicies
                                var restrictedTrainingIndicies = new List<IndiciesRow>();
                                foreach (var allProfile in allset)
                                {
                                    // if there is at least one species present
                                    if (allProfile.Value > 0)
                                    {
                                        var minutes = trainingIndiciesRows
                                            // find the associated acoustic profiles (match on minute)
                                            .Where(ir => ir.IndicesCount == allProfile.Key)
                                            // add the count of species in
                                            .Select(
                                                ir =>
                                                    {
                                                        ir.SpeciesCount = allProfile.Value;
                                                        return ir;
                                                    });

                                        restrictedTrainingIndicies.AddRange(minutes);
                                    }
                                    
                                }

                                // run extremeness
                                var extremenessTraining = MikesBasicZScore.CalculateExtremeness(restrictedTrainingIndicies.ToArray(), false, ir => ir.SpeciesCount);


                                // ORDER samples based on the most extremeness
                                var orderedUniqueSpeciesCount = extremenessTraining.OrderBy(m => m.Extremeness).ToList();

                                // choose the first sample with the most unique species
                                var trainingChoice = orderedUniqueSpeciesCount.First();

                                // or if a variance is set, choose a sample N randomly within the top V species
                                if (variance > 1)
                                {
                                    trainingChoice = orderedUniqueSpeciesCount.Take(variance).GetRandomElement();
                                }


                                // testing
                                // there could be multiple results from that minute, randomly choose one
                                // if each profile is a species * day * site tuple

                                /*
                                // we want to filter down to one site and one day
                                // WE WILL DO THIS WITH ACOUSTIC INDICIES
                                var possibleTestMinutes = 
                                    MikesBasicZScore
                                    .CalculateExtremeness(testIndiciesRows.Where(ir => ir.IndicesCount == trainingChoice.Minute).ToArray())
                                    .OrderBy(m => m.Extremeness)
                                    .ToList();

                                // pick the "best"
                                var chosenTest = possibleTestMinutes.First();
                                 * */

                                var randomTestDay = testDayCombos.GetRandomElement();
                                var randomSite = randomTestDay.Item1;
                                var randomDay = randomTestDay.Item2;
                                var chosenTest = new SiteDayMinuteExtremness() { Day = randomDay, Site = randomSite, Minute = trainingChoice.Minute };
                                
                                // translate back to profiles
                                var restrictedTestProfiles =
                                   testProfiles.Where(sdp => sdp.Site == chosenTest.Site && sdp.Day == chosenTest.Day).ToArray();

                                samplesChosen.Add(Tuple.Create(chosenTest.Minute, chosenTest.Site, chosenTest.Day, trainingChoice.Extremeness));

                                Contract.Assert(restrictedTestProfiles.All(sdp => sdp.Day == restrictedTestProfiles.First().Day));
                                Contract.Assert(restrictedTestProfiles.All(sdp => sdp.Site == restrictedTestProfiles.First().Site));
                                Contract.Assert(trainingChoice.Minute == chosenTest.Minute);


                                foreach (var sdp in restrictedTestProfiles)
                                {
                                    if (sdp.MinuteProfile[trainingChoice.Minute] == 1)
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
                            sb.AppendLine("*** Mikes's AND JASONS- dis iz meant 2b da shitdizzle (RUN NUMBER: " + runIndex + " )");
                            sb.AppendFormat("NumSamples,{0}\n", testRun);
                            sb.AppendLine(
                                "Species Found,"
                                + speciesFoundInTestData.Aggregate((build, current) => build + "," + current));
                            sb.AppendLine("Species Found count," + foundCount.ToString(CultureInfo.InvariantCulture));
                            sb.AppendLine(
                                "Minutes sampled (minute, site, day, !!!EXTREMENESS!!!)"
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
                                        current.Item4)));

                            lock (outputFiles)
                            {
                                outputFiles[testRun].AppendLine(sb.ToString());
                            }

                        }


                    }); // end  loops

            foreach (var stringBuilder in outputFiles)
            {
                File.AppendAllText(output + "\\" + stringBuilder.Key + ".txt", stringBuilder.Value.ToString());
            }
        }
    }
}
