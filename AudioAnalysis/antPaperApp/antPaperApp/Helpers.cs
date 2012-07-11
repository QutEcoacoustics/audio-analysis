using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace antPaperApp
{
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Text.RegularExpressions;

    public static class Helpers
    {

            private static Random random = new Random();

            public static T GetRandomElement<T>(this IEnumerable<T> list)
            {
                // If there are no elements in the collection, return the default value of T
                if (list.Count() == 0)
                    return default(T);

                return list.ElementAt(random.Next(list.Count()));
            }



        public static List<SiteDayProfile> ReadFiles(DirectoryInfo directory)
        {
            var regex = new Regex(@"(\d{4})_(\d{2})_(\d{2})_(.*).csv");
            var profiles = new List<SiteDayProfile>(3000);

            foreach (var file in directory.EnumerateFiles("*.csv"))
            {
                var match = regex.Match(file.Name);
                Contract.Assert(match.Success);

                var day = new DateTime(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value));
                var site = match.Groups[4].Value;


                var lines = File.ReadAllLines(file.FullName);

                if (lines.Length != 1441)
                {
                    throw new Exception();
                }

                // extract headers
                var hdr = lines.First();
                var cols =
                    hdr.Split(',').Select(
                        str => str.Trim('"'))
                    .ToArray();

                Contract.Assert("minute".Equals(cols.First(), StringComparison.InvariantCultureIgnoreCase));
                var species = cols.Skip(1).ToArray();

                // we want a profile for every species, set thatup now


                // extract minutes (we are looking for species profiles, e.g. column wise)
                var rowsAndCols = lines.Skip(1).Select(line => line.Split(',')).ToArray();

                for (int colIndex = 1; colIndex < (rowsAndCols[0]).Length; colIndex++)
                {
                    // subtract one because species list doesn't have minute col
                    var speciesName = species[colIndex - 1];
                    var minutes = new Dictionary<int, int>(1440);

                    for (int rowIndex = 0; rowIndex < rowsAndCols.Length; rowIndex++)
                    {
                        var cell = rowsAndCols[rowIndex][colIndex];
                        var val = int.Parse(cell);
                        Contract.Assert(val == 0 || val == 1);

                        var minute = int.Parse(rowsAndCols[rowIndex][0]);

                        minutes[minute] = val;
                    }


                    profiles.Add(new SiteDayProfile()
                    {
                        Day = day,
                        MinuteProfile = minutes,
                        Site = site,
                        SpeciesName = speciesName
                    });
                }
            }

            return profiles;
        }




    }
}
