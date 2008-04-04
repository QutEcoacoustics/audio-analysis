using System;
using System.Collections.Generic;
using System.Text;

namespace TowseyLib
{
 
/**
 * @author towsey
 *
 * TODO To change the template for this generated type comment go to
 * Window - Preferences - Java - Code Generation - Code and Comments
 */
public class RandomNumber
{
	Random R = new Random();

    /**
     * 
     */
    public RandomNumber()
    {
    }

	/**
	 * 
	 */
	public RandomNumber(int seed)
    {
        Random R = new Random(seed);
	}
	
	
	

    /// <summary>
    /// returns a random number between 0.0 and 1.0
    /// </summary>
    /// <returns></returns>
	public double getDouble()
	{ return R.NextDouble();		
	}

  public double getDouble(int max)
  { return R.NextDouble()* max;    
  }

	/**
	 * this generates numbers 0 to max-1
	 * @param max
	 * @return
   */
    public int getInt(int max)
    { return (int)Math.Floor(getDouble(max));		
    }

	/**
	 * this generates numbers 1 - 100
	 * @return
	 */
	public int getRandomPercent()
	{ return  1+ (int)(99.0 * R.NextDouble());
	}

  /**
   * returns a number that has standard normal distribution
   * ie mean = 0 and SD=1;
   */
    //public double getGaussian()
    //{
    //    return R.NextGaussian();
    //}


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
    /// IMPORTANT - THIS METHOD NEEDS WORK!!
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
