using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using MarkovModels;
using AudioTools;



namespace AudioAnalysisTools
{
    class Template_DCT2D : BaseTemplate
    {
        public double[] Scores;

        public Template_DCT2D(Configuration config) : base(config)
		{
            SonogramConfig = new CepstralSonogramConfig(config);
            FeatureVectorConfig   = new FVConfig(config);
            AcousticModel   = new Acoustic_Model(config);

            //DEAL WITH THE VOCALISATION MODEL TYPE
            var modelName = config.GetString("MODEL_TYPE");
            LanguageModelType modelType = (LanguageModelType)Enum.Parse(typeof(LanguageModelType), modelName);
            this.Modeltype = modelType;

            //do not init a Model if in create new template mode.
            if (this.mode == Mode.CREATE_NEW_TEMPLATE) return;       

            if (modelType == LanguageModelType.UNDEFINED) LanguageModel = new Model_Undefined();
            else if (modelType == LanguageModelType.ONE_PERIODIC_SYLLABLE) LanguageModel = new Model_OnePeriodicSyllable(config);
            else if (modelType == LanguageModelType.MM_TWO_STATE_PERIODIC) LanguageModel = new Model_2StatePeriodic(config);
            else if (modelType == LanguageModelType.MM_ERGODIC)            LanguageModel = new Model_MMErgodic(config);
        }

        /// <summary>
        /// The call to static method FVExtractor.ExtractFVsFromRecording() results in the
        /// creation of an array of feature vectors each representing a portion of a vocalisation.
        /// </summary>
        /// <param name="ar"></param>
        protected override void ExtractTemplateFromRecording(AudioRecording ar)
        {
            if (Log.Verbosity == 1) Console.WriteLine("START Template_DCT2D.ExtractTemplateFromRecording()");
            string opDir = this.DataDir;
            double startTime = this.FeatureVectorConfig.StartTime;
            double endTime   = this.FeatureVectorConfig.EndTime;

            //save full length spectrogram image
            var ss = new SpectralSonogram(this.SonogramConfig, ar.GetWavReader());
            //var image = new Image_MultiTrack(ss.GetImage(false, false));
            string path = opDir + ar.FileName + ".png";
            //image.Save(path);
            Console.WriteLine("Full length of sonogram = "+ss.FrameCount+" frames = "+ss.Duration.TotalSeconds+" s");


            //extract portion of spectrogram
            var s2 = new SpectralSonogram(ss, startTime, endTime);
            var image = new Image_MultiTrack(s2.GetImage(false, false));
            string fName = ar.FileName + "_" + startTime.ToString("F0") + "s-" + endTime.ToString("F0") + "s";
            path = opDir + fName + ".png";
            image.Save(path);

            //extract and save part of wav file
            AudioRecording ar2 = ar.ExportSignal(startTime, endTime);            
            path = opDir + fName + ".wav";
            ar2.Save(path);

            //save extracted portion of wav file as spectrogram image - to compare with preivous verison
            //ss = new SpectralSonogram(this.SonogramConfig, ar2.GetWavReader());
            //image = new Image_MultiTrack(ss.GetImage(false, false));
            //path = opDir + fName + "alternative.png";
            //image.Save(path);

            double[] fv = Extract2D_DCTFromSonogram(s2);
            //init array of configs and then assign extracted 2d-DCT fv
            FeatureVectorConfig.FVCount = 1;
            FeatureVectorConfig.FVArray    = new FeatureVector[FeatureVectorConfig.FVCount];
            FeatureVectorConfig.FVArray[0] = new FeatureVector(fv, "Marquee_2D-DCT");

            //Console.WriteLine("End of the Line");
            //Console.ReadLine();
        }


