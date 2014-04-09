using System;
using System.Collections.Generic;
//using System.Data;
using System.IO;
using System.Linq;
using System.Text;


namespace TowseyLibrary
{
    

    public class Spectrum
    {
        public double[] Values
        {
            get
            {
                return this.values;
            }
        }

        public int Index { get; set; }
        public string Name { get; set; }



        private readonly double[] values;

        public Spectrum(double[] values, int index, string name)
        {
            this.values = values;
            this.Index = index;
            this.Name = name;

            throw new NotImplementedException();
        }

        public static string SpectrumToCsvString(int index, double[] values)
        {
            var sb = new StringBuilder(index.ToString());
           
            for (int i = 0; i < values.Length; i++)
            {
                sb.Append("," + values[i]);
            }
            return sb.ToString();
        }

//        public string Spectrum2CSVLine()
//        {
//            return String.Format("{0},{1}", this.Index, this.SpectrumToCsvString());
//        }

//        public void WriteSpectrum2Line()
//        {
//            Console.WriteLine("{0}  {1}  {2}", this.Index, this.Name, this.SpectrumToCsvString());
//        }

//        public static void ListOfSpectraToCsvFile(string path, IEnumerable<Spectrum> list)
//        {
//            using (var file = File.CreateText(path))
//            {
//                var dictionaries = list.Select(x => x.ToDictionary());
//                CsvSerializer.SerializeToWriter(dictionaries, file);
//            }
//        }

//        private Dictionary<string, string> ToDictionary()
//        { 
//
//            var baseDictionary = new Dictionary<string, string>(this.values.Length + 2);
//            baseDictionary.Add("Index", this.Index.ToString());
//
//            for (int i = 0; i < values.Length; i++)
//            {
//                baseDictionary.Add("s" + i.ToString("000000"), values[i].ToString());
//            }
//
//            return baseDictionary;
//        }

        public static string GetHeader(int count)
        {
            var sb = new StringBuilder("Index");

            for (int i = 0; i < count; i++)
            {
                sb.Append(",h" + i.ToString("D6"));
            }

            return sb.ToString();
        }

//        public static void ListOfSpectra2CSVFile(string path, List<Spectrum> list)
//        {
////            int year = DateTime.Now.Year;
////            int month = DateTime.Now.Month;
////            int day = DateTime.Now.Day;
////            int hour = DateTime.Now.Hour;
////            int min = DateTime.Now.Minute;
//            //DateTime.Now.ToString("o")
//            //StringBuilder sb = new StringBuilder(String.Format("{0}-{1}-{2}-{3}-{4}", year, month, day, hour, min));

            

//            var lines = new List<string>();
//            lines.Add(sb.ToString());
//            foreach (Spectrum s in list)
//            {
//                lines.Add(s.Spectrum2CSVLine());
//            }
//            FileTools.WriteTextFile(path, lines);
//        }

    } // class Spectrum
}
