using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioAnalysisTools;
using System.IO;
using TowseyLibrary;
using AudioBase;
using Acoustics.Tools;
using Acoustics.Shared;
using Dong.Felt.Configuration;

namespace Dong.Felt.ResultsOutput
{
    public class OutputResults
    {
        /// <summary>
        /// Matching step: 1. change candidate file path into file name for doing matching, output the changed files. 
        /// </summary>
        /// <param name="inputDirectory"></param>
        /// <param name="groundTruthFile"></param>
        /// <param name="outputFileName"></param>
        public static void ChangeCandidateFileName(DirectoryInfo inputDirectory)
        {
            var csvFiles = Directory.GetFiles(inputDirectory.FullName, "*.csv", SearchOption.AllDirectories);
            var csvFileCount = csvFiles.Count();
            //var grountruthList = CSVResults.CsvToCandidatesList(new FileInfo(groundTruthFile));
            for (int i = 0; i < csvFileCount; i++)
            {
                var subCandicatesList = CSVResults.CsvToCandidatesList(new FileInfo(csvFiles[i]));
                foreach (var c in subCandicatesList)
                {
                    var audioFileName = Path.GetFileName(c.SourceFilePath);
                    c.SourceFilePath = audioFileName;
                }
                CSVResults.CandidateListToCSV(new FileInfo(csvFiles[i]), subCandicatesList);
            }
        }

        public static void ChangeValueInCSVResult(DirectoryInfo inputDirectory)
        {
            var csvFiles = Directory.GetFiles(inputDirectory.FullName, "*.csv", SearchOption.AllDirectories);
            var csvFileCount = csvFiles.Count();

            for (int i = 0; i < csvFileCount; i++)
            {
                var subCandicatesList = CSVResults.CsvToCandidatesList(new FileInfo(csvFiles[i]));
                foreach (var c in subCandicatesList)
                {
                    if (c.Score > 1)
                    {
                        c.Score = 0.0;
                    }
                }
                CSVResults.CandidateListToCSV(new FileInfo(csvFiles[i]), subCandicatesList);
            }
        }
        /// <summary>
        /// Matching step 2
        /// Match the improved csv files with groundtruth data, and output the results into original csv files. 
        /// The output csv files changed the score, if they found the match. 
        /// </summary>
        public static void AutomatedMatchingAnalysis(DirectoryInfo inputDirectory, string groundTruthFile)
        {
            var csvFiles = Directory.GetFiles(inputDirectory.FullName, "*.csv", SearchOption.AllDirectories);
            var csvFileCount = csvFiles.Count();
            var grounTruthList = CSVResults.CsvToCandidatesList(new FileInfo(groundTruthFile));
            var frequencyDifference = 1000; // 1000 hz
            var timeDifference = 500; // 1000 ms
            var secondToMilliSecondUnit = 1000;
            for (int i = 0; i < csvFileCount; i++)
            {
                var subCandicatesList = CSVResults.CsvToCandidatesList(new FileInfo(csvFiles[i]));
                var candidatesCount = subCandicatesList.Count();
                for (var index = 0; index < candidatesCount; index++)
                {
                    var currentCandidate = subCandicatesList[index];
                    foreach (var g in grounTruthList)
                    {
                        var gEndTime = g.EndTime * secondToMilliSecondUnit;
                        var gStartTime = g.StartTime * secondToMilliSecondUnit;
                        if (currentCandidate.SourceFilePath == g.SourceFilePath)
                        {
                            if ((Math.Abs(currentCandidate.MaxFrequency - g.MaxFrequency) < frequencyDifference) ||
                                (Math.Abs(currentCandidate.MinFrequency - g.MinFrequency) < frequencyDifference))
                            {
                                if ((Math.Abs(currentCandidate.StartTime  - gStartTime) < timeDifference) ||
                                   (Math.Abs(currentCandidate.EndTime - gEndTime) < timeDifference))
                                {
                                    currentCandidate.Score = index + 1;
                                    break;
                                }
                                else
                                {
                                    currentCandidate.Score = 100;
                                }
                            }
                            else
                            {
                                currentCandidate.Score = 100;
                            }
                        }
                        else
                        {
                            currentCandidate.Score = 100;
                        }
                    }
                }
                CSVResults.CandidateListToCSV(new FileInfo(csvFiles[i]), subCandicatesList);
            }
        }

