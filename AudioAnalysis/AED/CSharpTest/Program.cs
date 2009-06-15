using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpTest
{
    class Program
    {
        static void Main(string[] args)
        {
            double[,] a = new double[5, 5] { { 1, 2, 3, 4, 5 }, { 1, 2, 3, 4, 5 }, { 1, 2, 3, 4, 5 }, { 1, 2, 3, 4, 5 }, { 1, 2, 3, 4, 5 } };
            double[,] r = Wiener.wiener2(a, 3);
            Console.WriteLine(r + "");
            Console.ReadKey();
        }
    }
}
