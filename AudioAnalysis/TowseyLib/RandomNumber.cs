using System;
using System.Collections.Generic;
using System.Text;

namespace TowseyLib
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
		/// If seed is negative, it will be ignored ie different random order every time method called.
		/// </summary>
		public static int[] RandomizeNumberOrder(int n, int seed)
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
	}
}