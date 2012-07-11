using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minute = System.Int32;

namespace antPaperApp
{
    using LINQtoCSV;

    public class IndiciesRow
    {
        public string Site { get; set; }

        public DateTime Day { get; set; }

        [CsvColumn(Name = "Indices Count", FieldIndex = 0)]
        public int IndicesCount { get; set; }

        [CsvColumn(Name = "start-min", FieldIndex = 1)]
        public int Startmin { get; set; }

        [CsvColumn(Name = "SegTimeSpan", FieldIndex = 2)]
        public double SegTimeSpan { get; set; }

        [CsvColumn(Name = "avAmp-dB", FieldIndex = 3)]
        public double AvAmpdB { get; set; }

        [CsvColumn(Name = "snr-dB", FieldIndex = 4)]
        public double SnrdB { get; set; }

        [CsvColumn(Name = "bg-dB", FieldIndex = 5)]
        public double BgdB { get; set; }

        [CsvColumn(Name = "activity", FieldIndex = 6)]
        public double activity { get; set; }

        [CsvColumn(Name = "segCount", FieldIndex = 7)]
        public double segCount { get; set; }

        [CsvColumn(Name = "avSegDur", FieldIndex = 8)]
        public double avSegDur { get; set; }

        [CsvColumn(Name = "hfCover", FieldIndex = 9)]
        public double hfCover { get; set; }

        [CsvColumn(Name = "mfCover", FieldIndex = 10)]
        public double mfCover { get; set; }

        [CsvColumn(Name = "lfCover", FieldIndex = 11)]
        public double lfCover { get; set; }

        [CsvColumn(Name = "H[ampl]", FieldIndex = 12)]
        public double H_ampl_ { get; set; }

        [CsvColumn(Name = "H[peakFreq]", FieldIndex = 13)]
        public double H_peakFreq_ { get; set; }

        [CsvColumn(Name = "H[avSpectrum]", FieldIndex = 14)]
        public double H_avSpectrum_ { get; set; }

        [CsvColumn(Name = "H[varSpectrum]", FieldIndex = 15)]
        public double H_varSpectrum_ { get; set; }

        [CsvColumn(Name = "#clusters", FieldIndex = 16)]
        public double numClusters { get; set; }

        [CsvColumn(Name = "avClustDur", FieldIndex = 17)]
        public double avClustDur { get; set; }

        public int SpeciesCount { get; set; }
    }


    class MikesBasicWeightedIndex
    {

        // todo
    }
}
