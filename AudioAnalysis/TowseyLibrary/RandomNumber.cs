using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace TowseyLibrary
{
	public class RandomNumber
	{
		Random random;

		public RandomNumber()
		{
			random = new Random();
		}

		public RandomNumber(int seed)
		{
			random = new Random(seed);
		}

		/// <summary>
		/// returns a random number between 0.0 and 1.0
		/// </summary>
		public double GetDouble()
		{
			return random.NextDouble();
		}

		public double GetDouble(int max)
		{
			return random.NextDouble() * max;
		}

		/// <summary>
		/// generates numbers 0 to max-1
		/// </summary>
		public int GetInt(int max)
		{
			return random.Next(max);
		}

		/// <summary>
		/// generates numbers 1 - 100
		/// </summary>
		public int GetRandomPercent()
		{
			return 1 + (int)(99.0 * random.NextDouble());
		}

		/// <summary>
		/// Returns integers up to N in random order.
		/// Use of seed will always return the same order.
		/// Pass a negative seed value to ignore it and to have a different random order every time method called.
        /// Use this method if you want random numbers up to N without replacement.
		/// </summary>
		public static int[] RandomizeNumberOrder(int N, int seed)
		{
			RandomNumber rn = new RandomNumber(seed);
			if (seed < 0) rn = new RandomNumber();
			int R;      //: word;       {a random number between 0 and k-1}
			int valueAtIndexK;  // : word;      {holder for random number}

			int[] randomArray = new int[N];
			for (int i = 0; i < N; i++) randomArray[i] = i;   // integers in ascending order

			for (int k = N - 1; k >= 0; k--) // in decending order
			{
				R = rn.GetInt(k);                 // a random integer between 0 and k
				valueAtIndexK  = randomArray[k];  // swap the numbers in position K and romdon  position R 
				randomArray[k] = randomArray[R];
				randomArray[R] = valueAtIndexK;
			}
			return randomArray;
		} //end of RandomizeNumberOrder()


		/// <summary>
		/// IMPORTANT - THIS METHOD NEEDS WORK!!
		/// returns the passed array but with the elements in a random order
		/// see method above which was originally written for FuzzyART in 1995
		/// </summary>
		public static int[] RandomizeArray(int[] array, int seed)
		{
			int N = array.Length;
			int[] rArray = new int[N];

			Random r = new Random(seed);

			//for (int i = 0; i < N; i++)  // select instances at random without replacement
			//{
			//    int rn = r.getInt(N - i); //random number
			//    rArray[i] = array[rn];
			//  //  DataTools.removeValue(rn);
			//}
			return rArray;
		}


        /// <summary>
        /// Returns N random integers between 0 - K-1 without replacement.
        /// If seed is negative, it will be ignored ie different random order every time method called.
        /// </summary>
        public static int[] RandomNumbersWithoutReplacement(int n, int seed)
        {
            RandomNumber rn = new RandomNumber(seed);
            if (seed < 0) rn = new RandomNumber();
            int r;      //: word;      {a random number between 0 and k-1}
            int dummy;  // : word;      {holder for random number}

            int[] randomArray = new int[n];
            for (int i = 0; i < n; i++) randomArray[i] = i;   // integers in ascending order

            for (int k = n - 1; k >= 0; k--)
            {
                r = rn.GetInt(k);       //a random integer between 0 and k
                dummy = randomArray[k];
                randomArray[k] = randomArray[r];
                randomArray[r] = dummy;
            }
            return randomArray;
        } //end of RandomizeNumberOrder()



        /// <summary>
        /// generates vectors of numbers numbers 0 - 1.0
        /// </summary>
        public static double[] GetRandomVector(int length, RandomNumber rn)
        {
            double[] v = new double[length];
            v[0] = rn.GetDouble();
            v[1] = rn.GetDouble();
            v[2] = rn.GetDouble();
            return v;
        }

        public static int[] GetVectorOfRandomIntegers(int[] maxValues, RandomNumber rn)
        {
            int length = maxValues.Length;
            int[] array = new int[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = rn.GetInt(maxValues[i]);
            }
            return array;
        }

        /// <summary>
        /// generates numbers 1 - 100
        /// </summary>
        public static void GetRandomDistancesInEuclidianSpace(int trialCount, int dimensions)
        {
            double[] distanceArray = new double[trialCount];

            //int seed = 123456;
            int seed = (int)DateTime.Now.Ticks;
            var rn = new RandomNumber(seed);
            for (int i = 0; i < trialCount; i++)
            {
                double[] v1 = RandomNumber.GetRandomVector(dimensions, rn);
                double[] v2 = RandomNumber.GetRandomVector(dimensions, rn);
                distanceArray[i] = DataTools.EuclidianDistance(v1, v2);
            }
            double av, sd;
            NormalDist.AverageAndSD(distanceArray, out av, out sd);
            double[] avAndsd = {av, sd };
            Console.WriteLine(NormalDist.formatAvAndSD(avAndsd, 5));
            Console.WriteLine("Min --> Max: {0:f3} --> {1:f3}", distanceArray.Min(), distanceArray.Max());
        } //GetRandomDistancesInEuclidianSpace()

    }
}