        /// <summary>
        /// This method is called when constructing a DCT_2D template from entire sonogram.
        /// </summary>
        /// <param name="s">This is a short sonogram of the extracted portion of wav file</param>
        /// <param name="FVParams"></param>
        /// <param name="sonoConfig"></param>
        public static double[] Extract2D_DCTFromSonogram(SpectralSonogram s)
        {
            if (Log.Verbosity == 1) Console.WriteLine("START Template.Extract2D_DCTFromSonogram()");

            //Assume that the entire spectral sonogram is the marquee part required.
            var config = s.Configuration as CepstralSonogramConfig;
            double[,] cepstralM = Speech.DCT_2D(s.Data, config.MfccConfiguration.CcCount);
            int frameCount = cepstralM.GetLength(0);

            cepstralM = DataTools.normalise(cepstralM);

            Log.WriteIfVerbose("dim of cepstral matrix = " + frameCount + "*" + cepstralM.GetLength(1));
            ImageTools.DrawMatrix(cepstralM, @"C:\SensorNetworks\Templates\Template_4\matrix1.bmp");

            //pad to fixed number of frames
            double duration = 0.5; //duration of padded matrix in seconds
            int dim = (int)Math.Round(s.FramesPerSecond * duration);
            double[,] padM = new double[dim, config.MfccConfiguration.CcCount];
            Log.WriteIfVerbose("dim of padded matrix   = " + padM.GetLength(0) + "*" + padM.GetLength(1));
            for (int r = 0; r < frameCount; r++)
                for (int c = 0; c < config.MfccConfiguration.CcCount; c++) padM[r, c] = cepstralM[r, c];
            ImageTools.DrawMatrix(padM, @"C:\SensorNetworks\Templates\Template_4\matrix2.bmp");

            //do the DCT
            double[,] cosines = Speech.Cosines(dim, config.MfccConfiguration.CcCount + 1); //set up the cosine coefficients
            double[,] dctM = new double[config.MfccConfiguration.CcCount, config.MfccConfiguration.CcCount];
            Log.WriteIfVerbose("dim of DCT_2D matrix   = " + dctM.GetLength(0) + "*" + dctM.GetLength(1));
            for (int c = 0; c < config.MfccConfiguration.CcCount; c++)
            {
                double[] col = DataTools.GetColumn(padM, c);
                double[] dct = Speech.DCT(col, cosines);
                for (int r = 0; r < config.MfccConfiguration.CcCount; r++) dctM[r, c] = dct[r + 1]; //+1 in order to skip first DC value
            }
            ImageTools.DrawMatrix(dctM, @"C:\SensorNetworks\Templates\Template_4\matrix3.bmp");

            //store as single FV using the zig-zag 2D-DCT matrix
            if (Speech.zigzag12x12.GetLength(0) != config.MfccConfiguration.CcCount)
            {
                Log.WriteLine("zigzag dim != CcCount   " + Speech.zigzag12x12.GetLength(0) + " != " + config.MfccConfiguration.CcCount);
                throw new Exception("Fatal Error!");
            }
            else
                Log.WriteIfVerbose("zigzag dim = CcCount = " + Speech.zigzag12x12.GetLength(0));

            int FVdim = 70;
            double[] fv = new double[FVdim];
            for (int r = 0; r < config.MfccConfiguration.CcCount; r++)
                for (int c = 0; c < config.MfccConfiguration.CcCount; c++)
                {
                    int id = Speech.zigzag12x12[r, c];
                    if (id <= FVdim) fv[id - 1] = dctM[r, c];
                }
            return fv;
        } //end METHOD Extract2D_DCTFromSonogram(SpectralSonogram s)




