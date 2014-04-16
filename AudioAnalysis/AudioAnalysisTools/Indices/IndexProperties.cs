using AudioAnalysisTools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using AudioAnalysisTools.DSP;
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


        //KEYS FOR SUMMARY INDICES
        public const string keyCOUNT = "COUNT";
        public const string keySTART_MIN = "START-MIN";
        public const string keySEG_DURATION = "SEGMENT-DUR";
        public const string keyCLIP1 = "hiSIG-AMPL";
        public const string keyCLIP2 = "CLIPPING";
        public const string keySIG_AMPL = "SIGNAL-AMPL";
        public const string keyBKGROUND = "BKGROUND";
        public const string keySNR = "SNR";
        public const string keySNR_ACTIVE = "SNR-ACTIVE";
        public const string keyACTIVITY = "ACTIVITY";
        public const string keyEVENT_RATE = "EVENTS/SEC";
        public const string keyEVENT_DUR = "avEVENT-DUR";
        public const string keyHF_CVR = "HF-CVR";
        public const string keyMF_CVR = "MF-CVR";
        public const string keyLF_CVR = "LF-CVR";
        public const string keyHtemp = "H-TEMP";
        public const string keyHpeak = "H-PEAK";
        public const string keyHspec = "H-SPG";
        public const string keyHvari = "H-VAR";
        public const string keyACI   = "AcousticComplexity";
        public const string keyCLUSTER_COUNT = "CLUSTER-COUNT";
        public const string keyCLUSTER_DUR = "avCLUST-DUR";
        public const string key3GRAM_COUNT = "3GRAM-COUNT";
        public const string keySPT_PER_SEC = "SPT/SEC";
        public const string keySPT_DUR     = "avSPT-DUR";
        public const string keyRAIN = "RAIN";
        public const string keyCICADA = "CICADA";

        //KEYS FOR SPECTRAL INDICES
        // CONST string for referring to different types of spectrogram - these should really be an enum                
        //public enum keysForSpectralIndices { ACI, AVG, BGN, CLS, CVR, EVN, SPT, TEN, VAR }
        // WHEN CREATING NEW SPECTRAL INDEX, YOU NEED TO ENTER ITS KEY _AND_ INCORPORATE IT INTO THE string ALL_KNOWN_KEYS.
        public const string spKEY_ACI = "ACI";
        public const string spKEY_Average = "AVG";
        public const string spKEY_BkGround = "BGN";
        public const string spKEY_Cluster = "CLS";
        public const string spKEY_BinCover = "CVR";
        public const string spKEY_BinEvents = "EVN";
        public const string spKEY_SpPeakTracks = "SPT";
        public const string spKEY_TemporalEntropy = "TEN";
        public const string spKEY_Variance = "VAR";
        public const string spKEY_Combined = "CMB"; //discontinued - replaced by false colour spectrograms
        public const string spKEY_Colour = "COL"; //discontinued - 

        public const string ALL_KNOWN_SPECTRAL_KEYS = "ACI-AVG-BGN-CLS-CVR-EVN-SPT-TEN-VAR";


        // NORMALISING CONSTANTS FOR INDICES
        public const double ACI_MIN = 0.4;
        public const double ACI_MAX = 0.8;
        public const double AVG_MIN = 0.0;
        public const double AVG_MAX = 50.0;
        public const double BGN_MIN = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL - 20; //-20 adds more contrast into bgn image
        public const double BGN_MAX = -20.0;
        public const double CLS_MIN = 0.0;
        public const double CLS_MAX = 30.0;
        public const double CVR_MIN = 0.0;
        public const double CVR_MAX = 0.3;
        public const double EVN_MIN = 0.0;
        public const double EVN_MAX = 0.8;
        public const double TEN_MIN = 0.4;
        public const double TEN_MAX = 0.95;
        public const double SDV_MIN = 0.0; // for the variance bounds
        public const double SDV_MAX = 100.0;
        public const double VAR_MIN = SDV_MIN * SDV_MIN;
        public const double VAR_MAX = SDV_MAX * SDV_MAX; // previously 30000.0




        // do not change headers unnecessarily - otherwise will lose compatibility with previous csv files
        // if change a header record the old header in method below:         public static string ConvertHeaderToKey(string header)
        //public static string header_count = AudioAnalysisTools.AnalysisKeys.INDICES_COUNT;
        //public const string header_startMin = AudioAnalysisTools.AnalysisKeys.START_MIN;
        //public const string header_SecondsDuration = AudioAnalysisTools.AnalysisKeys.SEGMENT_DURATION;
        //public const string header_Clipping1 = "Clipping1";
        //public const string header_Clipping2 = "Clipping2";
        //public const string header_avAmpdB = "avAmp-dB";
        //public const string header_snr = "SNR";
        //public const string header_activeSnr = "ActiveSNR";
        //public const string header_bgdB = "Background";
        //public const string header_activity = "Activity";
        //public const string header_segPerSec = "Seg/Sec";
        //public const string header_avSegDur = "avSegDur";
        //public const string header_hfCover = "hfCover";
        //public const string header_mfCover = "mfCover";
        //public const string header_lfCover = "lfCover";
        //public const string header_HAmpl = "H[temporal]";
        //public const string header_HPeakFreq = "H[peakFreq]";
        //public const string header_HAvSpectrum = "H[spectral]";
        //public const string header_HVarSpectrum = "H[spectralVar]";
        //public const string header_AcComplexity = "AcComplexity";
        //public const string header_NumClusters = "ClusterCount";
        //public const string header_avClustDuration = "avClustDur";
        //public const string header_TrigramCount = "3gramCount";
        //public const string header_SPTracksPerSec = "Tracks/Sec";
        //public const string header_SPTracksDur = "avTrackDur";
        public const string header_rain = "Rain";
        public const string header_cicada = "Cicada";



        public string Key {set; get; }
        public string Name { set; get; }
        public Type DataType {private set; get; }
        public double DefaultValue { private set; get; }

        // for display purposes only
        public bool DoDisplay { private set; get; }
        private double normMin;
        private double normMax;
        public string Units { private set; get; }

        // use these when calculated combination index.
        private bool includeInComboIndex;
        private double comboWeight;

        /// <summary>
        /// constructor sets default values
        /// </summary>
        public IndexProperties()
        {
            Key = "NOT SET";
            Name = String.Empty;
            DataType = typeof(double);
            DefaultValue = 0.0;

            DoDisplay = true;
            normMin = 0.0;
            normMax = 1.0;
            Units = String.Empty;

            includeInComboIndex = false;
            comboWeight = 0.0;
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
            if (this.Units == "") 
                return String.Format(" {0} ({1:f2} .. {2:f2} {3})", this.Name, this.normMin, this.normMax, this.Units);
            if (this.Units == "%")
                return String.Format(" {0} ({1:f0} .. {2:f0}{3})",  this.Name, this.normMin, this.normMax, this.Units);
            if (this.Units == "dB")
                return String.Format(" {0} ({1:f0} .. {2:f0} {3})", this.Name, this.normMin, this.normMax, this.Units);
            if (this.Units == "ms")
                return String.Format(" {0} ({1:f0} .. {2:f0}{3})",  this.Name, this.normMin, this.normMax, this.Units);
            if (this.Units == "s")
                return String.Format(" {0} ({1:f1} .. {2:f1}{3})",  this.Name, this.normMin, this.normMax, this.Units);

            return     String.Format(" {0} ({1:f2} .. {2:f2} {3})", this.Name, this.normMin, this.normMax, this.Units);
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
            int dataLength = array.Length;
            string annotation = GetPlotAnnotation();
            double[] values = this.NormaliseIndexValues(array);

            int trackWidth = dataLength + IndexDisplay.TRACK_END_PANEL_WIDTH;
            int trackHeight = IndexDisplay.DEFAULT_TRACK_HEIGHT;
            Color[] grayScale = ImageTools.GrayScale();

            Bitmap bmp = new Bitmap(trackWidth, trackHeight);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(grayScale[240]);
            for (int i = 0; i < dataLength; i++) //for pixels in the line
            {
                double value = values[i];
                if (value > 1.0) value = 1.0; //expect normalised data
                int barHeight = (int)Math.Round(value * trackHeight);
                for (int y = 0; y < barHeight; y++) bmp.SetPixel(i, trackHeight - y - 1, Color.Black);
                bmp.SetPixel(i, 0, Color.Gray); //draw upper boundary
            }//end over all pixels

            int endWidth = trackWidth - dataLength;
            var font = new Font("Arial", 9.0f, FontStyle.Regular);
            g.FillRectangle(Brushes.Black, dataLength + 1, 0, endWidth, trackHeight);
            g.DrawString(annotation, font, Brushes.White, new PointF(dataLength + 5, 2));
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
            //bool doDisplay = true; 
            //bool includeInComboIndex = true;

            // use next line as template.
            //properties.Add("XXX", new IndexConstants("key", "name", typeof(double), doDisplay, 0.0, 1.0, "XX", !includeInComboIndex, 0.5));
            properties.Add(keyCOUNT,
                new IndexProperties { Key = keyCOUNT, Name = AudioAnalysisTools.AnalysisKeys.INDICES_COUNT, DataType = typeof(int), DoDisplay = false });

            properties.Add(keySTART_MIN,
                new IndexProperties { Key = keySTART_MIN, Name = AudioAnalysisTools.AnalysisKeys.START_MIN, DoDisplay = false });

            properties.Add(keySEG_DURATION, 
                new IndexProperties { Key = keySEG_DURATION, Name = AudioAnalysisTools.AnalysisKeys.SEGMENT_DURATION, DataType = typeof(TimeSpan), DoDisplay = false });

            properties.Add(keyCLIP1,
                new IndexProperties { Key = keyCLIP1, Name = "High Signal Ampl", normMax = 10.0, Units = "av/s" });

            properties.Add(keyCLIP2,
                new IndexProperties { Key = keyCLIP2, Name = "Clipping", normMax = 1.0, Units = "avClips/s" });

            properties.Add(keySIG_AMPL,
                new IndexProperties { Key = keySIG_AMPL, Name = "av Signal Ampl", normMin = -50.0, normMax = -5.0, Units = "dB", DefaultValue = IndexProperties.BGN_MIN });

            properties.Add(keyBKGROUND,
                new IndexProperties { Key = keyBKGROUND, Name = "Background", normMin = -80.0, normMax = -20.0, Units = "dB", DefaultValue = IndexProperties.BGN_MIN });

            properties.Add(keySNR, 
                new IndexProperties { Key = keySNR, Name = "SNR", normMin = 0.0, normMax = 50.0, Units = "dB"});

            properties.Add(keySNR_ACTIVE,
                new IndexProperties { Key = keySNR_ACTIVE, Name = "avSNRActive", normMin = 0.0, normMax = 50.0, Units = "dB" });

            properties.Add(keyACTIVITY, 
                new IndexProperties { Key = keyACTIVITY, Name = "Activity", normMax = 0.8, Units = String.Empty});

            properties.Add(keyEVENT_RATE,
                new IndexProperties { Key = keyEVENT_RATE, Name = "Events/s", normMax = 1.0, Units = ""});

            properties.Add(keyEVENT_DUR,
                new IndexProperties { Key = keyEVENT_DUR, Name = "av Event Duration", DataType = typeof(TimeSpan), normMax = 500, Units = "ms" });

            properties.Add(keyHF_CVR, 
                new IndexProperties { Key = keyHF_CVR, Name = "hf Cover", normMax = 30, DataType = typeof(int), Units = "%"});

            properties.Add(keyMF_CVR, 
                new IndexProperties { Key = keyMF_CVR, Name = "mf Cover", normMax = 30, DataType = typeof(int), Units = "%"});

            properties.Add(keyLF_CVR,
                new IndexProperties { Key = keyLF_CVR, Name = "lf Cover", normMax = 30, DataType = typeof(int), Units = "%" });

            properties.Add(keyHtemp, 
                new IndexProperties { Key = keyHtemp, Name = "H[temporal]", normMin = 0.4, normMax = 0.95, DefaultValue = 1.0,
                    includeInComboIndex = true, comboWeight = 0.3 });

            properties.Add(keyHpeak, 
                new IndexProperties { Key = keyHpeak, Name = "H[peak freq]", normMin = 0.4, normMax = 0.95, DefaultValue = 1.0 });

            properties.Add(keyHspec,
                new IndexProperties { Key = keyHspec, Name = "H[spectral]", normMin = 0.4, normMax = 0.95, DefaultValue = 1.0 });

            properties.Add(keyHvari,
                new IndexProperties { Key = keyHvari, Name = "H[spectral var]", normMin = 0.4, normMax = 0.95, DefaultValue = 1.0 });

            properties.Add(keyACI, 
                new IndexProperties { Key = keyACI, Name = "ACI", normMin = 0.4, normMax = 0.7, includeInComboIndex = true, comboWeight = 0.2 });

            properties.Add(keyCLUSTER_COUNT, 
                new IndexProperties { Key = keyCLUSTER_COUNT, Name = "Cluster Count", DataType = typeof(int), normMax = 50, includeInComboIndex = true, comboWeight = 0.3 });

            properties.Add(keyCLUSTER_DUR, 
                new IndexProperties { Key = keyCLUSTER_DUR, Name = "av Cluster Duration", DataType = typeof(TimeSpan), normMax = 200, Units = "ms" });

            properties.Add(key3GRAM_COUNT, 
                new IndexProperties { Key = key3GRAM_COUNT, Name = "3gramCount", DataType = typeof(int), normMax = 50 });

            properties.Add(keySPT_PER_SEC, 
                new IndexProperties { Key = keySPT_PER_SEC, Name = "av Tracks/Sec", normMax = 10 });

            properties.Add(keySPT_DUR, 
                new IndexProperties { Key = keySPT_DUR, Name = "av Track Duration", DataType = typeof(TimeSpan), normMax = 200, Units = "ms" });


            // ADD THE SUMMARY INDICES ABOVE HERE
            //==================================================================================================================================================
            // ADD THE SPECTRAL INDICES BELOW HERE

            //string key, string name, typeof(double[]), bool doDisplay, double normMin, double normMax, "dB", bool _includeInComboIndex, 

            properties.Add(spKEY_ACI, 
                new IndexProperties { Key = spKEY_ACI, Name = "av Track Duration", DataType = typeof(double[]), normMax = 200, Units = "ms" });
            //public const double ACI_MIN = 0.4;
            //public const double ACI_MAX = 0.8;

            properties.Add(spKEY_Average,
                new IndexProperties { Key = spKEY_Average, Name = "av Track Duration", DataType = typeof(double[]), normMax = 200, Units = "ms" });
            //public const double AVG_MIN = 0.0;
            //public const double AVG_MAX = 50.0;

            properties.Add(spKEY_BkGround,
                new IndexProperties { Key = spKEY_BkGround, Name = "av Track Duration", DataType = typeof(double[]), normMax = 200, Units = "ms" });
            //public const double BGN_MIN = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL - 20; //-20 adds more contrast into bgn image
            //public const double BGN_MAX = -20.0;

            properties.Add(spKEY_Cluster,
                new IndexProperties { Key = spKEY_Cluster, Name = "av Track Duration", DataType = typeof(double[]), normMax = 200, Units = "ms" });
            //public const double CLS_MIN = 0.0;
            //public const double CLS_MAX = 30.0;

            properties.Add(spKEY_BinCover,
                new IndexProperties { Key = spKEY_BinCover, Name = "av Track Duration", DataType = typeof(double[]), normMax = 200, Units = "ms" });
            //public const double CVR_MIN = 0.0;
            //public const double CVR_MAX = 0.3;

            properties.Add(spKEY_BinEvents,
                new IndexProperties { Key = spKEY_BinEvents, Name = "av Track Duration", DataType = typeof(double[]), normMax = 200, Units = "ms" });
            //public const double EVN_MIN = 0.0;
            //public const double EVN_MAX = 0.8;

            properties.Add(spKEY_SpPeakTracks,
                new IndexProperties { Key = spKEY_SpPeakTracks, Name = "av Track Duration", DataType = typeof(double[]), normMax = 200, Units = "ms" });

            properties.Add(spKEY_TemporalEntropy,
                new IndexProperties { Key = spKEY_TemporalEntropy, Name = "av Track Duration", DataType = typeof(double[]), normMax = 200, Units = "ms" });
            //public const double TEN_MIN = 0.4;
            //public const double TEN_MAX = 0.95;

            properties.Add(spKEY_Variance,
                new IndexProperties { Key = spKEY_Variance, Name = "av Track Duration", DataType = typeof(double[]), normMax = 200, Units = "ms" });
            //public const double SDV_MIN = 0.0; // for the variance bounds
            //public const double SDV_MAX = 100.0;
            //public const double VAR_MIN = SDV_MIN * SDV_MIN;
            //public const double VAR_MAX = SDV_MAX * SDV_MAX; // previously 30000.0

            //properties.Add(spKEY_Combined,
            //    new IndexProperties { Key = spKEY_Combined, Name = "av Track Duration", DataType = typeof(double[]), normMax = 200, Units = "ms" });

            //public const string ALL_KNOWN_SPECTRAL_KEYS = "ACI-AVG-BGN-CLS-CVR-EVN-SPT-TEN-VAR";

            return properties;
        }


        public static Dictionary<string, IndexProperties> GetDictionaryOfSpectralIndexProperties()
        {
            Dictionary<string, IndexProperties> indexProperties = InitialisePropertiesOfIndices();

            var dict = new Dictionary<string, IndexProperties>();
            foreach (IndexProperties ip in indexProperties.Values)
            {
                if (ip.DataType == typeof(double[]))
                {
                    dict.Add(ip.Key, ip);
                }
            }
            return dict;
        }

        public static Dictionary<string, IndexProperties> GetDictionaryOfSummaryIndexProperties()
        {
            Dictionary<string, IndexProperties> indexProperties = InitialisePropertiesOfIndices();

            var dict = new Dictionary<string, IndexProperties>();
            foreach (IndexProperties ip in indexProperties.Values)
            {
                if (ip.DataType != typeof(double[]))
                {
                    dict.Add(ip.Key, ip);
                }
            }
            return dict;
        }
        
        /// <summary>
        /// This method converts a csv file header into an appropriate key for the given index.
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
                mapName2Key.Add(ip.Name, key);
            }

            //now add in historical names from previous incarnations of csv file headers
            mapName2Key.Add("Start-min", keySTART_MIN);
            mapName2Key.Add("bg-dB", keyBKGROUND);
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
                nameArray[count] = ic.Name;
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
                doDisplayArray[count] = ic.DoDisplay;
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