        /// <summary>
        /// Matching step 3
        /// Summarize all the matching csv files into one file(outputFile)
        /// </summary>
        /// <param name="inputDirectory"></param>
        /// <param name="outputFile"></param>
        public static void MatchingSummary(DirectoryInfo inputDirectory, string outputFile)
        {
            var finalOutputResult = new List<Candidates>();
            var csvFiles = Directory.GetFiles(inputDirectory.FullName, "*.csv", SearchOption.AllDirectories);
            var csvFileCount = csvFiles.Count();

            for (int i = 0; i < csvFileCount; i++)
            {
                var subCandicatesList = CSVResults.CsvToCandidatesList(new FileInfo(csvFiles[i]));
                subCandicatesList = subCandicatesList.OrderBy(x => x.Score).ToList();
                if (subCandicatesList.Count != 0)
                {
                    finalOutputResult.Add(subCandicatesList[0]);
                }
                CSVResults.CandidateListToCSV(new FileInfo(outputFile), finalOutputResult);
            }
        }

        //For song scope
        public static void SCMatchingSummary(DirectoryInfo inputDirectory, string outputFile, int n)
        {
            var finalOutputResult = new List<SongScopeCandidates>();
            var csvFiles = Directory.GetFiles(inputDirectory.FullName, "*.csv", SearchOption.AllDirectories);
            var csvFileCount = csvFiles.Count();

            for (int i = 0; i < csvFileCount; i++)
            {
                var subCandicatesList = CSVResults.CsvToSCCandidatesList(new FileInfo(csvFiles[i]));
               
                var subCandicatesCount = subCandicatesList.Count;
                if (subCandicatesCount < n)
                {
                    n = subCandicatesCount;
                }
                for (var j = 0; j < n; j++)
                {
                    if (subCandicatesList[j].Score == 1)
                    {
                        finalOutputResult.Add(subCandicatesList[j]);                        
                    }
                }                
            }
            CSVResults.SCCandidateListToCSV(new FileInfo(outputFile), finalOutputResult);
        }

        public static void CSVMatchingAnalysisOfSongScope(DirectoryInfo inputDirectory, string groundTruthFile)
        {
            var csvFiles = Directory.GetFiles(inputDirectory.FullName, "*.csv", SearchOption.AllDirectories);
            var grounTruthList = CSVResults.CsvToCandidatesList(new FileInfo(groundTruthFile));
            var timeDifference = 500; // 1000 ms
            var secondToMilliSecondUnit = 1000;
            
                var subCandicatesList = CSVResults.CsvToCandidatesList(new FileInfo(csvFiles[0]));
                var candidatesCount = subCandicatesList.Count();
                for (var index = 0; index < candidatesCount; index++)
                {
                    var currentCandidate = subCandicatesList[index];
                    foreach (var g in grounTruthList)
                    {
                        var gEndTime = g.EndTime * secondToMilliSecondUnit;
                        var gStartTime = g.StartTime * secondToMilliSecondUnit;
                        if (currentCandidate.SourceFilePath == g.SourceFilePath)
                        {                           
                                if ((Math.Abs(currentCandidate.StartTime  - gStartTime) < timeDifference) ||
                                   (Math.Abs(currentCandidate.EndTime - gEndTime) < timeDifference))
                                {
                                    currentCandidate.Score = 1;
                                    break;
                                }
                                else
                                {
                                    currentCandidate.Score = 0;
                                }
                        }
                        else
                        {
                            currentCandidate.Score = 0;
                        }
                    }
                }
                CSVResults.CandidateListToCSV(new FileInfo(csvFiles[0]), subCandicatesList);
        }

        public static void SplitFiles(DirectoryInfo inputDirectory, DirectoryInfo outputFolder)
        {
            var csvFiles = Directory.GetFiles(inputDirectory.FullName, "*.csv", SearchOption.AllDirectories);
            var audioFiles = Directory.GetFiles(inputDirectory.FullName, "*.wav", SearchOption.AllDirectories);
            var audioFileCount = audioFiles.Count();
            var sepCandidatesList = new List<List<SongScopeCandidates>>();

            var candicatesList = CSVResults.CsvToSCCandidatesList(new FileInfo(csvFiles[0]));           
            if (candicatesList.Count != 0)
                {
                    for (int l = 0; l < audioFileCount; l++)
                    {
                        var audioFileName = new FileInfo(audioFiles[l]);
                        var fileName = audioFileName.Name;
                        var temp = new List<SongScopeCandidates>();
                        foreach (var s in candicatesList)
                        {
                            if (s.SourceFilePath == fileName)
                            {
                                temp.Add(s);
                            }
                        }
                        temp = temp.OrderByDescending(x => x.Probability).ToList();
                        sepCandidatesList.Add(temp);
                    }
                }
            if (sepCandidatesList.Count != 0)
                {
                    for (int index = 0; index < sepCandidatesList.Count; index++)
                    {
                        if (sepCandidatesList[index].Count != 0)
                        {
                            string outputFilePath = Path.Combine(outputFolder.FullName, sepCandidatesList[index][0].SourceFilePath);
                            var outputFileName = new FileInfo(outputFilePath);
                            var changedFileName = Path.ChangeExtension(outputFileName.FullName, ".csv");
                            CSVResults.SCCandidateListToCSV(new FileInfo(changedFileName), sepCandidatesList[index]);
                        }
                    }
                }           
        }

