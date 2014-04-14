﻿using AudioAnalysisTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TowseyLibrary;


/// TO CREATE AND IMPLEMENT A NEW ACOUSTIC SUMMARY INDEX, DO THE FOLLOWING:
/// 1) Create a KEY or IDENTIFIER for the index below.
/// 2) Give the index a name - see below
/// 3) Declare the new index and its properties in the method IndexConstants.InitialisePropertiesOfIndices();
/// 4) Calculate the INDEX in the class IndicesCalculate.cs
/// 5) Store the index in the class AcousticIndicesStore



namespace AudioAnalysisTools
{
    public class IndexProperties
    {
        public static int bitsPerSample = 16;
        public static double epsilon = Math.Pow(0.5, bitsPerSample - 1);
        public static double CLIPPING_THRESHOLD = epsilon * 4; // estimate of fraction of clipped values in wave form
        public const double ZERO_SIGNAL_THRESHOLD = 0.001; // all values in zero signal are less than this value


        public const string keyCOUNT = "COUNT";
        public const string keySTART_MIN = "START-MIN";
        public const string keySEC_DUR = "SEC-DUR";
        public const string keyCLIP1 = "CLIPPING1";
        public const string keyCLIP2 = "CLIPPING2";
        public const string keyAV_AMP = "AV-AMP";
        public const string keyBGN = "BGN";
        public const string keySNR = "SNR";
        public const string keySNR_ACTIVE = "SNR_ACTIVE";
        public const string keyACTIVITY = "ACTIVITY";
        public const string keyEVENT_RATE = "SEG/SEC";
        public const string keyEVENT_DUR = "SEG-DUR";
        public const string keyHF_CVR = "HF-CVR";
        public const string keyMF_CVR = "MF-CVR";
        public const string keyLF_CVR = "LF-CVR";
        public const string keyHtemp = "H-TEMP";
        public const string keyHpeak = "H-PEAK";
        public const string keyHspec = "H-SPG";
        public const string keyHvari = "H-VAR";
        public const string keyACI   = "ACI";
        public const string keyCLUSTER_COUNT = "CLUSTER-COUNT";
        public const string keyCLUSTER_DUR = "CLUST-DUR";
        public const string key3GRAM_COUNT = "3GRAM-COUNT";
        public const string keySPT_PER_SEC = "SPT/SEC";
        public const string keySPT_DUR     = "SPT-DUR";
        public const string keyRAIN = "RAIN";
        public const string keyCICADA = "CICADA";

        // do not change headers unnecessarily - otherwise will lose compatibility with previous csv files
        // if change a header record the old header in method below:         public static string ConvertHeaderToKey(string header)
        public static string header_count = AudioAnalysisTools.AnalysisKeys.INDICES_COUNT;
        public const string header_startMin = AudioAnalysisTools.AnalysisKeys.START_MIN;
        public const string header_SecondsDuration = AudioAnalysisTools.AnalysisKeys.SEGMENT_DURATION;
        public const string header_Clipping1 = "Clipping1";
        public const string header_Clipping2 = "Clipping2";
        public const string header_avAmpdB = "avAmp-dB";
        public const string header_snr = "SNR";
        public const string header_activeSnr = "ActiveSNR";
        public const string header_bgdB = "Background";
        public const string header_activity = "Activity";
        public const string header_segPerSec = "Seg/Sec";
        public const string header_avSegDur = "avSegDur";
        public const string header_hfCover = "hfCover";
        public const string header_mfCover = "mfCover";
        public const string header_lfCover = "lfCover";
        public const string header_HAmpl = "H[temporal]";
        public const string header_HPeakFreq = "H[peakFreq]";
        public const string header_HAvSpectrum = "H[spectral]";
        public const string header_HVarSpectrum = "H[spectralVar]";
        public const string header_AcComplexity = "AcComplexity";
        public const string header_NumClusters = "ClusterCount";
        public const string header_avClustDuration = "avClustDur";
        public const string header_TrigramCount = "3gramCount";
        public const string header_SPTracksPerSec = "Tracks/Sec";
        public const string header_SPTracksDur = "avTrackDur";
        public const string header_rain = "Rain";
        public const string header_cicada = "Cicada";



        string key = "INVALID";
        string name = "INVALID";
        public Type DataType {private set; get; }
        public double DefaultValue { private set; get; }

        // for display purposes only
        private bool doDisplay;
        public bool DoDisplay { private set; get; }
        private double normMin;
        private double normMax;
        private string units;
        public string Units { private set; get; }

        // use these when calculated combination index.
        private bool includeInComboIndex;
        private double comboWeight;

