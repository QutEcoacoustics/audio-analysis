using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioAnalysisTools;
using System.IO;
using TowseyLibrary;
using AudioBase;
using AnalysisRunner;
using Acoustics.Tools;
using Acoustics.Shared;

namespace Dong.Felt.ResultsOutput
{
    public class OutputResults
    {
        
        // Matching step: 1. species name check 2. file date, time match 3. location match
        public static void AutomatedMatchingAnalysis(FileInfo matchResultsFile, FileInfo AnnotationFile)
        {

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
                    candidatesList.Add(sc);
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


    }
}
