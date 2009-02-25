using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using QutSensors;


namespace AudioAnalysis
{
    public enum FV_Source { SELECTED_FRAMES, MARQUEE }
    public enum FV_MarqueeType { AT_ENERGY_PEAKS, AT_FIXED_INTERVALS }



    public class FVConfig
    {


        #region Properties
        public int CallID { get; set; } //required for constructing FV file names
        public int FVCount { get; set; }
        public string[] FVIniData { get; private set; }

        public FV_Source FVSourceType { get; set; }
        public FV_MarqueeType FVMarqueeType { get; set; }
        //public string[] SelectedFrames { get; set; } //store frame IDs as string array
        public int MarqueeStart { get; set; }
        public int MarqueeEnd { get; set; }
        public int? MarqueeInterval { get; set; }

        public int FVLength { get; set; }
        public string OPDir { get; set; }
        public string[] FVfNames { get; set; }
        public string[] FVSourceFiles { get; set; }
        private FeatureVector[] fvArray;
        public FeatureVector[] FVArray
        {
            get
            {
                //if (fvArray == null) fvArray = LoadFeatureVectors();
                return fvArray;
            }
            set
            {
                fvArray = value;
                FVfNames = null;
                FVCount = value.Length;
            }
        }
        #endregion




        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="config"></param>
        public FVConfig(Configuration config)
        {
            //FEATURE VECTORS
            FVSourceType = GetFVSource(config);
            //Log.WriteIfVerbose("\tFV_SOURCE=" + FVSourceType.ToString());
            CallID = config.GetInt("TEMPLATE_ID");
            FVCount = config.GetInt("FV_COUNT");
            FVIniData = new string[FVCount];


            switch (FVSourceType)
            {
                case FV_Source.SELECTED_FRAMES:
                    string frames = config.GetString("FV_SELECTED_FRAMES");
                    //Log.WriteIfVerbose("\tSelected frames=" + frames);
                    for (int i = 0; i < FVCount; i++)
                    {
                        FVIniData[i] = config.GetString("FV"+(i+1)+"_DATA");
                    }
                    break;
                case FV_Source.MARQUEE:
                    MarqueeStart = config.GetInt("MARQUEE_START");
                    MarqueeEnd = config.GetInt("MARQUEE_END");
                    int? interval;
                    FVMarqueeType = GetMarqueeType(config, out interval);
                    MarqueeInterval = interval;
                    break;
            }
        }//end Constructor()



        #region Feature Vector Parameter Reading

        public FV_Source GetFVSource(Configuration config)
        {
            if (!config.ContainsKey("FV_SOURCE"))
            {
                Log.WriteLine("FVConfig.GetFVSource():- WARNING! NO SOURCE FOR FEATURE VECTORS IS DEFINED!");
                Log.WriteLine("                         SET THE DEFAULT: FV_Source = SELECTED_FRAMES");
                return FV_Source.SELECTED_FRAMES;
            }

            string value = config.GetString("FV_SOURCE");
            if (value.StartsWith("MARQUEE"))
            {
                return FV_Source.MARQUEE;

            }
            else if (value.StartsWith("SELECTED_FRAMES"))
                return FV_Source.SELECTED_FRAMES;
            else
            {
                Log.WriteLine("FVConfig.GetFVSource():- WARNING! INVALID SOURCE FOR FEATURE VECTORS IS DEFINED! " + value);
                Log.WriteLine("                         SET THE DEFAULT: FV_Source = SELECTED_FRAMES");
                return FV_Source.SELECTED_FRAMES;
            }
        }


        public FV_MarqueeType GetMarqueeType(Configuration config, out int? interval)
        {
            interval = null;

            if (!config.ContainsKey("MARQUEE_TYPE"))
            {
                Log.WriteLine("FVConfig.GetMarqueeType():- WARNING! NO MARQUEE_TYPE (EXTRACTION PROCESS) IS DEFINED FOR FEATURE VECTORS!");
                Log.WriteLine("                            SET THE DEFAULT:- FV_MarqueeType = AT_ENERGY_PEAKS");
                return FV_MarqueeType.AT_ENERGY_PEAKS;
            }

            string value = config.GetString("MARQUEE_TYPE");
            if (value.StartsWith("AT_ENERGY_PEAKS"))
                return FV_MarqueeType.AT_ENERGY_PEAKS;
            else if (value.StartsWith("AT_FIXED_INTERVALS_OF_"))
            {
                string[] words = value.Split('_');
                int i;
                if (!int.TryParse(words[4], out i))
                {
                    Log.WriteLine("FVConfig.MarqueeType():- WARNING! INVALID INTEGER:- " + words[4]);
                    interval = 0;
                }
                else
                    interval = i;
                return FV_MarqueeType.AT_FIXED_INTERVALS;
            }
            else
            {
                Log.WriteLine("FVConfig.MarqueeType():- WARNING! INVALID EXTRACTION VALUE IS DEFINED FOR FEATURE VECTORS! " + value);
                Log.WriteLine("                         SET THE DEFAULT:- MarqueeType = AT_ENERGY_PEAKS");
                return FV_MarqueeType.AT_ENERGY_PEAKS;
            }
        } //end GetMarqueeType()
        #endregion


