using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FELT.Tests
{
    using System.Collections;

    using MQUTeR.FSharp.Shared;

    using Microsoft.FSharp.Collections;


    [TestClass]
    public class GroupTrainerTest
    {
        [TestMethod]
        public void GroupTainerWorks()
        {
            var gt = new Trainers.GroupTrainer();

            var hdrs = new FSharpMap<string, DataType>(new[] { new Tuple<string, DataType>("name", DataType.Text),  new Tuple<string, DataType>("age", DataType.Number) });

            var col1 = new Tuple<string, Value[]>(
                "name", new Value[] { new Text("billy"), new Text("billy"), new Text("ann") });

            var col2 = new Tuple<string, Value[]>(
                "age", new Value[] { new Number(3.0), new Number(4.0), new Number(5.0) });



            var instances = new FSharpMap<string, Value[]>(new[] { col1, col2 });

            var data = new Data(DataSet.Training, hdrs, instances, "Gender", new[] { "Boy", "Boy", "Girl" });

            var result = gt.Train(data);

            CollectionAssert.AreEqual(new[] { "Boy", "Girl" }, result.Classes);

            Assert.AreEqual("Gender", result.ClassHeader);

            Assert.AreEqual(hdrs, result.Headers);

            Assert.AreEqual(DataSet.Training, result.DataSet);

            var names = result.Instances["name"];
            var ages = result.Instances["age"];

            var expected = new Value[] { new Number(3.5), new Number(5) };
            CollectionAssert.AreEqual(expected, ages);

            var values = new Value[]
                {
                    new AverageText("billy", new[] { new Tuple<string, double>("billy", 0.5) }),
                    new AverageText("ann", new[] { new Tuple<string, double>("ann", 1) })
                };
            CollectionAssert.AreNotEqual(
                values,
                names);

            var values2 = new Value[]
                {
                    new AverageText("billy", new[] { new Tuple<string, double>("billy", 1.0) }),
                    new AverageText("ann", new[] { new Tuple<string, double>("ann", 1.0) })
                };
            CollectionAssert.AreEqual(
                values2,
                names);


        }
    }
}
