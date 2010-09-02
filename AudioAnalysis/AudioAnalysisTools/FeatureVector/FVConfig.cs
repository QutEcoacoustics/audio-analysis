using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AudioTools;
using TowseyLib;
using QutSensors;
using QutSensors.Shared;


namespace AudioAnalysisTools
{
    using QutSensors.Shared;

    public enum FV_Source { SELECTED_FRAMES, FIXED_INTERVALS }

	[Serializable]
    public class FVConfig
    {


        #region Properties
        public int CallID { get; set; } //required for constructing FV file names

        public double StartTime{ get; set; }
        public double EndTime { get; set; }
        public ConfigKeys.Feature_Type FeatureExtractionType { get; set; }
        public int FVCount { get; set; }
        public string[] FVIniData { get; private set; }

        public FV_Source FVSourceType { get; set; }

        public int FVLength { get; set; }
        public int ExtractionInterval { get; set; }//used for automatic extraction

        public string OPDir { get; set; }
        public string[] FVfNames { get; set; }
        public string   FVSourceDir { get; set; }    //used when in AUTO mode
        public string[] FVSourceFiles { get; set; }  //used when in manual mode
        public string FV_DefaultNoisePath { get; set; }
        public FeatureVector DefaultNoiseFV { get; set; } //default noise FV used if cannot construct one from recording to be scanned
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
        public double[] DefaultModalNoiseProfile { get; set; } //yet to do this.

        #endregion




        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="config"></param>
        public FVConfig(Configuration config)
        {
            //FEATURE VECTORS
            var  featureExtractionName = config.GetString(ConfigKeys.Template.Key_FVType);
            this.FeatureExtractionType = (ConfigKeys.Feature_Type)Enum.Parse(typeof(ConfigKeys.Feature_Type), featureExtractionName);

            CallID = config.GetInt("TEMPLATE_ID");
            FV_DefaultNoisePath = config.GetPath(ConfigKeys.Template.Key_FVDefaultNoiseFile);


            if (! File.Exists(FV_DefaultNoisePath))
            {
                Log.WriteLine("WARNING!! CONSTRUCTOR FVConfig: Default noise file does not exist: <" + FV_DefaultNoisePath + ">");
                throw new Exception("Fatal Error");
            }

            Log.WriteLine("CONSTRUCTOR FVConfig: reading wav file for deriving noise FV: <" + FV_DefaultNoisePath + ">");
            WavReader wav = new WavReader(FV_DefaultNoisePath);
            var sonoConfig = new SonogramConfig(config);
            TriAvSonogram s = new TriAvSonogram(sonoConfig, wav);
            this.DefaultModalNoiseProfile = s.SnrFullband.ModalNoiseProfile;
            DefaultNoiseFV = Acoustic_Model.GetNoiseFeatureVector(s.Data, s.DecibelsNormalised, s.Max_dBReference);

            if(DefaultNoiseFV == null)
            {
                Log.WriteLine("WARNING!! CONSTRUCTOR FVConfig: File exists but cannot extract noise FV: <" + FV_DefaultNoisePath + ">");
                Log.WriteLine("WARNING!! Check that recording has anough low energy content for noise estimation.");
                throw new Exception("Fatal Error");
            }

            switch (FeatureExtractionType)
            {
                case ConfigKeys.Feature_Type.MFCC:
                    this.FVSourceType = FV_Source.SELECTED_FRAMES;
                    FVCount = config.GetInt(ConfigKeys.Template.Key_FVCount);
                    FVIniData = new string[FVCount];
                    string frames = config.GetString("FV_SELECTED_FRAMES");
                    //Log.WriteIfVerbose("\tSelected frames=" + frames);
                    for (int i = 0; i < FVCount; i++)
                    {
                        FVIniData[i] = config.GetString("FV"+(i+1)+"_FILE");
                    }
                    break;
                case ConfigKeys.Feature_Type.CC_AUTO:
                    //this.FVSourceType = FV_Source.FIXED_INTERVALS;
                    FVSourceDir = config.GetString(ConfigKeys.Recording.Key_TrainingDirName);
                    this.ExtractionInterval = config.GetInt(ConfigKeys.Template.Key_ExtractInterval); //extract feature vectors at this interval
                    //FVCount = config.GetInt("NUMBER_OF_SYLLABLES");
                    //Log.WriteIfVerbose("\tNUMBER_OF_SYLLABLES=" + FVCount);
                    break;
                case ConfigKeys.Feature_Type.DCT_2D:
                    this.StartTime = config.GetDouble(ConfigKeys.Mfcc.Key_StartTime);
                    this.EndTime   = config.GetDouble(ConfigKeys.Mfcc.Key_EndTime);
                    break;
            }
        }//end Constructor()



        #region Feature Vector Parameter Reading

        //public FV_Source GetFVSource(Configuration config)
        //{
        //    if (!config.ContainsKey("FV_SOURCE"))
        //    {
        //        Log.WriteLine("FVConfig.GetFVSource():- WARNING! NO SOURCE FOR FEATURE VECTORS IS DEFINED!");
        //        Log.WriteLine("                         SET THE DEFAULT: FV_Source = SELECTED_FRAMES");
        //        return FV_Source.SELECTED_FRAMES;
        //    }

