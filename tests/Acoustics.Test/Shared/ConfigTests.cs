// <copyright file="ConfigTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Acoustics.Shared.ConfigFile;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigTests
    {
        public const string TestYaml = @"---
a: 123
b: 456
c:
  d: whatever
  e: 123.456
f:
  - g: 1
    h: 2
  - i: hello
  - j: world
k:
  - ~
  - null
  - Null
  - NULL
  -
l: true
m: false
";

        private Config config;

        [TestInitialize]
        public void Initialize()
        {
            var reader = new StringReader(TestYaml);
            this.config = new Config(reader, null);
        }

        [TestMethod]
        public void ToDictionary()
        {
            var expected = new Dictionary<string, string>()
                               {
                                   { "a", "123" },
                                   { "b", "456" },
                                   { "c/d", "whatever" },
                                   { "c/e", "123.456" },
                                   { "f/0/g", "1" },
                                   { "f/0/h", "2" },
                                   { "f/1/i", "hello" },
                                   { "f/2/j", "world" },
                                   { "k/0", "~" },
                                   { "k/0", "null" },
                                   { "k/0", "Null" },
                                   { "k/0", "NULL" },
                                   { "l", "true" },
                                   { "m", "false" },
                               };

#pragma warning disable 618 // obsolete method still needs to have tests
            var actual = this.config.ToDictionary();
#pragma warning restore 618
            CollectionAssert.AreEquivalent(expected, actual);
        }

        [DataTestMethod]
        [DataRow("a", 123)]
        [DataRow("b", 456)]
        [DataRow("c/d", "whatever")]
        [DataRow("c/e", 123.456)]
        [DataRow("f/0/g", 1)]
        [DataRow("f/0/h", 2)]
        [DataRow("f/1/i", "hello")]
        [DataRow("f/2/j", "world")]
        [DataRow("k/0", null)]
        [DataRow("k/1", null)]
        [DataRow("k/2", null)]
        [DataRow("k/3", null)]
        [DataRow("l", true)]
        [DataRow("m", false)]
        public void TestGetValue(string path, object value)
        {
            switch (value)
            {
                case bool b:
                    var actualBool = this.config.GetBoolOrNull(path);
                    Assert.AreEqual(b, actualBool);
                    break;
                case double d:
                    var actualDouble = this.config.GetDoubleOrNull(path);
                    Assert.AreEqual(d, actualDouble);
                    break;
                case int i:
                    var actualInt = this.config.GetIntOrNull(path);
                    Assert.AreEqual(i, actualInt);
                    break;
                case string s:
                    var actualString = this.config.GetDoubleOrNull(path);
                    Assert.AreEqual(s, actualString);
                    break;
                case null:
                    Assert.IsTrue(this.config.TryGetDouble(path, out var actual));
                    Assert.AreEqual(null, actual);
                    Assert.IsTrue(this.config.TryGetDouble(path, out var actual1));
                    Assert.AreEqual(null, actual1);
                    Assert.IsTrue(this.config.TryGetDouble(path, out var actual2));
                    Assert.AreEqual(null, actual2);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        // note the values here aren't actually used other than to get their type and call right method
        [DataTestMethod]
        [DataRow("", 123)]
        [DataRow("z", 456)]
        [DataRow("///", 456)]
        [DataRow("\\", 456)]
        [DataRow("a", "whatever")]
        [DataRow("c/d", 123.456)]
        [DataRow("f/10/g", 1)]
        [DataRow("f/-1/h", 2)]
        [DataRow("f/a/i", "hello")]
        [DataRow("f/b/j", "world")]
        public void TestGetValueFailing(string path, object value)
        {
            switch (value)
            {
                case double _:
                    Assert.IsFalse(this.config.TryGetDouble(path, out var _));
                    break;
                case int _:
                    Assert.IsFalse(this.config.TryGetInt(path, out var _));
                    break;
                case string _:
                    Assert.IsFalse(this.config.TryGetString(path, out var _));
                    break;
            }
        }
    }
}
