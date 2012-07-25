using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace antPaperApp
{
    using System.IO;
    using System.Threading.Tasks;

    class Program
    {
        static void Main(string[] args)
        {
            
            //GiveTagsASpecies();
             

            //TurnTagSpeciesListIntoMinuteProfiles();

            RunJasonsAdaptiveBit();
            
            RunJasonsAdaptiveFrequency();
            
            RunJasonsBasic3hrs();
            
            RunJasonsBasicFullDay();
            
            RunMikesBasicZScore();

            RunJasonAndMike_TheirPowersCombined_AreCaptainPLANET();

            RunJasonAndMike_TheirPowersCombined_AreCaptainPLANET_WithVariance();

            RunJasonsAdaptiveFrequencyWithVariance();

        }

        public static int[] LevelsOfTestingToDo = new[] { 1, 10, 15, 20, 30, 45, 60, 120, 180 };

        public static ParallelOptions ParallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 8 };


        public static void GiveTagsASpecies()
        {
            FileInfo mapping =
                new FileInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\Species Call Map unique.csv");
            FileInfo old =
                new FileInfo(
                    @"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\AudioTags_SERF_SAME_AS_JASONS_new.csv");
            FileInfo newFile =
                new FileInfo(
                    @"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\AudioTags_SERF_WITH_SPECIES.csv");

            var p = new PutSpeciesInTags(mapping, old, newFile);

        }

        public static void TurnTagSpeciesListIntoMinuteProfiles()
        {
            FileInfo tags = new FileInfo(
                    @"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\AudioTags_SERF_WITH_SPECIES.csv");
            DirectoryInfo dest =
                new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\DaySiteMinuteProfiles");
            var p = new GroupTagsIntoMinutesForEachDayAndSite(tags, dest);
        }

        public static void RunJasonsBasicFullDay()
        {
            DirectoryInfo training = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TrainingData");
            DirectoryInfo test = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TestData");
            DirectoryInfo output = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\0_TotallyRandom");
            var p = new JasonsBasic(training, test, output,0, 60 * 24, "Jason's Basic - full day" );
        }

        public static void RunJasonsBasic3hrs()
        {
            DirectoryInfo training = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TrainingData");
            DirectoryInfo test = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TestData");
            DirectoryInfo output = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\1_Jason_Base");
            var sunrise = 60 * 4 + 28;
            var p = new JasonsBasic(training, test, output, sunrise, sunrise + 60 * 3, "Jason's basic - dawn");
        }

        public static void RunJasonsAdaptiveBit()
        {
            DirectoryInfo training = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TrainingData");
            DirectoryInfo test = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TestData");
            DirectoryInfo output = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\2_Jason_Adaptive_Bit");
            var p = new JasonsAdaptiveBit(training, test, output);
        }

        public static void RunJasonsAdaptiveFrequency()
        {
            DirectoryInfo training = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TrainingData");
            DirectoryInfo test = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TestData");
            DirectoryInfo output = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\3_Jason_Adaptive_Count");
            var p = new JasonsAdaptiveFrequency(training, test, output);
        }

        public static void RunJasonsAdaptiveFrequencyWithVariance()
        {
            DirectoryInfo training = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TrainingData");
            DirectoryInfo test = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TestData");
            DirectoryInfo output = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\6_Adaptive_Count_Variance");
            var p = new JasonsAdaptiveFrequency(training, test, output, 15);
        }

        public static  void RunMikesBasicZScore()
        {
            DirectoryInfo training = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TrainingData");
            DirectoryInfo test = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TestData");
            DirectoryInfo trainingIndicies = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TrainingData\Indicies");
            DirectoryInfo testIndicies = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TestData\Indicies");
            DirectoryInfo output = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\4_Mike_Adaptive_Zscore");
            var p = new MikesBasicZScore(training, test, trainingIndicies, testIndicies, output);
        }

        public static void RunJasonAndMike_TheirPowersCombined_AreCaptainPLANET()
        {
            DirectoryInfo training = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TrainingData");
            DirectoryInfo test = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TestData");
            DirectoryInfo trainingIndicies = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TrainingData\Indicies");
            DirectoryInfo testIndicies = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TestData\Indicies");
            DirectoryInfo output = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\5_Combined");
            var p = new JasonAndMike_TheirPowersCombined_AreCaptainPLANET(training, test, trainingIndicies, testIndicies, output);
        }        
        
        public static void RunJasonAndMike_TheirPowersCombined_AreCaptainPLANET_WithVariance()
        {
            DirectoryInfo training = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TrainingData");
            DirectoryInfo test = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TestData");
            DirectoryInfo trainingIndicies = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TrainingData\Indicies");
            DirectoryInfo testIndicies = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\TestData\Indicies");
            DirectoryInfo output = new DirectoryInfo(@"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\7_Combined_Variance");
            var p = new JasonAndMike_TheirPowersCombined_AreCaptainPLANET(training, test, trainingIndicies, testIndicies, output, 15);
        }

    }
}
