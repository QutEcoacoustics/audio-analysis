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
    public class z1440Test
    {
        [TestMethod]
        public void CreateZ1440()
        {
            var tests = new[]     { -1528, -300,   -1, 0, 1, 100, 1399, 1400, 1532};
            var expected  = new[] {  1352, 1140, 1439, 0, 1, 100, 1399, 1400,   92};

            var createdZs = tests.Select(IntegerZ1440.Create).ToArray();
            var newsZs = tests.Select(IntegerZ1440.NewZ1440).ToArray();

            CollectionAssert.AreEqual(expected, createdZs);
            
            // basically the tuple constructor will not validate the tuple
            CollectionAssert.AreNotEqual(expected, newsZs);
            CollectionAssert.AreEqual(tests, createdZs);

        }

        [TestMethod]
        public void BasicOpsZ1440()
        {
            // test cast
            var z1 = IntegerZ1440TopLevelOperations.z1440(1529);
            var z2 = NumericLiteralZ.FromInt32(100);

            Assert.AreEqual(89, z1);
            Assert.AreEqual(100, z2);
            
            // test ops
            var z5 = z1 + NumericLiteralZ.FromInt32(1000);
            var z6 = z1 + z2;
            var z7 = z1 - z2;
            var z8 = z2 - z1;
            
            var z3 = z1 * z2;
            var z4 = z1 / z2;





        }
    }
}
