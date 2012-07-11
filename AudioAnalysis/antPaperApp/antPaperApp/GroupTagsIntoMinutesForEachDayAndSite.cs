using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace antPaperApp
{
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;

    using LINQtoCSV;

    public class GroupTagsIntoMinutesForEachDayAndSite
    {
        public class OutputHolder
        {
            public Dictionary<int,  List<string>> Minutes { get; set; }

            [CsvColumn(OutputFormat = "d" )]
            public DateTime Day { get; set; }

            public string Site { get; set; }
        }

        public GroupTagsIntoMinutesForEachDayAndSite(FileInfo tagsWithSpeciesNames, DirectoryInfo destinationDirectory)
        {
            Contract.Requires(tagsWithSpeciesNames.Exists);
            Contract.Requires(destinationDirectory.Exists);

            // read in tags
            var fileDescription = new LINQtoCSV.CsvFileDescription { SeparatorChar = ',', FirstLineHasColumnNames = true };
            var context = new CsvContext();

            var tags = context.Read<TagModel>(tagsWithSpeciesNames.FullName, fileDescription).ToArray();

            // aim here is to group into days and sites.
            // each will get its own file
            foreach (TagModel tm in tags)
            {
                if (string.IsNullOrWhiteSpace(tm.SpeciesName))
                {
                    throw new Exception("All species must be populated, this tag isn't, id:" + tm.AudioTagID);
                }
            }

            // unique species tags
            var uniqueSpecies = tags.Select(tm => tm.SpeciesName).Distinct().OrderBy(x => x).ToArray();

            // make distinct days
            DateTime dt0 = new DateTime(2000, 1, 1);
            var days = tags.Select(tm => (int)Math.Floor((tm.ProperDate - dt0).TotalDays)).Distinct().ToArray();
            var sites = tags.Select(tm => tm.Site).Distinct().ToArray();

            // generate a "minutes array"
            var minutes = new Dictionary<int, List<string>>();
            for (int i = 0; i < 1440; i++)
            {
                minutes.Add(i, null);
            }

            // make combinations
            var finals = new List<OutputHolder>();
            foreach (var day in days)
            {
                foreach (var site in sites)
                {
                    var copy = new Dictionary<int, List<string>>(minutes);

                    foreach (var tag in tags)
                    {
                        if (tag.Site == site &&
                            (int)Math.Floor((tag.ProperDate - dt0).TotalDays) == day
                            )
                        {
                            if (copy[(int)tag.ProperDate.TimeOfDay.TotalMinutes] == null)
                            {
                                copy[(int)tag.ProperDate.TimeOfDay.TotalMinutes] = new List<string>();
                            }

                            copy[(int)tag.ProperDate.TimeOfDay.TotalMinutes].Add(tag.SpeciesName);

                        }
                    }


                    finals.Add(new OutputHolder() { Day = dt0.AddDays(day), Site = site, Minutes = copy });

                }
            }



            // file format: YYYY_DD_SiteName.csv
            int tagcount = 0;
            foreach (var outputHolder in finals)
            {
                var fileName = outputHolder.Day.ToString("yyyy_MM_dd") + "_" + outputHolder.Site + ".csv";
                using (var sw = new StreamWriter(destinationDirectory.FullName + "\\" + fileName))
                {
                    // write the header
                    var header = "Minute";
                    foreach (var species in uniqueSpecies)
                    {
                        header += ",\"" + species + "\"";
                    }
                    sw.WriteLine(header);

                    foreach (var minute in outputHolder.Minutes)
                    {
                        string line = minute.Key.ToString(CultureInfo.InvariantCulture);
                        var profile = minute.Value ?? new List<string>();

                        foreach (var species in uniqueSpecies)
                        {
                            var present = profile.Contains(species) ? 1 : 0;
                            line += "," + present;
                            
                            if (present == 1) tagcount++;
                        }

                        sw.WriteLine(line);
                    }
                   
                }

                
            }
        }

    }
}
