namespace AnalysisRunner
{
    using System.IO;

    using Acoustics.Shared;

    public class Program
    {

        public static void Main(string[] args)
        {
            //PageEventsToCsv();
        }

        private static void PageEventsToCsv()
        {
            //var propNames =
            //    File.ReadAllLines(@"C:\Work\Masters\Papers\eScience experiment documents\user12002_propnames.txt");
            //var propValues =
            //    File.ReadAllLines(@"C:\Work\Masters\Papers\eScience experiment documents\user12002_propvalues.txt");

            //var propsFile = @"C:\Work\Masters\Papers\eScience experiment documents\user12002_props.txt";

            StringKeyValueStore.SaveToCsv(
                new FileInfo(@"C:\Users\n5414628\Dropbox\Sensors\Mark\user12003_all.csv"),
                new FileInfo(@"C:\Users\n5414628\Dropbox\Sensors\Mark\user12003_output.csv"));

        }
    }
}
