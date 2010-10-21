using System;
using System.Collections.Generic;
using System.Text;

namespace TowseyLib
{


    public class NormalDist
    {

        double av;        //mean of the data 
        double sd;        //SD of the data
        double[,] bins;   //histogram of distribution over 16 bins about the mean
        int totalCount;   //number of samples
        double min;       //min and max of data
        double max; 

       
        /// <summary>
        /// CONSTRUCTOR for integer data
        /// </summary>
        /// <param name="data"></param>
        public NormalDist(int[] data)
        {
            totalCount = data.Length;
            double average;
            double stdev;
            AverageAndSD(data, out average, out stdev);
            this.av = average;
            this.sd = stdev;
            //convert data to doubles
            double[] doubleData = new double[totalCount];
            for (int i = 0; i < totalCount; i++) doubleData[i] = (double)data[i];
            bins = get16binDistribution(doubleData, this.av, this.sd);
            
            DataTools.MinMax(doubleData, out min, out max);
        }

        /// <summary>
        /// CONSTRUCTOR for real valued data
        /// </summary>
        /// <param name="data"></param>
        public NormalDist(double[] data)
        {
            totalCount = data.Length;
            double average;
            double stdev;
            AverageAndSD(data, out average, out stdev);
            this.av = average;
            this.sd = stdev;
            bins = get16binDistribution(data, average, stdev);
            DataTools.MinMax(data, out this.min, out this.max);
        }


        public NormalDist(double[] data, double AV, double SD)
        {
            totalCount = data.Length;
            this.av = AV;
            this.sd = SD;
            bins = get16binDistribution(data, av, SD);
            DataTools.MinMax(data, out this.min, out this.max);
        }




        //=============================================================================



        /**
         * returns the histogram bins containing data distribution
         * @return
         */
        public double[,] getBins()
        {
            return bins;
        }

        /**
         * returns the sum of a set of integer values
         * @param data
         * @return
         */
        public static int getSum(int[] data)
        {
            int N = data.Length;
            int sum = 0;
            for (int i = 0; i < N; i++)
            {
                sum += data[i];
            }
            return sum;
        }

        public static double[] convert2ZScores(int[] scores, double av, double sd)
        {
            int length = scores.Length;
            double[] zs = new double[length];
            for (int i = 0; i < length; i++)
            {
                zs[i] = (double)(scores[i] - av) / sd;
            }

            return zs;
        }



        /**
         * returns the average and SD of a set of integer values
         * @param data
         * @return
         */
        static public void AverageAndSD(int[] data, out double av, out double sd)
        {
            int N = data.Length;
            int sum = 0;
            for (int i = 0; i < N; i++)
            {
                sum += data[i];
            }
            av = sum / (double)N;

            double var = 0.0;
            for (int i = 0; i < N; i++)
            {
                double diff = (double)data[i] - av;
                var += (diff * diff);
            }

            if (N > 30) var /= (double)N;
            else
                if (N > 2) var /= (double)(N - 1);
                else var = 0.0;

            sd = Math.Sqrt(var);
            //Console.WriteLine("av="+av+" SD="+SD);
            //Console.WriteLine("VAR="+var+" SD="+SD+"  N="+N);
        }


        /**
         * returns the average and SD of a set of real values
         * @param data
         * @return
         */
        static public void AverageAndSD(double[] data, out double av, out double sd)
        {
            int N = data.Length;
            double sum = 0.0;
            for (int i = 0; i < N; i++)
            {
                sum += data[i];
            }
            av = sum / (double)N;

            double var = 0.0;
            for (int i = 0; i < N; i++)
            {
                var += ((data[i] - av) * (data[i] - av));
            }

            if (N > 30) var /= (double)N;
            else
                if (N > 2) var /= (double)(N - 1);
                else var = 0.0;

            sd = Math.Sqrt(var);

            //Console.WriteLine("av="+av+" SD="+SD);
        }



        public static void AverageAndSD(double[,] data, out double av, out double sd)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            
            double[] values = new double[rows*cols];
            int id = 0;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    values[id++] = data[i,j];

            AverageAndSD(values, out av, out sd);
        }


