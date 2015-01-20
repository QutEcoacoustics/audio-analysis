using Acoustics.Shared;

namespace Dong.Felt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using Acoustics.Shared.Csv;
    using Representations;
    using AudioAnalysisTools;
    using AudioAnalysisTools.StandardSpectrograms;
    using System.Drawing;
    using Dong.Felt.Configuration;
    using TowseyLibrary;
    using AnalysisBase;
    using Dong.Felt.ResultsOutput;
    using Dong.Felt.Features;

    public class CSVResults
    {

        #region  Public Methods

        public static List<Query> CsvToQuery(FileInfo queryCsvfile)
        {
            return Csv.ReadFromCsv<Query>(queryCsvfile).ToList();           
        }

        //public static string Test(IEnumerable<string> i)
        //{ 
        //    return string.Join(", ", i); 
        //}

        public static AcousticEvent CsvToAcousticEvent(FileInfo file)
        {           
            var lines = File.ReadAllLines(file.FullName).Select(i => i.Split(','));
            var header = lines.Take(1).ToList();
            var lines1 = lines.Skip(1);
            var startTime = 0.0;
            var duration = 0.0;
            var minFreq = 0;
            var maxFreq = 0;
            var result = new AcousticEvent(startTime, duration, minFreq, maxFreq);
            foreach (var csvRow in lines1)
            {
                if (csvRow[3] != "")
                {
                    result.MinFreq = int.Parse(csvRow[1]);
                    result.MaxFreq = int.Parse(csvRow[2]);
                    result.TimeStart = double.Parse(csvRow[3]);
                    result.TimeEnd = double.Parse(csvRow[4]);
                    result.Duration = double.Parse(csvRow[5]);
                    //result.FreqBinCount = int.Parse(csvRow[6]);   
                }               
            }
            return result;
        }

        public static List<RidgeDescriptionNeighbourhoodRepresentation> CSVToNeighbourhoodRepresentation(FileInfo file)
        {
            return Csv.ReadFromCsv<RidgeDescriptionNeighbourhoodRepresentation>(file).ToList();
        }
       
        /// Write ridgeNeighbourhoodRepresentation list into csv file by using CsvTools.WriteResultsToCsv. 
        /// Notice, this method can only write the public properties in the class and it should have get and set.  
        public static void NeighbourhoodRepresentationsToCSV(FileInfo outputFilePath, List<RidgeDescriptionNeighbourhoodRepresentation> nhList)
        {
            Csv.WriteToCsv(outputFilePath, nhList);
        }

        public static void NeighbourhoodRepresentationsToCSV(FileInfo outputFilePath, List<NeighbourhoodRepresentationOutput> nhList)
        {
            Csv.WriteToCsv(outputFilePath, nhList);
        }

        public static void PointOfInterestListToCSV(FileInfo outputFilePath, List<PointOfInterest> poiList)
        {
            Csv.WriteToCsv(outputFilePath, poiList);       
        }

        public static void RidgeListToCSV(FileInfo outputFilePath, List<Ridge> ridgeList)
        {
            Csv.WriteToCsv(outputFilePath, ridgeList);
        }

        public static List<double> CSVToSpectrogramData(FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName).Select(i => i.Split(','));
            var header = lines.Take(1).ToList();
            var result = new List<Double>();
            foreach (var csvRow in lines)
            {
                var item = 0.0;
                if (csvRow[0] != "")
                {
                    item = double.Parse(csvRow[0]); 
                }
                result.Add(item);
            }
            return result;
        }

        public static void MatchingStatResultsToCSV(FileInfo file, List<MathingResultsAnalysis> matchedResults)
        {         
            Csv.WriteToCsv(file, matchedResults);
        }
        
        /// <summary>
        /// Write ridgeRegionRepresentation list into csv file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ridgeRegion"></param>
        public static void RegionRepresentationListToCSV(FileInfo file, List<RegionRerepresentation> ridgeRegion)
        {
            Csv.WriteToCsv(file, ridgeRegion);
        }

        /// <summary>
        /// Write candidate list into csv file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="candidates"></param>
        public static void CandidateListToCSV(FileInfo file, List<Candidates> candidates)
        {
            Csv.WriteToCsv(file, candidates);
        }

        public static List<Candidates> CsvToCandidatesList(FileInfo candidatesCsvfile)
        {
            return Csv.ReadFromCsv<Candidates>(candidatesCsvfile).ToList();
        }
      
        public static List<Tuple<double, double, double>> CSVToSimilarityDistanceSocre(FileInfo file)
        {
            var lines = File.ReadAllLines(file.FullName).Select(i => i.Split(','));
            var header = lines.Take(1).ToList();
            var lines1 = lines.Skip(1);
            var results = new List<Tuple<double, double, double>>();
            foreach (var csvRow in lines1)
            {
                var distance = double.Parse(csvRow[0]);
                var regionTimePostion = double.Parse(csvRow[1]);
                var regionFrequencyPostion = double.Parse(csvRow[2]);
                results.Add(Tuple.Create(distance, regionTimePostion, regionFrequencyPostion));
            }
            return results;
        }

        public static void BatchProcess(string fileDirectoryPath, SpectrogramConfiguration spectrogramConfig)
        {
            string[] fileEntries = Directory.GetFiles(fileDirectoryPath);

            var fileCount = fileEntries.Count();
            for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
            {
                var poi = new List<PointOfInterest>();
                var poiList = new POISelection(poi);
                var ridgeLength = 5;
                var magnitudeThreshold = 5.5;
                poiList.SelectPointOfInterestFromAudioFile(fileEntries[fileIndex], ridgeLength, magnitudeThreshold);
                var filterPoi = POISelection.FilterPointsOfInterest(poiList.poiList, poiList.RowsCount, poiList.ColsCount);               
                var file = new FileInfo(fileEntries[fileIndex] + "fileIndex.csv");
                PointOfInterestListToCSV(file, filterPoi);
            }
        }

        public static void ReadSimilarityDistanceToCSV(List<Tuple<double, double, double>> scoreList, string outputFilePath)
        {
            var results = new List<List<string>>();
            results.Add(new List<string>() { "DistanceScore", "RegionTimePostion-ms", "RegionFrequencyPosition-hz" });
            var ItemCount = scoreList.Count();
            for (int i = 0; i < ItemCount; i++)
            {
                var similarityDistanceScore = scoreList[i].Item1;
                var regionTimePostion = scoreList[i].Item2;
                var regionFrequencyPosition = scoreList[i].Item3;
                results.Add(new List<string>() { similarityDistanceScore.ToString(), regionFrequencyPosition.ToString(), 
                            regionTimePostion.ToString() });
            }
            File.WriteAllLines(outputFilePath, results.Select((IEnumerable<string> i) => { return string.Join(",", i); }));
        }

        #endregion
    }
}