        /// <summary>
        /// This method is called when extracting a 2D-DCT feature vector from portion of sonogram.
        /// </summary>
        /// <param name="s">This is sonogram of the extracted portion of wav file</param>
        /// <param name="FVParams"></param>
        /// <param name="sonoConfig"></param>
        public static double[] Extract2D_DCTFromMarquee(SpectralSonogram s, int startFrame, int endFrame)
        {
            //if (Log.Verbosity == 1) Console.WriteLine("START Template.ExtractFVsFromMarquee()");

            var config = s.Configuration as CepstralSonogramConfig;

            //the spectrogram data matrix
            int frameCount = endFrame - startFrame + 1;
            int featureCount = s.Data.GetLength(1);
            var data = new double[frameCount, featureCount];
            for (int i = 0; i < frameCount; i++) //each row of matrix is a frame
                for (int j = 0; j < featureCount; j++) //each col of matrix is a feature
                    data[i, j] = s.Data[startFrame + i, j];

            //convert spectral data to cepstral and noramlise in [0,1]
            double[,] cepstralM = Speech.DCT_2D(s.Data, config.MfccConfiguration.CcCount);
            cepstralM = DataTools.normalise(cepstralM);
            //Log.WriteIfVerbose("dim of cepstral matrix = " + frameCount + "*" + cepstralM.GetLength(1));

            //pad to fixed number of frames
            double duration = 0.5; //duration of padded matrix in seconds
            int dim = (int)Math.Round(s.FramesPerSecond * duration);
            double[,] padM = new double[dim, config.MfccConfiguration.CcCount];
            //Log.WriteIfVerbose("dim of padded matrix   = " + padM.GetLength(0) + "*" + padM.GetLength(1));
            for (int r = 0; r < frameCount; r++)
                for (int c = 0; c < config.MfccConfiguration.CcCount; c++) padM[r, c] = cepstralM[r, c];

            //do the DCT
            double[,] cosines = Speech.Cosines(dim, config.MfccConfiguration.CcCount + 1); //set up the cosine coefficients
            double[,] dctM = new double[config.MfccConfiguration.CcCount, config.MfccConfiguration.CcCount];
            //Log.WriteIfVerbose("dim of DCT_2D matrix   = " + dctM.GetLength(0) + "*" + dctM.GetLength(1));
            for (int c = 0; c < config.MfccConfiguration.CcCount; c++)
            {
                double[] col = DataTools.GetColumn(padM, c);
                double[] dct = Speech.DCT(col, cosines);
                for (int r = 0; r < config.MfccConfiguration.CcCount; r++) dctM[r, c] = dct[r + 1]; //+1 in order to skip first DC value
            }

            //store as single FV using the zig-zag 2D-DCT matrix
            if (Speech.zigzag12x12.GetLength(0) != config.MfccConfiguration.CcCount)
            {
                Log.WriteLine("zigzag dim != CcCount   " + Speech.zigzag12x12.GetLength(0) + " != " + config.MfccConfiguration.CcCount);
                throw new Exception("Fatal Error!");
            }
           // else
           //     Log.WriteIfVerbose("zigzag dim = CcCount = " + Speech.zigzag12x12.GetLength(0));

            int FVdim = 70;
            double[] fv = new double[FVdim];
            for (int r = 0; r < config.MfccConfiguration.CcCount; r++)
                for (int c = 0; c < config.MfccConfiguration.CcCount; c++)
                {
                    int id = Speech.zigzag12x12[r, c];
                    if (id <= FVdim) fv[id - 1] = dctM[r, c];
                }
            return fv;
        } //end METHOD Extract2D_DCTFromMarquee(SpectralSonogram s)

        /// <summary>
        /// PROBLEM IDENTIFIED IN THIS METHOD - EVERY EXTRACTED FV IS THE SAME!!!
        /// </summary>
        /// <param name="recording"></param>
        /// <param name="opDir"></param>
        public void ScanRecording(AudioRecording recording, string opDir)
        {
            Log.WriteIfVerbose("\n\nSTART Template_DCT2D.ScanRecording(opDir=" + opDir + ")");
            var spectralSono = new SpectralSonogram(this.SonogramConfig, recording.GetWavReader());
            var fv1 = this.FeatureVectorConfig.FVArray[0].Features;
            var fv1norm = DataTools.DiffFromMean(fv1);
            int[] segmentation = spectralSono.SigState;

            double[] scores = new double[segmentation.Length];

            double buffer = 0.24; //seconds
            int frameBuffer = (int)(spectralSono.FramesPerSecond * buffer);

            for (int i = frameBuffer; i < (spectralSono.FrameCount - frameBuffer); i++)
            {
                if (segmentation[i] < 2) continue;

                //if(i<100)Console.WriteLine(i.ToString("D4") + "\tstate=" + segmentation[i]+"   time="+time);
                double[] fv2 = Extract2D_DCTFromMarquee(spectralSono, i - frameBuffer, i + frameBuffer);
                scores[i] = DataTools.DotProduct(fv1norm, DataTools.DiffFromMean(fv2));
                Console.WriteLine(" score" + i.ToString("D4") + "=" + scores[i].ToString("F5") + "   fv2[0]=" + fv2[0].ToString("F3") + " fv2[1]=" + fv2[1].ToString("F3"));
                Console.WriteLine("PROBLEM IDENTIFIED IN THIS METHOD - EVERY EXTRACTED FV IS THE SAME!!!");
                Console.WriteLine("FUTURE WORK");
            }
            this.Scores = scores;
        }


