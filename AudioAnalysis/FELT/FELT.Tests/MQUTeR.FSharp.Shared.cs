using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FELT.Tests
{
    using System.Diagnostics;
    using System.IO;

    using MQUTeR.FSharp.Shared;

    using Microsoft.FSharp.Collections;

    [TestClass]
    public class MQUTeR_FSharp_Shared_CSV
    {
        [TestMethod]
        public void GuessTypes_strings()
        {
            string[] test = new[] { "abc", "donkey", "123" };

            var fSharpList = ListModule.OfSeq(test);

            DataType dataType = CSV.guessType(fSharpList);

            Assert.AreEqual(dataType, DataType.Text);

            dataType = CSV.guessType(ListModule.Reverse(fSharpList));

            Assert.AreEqual(dataType, DataType.Text);
        }

        [TestMethod]
        public void GuessTypes_dates()
        {
            string[] test = new[] { "2010-03-06T14:53:19.723", "2010/03/06T14:53:19.723", "09-Jan-12 15:11:00" };

            var fSharpList = ListModule.OfSeq(test);

            DataType dataType = CSV.guessType(fSharpList);

            Assert.AreEqual(dataType, DataType.Date);
        }


        [TestMethod]
        public void GuessTypes_numbers()
        {
            string[] test = new[] { "0.0125", "-16", "13" };

            var fSharpList = ListModule.OfSeq(test);

            DataType dataType = CSV.guessType(fSharpList);

            Assert.AreEqual(dataType, DataType.Number);
        }


        [TestMethod]
        public void RowToList()
        {

            string test =
                "Hello, this is  a,test,\"hell,lo\", to see,, what happen, soooo,\"\"\"we need a value\"\", said bree\", something else ";
            FSharpList<string> expected = ListModule.OfArray( new[]
                {
                    "Hello", " this is  a", "test", "hell,lo", " to see", "", " what happen", " soooo",
                    "\"we need a value\", said bree", " something else "
                });

            string[] result = null;// = CSV.rowToList(',', test);


            Stopwatch st2 = Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
            {
                result = CSV.rowToList(',', test);
            }

            st2.Stop();

            Console.WriteLine("Time taken for a 1000 iterations: " + st2.ElapsedMilliseconds);

            var enumer = result.AsEnumerable().GetEnumerator();
            foreach (var value in expected)
            {
                enumer.MoveNext();
                var res = enumer.Current;
                Assert.AreEqual(value, res);
               

            }

        }

        [TestMethod]
        public void CSVToVectors()
        {
            string csvDemo = @"D:\Work\Sensors\AudioAnalysis\FELT\FELT.Tests\democsv2.csv";

            var lines = File.ReadAllLines(csvDemo);

            Stopwatch st = Stopwatch.StartNew();

            var vectors = CSV.csvToVectors(lines);

            st.Stop();

            Console.WriteLine("Time taken: " + st.ElapsedMilliseconds);

            string[] headers = vectors.Item1;
            CollectionAssert.AreEqual(new[]{"Age","Coolness","Date of Birth","Somehting else"}, headers);

            var numbers = vectors.Item2;
            Assert.AreEqual(2, numbers.Count);
            Assert.IsTrue(numbers.ContainsKey("Age"));
            Assert.IsTrue(numbers.ContainsKey("Coolness"));
            double[] doubles = numbers["Age"];
            CollectionAssert.AreEqual(new double[] { 1, 2, 3, 4, 5, 6, 1, 8, 9, 10 }, doubles);
            CollectionAssert.AreEqual(new double[] { 0.2, 12.05, 15, 30, 0.69, 0.82, 5.73, 1.25, 11.2, 0.2235 }, numbers["Coolness"]);

            var strings = vectors.Item3;
            Assert.AreEqual(1, strings.Count);
            Assert.IsTrue(strings.ContainsKey("Somehting else"));
            string[] actual = strings["Somehting else"];
            CollectionAssert.AreEqual(new string[] { "this", "is ", "a", "99", "string, field", "with a ", "few", "\"si\"mple\"", "tests", "!" }, actual);

            var dates = vectors.Item4;
            Assert.IsTrue(dates.ContainsKey("Date of Birth"));
            CollectionAssert.AreEqual(
                (new string[]
                    {
                        "01-07-89 00:00:00.00", "02-07-89 12:00:00.00", "03-07-89 10:00:00.00", "04-07-89 00:03:00.00",
                        "05-07-89 00:00:00.00", "06-07-89 00:00:00.00", "07-07-89 00:40:00.00", "08-07-1989 19:53:12.623",
                        "09-07-89 05:00:00.00", "10-07-89 00:00:13.00"
                    }).Select(DateTime.Parse).ToArray(),
                dates["Date of Birth"]);


        }
    }
}