        public static int ClassificationStatistics(DirectoryInfo inputDirectory, int N)
        {
            var count = 0;
            var csvFiles = Directory.GetFiles(inputDirectory.FullName, "*.csv", SearchOption.AllDirectories);
            var csvFileCount = csvFiles.Count();
            for (int i = 0; i < csvFileCount; i++)
            {
                var subCandicatesList = CSVResults.CsvToSCCandidatesList(new FileInfo(csvFiles[i]));
                int modifiedCount = 0;
                var topNCandidates = new List<SongScopeCandidates>();
                if (subCandicatesList.Count >= N)
                {
                    modifiedCount = N;
                    
                }
                else
                {
                    modifiedCount = subCandicatesList.Count;
                }
                for (int j = 0; j < modifiedCount; j++)
                {
                    topNCandidates.Add(subCandicatesList[j]);
                }
                foreach (var c in topNCandidates)
                {
                    if (c.Score == 1)
                    {
                        count++;
                        break;
                    }
                }
            }
            return count; 
        }
        /// <summary>
        /// To summarize the matching results by taking into inputDirectory, output the results to the output file.
        /// Especially, the input directory include all seperated matching results for all queries. 
        /// </summary>
        /// <param name="inputDirectory"></param>
        /// <param name="outputFileName"></param>       
        public static void MatchingResultsSummary(DirectoryInfo inputDirectory, FileInfo outputFileName)
        {
            var csvFiles = Directory.GetFiles(inputDirectory.FullName, "*.csv", SearchOption.AllDirectories);
            var csvFileCount = csvFiles.Count();
            var candidatesList = new List<Candidates>();
            for (int i = 0; i < csvFileCount; i++)
            {
                var subCandicatesList = CSVResults.CsvToCandidatesList(new FileInfo(csvFiles[i]));
                foreach (var sc in subCandicatesList)
                {
                    if (sc.Score >= 1)
                    {
                        candidatesList.Add(sc);
                    }
                }
            }
            CSVResults.CandidateListToCSV(outputFileName, candidatesList);
        }

