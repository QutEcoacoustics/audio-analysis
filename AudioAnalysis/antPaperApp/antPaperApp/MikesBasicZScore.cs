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

    using LinqStatistics;

    public class MikesBasicZScore
    {
        /// <summary>
        /// THIS is an adaptive sampling algorithm.
        /// It has NO random element in it!.
        /// THis algorithm choose the test samples with most extreme acoustic profiles first.
        /// The minute chosen, is then evaluated against ONE test sample
        /// 
        /// BECAUSE it does not need training data, (it looks at the audio itself) - we dont need to load any training data!
        /// </summary>
        public MikesBasicZScore(DirectoryInfo trainingTagProfiles, DirectoryInfo testTagProfiles, DirectoryInfo trainingIndicies, DirectoryInfo testIndicies, DirectoryInfo output)
        {
            Contract.Requires(trainingTagProfiles.Exists);
            Contract.Requires(testTagProfiles.Exists);
            Contract.Requires(trainingIndicies.Exists);
            Contract.Requires(testIndicies.Exists);
            Contract.Requires(output.Exists);

            ////var trainingProfiles = Helpers.ReadFiles(trainingTagProfiles);
            var testProfiles = Helpers.ReadFiles(testTagProfiles);

            ////var trainingIndiciesRows = Helpers.ReadIndiciesFiles(trainingIndicies);
            var testIndiciesRows = Helpers.ReadIndiciesFiles(testIndicies);


            var numSamples = Program.LevelsOfTestingToDo;
            var outputFiles = new Dictionary<int, StringBuilder>();
            foreach (var numSample in numSamples)
            {
                outputFiles.Add(numSample, new StringBuilder());
            }

            // NOT RANDOM (do 5 to ensure consistent results
            const int RandomRuns = 8;

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

                        // go through the TEST data
                        // pick the sample with the most "extreme" acoustic profile
                        // use that sample's Minute to find what species called in that exact test minute!

                        // we can premptively choose our samples!
                        var extremeTestSamples = CalculateExtremeness(testIndiciesRows.ToArray());

                        // ORDER samples based on the most MOST EXTREMENESSEST
                        var orderedSamples = extremeTestSamples.OrderBy(m => m.Extremeness).ToList();


                        for (int sample = 0; sample < testRun; sample++)
                        {


                            // choose the next most extreme samples
                            var choice = orderedSamples[sample];


                            //  there will only  be one test sample chosen - so use it
                            // if each profile is a species * day * site tuple
                            // we want to filter down to one site and one day
  

                            samplesChosen.Add(Tuple.Create(choice.Minute, choice.Site, choice.Day, choice.Extremeness));



                            // now we have our sample,
                            // we do our evaluation
                            // we take the minute from the chosen training sample
                            // and grab all the unique species from the test data
                            var restrictedTestProfiles =
                                testProfiles.Where(sdp => sdp.Site == choice.Site && sdp.Day == choice.Day).ToArray();

                            Contract.Assert(restrictedTestProfiles.All(sdp => sdp.Day == restrictedTestProfiles.First().Day));
                            Contract.Assert(restrictedTestProfiles.All(sdp => sdp.Site == restrictedTestProfiles.First().Site));

                            foreach (var sdp in restrictedTestProfiles)
                            {
                                if (sdp.MinuteProfile[choice.Minute] == 1)
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
                        sb.AppendLine("*** Mikes's indicies zscore (RUN NUMBER: " + runIndex + " )");
                        sb.AppendFormat("NumSamples,{0}\n", testRun);
                        sb.AppendLine(
                            "Species Found," + speciesFoundInTestData.Aggregate((build, current) => build + "," + current));
                        sb.AppendLine(
                            "Species Found count," + foundCount.ToString(CultureInfo.InvariantCulture));
                        sb.AppendLine("Minutes sampled (minute, site, day, !!!EXTREMENESS!!!)"
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

        public static List<SiteDayMinuteExtremness> CalculateExtremeness(IndiciesRow[] testIndicies, bool smooothing = true, params Func<IndiciesRow, double>[] additionalSelectors)
        {
            var results = new List<SiteDayMinuteExtremness>(testIndicies.Length);

            // does a z-score calculation. Need a mean and stddev for each indicies.
            // only works on a few types of indicies

            var selectors =( new Func<IndiciesRow, double>[]
                {
                    //ir => ir.AvAmpdB,                       // 0*
                    //ir => ir.SnrdB,                         // 1*
                    //ir => ir.BgdB,                          // 2*
                    //ir => ir.activity,                      // 3
                    ir => ir.segCount,                      // 4*
                    //ir => ir.avSegDur,                      // 5
                    ir => ir.hfCover,                       // 6+
                    ir => ir.mfCover,                       // 7
                    ir => ir.H_peakFreq_,
                    //ir => ir.lfCover,                       // 8*
                    //ir => ir.H_ampl_,                       // 9*
                    //ir => ir.H_avSpectrum_,                 // 10*
                    ir => ir.numClusters,                   // 11+
                    ////ir => ir.avClustDur                     // 12
                }).Concat(additionalSelectors).ToArray();

            //if (runIndex >= 0)
            //{
            //    selectors = new[] { selectors[runIndex] };
            //}


            var newValues = new List<Tuple<Func<IndiciesRow, double>, double, double, double[]>>();

            for (int index = 0; index < selectors.Length; index++)
            {
                var selector = selectors[index];
                var values = testIndicies.Select(selector);

                // then do aggregates
                double mean = values.Average();
                double stdDev = values.StandardDeviationP();

                var zscores = values.Select(v => (v - mean) / stdDev).ToArray();

                newValues.Add(Tuple.Create(selector, mean, stdDev, zscores.ToArray()));
            }

            // now we have the z-scores, aggregate them. we are looking for the closest score to (1,1,1,1,1,1,1,1,1...
            // STOP ^^^ that's a bad idea... lets try this in a more primitive fashion.
            //  transform results so they are absolute. this solves the problem where features indicate in a inverse fashion
            var MOSTEXTREMEPOINT = selectors.Select(x => 10.0);
            for (int index = 0; index < testIndicies.Length; index++)
            {
                var indiciesRow = testIndicies[index];

                var zPoint = new List<double>();
                for (int i = 0; i < newValues.Count; i++)
                {
                    var values = newValues[i];
                    zPoint.Add(values.Item4[index]);
                }

                var extremness = MOSTEXTREMEPOINT.EuclideanDistance(zPoint);
                //var extremness = zPoint.Sum();

                results.Add(
                    new SiteDayMinuteExtremness()
                        {
                            Day = indiciesRow.Day,
                            Site = indiciesRow.Site,
                            Minute = indiciesRow.IndicesCount,
                            Extremeness = extremness
                        });
            }

            var ord = results.OrderBy(x => x.Minute).ToArray();
            int smoothing = 12;
            var newPoints =
                ord.Aggregate(
                    Tuple.Create(new Queue<double>(smoothing), new List<SiteDayMinuteExtremness>()),
                    (tuple, extremness) =>
                        {
                            var queue = tuple.Item1;
                            var finals = tuple.Item2;
                            if (queue.Count < smoothing)
                            {
                                queue.Enqueue(extremness.Extremeness);
                            }
                            else
                            {
                                queue.Dequeue();
                                queue.Enqueue(extremness.Extremeness);
                            }

                            var avg = queue.Average();

                            extremness.Extremeness = (extremness.Extremeness + avg) /2;
                            finals.Add(extremness);

                            return Tuple.Create(queue, finals);

                        }).Item2;



            return newPoints;
        }
    }

    public class SiteDayMinuteExtremness
    {
        public DateTime Day { get; set; }

        public string Site { get; set; }

        public int Minute { get; set; }

        public double Extremeness { get; set; }
    }
}
