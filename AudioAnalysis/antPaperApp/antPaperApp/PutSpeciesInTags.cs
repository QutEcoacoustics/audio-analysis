using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace antPaperApp
{
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;

    using LINQtoCSV;

    public class PutSpeciesInTags
    {
        public PutSpeciesInTags(FileInfo speciesCallMappingFile, FileInfo originalTags,  FileInfo newTags)
        {
            Contract.Requires(speciesCallMappingFile.Exists);
            Contract.Requires(originalTags.Exists);
            Contract.Requires(!newTags.Exists);

            // get mapping
            var fileDescription = new LINQtoCSV.CsvFileDescription { SeparatorChar = ',', FirstLineHasColumnNames = true };
            var context = new CsvContext();

            var m = context.Read<SpeciesCall>(speciesCallMappingFile.FullName, fileDescription).ToArray();

            var nullCalls = m.Where(x => x.Call == null).ToArray();
            if (nullCalls.Length != 0)
            {
                Debugger.Break();
            }
            
            var mappings = m.ToDictionary(
                    sc => sc.Call, sc => sc.Species, StringComparer.InvariantCultureIgnoreCase);

            // get tags
            var tags = context.Read<TagModel>(originalTags.FullName, fileDescription).ToArray();

            var problems = new List<string>();
            // for each tag, add species information
            foreach (var tagModel in tags)
            {
                if (mappings.ContainsKey(tagModel.Tag))
                {
                    tagModel.SpeciesName = mappings[tagModel.Tag];

                }
                else
                {
                    //Debugger.Break();
                    //Debug.WriteLine(tagModel.Tag);
                    problems.Add(tagModel.Tag);
                }
            }

            if (problems.Count > 0 )
            {
                var ps = string.Join(Environment.NewLine, problems);
                Debug.WriteLine(ps);
                Debugger.Break();
                return;
            }

            // save modified results
            context.Write(tags, newTags.FullName);

        }
    }
}