        /// <summary>
        /// Three CONSTRUCTORS depending on what properties are passed.
        /// </summary>
        /// <param name="_key"></param>
        /// <param name="_name"></param>
        /// <param name="_dataType"></param>
        public IndexProperties(string _key, string _name, Type _dataType)
        {
            key = _key;
            name = _name;
            DataType = _dataType;
            SetExceptionDefaultValues(_key);
            DoDisplay = false;
            normMin = 0.0;
            normMax = 1.0;

            includeInComboIndex = false;
            comboWeight = 0.0;
        }
        public IndexProperties(string _key, string _name, Type _dataType, bool _doDisplay, double _normMin, double _normMax, string _units)
        {
            key     = _key;
            name    = _name;
            DataType = _dataType;
            SetExceptionDefaultValues(_key);
            DoDisplay = _doDisplay;
            normMin = _normMin;
            normMax = _normMax;
            units   = _units;
            includeInComboIndex = false;
            comboWeight = 0.0;
        }
        public IndexProperties(string _key, string _name, Type _dataType, bool _doDisplay, double _normMin, double _normMax, string _units, bool _includeInComboIndex, double _weight)
        {
            key   = _key;
            name = _name;
            DataType = _dataType;
            SetExceptionDefaultValues(_key);
            DoDisplay = _doDisplay;
            normMin = _normMin;
            normMax = _normMax;
            units = _units;

            includeInComboIndex = _includeInComboIndex;
            comboWeight = _weight;
        }

        public void SetExceptionDefaultValues(string key)
        {
            //all values set 0.0 by default.//however the following are exceptions
            if (key == keyAV_AMP) this.DefaultValue = SpectrogramConstants.AVG_MIN;
            else
                if (key == keyAV_AMP) this.DefaultValue = 1.0;
                else
                    if (key == keyBGN) this.DefaultValue = SpectrogramConstants.BGN_MIN;
                    else
                        if (key == keySEC_DUR) this.Units = "s";
                        else
                            if (key == keySPT_DUR) this.Units = "ms";
                            else
                                if (key == keyCLUSTER_DUR) this.Units = "ms";

        }



        public double NormaliseValue(double val)
        {
            double range = this.normMax - this.normMin;
            double norm = (val - this.normMin) / range;
            return norm;
        }

        public double[] NormaliseIndexValues(double[] val)
        {
            double range = this.normMax - this.normMin;
            double[] norms = new double[val.Length];
            for (int i = 0; i < val.Length; i++)
            {
                norms[i] = (val[i] - this.normMin) / range;
            }
            return norms;
        }
        public double[] NormaliseValues(int[] val)
        {
            double range = this.normMax - this.normMin;
            double[] norms = new double[val.Length];
            for (int i = 0; i < val.Length; i++)
            {
                norms[i] = (val[i] - this.normMin) / range;
            }
            return norms;
        }
        /// <summary>
        /// units for indices include: dB, ms, % and dimensionless
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetPlotAnnotation()
        {
            if (this.units == "") 
                return String.Format(" {0} ({1:f2} .. {2:f2} {3})", this.name, this.normMin, this.normMax, this.units);
            if (this.units == "%")
                return String.Format(" {0} ({1:f0} .. {2:f0}{3})", this.name, this.normMin, this.normMax, this.units);
            if (this.units == "dB")
                return String.Format(" {0} ({1:f0} .. {2:f0} {3})", this.name, this.normMin, this.normMax, this.units);
            if (this.units == "ms")
                return String.Format(" {0} ({1:f0} .. {2:f0}{3})", this.name, this.normMin, this.normMax, this.units);
            if (this.units == "s")
                return String.Format(" {0} ({1:f1} .. {2:f1}{3})", this.name, this.normMin, this.normMax, this.units);

            return     String.Format(" {0} ({1:f2} .. {2:f2} {3})", this.name, this.normMin, this.normMax, this.units);
        }

