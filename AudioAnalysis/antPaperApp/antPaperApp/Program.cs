using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace antPaperApp
{
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            
           // GiveTagsASpecies();
             

           // TurnTagSpeciesListIntoMinuteProfiles();

            RunJasonsAdaptiveBit();

            //RunJasonsBasic();

           // RunJasonsBasicFullDay();
        }


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
                    @"D:\Antman\DropBox\Sensors\Anthony\eScience 2012\Experiments\AudioTags_SERF_WITH_SPECIES__CLEAN2.csv");
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

    }
}
