// <copyright file="Plot.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System.Collections.Generic;
    using System.Drawing;

    /// <summary>
    /// Represents a single array of data with Xand Y scales and other info useful for pltting a graph.
    /// Was first used to represent a track of scores at the bottom of a sonogram image.
    /// </summary>
    public class Plot
    {
        public Plot()
        {
        }

        public Plot(string _title, double[] _data, double _threshold)
        {
            this.title = _title;
            this.data = _data;
            this.threshold = _threshold;
        }

        public string title { get; set; }

        public double[] data { get; set; }

        public double threshold { get; set; }

        public void NormaliseData()
        {
            this.data = DataTools.normalise(this.data);
        }

        public void ScaleDataArray(int newLength)
        {
            this.data = DataTools.ScaleArray(this.data, newLength);
        }

        public void NormaliseData(double min, double max)
        {
            this.data = DataTools.NormaliseInZeroOne(this.data, min, max);
        }

        public static double[] PruneScoreArray(double[] scores, double scoreThreshold, int minDuration, int maxDuration)
        {
            double[] returnData = new double[scores.Length];

            int count = scores.Length;
            bool isHit = false;
            int startId = 0;

            // pass over all frames
            for (int i = 0; i < count; i++)
            {
                if ((isHit == false) && scores[i] >= scoreThreshold)
                {
                    //start of an event
                    isHit = true;
                    startId = i;
                }
                else // check for the end of an event
                if ((isHit == true) && scores[i] < scoreThreshold)
                {
                    // this is end of an event, so initialise it
                    isHit = false;
                    int endId = i;

                    int duration = endId - startId;
                    if ((duration < minDuration) || (duration > maxDuration))
                    {
                        continue; //skip events with duration shorter than threshold
                    }

                    // pass over all frames
                    for (int j = startId; j < endId; j++)
                    {
                        returnData[j] = scores[j];
                    }
                }
            } //end of pass over all frames

            return returnData;
        }

        public static void FindStartsAndEndsOfScoreEvents(
            double[] scores,
            double scoreThreshold,
            int minDuration,
            int maxDuration,
            out double[] prunedScores,
            out List<Point> startEnds)
        {
            prunedScores = new double[scores.Length];
            startEnds = new List<Point>();

            int count = scores.Length;
            bool isHit = false;
            int startId = 0;

            // pass over all frames
            for (int i = 0; i < count; i++)
            {
                //start of an event
                if (isHit == false && scores[i] >= scoreThreshold)
                {
                    isHit = true;
                    startId = i;
                }
                else // check for the end of an event
                if (isHit == true && scores[i] < scoreThreshold)
                {
                    // this is end of an event, so initialise it
                    isHit = false;
                    int endId = i - 1;

                    int duration = endId - startId + 1;
                    if ((duration < minDuration) || (duration > maxDuration))
                    {
                        continue; //skip events with duration shorter than threshold
                    }

                    // pass over all frames
                    for (int j = startId; j <= endId; j++)
                    {
                        prunedScores[j] = scores[j];
                    }

                    startEnds.Add(new Point(startId, endId));
                }
            } //end of pass over all frames
        }
    }
}
