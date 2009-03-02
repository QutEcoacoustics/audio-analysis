using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TowseyLib
{
    public static class FunctionalTests
    {


        public static void AssertAreEqual(double[,] a, double[,] b)
        {
            if (a.GetLength(0) != b.GetLength(0))
                throw new Exception("First dimension is not equal");
            if (a.GetLength(1) != b.GetLength(1))
                throw new Exception("Second dimension is not equal");
            for (int i = 0; i < a.GetLength(0); i++)
                for (int j = 0; j < a.GetLength(1); j++)
                    if (a[i, j] != b[i, j])
                        throw new Exception("Not equal: " + i + "," + j);
        }


        public static void AssertAreEqual(double[] a, double[] b)
        {
            if (a.GetLength(0) != b.GetLength(0))
                throw new Exception("First dimension is not equal");
            for (int i = 0; i < a.GetLength(0); i++)
                if (a[i] != b[i])
                    throw new Exception("Not equal: " + i);
        }


        public static void AssertAreEqual(string a, string b)
        {
            if (a.Length != b.Length)
                throw new Exception("String lengths are not equal");
            if (String.Compare(a, b) != 0)
                throw new Exception("Strings a and b are not equal.  " + a + " != " + b);
        }


        public static void AssertAreEqual(FileInfo A, FileInfo B, bool throwException)
        {
            Log.WriteLine("TESTING SIMILARITY OF TWO FILES:- " + A.Name + "  and  " + B.Name);
            List<string> listA = FileTools.ReadTextFile(A.FullName);
            List<string> listB = FileTools.ReadTextFile(B.FullName);

            if (listA.Count != listB.Count)
            {
                if (throwException) throw new Exception("Files do not have the same number of lines: " + listA.Count + " != " + listB.Count);
                else Console.WriteLine("#### WARNING: Files do NOT have same number of lines: " + listA.Count + " != " + listB.Count);
            }

            for (int i = 1; i < listA.Count; i++) //skip first line
            {
                if (String.Compare(listA[i], listB[i]) != 0) // if (listA[i] != listB[i])
                {    
                    if (throwException) throw new Exception("Line " + i + " of files a and b not same: " + listA[i] + " != " + listB[i]);
                    else Console.WriteLine("#### WARNING: Line " + i + " of files a and b not same: " + listA[i] + " != " + listB[i]);
                }
            }
            Log.WriteLine("\t\t\t###################### PASS SIMILARITY TEST:- " + A.Name + "  and  " + B.Name + "\n");
        }


    }
}
