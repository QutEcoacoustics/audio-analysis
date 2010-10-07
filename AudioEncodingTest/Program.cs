using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AudioEncodingTest
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: <directory> <file pattern>");
                Environment.Exit(1);
            }

            var directory = args[0];
            var pattern = args[1];

            var files = Directory.EnumerateFiles(directory, pattern);

            var options = Config.Parameters.
                Select(param => new KeyValuePair<string, IEnumerable<string>>(param, Config.GetValues(param))).
                ToList();

            Backend.Start(files, options);
        }
    }
}
