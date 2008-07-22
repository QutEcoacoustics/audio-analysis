using System;
using System.Collections.Generic;
using System.Text;

namespace TowseyLib
{
 
public class RandomNumber
{
	Random R;
    
    /// <summary>
    /// CONSTRUCTOR 1
    /// </summary>
    public RandomNumber()
    {
        R = new Random();
    }

    /// <summary>
    /// CONSTRUCTOR 2
    /// </summary>
    public RandomNumber(int seed)
    {
        R = new Random(seed);
	}
	
    /// <summary>
    /// returns a random number between 0.0 and 1.0
    /// </summary>
    /// <returns></returns>
	public double getDouble()
	{ 
        return R.NextDouble();		
	}

    public double getDouble(int max)
    { 
        return R.NextDouble()* max;    
    }

    /// <summary>
    ///  generates numbers 0 to max-1
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public int getInt(int max)
    { 
        return (int)Math.Floor(getDouble(max));		
    }

    /// <summary>
    /// generates numbers 1 - 100
    /// </summary>
    /// <returns></returns>
	public int getRandomPercent()
	{ return  1+ (int)(99.0 * R.NextDouble());
	}


  /**
   * selects C objects at random out of array of N
   * without replacements
   */
  //public bool[] selectCombination(int N, int C)
  //{ if(C>N)  return null;
  //  if(C<=0) return null;  
  //  bool[] array = new bool[N];

  //  //if select all then return array of true
  //  if(C==N)
  //  { for(int i=0;i<N;i++) array[i] = true;
  //    return array;
  //  }
    
  //  //init boolean array to false
  //  for(int i=0;i<N;i++) array[i] = false;
    
  //  // make a vector containing numbers
  //  Vector v = new Vector();
  //  for(int i=0;i<N;i++) v.add(new Integer(i));
    
  //  for(int i=0;i<C;i++)  // select an object C times
  //  { int rn = R.nextInt(N-i);
  //    int value = ((Integer)v.get(rn)).intValue();
  //    array[value] = true;
  //    v.remove(rn);
  //  }
    
  //  return array;
  //}


    /// <summary>
    /// returns integers up to N in random order.
    /// Use of seed will always return the same order.
    /// If seed is negative, it will be ignored ie different random order every time method called.
    /// </summary>
    /// <param name="n"></param>
    /// <param name="seed"></param>
    /// <returns></returns>
    public static int[] RandomizeNumberOrder(int n, int seed)
    {
        RandomNumber rn = new RandomNumber(seed);
        if(seed <0) rn = new RandomNumber();
        int r;      //: word;      {a random number between 0 and k-1}
        int dummy;  // : word;      {holder for random number}

        int[] randomArray = new int[n];
        for (int i = 0; i < n; i++) randomArray[i] = i;   // integers in ascending order

        for (int k = n - 1; k >= 0; k--)
        {
            r = rn.getInt(k);       //a random integer between 0 and k
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
    /// <param name="array"></param>
    /// <param name="seed"></param>
    /// <returns></returns>
    public static int[] randomizeArray(int[] array, int seed)
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
 
 
  /**
   * returns a boolean array as a string of bits 0/1
   */
  //public static String booleanArray2String(bool[] BB)
  //{
  //    StringBuilder sb = new StringBuilder();
  //  for(int i=0;i<BB.Length;i++)
  //  { if (BB[i]) sb.Append("1");
  //    else       sb.Append("0");  
  //  }
  //  return sb.ToString();
  //}   
    
	public static void main(String[] args)
	{ 
		
        //if(true)
        //{
        //    int[] array = {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20};
        //    array = randomizeArray(array, 1234);
        //    for(int i=0;i<array.Length;i++)Console.WriteLine(array[i]);
        //    Console.WriteLine("FINISHED!");	 
        //    //System.exit(999);
        //}
		
		
		
		
    //    RandomNumber R = new RandomNumber();
	
    //  int COUNT  = 10;
    //  int SELECT = 9;
    //  int[] array = new int[COUNT];
    //for(int n=0;n<COUNT;n++) array[n] = 0;
    
    //for(int i=0;i<1000;i++)
    //{ bool[] a = R.selectCombination(COUNT,SELECT);
    //  for(int n=0;n<COUNT;n++) {if(a[n]) array[n]++;}
          
    //}
    //for(int n=0;n<COUNT;n++) Console.Write(array[n]+" ");
    //Console.WriteLine("");

    /*********************************************************/
//    int max = 3;    
//	  int[] array = new int[max];
//    for(int i=0;i<1000;i++)
//    { array[R.getInt(max)]++;
//    }
//    for(int n=0;n<max;n++) Console.Write(array[n]+" ");
//    Console.WriteLine("");
    /*********************************************************/

	} 

}

}