        public static double[] Normalise(double[] data)
        {
            double av;
            double sd;
            AverageAndSD(data, out av, out sd);
            double[] ndata = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                ndata[i] = (data[i] - av) / sd;
            }
            return ndata;
        }

        /**
         * Variance = (av of squares) - (square of the average)
         * i.e.  SS/n - mean^2  
         *    =  SS/n - (sum/n)^2
         *    = (SS*n - sum^2)/n^2
         * @param SumSq
         * @param Sum
         * @param count
         * @return
         */
        public static double Variance(double SumSq, double Sum, int count)
        {
            double var = count * SumSq - Sum * Sum;
            var /= (count * (count - 1));
            return var;
        }
        public static double SD(double SumSq, double Sum, int count)
        {
            double var = Variance(SumSq, Sum, count);
            return Math.Sqrt(var);
        }


        public static double[] bias(double[] data, double bias)
        {
            double[] ndata = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                ndata[i] = data[i] + bias;
            }
            return ndata;
        }



        public static double[] threshold(double[] data, double th)
        {
            double[] ndata = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                ndata[i] = data[i] - th;
                if (ndata[i] < 0.0) ndata[i] = 0.0000;
            }
            return ndata;
        }

        /**
         * returns the upper limit for each bin in a 16 bin histogram 
         * from an array of data given the data's
         * average and standard deviation. 
         * @param data
         * @param av
         * @param SD
         * @return
         */
        public static double[] getBinLimits(double av, double SD)
        {
            double[] limits = new double[16];
            double halfSD = SD / 2;

            //set upper limits for 15 bins. bin 16 holds highest values.
            limits[0] = av - (3 * SD) - halfSD;
            limits[1] = av - (3 * SD);
            limits[2] = av - (2 * SD) - halfSD;
            limits[3] = av - (2 * SD);
            limits[4] = av - SD - halfSD;
            limits[5] = av - SD;
            limits[6] = av - halfSD;
            limits[7] = av;
            limits[8] = av + halfSD;
            limits[9] = av + SD;
            limits[10] = av + SD + halfSD;
            limits[11] = av + (2 * SD);
            limits[12] = av + (2 * SD) + halfSD;
            limits[13] = av + (3 * SD);
            limits[14] = av + (3 * SD) + halfSD;
            limits[15] = av + (100 * SD); // upper bound to very large number
            return limits;
        }

        /**
         * returns an 8 bin histogram from an array of data given the data's
         * average and standard deviation. 
         * @param data
         * @param av
         * @param SD
         * @return
         */
        public static int[] get8binDistribution(double[] data, double av, double SD)
        {
            int[] dist = new int[8];
            for (int i = 0; i < 8; i++) dist[i] = 0; // initialise array

            //set upper limits for 7 bins. bin8 holds highest values.
            double bin1 = av - (3 * SD);
            double bin2 = av - (2 * SD);
            double bin3 = av - SD;
            double bin4 = av;
            double bin5 = av + SD;
            double bin6 = av + (2 * SD);
            double bin7 = av + (3 * SD);

            // loop through data and put values in bins.
            for (int i = 0; i < data.Length; i++)
            {
                double dd = data[i];
                if (dd < bin1) dist[0]++;
                else
                    if (dd < bin2) dist[1]++;
                    else
                        if (dd < bin3) dist[2]++;
                        else
                            if (dd < bin4) dist[3]++;
                            else
                                if (dd < bin5) dist[4]++;
                                else
                                    if (dd < bin6) dist[5]++;
                                    else
                                        if (dd < bin7) dist[6]++;
                                        else
                                            dist[7]++;
            }
            return dist;
        }


        /**
         * returns a 16 bin histogram from an array of data given 
         * the data's average and standard deviation. 
         * The histogram is represented by a matrix of doubles.
         * One row represents data in one bin.
         * 5 columns are: 1 upper bound of bin
         *                2 midpoint of bin
         *                3 absolute counts in bin
         *                4 fraction of counts in bin
         *                5 ln(fraction)  
         * @param data
         * @param av
         * @param SD
         * @return
         */
        public static double[,] get16binDistribution(double[] data, double av, double SD)
  { 
  	//get the upper bounds on the 16 bins
  	//highest bin has no upper bound - therefore bounds.mapLength=15
  	double[] bounds = getBinLimits(av, SD);

  	// init array to collect counts
  	int[] counts = new int[16];
    for(int c=0;c<16;c++) counts[c] = 0; 
  	
    // loop through data and collect the counts for each bin.
    for(int i=0;i<data.Length;i++)
    { double dd = data[i];
    	counts[getBin(dd, bounds)]++;
    }
    
    int totalCounts = data.Length;
    double delta = SD/4;  //delta for midpoint of each bin.
    // init matrix to return
    double[,] dist = new double[16,5];
    for(int r=0;r<16;r++)
    { dist[r,0] = bounds[r];
      dist[r,1] = bounds[r]-delta; //there is error on last midpoint
      dist[r,2] = (double)counts[r];
      dist[r,3] = (double)counts[r]/(double)totalCounts;
      // put ln(fraction) in fifth column
      if(dist[r,3]== 0.0) dist[r,4] = - 1000.0;
      else
                           dist[r,4]=Math.Log(dist[r,3]);
    }
    // correct error on last midpoint
    dist[15,1] = bounds[14]+delta; //there is error on last midpoint
    
    return dist;
  }

       

        public static double[,] get16binDistribution(int[] data, double av, double SD)
        {
            int length = data.Length;
            double[] ddData = new double[length];
            // convert array of integers to array of doubles.
            for (int i = 0; i < data.Length; i++)
            {
                ddData[i] = (double)data[i];
            }
            return get16binDistribution(ddData, av, SD);
        }


        static private int getBin(double dd, double[] bounds)
        {
            if (dd < bounds[0]) return 0;
            else
                if (dd < bounds[1]) return 1;
                else
                    if (dd < bounds[2]) return 2;
                    else
                        if (dd < bounds[3]) return 3;
                        else
                            if (dd < bounds[4]) return 4;
                            else
                                if (dd < bounds[5]) return 5;
                                else
                                    if (dd < bounds[6]) return 6;
                                    else
                                        if (dd < bounds[7]) return 7;
                                        else
                                            if (dd < bounds[8]) return 8;
                                            else
                                                if (dd < bounds[9]) return 9;
                                                else
                                                    if (dd < bounds[10]) return 10;
                                                    else
                                                        if (dd < bounds[11]) return 11;
                                                        else
                                                            if (dd < bounds[12]) return 12;
                                                            else
                                                                if (dd < bounds[13]) return 13;
                                                                else
                                                                    if (dd < bounds[14]) return 14;
                                                                    else
                                                                        return 15;
        }


        public String write16binDistribution()
        {
            return write16binDistribution(bins);
        }

        public static String write16binDistribution(double[,] bins)
        {
            StringBuilder sb = new StringBuilder("NORMAL DISTRIBUTION (16 bins)\n");
            sb.Append("bin\tup_bnd\tmidpt\tcount\t\tfraction\tln\n");
            for (int r = 0; r < 16; r++)
            {
                if (r < 9) sb.Append(" ");
                sb.Append((r + 1) + "\t");
                if (bins[r,0] >= 0.0) sb.Append("+");
                sb.Append(bins[r,0].ToString("F2.2") + "\t");
                if (bins[r,1] >= 0.0) sb.Append("+");
                sb.Append(bins[r,1].ToString("F2.2") + "\t");
                sb.Append(bins[r,2].ToString("F0") + "\t\t");
                sb.Append(bins[r,3].ToString("F3") + "\t\t");
                sb.Append(bins[r,4].ToString("F3") + "\n");
            }
            return sb.ToString();
        }

        public String writeAllStatistics()
        {
            StringBuilder sb = new StringBuilder("   DISTRIBUTION STATISTICS \n");
            sb.Append("Total count=" + totalCount + "\n");
            sb.Append("Average = " + DataTools.roundDouble(this.av, 5));
            sb.Append(" +/- " + DataTools.roundDouble(this.sd, 5) + "\n");
            sb.Append("Min=" + this.min + "  Max=" + this.max + "\n");
            sb.Append(write16binDistribution());
            return sb.ToString();
        }

  public static void writeBinDistribution(int[] data, int binWidth)
  { 
    int min;
    int max;
    DataTools.MinMax(data, out min, out max);
    int length = max / binWidth;
    
    int[] counts = new int[length+1];
    for(int i=0;i<data.Length;i++)
    { counts[(data[i] / binWidth)]++;
    }
    
        Console.WriteLine("\n DISTRIBUTION");
        Console.WriteLine("bin width = "+binWidth);
        Console.WriteLine("geneStart \tend \tcount");
        for(int i=0;i<counts.Length;i++)
        {
        Console.WriteLine((i*binWidth)+" \t"+((i+1)*binWidth)+" \t"+counts[i]);   
        }
    }

    public static void writeScoreDistribution(double[] scores)
    {
        double min;
        double max;
        DataTools.MinMax(scores, out min, out max);
        double av;
        double sd;
        AverageAndSD(scores, out av, out sd);

        double[,] histo = get16binDistribution(scores,av,sd);
        Console.WriteLine(" ===== SCORE STATISTICS =====");
        Console.WriteLine("Average ="+av+"+/-"+sd);
        Console.WriteLine("Min score ="+min+"  Max="+max);
        Console.WriteLine(write16binDistribution(histo));
        Console.WriteLine(" =============================");
    }

    public static void writeScoreDistribution(int[] scores)
    {
        double av;
        double sd;
        AverageAndSD(scores, out av, out sd);
        int min;
        int max;
        DataTools.MinMax(scores, out min, out max);
        double[,] histo = get16binDistribution(scores, av, sd);
        Console.WriteLine(" ===== SCORE STATISTICS =====");
        Console.WriteLine("Average ="+av+"+/-"+sd);
        Console.WriteLine("Min score ="+min+"  Max="+max);
        Console.WriteLine(write16binDistribution(histo));
        Console.WriteLine(" =============================");
    }




    public static void writeScoreStatistics(double[] scores)
    {
        double av;
        double sd;
        AverageAndSD(scores, out av, out sd);
        double min;
        double max;
        DataTools.MinMax(scores, out min, out max);
        //double[,] histo = NormalDist.get16binDistribution(scores, av, sd);
        Console.WriteLine(" ===== SCORE STATISTICS =====");
        Console.WriteLine("Average ="+av+"+/-"+sd);
        Console.WriteLine("Min score ="+min+"  Max="+max);
        Console.WriteLine(" =============================");
    }

        public static String formatAvAndSD(double[] avsd, int places)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((avsd[0]).ToString("F4"));
            sb.Append("+/-");
            sb.Append(avsd[1].ToString("F4"));
            return sb.ToString();
        }

        public static double[] CalculateZscores(double[] scores, double av, double sd)
        {
            int length = scores.Length;
            double[] zscores = new double[length];
            for (int i = 0; i < length; i++)
                zscores[i] = (scores[i] - av) / sd;
            return zscores;
        }

        /**
         * returns the Z score (absolute value) of a value with reference to
         * a normal distribution whose av and sd are as passed.
         * @param value
         * @param av
         * @param sd
         * @return
         */
        public static double zScore(double value, double av, double sd)
        {
            return Math.Abs((value - av) / sd);
        }

        public static double zScore2pValue(double z)
        {
            double[] table = { 1.2816, 1.6449, 1.9600, 2.3263, 2.5758, 3.0902, 3.7190, 4.2649 };
            double[] alpha = { 0.100, 0.050, 0.025, 0.010, 0.0050, 0.0010, 0.0001, 0.00001 };
            double p = 0.5;
            for (int i = 7; i >= 0; i--)
            {
                if (z > table[i])
                {
                    p = alpha[i];
                    break;
                }
            }
            return p;
        }


        public static void main(String[] args)
	{
		
    
/*	
  	double[] roots = NormalDist.quadraticRoots(6, -13, 6);
		Console.WriteLine("root1="+roots[0]+"  root2="+roots[1]);
*/


    
  }

    }
}