        //    string value = config.GetString("FV_SOURCE");
        //    if (value.StartsWith("MARQUEE"))
        //    {
        //        return FV_Source.MARQUEE;

        //    }
        //    else if (value.StartsWith("SELECTED_FRAMES"))
        //        return FV_Source.SELECTED_FRAMES;
        //    else
        //    {
        //        Log.WriteLine("FVConfig.GetFVSource():- WARNING! INVALID SOURCE FOR FEATURE VECTORS IS DEFINED! " + value);
        //        Log.WriteLine("                         SET THE DEFAULT: FV_Source = SELECTED_FRAMES");
        //        return FV_Source.SELECTED_FRAMES;
        //    }
        //}


        //public FV_MarqueeType GetMarqueeType(Configuration config, out int? interval)
        //{
        //    interval = null;

        //    if (!config.ContainsKey("MARQUEE_TYPE"))
        //    {
        //        Log.WriteLine("FVConfig.GetMarqueeType():- WARNING! NO MARQUEE_TYPE (EXTRACTION PROCESS) IS DEFINED FOR FEATURE VECTORS!");
        //        Log.WriteLine("                            SET THE DEFAULT:- FV_MarqueeType = AT_ENERGY_PEAKS");
        //        return FV_MarqueeType.AT_ENERGY_PEAKS;
        //    }

        //    string value = config.GetString("MARQUEE_TYPE");
        //    if (value.StartsWith("AT_ENERGY_PEAKS"))
        //        return FV_MarqueeType.AT_ENERGY_PEAKS;
        //    else if (value.StartsWith("AT_FIXED_INTERVALS_OF_"))
        //    {
        //        string[] words = value.Split('_');
        //        int i;
        //        if (!int.TryParse(words[4], out i))
        //        {
        //            Log.WriteLine("FVConfig.MarqueeType():- WARNING! INVALID INTEGER:- " + words[4]);
        //            interval = 0;
        //        }
        //        else
        //            interval = i;
        //        return FV_MarqueeType.AT_FIXED_INTERVALS;
        //    }
        //    else
        //    {
        //        Log.WriteLine("FVConfig.MarqueeType():- WARNING! INVALID EXTRACTION VALUE IS DEFINED FOR FEATURE VECTORS! " + value);
        //        Log.WriteLine("                         SET THE DEFAULT:- MarqueeType = AT_ENERGY_PEAKS");
        //        return FV_MarqueeType.AT_ENERGY_PEAKS;
        //    }
        //} //end GetMarqueeType()
        #endregion


        /// <summary>
        /// Saves both the config info and the actual FV files.
        /// Must save the config info first because this method calculates the FV file names.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="opDir"></param>
        public void SaveConfigAndFeatureVectors(TextWriter writer, string opDir, BaseTemplate t)
        {
            Log.WriteIfVerbose("START FVConfig.SaveConfigAndFeatureVectors()");
            Save(writer, opDir);  //######### MUST DO THIS FIRST

            var fName = FVArray[0].name; //check that first FV has a destination path and assume all do!
        
            // Ensure to save feature vectors first so paths are correctly set.
            //SaveFeatureVectors(Path.GetDirectoryName(templateFilePath), templateName + "_FV{0}.txt");

            Validation.Begin()
                        .IsNotNull(opDir, "Target folder must be supplied")
                        .IsNotNull(fName, "A pattern for feature vector filenames must be provided")
                        .Check();

            FVfNames = new string[FVArray.Length];

            for (int i = 0; i < FVArray.Length; i++)
            {
                fName = FVArray[i].name;
                var path = Path.Combine(opDir, string.Format(fName, i)+".txt");
                Log.WriteIfVerbose("\tSaving FV file to:  "+path);

                FVArray[i].SaveDataAndImageToFile(path, t);
                FVfNames[i] = path;


                if (BaseTemplate.InTestMode)
                {
                    Log.WriteLine("COMPARE FEATURE VECTOR FILES "+i);
                    FunctionalTests.AssertAreEqual(new FileInfo(path),
                                             new FileInfo(path + "OLD.txt"), true);
                }
            }// end pass over the array of FVs
            Log.WriteIfVerbose("END FVConfig.SaveConfigAndFeatureVectors()");


        } //end SaveFeatureVectors()


        public void Save(TextWriter writer, string opDir)
        {
            Log.WriteIfVerbose("START FVConfig.Save()");
            this.OPDir = opDir;

            writer.WriteLine("#**************** INFO ABOUT FEATURE VECTORS **************************");

            //FV_DEFAULT_NOISE_FILE=C:\SensorNetworks\Templates\template_2_DefaultNoise.txt
            writer.WriteConfigValue("FV_DEFAULT_NOISE_FILE", FV_DefaultNoisePath);
            writer.WriteConfigValue("FV_SOURCE", FVSourceType.ToString());
            writer.Flush();
            writer.WriteConfigValue("FV_EXTRACTION_INTERVAL", ExtractionInterval);
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
                writer.WriteConfigValue("FV" + (n + 1) + "_FILE", FVArray[n].GetIniData());
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

            this.DefaultNoiseFV = new FeatureVector(FV_DefaultNoisePath);

        }//end LoadFromFile()



    } // end of class FVConfig
}
