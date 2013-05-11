using System;
using System.Collections.Generic;
//using System.Data;
using System.IO;
using System.Linq;
using System.Text;


namespace TowseyLib
{
    public class Spectrum
    {
        public double[] values { get; set; }
        public int index { get; set; }
        public string name { get; set; }

        public Spectrum(double[] _values, int _index, string _name)
        {
            values = _values;
            index = _index;
            name = _name;
        }

        public string Spectrum2String()
        {
            StringBuilder sb = new StringBuilder(values[0].ToString());
            for (int i = 1; i < values.Length; i++) sb.Append("," + values[i]);
            return sb.ToString();
        }

        public string Spectrum2CSVLine()
        {
            return String.Format("{0},{1}", index, Spectrum2String());
        }

        public void WriteSpectrum2Line()
        {
            Console.WriteLine(String.Format("{0}  {1}  {2}", index, name, Spectrum2String()));
        }

        public static void ListOfSpectra2CSVFile(string path, IEnumerable<Spectrum> list)
        {
            using (var file = File.CreateText(path))
            {
//                ServiceStack.Text.CsvSerializer.SerializeToWriter(list, file);
            }
        }

        public static void ListOfSpectra2CSVFile(string path, List<Spectrum> list)
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int day = DateTime.Now.Day;
            int hour = DateTime.Now.Hour;
            int min = DateTime.Now.Minute;
            StringBuilder sb = new StringBuilder(String.Format("{0}-{1}-{2}-{3}-{4}", year, month, day, hour, min));
            for (int i = 0; i < list[0].values.Length; i++) sb.Append(",h" + i);

            var lines = new List<string>();
            lines.Add(sb.ToString());
            foreach (Spectrum s in list)
            {
                lines.Add(s.Spectrum2CSVLine());
            }
            FileTools.WriteTextFile(path, lines);
        }

        public static void DrawSpectrogramsOfIndices(string spectrogramCsvPath, string imagePath, string ID)
        {
            double[,] matrix = CsvTools.ReadCSVFile2Matrix(spectrogramCsvPath);
            // remove left most column - consists of index numbers
            matrix = MatrixTools.Submatrix(matrix, 0, 1, matrix.GetLength(0)-1, matrix.GetLength(1)-3); // -3 to avoid anomalies in top freq bin
            matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
            if(ID.Equals("ACI"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, 0.3, 0.7);
            }else
                if (ID.Equals("TEN"))
                {
                    matrix = DataTools.NormaliseInZeroOne(matrix, 0.5, 1.0);
                }
                else
                if (ID.Equals("AVG"))
                {
                    matrix = DataTools.NormaliseInZeroOne(matrix, -7.0, 0.5);
                }
                else
                if (ID.Equals("BGN"))
                {
                    matrix = DataTools.NormaliseInZeroOne(matrix, -7.0, 0.5);
                }
                else
                if (ID.Equals("VAR"))
                {
                    matrix = DataTools.NormaliseInZeroOne(matrix, -10.0, 0.5);
                }
                else
                if (ID.Equals("CVR"))
                {
                    matrix = DataTools.NormaliseInZeroOne(matrix, 0.0, 0.8);
                }
                else
                {
                matrix = DataTools.Normalise(matrix, 0, 1);
            }
            ImageTools.DrawMatrix(matrix, imagePath);
        }


    } // class Spectrum
}
