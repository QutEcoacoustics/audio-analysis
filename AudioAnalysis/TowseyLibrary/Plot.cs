namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents a single array of data with Xand Y scales and other info useful for pltting a graph.
    /// Was first used to represent a track of scores at the bottom of a sonogram image.
    /// </summary>
    public class Plot
    {
        public string title {set; get;}
        public double[] data { set; get; }
        public double threshold { set; get; }

        public Plot()
        {
        }

        public Plot(string _title, double[] _data, double _threshold)
        {
            this.title = _title;
            this.data = _data;
            this.threshold = _threshold;
        }

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
            int startID = 0;

            for (int i = 0; i < count; i++) // pass over all frames
            {
                if ((isHit == false) && (scores[i] >= scoreThreshold))//start of an event
                {
                    isHit = true;
                    startID = i;
                }
                else  // check for the end of an event
                if ((isHit == true) && (scores[i] < scoreThreshold)) // this is end of an event, so initialise it
                {
                    isHit = false;
                    int endID = i;

                    int duration = endID - startID;
                    if ((duration < minDuration) || (duration > maxDuration)) continue; //skip events with duration shorter than threshold

                    for (int j = startID; j < endID; j++) // pass over all frames
                    {
                        returnData[j] = scores[j];
                    }

                }
            } //end of pass over all frames
            return returnData;
        }


        public static void FindStartsAndEndsOfScoreEvents(double[] scores, double scoreThreshold, int minDuration, int maxDuration,
                                                          out double[] prunedScores, out List<Point> startEnds)
        {
            prunedScores = new double[scores.Length];
            startEnds = new List<Point>();

            int count = scores.Length;
            bool isHit = false;
            int startID = 0;

            for (int i = 0; i < count; i++) // pass over all frames
            {
                if ((isHit == false) && (scores[i] >= scoreThreshold))//start of an event
                {
                    isHit = true;
                    startID = i;
                }
                else  // check for the end of an event
                if ((isHit == true) && (scores[i] < scoreThreshold)) // this is end of an event, so initialise it
                {
                    isHit = false;
                    int endID = i-1;

                    int duration = endID - startID + 1;
                    if ((duration < minDuration) || (duration > maxDuration)) continue; //skip events with duration shorter than threshold

                    for (int j = startID; j <= endID; j++) // pass over all frames
                    {
                        prunedScores[j] = scores[j];
                    }
                    startEnds.Add(new Point(startID, endID));
                }
            } //end of pass over all frames
        }







    }
}
