using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MQUTeR.FSharp.Shared;

namespace FELT.Tests
{
    using Microsoft.FSharp.Numerics;

    [TestClass]
    public class Z1440Test
    {
        [TestMethod]
        public void CreateZ1440()
        {
            var tests = new[] { -1528, -300, -1, 0, 1, 100, 1399, 1400, 1532 };
            var expected = new[] { 1352, 1140, 1439, 0, 1, 100, 1399, 1400, 92 };

            var createdZs = tests.Select(IntegerZ1440.Create).Select(x => x.ToInt32()).ToArray();
            var newsZs = tests.Select(IntegerZ1440.NewZ1440).Select(x => x.ToInt32()).ToArray();

            CollectionAssert.AreEqual(expected, createdZs);
            
            // basically the tuple constructor will not validate the tuple
            CollectionAssert.AreNotEqual(expected, newsZs);
            CollectionAssert.AreEqual(tests, newsZs);
        }

        [TestMethod]
        public void BasicOpsZ1440()
        {
            // test cast
            var z1 = IntegerZ1440.Create(1529);
            var z2 = NumericLiteralZ.FromInt32(100);

            Assert.AreEqual(89, (int)z1);
            Assert.AreEqual(100, (int)z2);
            
            // test ops
            var z5 = z1 + NumericLiteralZ.FromInt32(1000);
            var z6 = z1 + z2;
            var z7 = z1 - z2;
            var z8 = z2 - z1;
            
            var z3 = z1 * z2;
            var z4 = z1 / z2;
            var z9 = NumericLiteralZ.FromInt32(1000) / NumericLiteralZ.FromInt32(50);

            Assert.AreEqual(1089, z5.ToInt32());
            Assert.AreEqual(189, z6.ToInt32());
            Assert.AreEqual(1429, z7.ToInt32());
            Assert.AreEqual(11, z8.ToInt32());
            Assert.AreEqual(260, z3.ToInt32());

            // this is the least accurate of all the operations
            Assert.AreEqual(0, z4.ToInt32());
            Assert.AreEqual(1, (int)(z2 / z1));
            Assert.AreEqual(20, z9.ToInt32());

        }

        [TestMethod]
        public void StandardDeviationTest()
        {
            // standard deviation tests
            var a = new[] { 700, 704, 708, 712, 735 };
            var b = new[] { 1400, 1420, 1440, 20, 40 };

            var az = a.Select(IntegerZ1440.Create).ToArray();
            var bz = b.Select(IntegerZ1440.Create).ToArray();

            // test summation
            var suma = Maths.Array.sum<IntegerZ1440, IntegerZ1440>(az);
            var sumb = Maths.Array.sum<IntegerZ1440, IntegerZ1440>(bz);

            Assert.AreEqual(IntegerZ1440.Create(679), suma);
            Assert.AreEqual(IntegerZ1440.Create(0), sumb);

            // test mean
            var meana = Maths.Array.meanZ1440(az);
            var meanb = Maths.Array.meanZ1440(bz);

            Assert.AreEqual(711.8, meana);
            Assert.AreEqual(40.0, meanb);

            // test standard deviation
            var stda = Maths.Array.stdDeviation<IntegerZ1440, IntegerZ1440, IntegerZ1440, IntegerZ1440, float>(az);
            var stdb = Maths.Array.stdDeviation<IntegerZ1440, IntegerZ1440, IntegerZ1440, IntegerZ1440, float>(bz);
                             
            Assert.AreEqual(13.7186005117140, stda, MathTest.minDeltaForDoubleTests);
            Assert.AreEqual(31.6227766016838, stdb, MathTest.minDeltaForDoubleTests);
        }
    }
}