        /// <summary>
        /// Saves both the config info and the actual FV files.
        /// Must save the config info first because this method calculates the FV file names.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="opDir"></param>
        public void SaveConfigAndFeatureVectors(TextWriter writer, string opDir)
        {
            Save(writer, opDir);
            SaveFeatureVectors(opDir);
        }


        public void SaveFeatureVectors(string opDir)
        {
            Log.WriteIfVerbose("START SaveFeatureVectors.Save()");
            var fName = FVArray[0].VectorFName; //check that first FV has a destination path and assume all do!
        
            // Ensure to save feature vectors first so paths are correctly set.
            //SaveFeatureVectors(Path.GetDirectoryName(templateFilePath), templateName + "_FV{0}.txt");

            Validation.Begin()
                        .IsNotNull(opDir, "Target folder must be supplied")
                        .IsNotNull(fName, "A pattern for feature vector filenames must be provided")
                        .Check();

            FVfNames = new string[FVArray.Length];

            for (int i = 0; i < FVArray.Length; i++)
            {
                fName = FVArray[i].VectorFName;
                var path = Path.Combine(opDir, string.Format(fName, i)+".txt");
                Log.WriteIfVerbose("\tSaving FV file to:  "+path);
                FVArray[i].SaveDataToFile(path);
                FVfNames[i] = path;


                if (BaseTemplate.InTestMode)
                {
                    Log.WriteLine("COMPARE FEATURE VECTOR FILES "+i);
                    UnitTests.AssertAreEqual(new FileInfo(path),
                                             new FileInfo(path + ".OLD"), true);
                }
            }// end pass over the array of FVs
            Log.WriteIfVerbose("END SaveFeatureVectors.Save()");


        } //end SaveFeatureVectors()


        public void Save(TextWriter writer, string opDir)
        {
            Log.WriteIfVerbose("START FVConfig.Save()");
            this.OPDir = opDir;

            writer.WriteLine("#**************** INFO ABOUT FEATURE VECTORS **************************");

            writer.WriteConfigValue("FV_SOURCE", FVSourceType.ToString());
            if (FVSourceType == FV_Source.MARQUEE)
            {
                writer.WriteConfigValue("MARQUEE_START", MarqueeStart);
                writer.WriteConfigValue("MARQUEE_END", MarqueeEnd);
                writer.WriteConfigValue("MARQUEE_TYPE", FVMarqueeType);
                if (FVMarqueeType == FV_MarqueeType.AT_FIXED_INTERVALS)
                {
                    writer.WriteConfigValue("MARQUEE_INTERVAL", MarqueeInterval);
                }
            }
            writer.Flush();
            writer.WriteConfigValue("FEATURE_VECTOR_LENGTH", FVLength);
            writer.WriteConfigValue("FV_COUNT", FVCount);
            writer.Flush();
            for (int n = 0; n < FVCount; n++)
            {
                if (FVArray == null)
                {
                    Log.WriteLine("FVConfig.Save. WARNING! FVArray == null");
                    break;
                }
                writer.WriteConfigValue("FV" + (n + 1) + "_DATA", FVArray[n].GetIniData());
                writer.WriteConfigValue("FV" + (n + 1) + "_SOURCE", FVArray[n].SourceFile);
            }
            writer.WriteLine("#");
            writer.Flush();
            Log.WriteIfVerbose("END FVConfig.Save()");
        }


        public void LoadFromFile(string templateDir)
        {
            Log.WriteIfVerbose("Loading " + FVCount + " feature vector(s) from file.");
            this.FVArray = new FeatureVector[FVCount];
            for (int i = 0; i < FVCount; i++)
            {
                string[] parts = FVIniData[i].Split('\t');
                Log.WriteIfVerbose("\tInit FeatureVector[" + (i + 1) + "] from file <" + parts[0] + ">, frames " + parts[1]);
                string fvPath = Path.Combine(templateDir, parts[0]+".txt");
                //Log.WriteIfVerbose("   Reading FV file " + fvPath);
                this.FVArray[i] = new FeatureVector(fvPath);
            }
        }//end LoadFromFile()



    } // end of class FVConfig
}