        /// <summary>
        /// For writing this method:
        ///    See CLASS: IndicesCsv2Display
        ///       METHOD: Bitmap ConstructVisualIndexImage(DataTable dt, string title, int timeScale, double[] order, bool doNormalise)
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public Image GetPlotImage(double[] array)
        {
            string annotation = GetPlotAnnotation();
            double[] values = this.NormaliseIndexValues(array);

            int trackWidth = array.Length + IndexDisplay.TRACK_END_PANEL_WIDTH;
            int trackHeight = IndexDisplay.DEFAULT_TRACK_HEIGHT;
            Color[] grayScale = ImageTools.GrayScale();

            Bitmap bmp = new Bitmap(trackWidth, trackHeight);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(grayScale[240]);
            for (int i = 0; i < array.Length; i++) //for pixels in the line
            {
                double value = array[i];
                if (value > 1.0) value = 1.0; //expect normalised data
                int barHeight = (int)Math.Round(value * trackHeight);
                for (int y = 0; y < barHeight; y++) bmp.SetPixel(i, trackHeight - y - 1, Color.Black);
                bmp.SetPixel(i, 0, Color.Gray); //draw upper boundary
            }//end over all pixels

            int endWidth = trackWidth - array.Length;
            var font = new Font("Arial", 9.0f, FontStyle.Regular);
            g.FillRectangle(Brushes.Black, array.Length + 1, 0, endWidth, trackHeight);
            g.DrawString(annotation, font, Brushes.White, new PointF(array.Length + 5, 2));
            return bmp;
        } // GetPlotImage()


        // ****************************************************************************************************************************************
        // ********* STATIC METHODS BELOW HERE ****************************************************************************************************
        // ****************************************************************************************************************************************

        /// <summary>
        /// Creates and returns all info about the currently calculated indices.
        /// The min-max bounds parameters were original declared in AcousticIndices.cs line 530
        ///  method  public static DataTable NormaliseColumnsOfAcousticIndicesInDataTable(DataTable dt)
        /// </summary>
        /// <returns>A DICTIONARY OF INDICES</returns>
        public static Dictionary<string, IndexProperties> InitialisePropertiesOfIndices()
        {
            var properties = new Dictionary<string, IndexProperties>();

            //string _key, string _name, Type _dataType, bool _doDisplay, double _normMin, double _normMax, "dB", bool _includeInComboIndex, 
            bool doDisplay = true; 
            bool includeInComboIndex = true;

            // use next line as template.
            //properties.Add("XXX", new IndexConstants("key", "name", typeof(double), doDisplay, 0.0, 1.0, "XX", !includeInComboIndex, 0.5));
            properties.Add(keyCOUNT,
                new IndexProperties(keyCOUNT, header_count, typeof(int)));
            properties.Add(keySTART_MIN,
                new IndexProperties(keySTART_MIN, header_startMin, typeof(double)));
            properties.Add(keySEC_DUR, 
                new IndexProperties(keySEC_DUR, header_SecondsDuration, typeof(TimeSpan)));
            properties.Add(keyCLIP1,
                new IndexProperties(keyCLIP1, header_Clipping1, typeof(double), doDisplay, 0.0, 100.0, "av/s"));
            properties.Add(keyCLIP2,
                new IndexProperties(keyCLIP2, header_Clipping2, typeof(double), doDisplay, 0.0, 1.0, "av/s"));
            properties.Add(keyAV_AMP, 
                new IndexProperties(keyAV_AMP, header_avAmpdB, typeof(double), doDisplay, -50.0, -5.0, "dB"));
            properties.Add(keyBGN, 
                new IndexProperties(keyBGN, header_bgdB, typeof(double), doDisplay, -80.0, -20.0, "dB"));
            properties.Add(keySNR, 
                new IndexProperties(keySNR, header_snr, typeof(double), doDisplay, 3.0, 50.0, "dB"));
            properties.Add(keySNR_ACTIVE, 
                new IndexProperties(keySNR_ACTIVE, header_activeSnr, typeof(double), doDisplay, 3.0, 50.0, "dB"));
            properties.Add(keyACTIVITY, 
                new IndexProperties(keyACTIVITY, header_activity, typeof(double), doDisplay, 0.2, 0.80, ""));
            properties.Add(keyEVENT_RATE,
                new IndexProperties(keyEVENT_RATE, header_segPerSec, typeof(double), doDisplay, 0.0, 5.0, "av/s"));
            properties.Add(keyEVENT_DUR, 
                new IndexProperties(keyEVENT_DUR, header_avSegDur, typeof(TimeSpan), doDisplay, 0.0, 500, "ms"));
            properties.Add(keyHF_CVR, 
                new IndexProperties(keyHF_CVR, header_hfCover, typeof(int), doDisplay, 0.0, 30.0, "%"));
            properties.Add(keyMF_CVR, 
                new IndexProperties(keyMF_CVR, header_mfCover, typeof(int), doDisplay, 0.0, 30.0, "%"));
            properties.Add(keyLF_CVR, 
                new IndexProperties(keyLF_CVR, header_lfCover, typeof(int), doDisplay, 0.0, 30.0, "%"));
            properties.Add(keyHtemp, 
                new IndexProperties(keyHtemp, header_HAmpl, typeof(double), doDisplay, 0.4, 0.95, "", includeInComboIndex, 0.3));
            properties.Add(keyHpeak, 
                new IndexProperties(keyHpeak, header_HPeakFreq, typeof(double), doDisplay, 0.4, 0.95, ""));
            properties.Add(keyHspec, 
                new IndexProperties(keyHspec, header_HAvSpectrum, typeof(double), doDisplay, 0.4, 0.95, "", includeInComboIndex, 0.2));
            properties.Add(keyHvari, 
                new IndexProperties(keyHvari, header_HVarSpectrum, typeof(double), doDisplay, 0.4, 0.95, ""));
            properties.Add(keyACI, 
                new IndexProperties(keyACI, header_AcComplexity, typeof(double), doDisplay, 0.4, 0.7, "", includeInComboIndex, 0.2));
            properties.Add(keyCLUSTER_COUNT, 
                new IndexProperties(keyCLUSTER_COUNT, header_NumClusters, typeof(int), doDisplay, 0.0, 50.0, "", includeInComboIndex, 0.3));
            properties.Add(keyCLUSTER_DUR, 
                new IndexProperties(keyCLUSTER_DUR, header_avClustDuration, typeof(TimeSpan), doDisplay, 50.0, 200.0, "ms"));
            properties.Add(key3GRAM_COUNT, 
                new IndexProperties(key3GRAM_COUNT, header_TrigramCount, typeof(int), doDisplay, 0.0, 50.0, ""));
            properties.Add(keySPT_PER_SEC, 
                new IndexProperties(keySPT_PER_SEC, header_SPTracksPerSec, typeof(double), doDisplay, 0.0, 10.0, "av/s"));
            properties.Add(keySPT_DUR, 
                new IndexProperties(keySPT_DUR, header_SPTracksDur, typeof(TimeSpan), doDisplay, 0.0, 200.0, "ms"));

            return properties;
        }