        /// <summary>
        /// It takes in all the seperated matching results, and get the statistical results.
        /// The results will be dependent on the content of matching results. If the results have top 5 matched hits, then the statistical results will be done
        /// based on these results. 
        /// </summary>
        /// <param name="resultsDirectory"></param>
        /// <returns></returns>
        public static List<MathingResultsAnalysis> MatchingStatAnalysis(DirectoryInfo resultsDirectory)
        {
            var results = new List<MathingResultsAnalysis>();
            var fullPath = Path.GetFullPath(resultsDirectory.FullName);

            if (!Directory.Exists(resultsDirectory.FullName))
            {
                throw new DirectoryNotFoundException(string.Format("Could not find directory for numbered audio files {0}.", fullPath));
            }

            var matchedResults = Directory.GetFiles(fullPath, "*.csv", SearchOption.AllDirectories);
            var resultsCount = matchedResults.Count();

            for (int index = 0; index < resultsCount; index++)
            {
                // get the query species name                
                var queryPathWithoutExtension = Path.GetFileNameWithoutExtension(matchedResults[index]);
                var querySplittedName = queryPathWithoutExtension.Split('-');
                var querySpeciesName = querySplittedName[querySplittedName.Count() - 2];
                // read the audio source file
                var content = CSVResults.CsvToCandidatesList(new FileInfo(matchedResults[index]));
                var contentCount = content.Count();
                var hit = new int[contentCount];
                var tempScore = 0.0;
                var firstPositionIndicator = 0;
                var secondPositionIndicator = 0;
                for (var i = 0; i < contentCount; i++)
                {
                    var matchedItemFileName = Path.GetFileNameWithoutExtension(content[i].SourceFilePath);
                    var splittedCandidateFileName = matchedItemFileName.Split('-');
                    var candidateSpeciesName = splittedCandidateFileName[splittedCandidateFileName.Count() - 1];
                    var score = content[i].Score;
                    // read the similarity score to check its position
                    if (score != tempScore)
                    {
                        if (hit[i] == 0)
                        {
                            tempScore = score;
                        }
                    }
                    if (querySpeciesName.Contains(candidateSpeciesName))
                    {
                        hit[i] = 1;
                    }
                }
                var correctHitCount = 0;
                for (var j = 0; j < contentCount; j++)
                {
                    if (hit[j] == 1)
                    {
                        correctHitCount++;
                        if (correctHitCount == 1)
                        {
                            firstPositionIndicator = j + 1;
                        }
                        if (correctHitCount == 2)
                        {
                            secondPositionIndicator = j + 1;
                        }
                    }
                }
                string firstHitFileName;
                string matchedHitName;
                if (correctHitCount == 0)
                {
                    firstHitFileName = Path.GetFileNameWithoutExtension(content[firstPositionIndicator].SourceFilePath);
                    matchedHitName = Path.GetFileNameWithoutExtension(content[firstPositionIndicator].SourceFilePath) + ".csv";
                }
                else
                {
                    firstHitFileName = Path.GetFileNameWithoutExtension(content[firstPositionIndicator - 1].SourceFilePath);
                    matchedHitName = Path.GetFileNameWithoutExtension(content[firstPositionIndicator - 1].SourceFilePath) + ".csv";
                }
                var splittedHitFileName = firstHitFileName.Split('-');
                var hitSpeciesName = splittedHitFileName[splittedHitFileName.Count() - 1];
                var item = new MathingResultsAnalysis
                {
                    QuerySpeciesName = querySpeciesName,
                    HitSpeciesName = hitSpeciesName,
                    FirstHitPosition = firstPositionIndicator,
                    SecondHitPosition = secondPositionIndicator,
                    FirstHit = hit[0],
                    SecondHit = hit[1],
                    ThirdHit = hit[2],
                    FourthHit = hit[3],
                    FifthHit = hit[4],
                    QueryAudioName = Path.GetFileNameWithoutExtension(matchedResults[index]),
                    MatchedAudioName = matchedHitName
                };
                results.Add(item);
            }

            // the return value will be the position in the ranking list, and the bool value to indicate whether it hits correctly.  
            return results;
        }

        /// <summary>
        /// To use this method, you have to make sure there is no file with the same name of fiOutputSegment in the specific fold. 
        /// </summary>
        /// <param name="candidate"></param>
        /// <param name="fiOutputSegment"></param>
        public static void AudioSegmentBasedCandidates(Candidates candidate, FileInfo fiOutputSegment)
        {
            // to cut out 2 sec either side, here buffer means the time offset.
            var buffer = new TimeSpan(0, 0, 1);
            var startTime = TimeSpan.FromMilliseconds(candidate.StartTime - buffer.TotalMilliseconds);
            var endTime = TimeSpan.FromMilliseconds(candidate.EndTime + buffer.TotalMilliseconds);
            var sampleRate = 22050;
            if (startTime.TotalMilliseconds < 0)
            {
                startTime = TimeSpan.FromMilliseconds(0);  // if startTime is 0 second
                endTime = TimeSpan.FromMilliseconds(candidate.EndTime + 2 * buffer.TotalMilliseconds);
            }
            // To check whether endTime is greater 60 seconds 
            if (endTime.TotalMilliseconds > 60000)
            {
                startTime = TimeSpan.FromMilliseconds(candidate.EndTime - 2 * buffer.TotalMilliseconds);
                var tempEndTime = new TimeSpan(0, 1, 0);
                endTime = tempEndTime;
            }
            var request = new AudioUtilityRequest
                    {
                        TargetSampleRate = sampleRate,
                        OffsetStart = startTime,
                        OffsetEnd = endTime
                    };

            var result = AudioFilePreparer.PrepareFile(candidate.SourceFilePath.ToFileInfo(), fiOutputSegment, request, TempFileHelper.TempDir());
        }

        public static List<Tuple<double, double, double>> OutputTopRank(List<List<Tuple<double, double, double>>> similarityScoreTupleList, int rank)
        {
            var result = new List<Tuple<double, double, double>>();
            var count = similarityScoreTupleList.Count;
            for (int i = 1; i <= rank; i++)
            {
                var subListCount = similarityScoreTupleList[count - i].Count;
                for (int j = 0; j < subListCount; j++)
                {
                    if (similarityScoreTupleList[count - i][j].Item1 > 0.7)
                    {
                        result.Add(similarityScoreTupleList[count - i][j]);
                    }
                }
            }
            return result;
        }

    }
}
