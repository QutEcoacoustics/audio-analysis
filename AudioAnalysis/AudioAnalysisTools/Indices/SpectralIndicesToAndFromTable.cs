using AudioAnalysisTools.LongDurationSpectrograms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TowseyLibrary;



namespace AudioAnalysisTools.Indices
{
    /// <summary>
    /// This class contains methods for interconversion of files of Spectral Indices to/from a single "pivot-table" file.
    /// </summary>
    public static class SpectralIndicesToAndFromTable
    {

        private static Arguments Dev()
        {
            DateTime time = DateTime.Now;
            string datestamp = String.Format("{0}{1:d2}{2:d2}", time.Year, time.Month, time.Day);
            return new Arguments
            {
                IndexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml".ToFileInfo(),
                //InputDir = @"Y:\Results\2013Feb05-184941 - Indicies Analysis of all of availae\SERF\Veg".ToDirectoryInfo(),
                InputDir = @"C:\SensorNetworks\OutputDataSets\SERF - November 2013 Download".ToDirectoryInfo(),
                //InputDir = @"Y:\Results\2013Nov30-023140 - SERF - November 2013 Download\SERF\November 2013 Download\Veg Plot WAV".ToDirectoryInfo(),
                //SonogramConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.yml".ToFileInfo(),
                TableDir = (@"C:\SensorNetworks\OutputDataSets\Spectrograms3D\" + datestamp).ToDirectoryInfo(),
                OutputDir = (@"C:\SensorNetworks\Output\FalseColourSpectrograms\Spectrograms3D\" + datestamp).ToDirectoryInfo(),
            };
        }


        // use the following paths for the command line. 
        public class Arguments
        {
            //FileInfo indexPropertiesConfig, DirectoryInfo inputDirInfo, DirectoryInfo opDir

            public FileInfo IndexPropertiesConfig { get; set; }
            public DirectoryInfo InputDir  { get; set; }
            //public FileInfo SonogramConfig { get; set; }
            public DirectoryInfo TableDir { get; set; }
            public DirectoryInfo OutputDir { get; set; }
            public static string Description()
            {
                return "Reads Spectral Indices from multiple spectrogram.csv files and combines into a single table which is written to file.";
            }

            public static string AdditionalNotes()
            {
                return "These methods were written to generate 3D spectrograms.";
            }
        }


        /// <summary>
        /// This method started 04-12-2014 to process consecutive days of acoustic indices data for 3-D spectrograms.
        /// </summary>
        public static void Main(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            if (!arguments.OutputDir.Exists)
            {
                arguments.OutputDir.Create();
            }

            const string Title = "# READ FILES OF SPECTRAL INDICES INTO SINGLE PIVOT-TABLE - used for preparing 3D-Spectrograms";
            string dateNow = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(Title);
            LoggedConsole.WriteLine(dateNow);
            LoggedConsole.WriteLine("# Input directory:  " + arguments.InputDir.Name);
            LoggedConsole.WriteLine("# Index Properties: " + arguments.IndexPropertiesConfig.Name);
            LoggedConsole.WriteLine("# Intermediate dir: " + arguments.TableDir.Name);
            LoggedConsole.WriteLine("# Output directry:  " + arguments.OutputDir.Name);


            //bool verbose = arguments.Verbose;

            // 1. set up the necessary files
            DirectoryInfo inputDirInfo = arguments.TableDir;
            DirectoryInfo opDir = arguments.OutputDir;
            FileInfo indexPropertiesConfig = arguments.IndexPropertiesConfig;

            ReadAllSpectralIndicesAndWriteToDataTable(indexPropertiesConfig, inputDirInfo, opDir);
        }



        /// <summary>
        /// Reads through multiple directories to read multiple files of spectral indices.
        /// The spectral indices are combined day-wise into pivot-tables which are written to file.
        /// </summary>
        /// <param name="indexPropertiesConfig"></param>
        /// <param name="inputDirInfo"></param>
        /// <param name="opDir"></param>
        public static void ReadAllSpectralIndicesAndWriteToDataTable(FileInfo indexPropertiesConfig, DirectoryInfo inputDirInfo, DirectoryInfo opDir)
        {
            Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indexPropertiesConfig);
            dictIP = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties(dictIP);
            string[] spectrogramKeys = dictIP.Keys.ToArray();

            int count = 0;
            DirectoryInfo[] dirList = inputDirInfo.GetDirectories();
            foreach (DirectoryInfo dir in dirList)
            {
                // ASSUME THAT FILE PATHS IN DIRECTORY HAVE THIS STRUCTURE
                // SERF_20130915_201727_000.wav\Towsey.Acoustic\SERF_20130915_201727_000.ACI.csv; SERF_20130915_201727_000.BGN.csv etc

                string targetFileName = dir.Name;
                string[] nameArray = targetFileName.Split('_');
                string stem = nameArray[0];
                string date = nameArray[1];
                string time = nameArray[2];
                int year = Int32.Parse(date.Substring(0, 4));
                int month = Int32.Parse(date.Substring(4, 2));
                int day = Int32.Parse(date.Substring(6, 2));
                int hour = Int32.Parse(time.Substring(0, 2));
                int minute = Int32.Parse(time.Substring(2, 2));
                int second = Int32.Parse(time.Substring(4, 2));
                DateTime thisDate = new DateTime(year, month, day, hour, minute, second);


                // get target file name without extention
                nameArray = targetFileName.Split('.');
                targetFileName = nameArray[0];
                string targetDirectory = dir.FullName + @"\Towsey.Acoustic";
                var targetDirInfo = targetDirectory.ToDirectoryInfo();

                // construct the output file name
                string opFileName = stem + "_" + date + ".SpectralIndices.DataTable.csv";
                string opFilePath = Path.Combine(opDir.FullName, opFileName);
                //Logger.Info("Reading spectral-indices for file: " + targetFileName);


                ReadSpectralIndicesAndWriteToDataTable(spectrogramKeys, thisDate, targetDirInfo, targetFileName, opFilePath);

                // for DEBUG
                //count++;
                //if (count >= 20) break;
            } // foreach (DirectoryInfo dir in dirList)

        }

        public static void ReadSpectralIndicesAndWriteToDataTable(string[] spectrogramKeys, DateTime thisDate, DirectoryInfo targetDirInfo, string targetFileName, string opFilePath)
        {
            TimeSpan roundingInterval = TimeSpan.FromMinutes(1);
            // thisDate.Round(roundingInterval); // could not get this to work
            int year = thisDate.Year;
            int thisDayOfYear = thisDate.DayOfYear;
            int thisStartMinute = (thisDate.Hour * 60) + thisDate.Minute;
            if(thisDate.Second > 30) thisStartMinute ++;

            // reads all known files spectral indices
            int freqBinCount;
            Dictionary<string, double[,]> dict = LDSpectrogramRGB.ReadSpectrogramCSVFiles(targetDirInfo, targetFileName, spectrogramKeys, out freqBinCount);


            if (dict.Count() == 0)
            {
                LoggedConsole.WriteLine("No spectrogram matrices in the dictionary. Spectrogram files do not exist?");
                return;
            }

            // set up the output file with headers if it does not exist
            if (! File.Exists(opFilePath))
            {
                string outputCSVHeader = "Year,DayOfYear,MinOfDay,FreqBin";
                foreach (string key in dict.Keys)
                {
                    outputCSVHeader = outputCSVHeader + "," + key;
                }
                FileTools.WriteTextFile(opFilePath, outputCSVHeader);
            }

            List<string> lines = new List<string>();
            string linestart = String.Format("{0},{1}", year, thisDayOfYear);

            //int minutesInThisMatrix = 2;
            // number of minutes = number of columns in matrix
            int minutesInThisMatrix = dict[spectrogramKeys[1]].GetLength(1);
            freqBinCount = dict[spectrogramKeys[1]].GetLength(0);

            for (int min = 0; min < minutesInThisMatrix; min++)
            {
                int numberOfMinutes = thisStartMinute + min;
                for (int bin = 0; bin < freqBinCount; bin++)
                {
                    int binID = freqBinCount - bin - 1;
                    StringBuilder line = new StringBuilder(linestart + "," + numberOfMinutes + "," + binID);

                    foreach (string key in dict.Keys)
                    {
                        double[,] matrix = dict[key];
                        // do not need more than 6 decimal places for values which will ultimately transformed to colour bytes.
                        // cuts file size from 12.2 MB to 7.4 MB
                        string str = String.Format(",{0:F6}", matrix[bin, min]);
                        line.Append(str);
                    }

                    lines.Add(line.ToString());
                }
            }

            FileTools.Append2TextFile(opFilePath, lines);

        }


        /// <summary>
        /// reads a single csv file in form of table and returns a dictionary of spectral indices.  
        /// </summary>
        /// <param name="csvFileName"></param>
        /// <returns></returns>
        public static Dictionary<string, double[,]> ReadPivotTableToSpectralIndices(string csvFileName)
        {
            // MICHAEL: the new Csv class can read this in, and optionally transpose as it reads
            Tuple<List<string>, List<double[]>> tuple = CsvTools.ReadCSVFile(csvFileName);
            List<string> headers = tuple.Item1;
            List<double[]> columns = tuple.Item2;

            // set up dictionary of matrices
            var dict = new Dictionary<string, double[,]>();


            double min, max;
            DataTools.MinMax(columns[2], out min, out max);
            int minMinute = (int)min;
            int maxMinute = (int)max;
            DataTools.MinMax(columns[3], out min, out max);
            int minFreqBin = (int)min;
            int maxFreqBin = (int)max;
            int rowCount = maxFreqBin - minFreqBin + 1;
            int colCount = maxMinute - minMinute + 1;

            int pivotTableRowCount = columns[0].Length;

            for (int i = 4; i < headers.Count; i++)
            {
                var matrix = new double[rowCount, colCount];

                for (int ptRow = 0; ptRow < pivotTableRowCount; ptRow++)
                {
                    int col = (int)columns[2][ptRow];
                    int row = maxFreqBin - (int)columns[3][ptRow];
                    matrix[row, col] = columns[i][ptRow];
                }
                string key = headers[i];
                dict[key] = matrix;
            }

            return dict;
        }


    } // end class
}
