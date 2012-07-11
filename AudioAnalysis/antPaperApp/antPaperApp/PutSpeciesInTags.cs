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
            
            var mappings = m.ToDictionary(
                    sc => sc.Call, sc => sc.Species, StringComparer.InvariantCultureIgnoreCase);

            // get tags
            var tags = context.Read<TagModel>(originalTags.FullName, fileDescription).ToArray();

            // for each tag, add species information
            foreach (var tagModel in tags)
            {
                if (mappings.ContainsKey(tagModel.Tag))
                {
                    tagModel.SpeciesName = mappings[tagModel.Tag];

                }
                else
                {
                    Debugger.Break();
                    Debug.WriteLine(tagModel.Tag);
                }
            }

            // save modified results
            context.Write(tags, newTags.FullName);

        }
    }
}