		public override void Save(string targetPath)
		{
            Log.WriteIfVerbose("START Template_DCT2D.Save(targetPath=" + targetPath + ")");
            this.DataPath = targetPath;
            string opDir = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(opDir)) Directory.CreateDirectory(opDir);

            if (File.Exists(targetPath)) File.Copy(targetPath, targetPath + "OLD.txt", true); //overwrite
  
            Save(new StreamWriter(targetPath), opDir);
		}


		public void Save(TextWriter writer, string opDir)
		{
            base.Save(writer);
            //FftConfiguration.Save(writer); //do not print here because printed by FeatureVectorConfig
            SonogramConfig.Save(writer);
            FeatureVectorConfig.SaveConfigAndFeatureVectors(writer, opDir, this);
            AcousticModel.Save(writer);

            //write the default language model if only creating a new template
            if (this.mode == Mode.CREATE_NEW_TEMPLATE)
            {
                writer.WriteLine("#**************** INFO ABOUT THE LANGUAGE MODEL ***************");
                writer.WriteLine("# A LANGUAGE MODEL IS NOT DEFINED FOR THIS TEMPLATE TYPE ");
                //writer.WriteLine("#Options: UNDEFINED, ONE_PERIODIC_SYLLABLE, MM_ERGODIC, MM_TWO_STATE_PERIODIC");
                //writer.WriteLine("MODEL_TYPE=UNDEFINED");
                //writer.WriteLine("#MODEL_TYPE=" + this.Modeltype);
                //writer.WriteLine("NUMBER_OF_WORDS=1");
                //writer.WriteLine("WORD1_NAME=Dummy");
                //writer.WriteLine("WORD1_EXAMPLE1=1234");
                //writer.WriteLine("WORD1_EXAMPLE2=5678");
                writer.WriteLine("#");
                writer.Flush();
            }
            else LanguageModel.Save(writer);
        }

        public override void SaveResultsImage(SpectralSonogram sonogram, string imagePath, BaseResult result)
        {
            Log.WriteIfVerbose("Template_DCT2D.SaveResultsImage(SpectralSonogram sonogram, string imagePath, Results result)");
            bool doHighlightSubband = true;
            bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            int garbageID = this.AcousticModel.FvCount + 2 - 1;
            image.AddTrack(Image_Track.GetSyllablesTrack(this.AcousticModel.SyllableIDs, garbageID));
            image.AddTrack(Image_Track.GetScoreTrack(result.Scores, result.MinDisplayScore, result.MaxDisplayScore, result.DisplayThreshold));
            image.Save(imagePath);
        }

        public void SaveScanImage(SpectralSonogram sonogram, string imagePath)
        {
            Log.WriteIfVerbose("Template_DCT2D.SaveScanImage(SpectralSonogram sonogram, string imagePath)");
            bool doHighlightSubband = true;
            bool add1kHzLines = true;
            var image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
            int garbageID = this.AcousticModel.FvCount + 2 - 1;
            image.AddTrack(Image_Track.GetSyllablesTrack(this.AcousticModel.SyllableIDs, garbageID));
            double MinDisplayScore  = 0;
            double MaxDisplayScore  = 8;
            double DisplayThreshold = 2;
            image.AddTrack(Image_Track.GetScoreTrack(this.Scores, MinDisplayScore, MaxDisplayScore, DisplayThreshold));
            image.Save(imagePath);
        }


    }//end class
}