        /// <summary>
        /// This method converts a csv file header into an apporpriate key for the given index.
        /// The headers in the csv fiels have changed over the years so there may be several headers for any one index.
        /// Enter any new header you come across into the file.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetDictionaryOfName2Key()
        {
            Dictionary<string, string> mapName2Key = new Dictionary<string, string>();

            Dictionary<string, IndexProperties> indexProperties = InitialisePropertiesOfIndices();

            foreach (string key in indexProperties.Keys)
            {
                IndexProperties ip = indexProperties[key];
                mapName2Key.Add(ip.name, key);
            }

            //now add in historical names from previous incarnations of csv file headers
            mapName2Key.Add("Start-min", keySTART_MIN);
            mapName2Key.Add("bg-dB", keyBGN);
            mapName2Key.Add("snr-dB", keySNR);
            mapName2Key.Add("activeSnr-dB", keySNR_ACTIVE);
            mapName2Key.Add("activity", keyACTIVITY);
            mapName2Key.Add("segCount", keyEVENT_RATE);
            mapName2Key.Add("ACI", keyACI);
            mapName2Key.Add("clusterCount", keyCLUSTER_COUNT);
            mapName2Key.Add("rain", keyRAIN);
            mapName2Key.Add("cicada", keyCICADA);

            return mapName2Key;
        }


        public static Type[] GetArrayOfIndexTypes(Dictionary<string, IndexProperties> properties)
        {
            Type[] typeArray = new Type[properties.Count];
            int count = 0;
            foreach (string key in properties.Keys)
            {
                IndexProperties ic = properties[key];
                typeArray[count] = ic.DataType;
                count++;
            }
            return typeArray;
        }

        public static string[] GetArrayOfIndexNames(Dictionary<string, IndexProperties> properties)
        {
            string[] nameArray = new string[properties.Count];
            int count = 0;
            foreach (string key in properties.Keys)
            {
                IndexProperties ic = properties[key];
                nameArray[count] = ic.name;
                count++;
            }
            return nameArray;
        }

        public static bool[] GetArrayOfDisplayBooleans(Dictionary<string, IndexProperties> properties)
        {
            bool[] doDisplayArray = new bool[properties.Count];
            int count = 0;
            foreach (string key in properties.Keys)
            {
                IndexProperties ic = properties[key];
                doDisplayArray[count] = ic.doDisplay;
                count++;
            }
            return doDisplayArray;
        }

        public static double[] GetArrayOfComboWeights(Dictionary<string, IndexProperties> properties)
        {
            double[] weightArray = new double[properties.Count];
            int count = 0;
            foreach (string key in properties.Keys)
            {
                IndexProperties ic = properties[key];
                weightArray[count] = ic.comboWeight;
                count++;
            }
            return weightArray;
        }

    } // IndexConstants
}
