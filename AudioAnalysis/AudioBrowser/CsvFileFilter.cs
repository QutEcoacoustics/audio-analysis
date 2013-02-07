using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioBrowser
{
    using System.IO;

    using LINQtoCSV;

    using QutSensors.Shared;

    public class CsvFileFilter
    {
        public IEnumerable<FilteredCsv> Run(DirectoryInfo topDir, IEnumerable<CsvFilter> filters)
        {
            var files = GetFiles(topDir);

            var results = files.Select(f => Filter(f, filters));

            return results;
        }

        public IEnumerable<FileInfo> GetFiles(DirectoryInfo topDir)
        {
            var files = Directory.EnumerateFiles(topDir.FullName, "*.csv", SearchOption.AllDirectories);

            return files.Select(i => new FileInfo(i));
        }

        private class CsvFileFilterDataRow : List<DataRowItem>, IDataRow
        {
        }

        private IEnumerable<CsvFileFilterDataRow> GetDataRows(FileInfo file)
        {
            CsvFileDescription inputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = true // skips first line
            };

            var cc = new CsvContext();

            var lines = cc.Read<CsvFileFilterDataRow>(file.FullName, inputFileDescription);
            return lines;
        }

        private CsvFileFilterDataRow GetHeaders(FileInfo file)
        {
            CsvFileDescription inputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = false // otherwise can't get headers
            };

            var cc = new CsvContext();

            var lines = cc.Read<CsvFileFilterDataRow>(file.FullName, inputFileDescription).First();
            return lines;
        }

        public class CsvFilter
        {
            public string FieldName { get; set; }
            public double Maximum { get; set; }
            public double Minimum { get; set; }
        }

        public class FilteredCsv
        {
            public FilteredCsv()
            {
                Rows = new List<double>();
                Headers = new List<string>();
                ColumnStats = new Dictionary<string, StatDescriptiveResult>();
            }

            public List<double> Rows { get; private set; }
            public List<string> Headers { get; private set; }
            public Dictionary<string, StatDescriptiveResult> ColumnStats { get; private set; }
            public FileInfo ProcessedFile { get; set; }
            public IEnumerable<CsvFilter> Filters { get; set; }
        }

        public FilteredCsv Filter(FileInfo file, IEnumerable<CsvFilter> filters)
        {
            // filters are AND: all filters must match, otherwise reject row
            // if no filter's column name is in the csv headers, reject the row

            var rows = GetDataRows(file);

            Dictionary<string, List<double>> intermediateColumnStats = new Dictionary<string, List<double>>();

            // first line should be column headers
            var headers = GetHeaders(file);
            var filterResult = new FilteredCsv { ProcessedFile = file, Filters = filters };
            filterResult.Headers.AddRange(headers.Select(i => i.Value));

            foreach (var row in rows)
            {
                for (var columnIndex = 0; row.Count > columnIndex; columnIndex++)
                {
                    var matchCount = 0;
                    var rejectCount = 0;
                    var colHeader = headers[columnIndex].Value;
                    var value = double.Parse(row[columnIndex].Value);

                    if (!intermediateColumnStats.ContainsKey(colHeader))
                    {
                        intermediateColumnStats.Add(colHeader, new List<double>());
                    }

                    intermediateColumnStats[colHeader].Add(value);


                    foreach (var filter in filters)
                    {
                        if (filter.FieldName == colHeader)
                        {
                            if (filter.Minimum <= value && filter.Maximum >= value)
                            {
                                // filter matches, include this row if all filters match
                                matchCount++;
                            }
                            else
                            {
                                // filter does not match, reject this row
                                rejectCount++;
                            }
                        }
                    }

                    if (matchCount > 1 && rejectCount == 0)
                    {
                        filterResult.Rows.AddRange(row.Select(i => double.Parse(i.Value)));
                    }
                }
            }

            // calculate stats for columns
            foreach (var column in intermediateColumnStats)
            {
                var stats = new StatDescriptive(column.Value.ToArray());
                stats.Analyze();

                filterResult.ColumnStats.Add(column.Key, stats.Result);
            }


            return filterResult;
        }
    }
}
