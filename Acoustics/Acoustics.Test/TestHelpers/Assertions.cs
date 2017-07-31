// <copyright file="Assertions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class Assertions
    {
        public static void DirectoryExists(this Assert assert, DirectoryInfo directory)
        {
            DirectoryExists(assert, directory.FullName);
        }

        public static void DirectoryExists(this Assert assert, string path)
        {
            Assert.IsTrue(
                Directory.Exists(Path.GetFullPath(path)),
                $"Expected path {path} to exist but it could not be found");
        }

        public static void PathExists(this Assert assert, string path, string message = "")
        {
            path = Path.GetFullPath(path);

            Assert.IsTrue(File.Exists(path), $"Expected path {path} to exist but it could not be found. {message}");
        }

        public static void PathNotExists(this Assert assert, string path, string message = "")
        {
            path = Path.GetFullPath(path);

            Assert.IsFalse(File.Exists(path), $"Expected path {path} NOT to exist but it was found. {message}");
        }
    }
}
